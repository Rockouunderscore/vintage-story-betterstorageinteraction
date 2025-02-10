
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.Crate;

public class BlockEntityCratePatched
{

    #region Original

    private BlockEntityCrate orig;

    public ICoreAPI Api => orig.Api;
    public BlockPos Pos => orig.Pos;
    
    private InventoryGeneric inventory;
    private ItemStack labelStack;
    private MeshData labelMesh;
    private int labelColor;
    public bool dontSkipOriginal;
    
    private MethodInfo _FreeAtlasSpace;
    private MethodInfo _didMoveItems;

    #endregion

    public BlockEntityCratePatched(BlockEntityCrate __instance, ref bool __result, ref InventoryGeneric ___inventory, ref ItemStack ___labelStack, ref MeshData ___labelMesh, ref int ___labelColor)
    {
        orig = __instance;
        dontSkipOriginal = false;
        
        inventory  = ___inventory;
        labelStack = ___labelStack;
        labelMesh = ___labelMesh;
        labelColor = ___labelColor;
        
        _FreeAtlasSpace = __instance.GetType()
            .GetMethod("FreeAtlasSpace", BindingFlags.Instance | BindingFlags.NonPublic);
        
        _didMoveItems = __instance.GetType()
            .GetMethod("didMoveItems", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="byPlayer"></param>
    /// <param name="blockSel"></param>
    /// <returns>Whether or not we execute the original stuff</returns>
    public bool OnBlockInteractStart(IPlayer byPlayer, BlockSelection blockSel)
    {
        // https://github.com/anegostudios/vssurvivalmod/blob/master/BlockEntity/BECrate.cs
        // Api.Logger.Debug($"{nameof(BlockEntityCratePatched)}.{nameof(OnBlockInteractStart)}()");
        
        /**
         * right-click = pull 1
         * control right-click = pull bulk
         * shift right-click = push 1
         * shift control right-click = push bulk
         */
        
        bool put = byPlayer.Entity.Controls.ShiftKey;
        bool bulk = byPlayer.Entity.Controls.CtrlKey;
        
        // Api.Logger.Debug($"Bulk {bulk}");

        #region Verifications

        // take && inventory empty
        if (!put && (inventory.Empty || inventory.FirstNonEmptySlot == null))
        {
            // we don't add any special behavior
            dontSkipOriginal = true;
            return false;
        }
        
        ItemSlot handSlot = byPlayer.InventoryManager?.ActiveHotbarSlot;
        // we have no handslot
        if (handSlot == null)
        {
            // shouldn't happen, fallback to original
            dontSkipOriginal = true;
            return false;
        }

        // put && inventory empty && hand empty
        if (put && inventory.Empty && handSlot.Itemstack == null)
        {
            // we don't add any special behavior
            dontSkipOriginal = true;
            return false;
        }
        
        // drawing
        bool drawIconLabel = put && handSlot.Itemstack?.ItemAttributes?["pigment"]?["color"].Exists == true && blockSel.SelectionBoxIndex == 1;
        if (drawIconLabel)
        {
            // we don't add any special behavior
            dontSkipOriginal = true;
            return false;
        }

        #endregion

        int amountTakeTotal = 0;
        int amountPutTotal = 0;
        AssetLocation auditCode = null;
        
        ItemSlot crateSlot = inventory.FirstNonEmptySlot;
        
        ItemStack crateItemType = (crateSlot ?? handSlot).Itemstack.GetEmptyClone();
        auditCode = crateItemType.Collectible.Code;
        
        if (put) // put 
        {
            // Api.Logger.Debug("Put");
            if (inventory.Empty)
            {
                int amountPut = PutCrate(handSlot, bulk ? 0 : 1);
                if (amountPut > 0)
                {
                    didMoveItems(crateItemType, byPlayer);
                    amountPutTotal += amountPut;
                }
            }
            else
            {
                IEnumerable<ItemSlot> invSlots = Util.GetPlayerMatchingItemSlots(byPlayer, crateItemType, false);
                ItemSlot matching = invSlots.FirstOrDefault();
                
                if (matching == null)
                {
                    dontSkipOriginal = true; // shouldnt happen, fallback
                    return false;
                }
                
                int amountPut = PutCrate(matching, bulk ? 0 : 1);
                if (amountPut > 0)
                {
                    didMoveItems(crateItemType, byPlayer);
                    amountPutTotal += amountPut;
                }
            }
            
            MarkDirty();
        }
        else // take 
        {
            // Api.Logger.Debug("Take");
            // we have to use a temp slot in case we do want a refill a full stack, even if it overfills our handslot
            ItemSlot tempSlot = new DummySlot();

            // take the amount wanted into a dummy slot, zero is until sinkslot is full
            amountTakeTotal = CrateTake(tempSlot, bulk ? 0 : 1);
            // Api.Logger.Debug($"amountTakeTotal {amountTakeTotal}");
            
            // pull into our hand first
            if (Global.Config.FillHandFirst)
            {
                bool handleOverflow = false;

                int amountTaken = tempSlot.TryPutInto(handSlot);
                // Api.Logger.Debug($"amountTaken {amountTaken}");
                
                // Api.Logger.Debug($"Take {amountTaken}");
                if (amountTaken > 0)
                {
                    handleOverflow = true;
                    didMoveItems(crateItemType, byPlayer);
                }
                    
                // if we don't want to overflow when refilling our hand, push it back into the crate
                bool tempStackValid = tempSlot.Itemstack != null && tempSlot.StackSize > 0;
                if (!Global.Config.FillHandFirstAllowOverflow && handleOverflow && bulk && tempStackValid)
                {
                    // Api.Logger.Debug("putback");
                    int amountPutBack = PutCrate(tempSlot);
                    amountTakeTotal -= amountPutBack;
                }
            }
            
            // pull the leftovers into our inventory
            // this is default behavior we fallback to
            

            if (tempSlot.StackSize > 0)
            {
                bool successfullyMovedAllItems = byPlayer.InventoryManager.TryGiveItemstack(tempSlot.Itemstack, true);
                if (!successfullyMovedAllItems)
                {
                    Api.World.SpawnItemEntity(tempSlot.Itemstack, Pos.ToVec3d().Add(0.5f + blockSel.Face.Normalf.X, 0.5f + blockSel.Face.Normalf.Y, 0.5f + blockSel.Face.Normalf.Z));
                }
                else
                {
                    didMoveItems(tempSlot.Itemstack, byPlayer);
                }
            }
            
            // if empty remove label
            
            if (inventory.Empty)
            {
                FreeAtlasSpace();
                labelStack = null;
                labelMesh = null;
            }
            MarkDirty();
            
            // __instance.Inventory.Do(slot => slot.MarkDirty());
            // firstNonEmptySlot.MarkDirty();
            
        }
        
        if (amountTakeTotal > 0 && auditCode != null)
        {
            Api.World.Logger.Audit(
                "{0} Took {1}x{2} from Crate at {3}.",
                byPlayer.PlayerName,
                amountTakeTotal,
                auditCode,
                Pos);
        }
        
        if (amountPutTotal > 0 && auditCode != null)
        {
            Api.World.Logger.Audit(
                "{0} Put {1}x{2} into Crate at {3}.", 
                byPlayer.PlayerName, 
                amountPutTotal,
                auditCode, 
                Pos);
        }
        
        dontSkipOriginal = false; // dont execute the others
        return true; // we did our thing and we skip any other prefixes and the original
    }

    #region Original method calls
    
    private void FreeAtlasSpace()
    {
        _FreeAtlasSpace.Invoke(orig, new object[]{ });
    }
    
    protected void didMoveItems(ItemStack stack, IPlayer byPlayer)
    {
        _didMoveItems.Invoke(orig, new object[] { stack, byPlayer });
    }

    public void MarkDirty(bool redrawOnClient = false, IPlayer skipPlayer = null)
    {
        orig.MarkDirty(redrawOnClient, skipPlayer);
    }

    #endregion

    private int CrateTake(ItemSlot sinkSlot, int quantity = 0)
    {
        // Api.Logger.Debug($"{nameof(BlockEntityCratePatched)}.{nameof(CrateTake)}(int quantity {quantity})");

        if (inventory.Empty)
        {
            return 0;
        }

        int amountMovedTotal = 0;
        foreach (ItemSlot crateSlot in inventory)
        {
            if (crateSlot.Empty)
            {
                continue;
            }

            if (quantity != 0 && amountMovedTotal >= quantity)
            {
                break;
            }
            
            int amountMoved = crateSlot.TryPutInto(sinkSlot, quantity);
            amountMovedTotal += amountMoved;
        }

        return amountMovedTotal;
    }

    private int PutCrate(ItemSlot sourceSlot, int quantity = 0)
    {
        // Api.Logger.Debug($"{nameof(BlockEntityCratePatched)}.{nameof(PutCrate)}()");
        
        if (quantity == 0)
        {
            quantity = sourceSlot.StackSize;
        }
        
        int amountMoved = 0;
        
        List<ItemSlot> skipSlots = new List<ItemSlot>();
        while (sourceSlot.StackSize > 0 && skipSlots.Count < inventory.Count)
        {
            if (amountMoved >= quantity)
            {
                break;
            }
            WeightedSlot bestSuitedSlot = inventory.GetBestSuitedSlot(sourceSlot, null, skipSlots);
            if (bestSuitedSlot.slot != null)
            {
                ItemSlot crateSlot = bestSuitedSlot.slot;
                skipSlots.Add(crateSlot);
                
                amountMoved += sourceSlot.TryPutInto(crateSlot, quantity);
            }
            else
            {
                break;
            }
        }
        
        return amountMoved;
    }
    
}