using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace betterstorageinteraction;

public class BlockEntityCratePatches
{
    
    public static bool BlockEntityCrate_OnBlockInteractStart_Prefix(
    // public static bool Prefix(
        IPlayer byPlayer, 
        BlockSelection blockSel,
        BlockEntityCrate __instance,
        ref bool __result,
        ref InventoryGeneric ___inventory,
        ref ItemStack ___labelStack,
        ref MeshData ___labelMesh,
        ref int ___labelColor)
    {
        // https://github.com/anegostudios/vssurvivalmod/blob/master/BlockEntity/BECrate.cs
        // Global.Api.Logger.Debug("BlockEntityCratePatches.BlockEntityCrate_OnBlockInteractStart_Prefix");
        BlockEntityCratePatched original = new BlockEntityCratePatched(
            __instance,
            ref __result,
            ref ___inventory,
            ref ___labelStack,
            ref ___labelMesh,
            ref ___labelColor);
        bool returnVal = original.OnBlockInteractStart(byPlayer, blockSel);
        
        // Global.Api.Logger.Debug($"OnBlockInteractStart: {returnVal}, dontSkipOriginal: {original.dontSkipOriginal}");
        __result = returnVal;
        return original.dontSkipOriginal;
        
        // bool put = byPlayer.Entity.Controls.ShiftKey;
        // bool take = !put;
        // bool bulk = byPlayer.Entity.Controls.CtrlKey;
        //
        // if (take && (___inventory.Empty || ___inventory.FirstNonEmptySlot == null))
        // {
        //     // we take and inventory is empty
        //     // we don't add any special behavior
        //     return true;
        // }
        //
        // ItemSlot handSlot = byPlayer.InventoryManager?.ActiveHotbarSlot;
        // if (handSlot == null)
        // {
        //     // we can have no hotbar ???
        //     // we don't handle it
        //     return true; 
        // }
        //
        // if (put && ___inventory.Empty && handSlot.Empty)
        // {
        //     // inventory is empty
        //     // hand is empty
        //     // we don't add any special behavior
        //     return true;
        // }
        //
        // // draw
        // bool drawIconLabel = put && handSlot.Itemstack?.ItemAttributes?["pigment"]?["color"].Exists == true && blockSel.SelectionBoxIndex == 1;
        // if (drawIconLabel)
        // {
        //     // we don't add any special behavior
        //     return true;
        // }
        //
        // MethodInfo ___FreeAtlasSpace = __instance.GetType().GetMethod("FreeAtlasSpace", BindingFlags.Instance | BindingFlags.NonPublic);
        // MethodInfo ___didMoveItems = __instance.GetType().GetMethod("didMoveItems", BindingFlags.Instance | BindingFlags.NonPublic);
        //
        // int amountTakeTotal = 0;
        // int amountPutTotal = 0;
        // AssetLocation auditCode = null;
        //
        // ItemSlot crateSlot = ___inventory.FirstNonEmptySlot;
        // ItemStack crateItemType = crateSlot?.Itemstack.GetEmptyClone();
        //
        // // take
        // if (take)
        // {
        //     DummySlot tempSlot = new DummySlot();
        //     
        //     // take the amount wanted
        //     tempSlot.Itemstack = bulk ? Util.BulkCratePickUp(crateSlot) : crateSlot!.TakeOut(1);
        //     crateSlot!.MarkDirty();
        //     __instance.MarkDirty();
        //
        //     auditCode = tempSlot.Itemstack.Collectible.Code;
        //     
        //     // pull into our hand first
        //     
        //     if (Global.Config.FillHandFirst)
        //     {
        //         bool haveOverflow = false; // we don't send back into the crate
        //         
        //         if (!handSlot.Empty)
        //         {
        //             // send back into the crate if we can fit more
        //             haveOverflow = handSlot.Itemstack.StackSize < handSlot.Itemstack.Collectible.MaxStackSize;
        //             int handSpace = handSlot.Itemstack.Collectible.GetMergableQuantity(handSlot.Itemstack, tempSlot.Itemstack, EnumMergePriority.AutoMerge);
        //             __instance.Api.Logger.Debug($"handSpace {handSpace}");
        //             if (handSpace > 0)
        //             {
        //                 if (handSpace > tempSlot.Itemstack.StackSize)
        //                     handSpace = tempSlot.Itemstack.StackSize;
        //                 tempSlot.Itemstack.StackSize -= handSpace;
        //                 handSlot.Itemstack.StackSize += handSpace;
        //                 // int amountOut = tempSlot.TryPutInto(__instance.Api.World, handSlot, handSpace);
        //                 amountTakeTotal += handSpace;
        //                 handSlot.MarkDirty();
        //                 ___didMoveItems!.Invoke(__instance, new object[]{ handSlot.Itemstack, byPlayer });
        //             }
        //         }
        //         else
        //         {
        //             handSlot.Itemstack = tempSlot.TakeOutWhole();
        //             ___didMoveItems!.Invoke(__instance, new object[]{ handSlot.Itemstack, byPlayer });
        //         }
        //         
        //         // if we don't want to overflow when refilling our hand, push it back into the crate
        //         bool tempStackValid = tempSlot.Itemstack != null && tempSlot.Itemstack.StackSize > 0;
        //         bool crateStackable = crateItemType == null || crateItemType.Equals(__instance.Api.World, tempSlot.Itemstack, GlobalConstants.IgnoredStackAttributes);
        //         if (haveOverflow && bulk && Global.Config.FillHandFirstAllowOverflow && tempStackValid && crateStackable)
        //         {
        //             foreach (ItemSlot invSlot in ___inventory)
        //             {
        //                 if (invSlot.Itemstack == null)
        //                 {
        //                     invSlot.Itemstack = tempSlot.TakeOutWhole();
        //                     break;
        //                 }
        //                 int crateSlotSpace = invSlot.Itemstack.Collectible.GetMergableQuantity(invSlot.Itemstack, tempSlot.Itemstack, EnumMergePriority.AutoMerge);
        //                 int amountBackIn = invSlot.TryPutInto(__instance.Api.World, invSlot, crateSlotSpace);
        //                 amountTakeTotal -= amountBackIn;
        //                 if (invSlot.Itemstack.StackSize <= 0)
        //                 {
        //                     break;
        //                 }
        //             }
        //             
        //         }
        //     }
        //     
        //     // pull the leftovers into our inventory
        //     
        //     if (!byPlayer.InventoryManager.TryGiveItemstack(tempSlot.Itemstack, true))
        //     {
        //         __instance.Api.World.SpawnItemEntity(tempSlot.Itemstack, __instance.Pos.ToVec3d().Add(0.5f + blockSel.Face.Normalf.X, 0.5f + blockSel.Face.Normalf.Y, 0.5f + blockSel.Face.Normalf.Z));
        //     }
        //     else
        //     {
        //         // ___didMoveItems(tempSlot.Itemstack, byPlayer);
        //         ___didMoveItems!.Invoke(__instance, new object[]{ tempSlot.Itemstack, byPlayer });
        //     }
        //     
        //     // if empty remove label
        //     
        //     if (___inventory.Empty)
        //     {
        //         // ___FreeAtlasSpace();
        //         ___FreeAtlasSpace!.Invoke(__instance, new object[]{ });
        //         ___labelStack = null;
        //         ___labelMesh = null;
        //     }
        //     
        //     // __instance.Inventory.Do(slot => slot.MarkDirty());
        //     // firstNonEmptySlot.MarkDirty();
        //     
        // }
        //
        // /**
        //  * put
        //  */
        // if (put)
        // {
        //     if (___inventory.Empty)
        //     {
        //         
        //         // we'll use `___inventory[0]` because our entire inventory is empty, so any ItemSlot is okay
        //         ItemSlot anyCrateSlot = ___inventory[0];
        //         
        //         int quantity = bulk ? handSlot.StackSize : 1;
        //         
        //         int amountPut = handSlot.TryPutInto(__instance.Api.World, anyCrateSlot, quantity);
        //         
        //         if (amountPut > 0)
        //         {
        //             // ___didMoveItems(___inventory[0].Itemstack, byPlayer);
        //             ___didMoveItems!.Invoke(__instance, new object[]{ anyCrateSlot.Itemstack, byPlayer });
        //             
        //             amountPutTotal += amountPut;
        //             auditCode = (handSlot.Itemstack ?? anyCrateSlot.Itemstack)?.Collectible.Code;
        //         }
        //         handSlot.MarkDirty();
        //         anyCrateSlot.MarkDirty();
        //     }
        //     else //if (!___inventory.Empty)
        //     {
        //         IEnumerable<ItemSlot> invSlots = BlockEntityCratePatchUtils.GetPlayerMatchingItemSlots(byPlayer, crateItemType, false);
        //         ItemSlot matching = invSlots.FirstOrDefault();
        //         
        //         List<ItemSlot> skipSlots = new List<ItemSlot>();
        //         while (matching != null && matching.StackSize > 0 && skipSlots.Count < ___inventory.Count)
        //         {
        //             WeightedSlot bestSuitedSlot = ___inventory.GetBestSuitedSlot(matching, null, skipSlots);
        //             if (bestSuitedSlot.slot != null)
        //             {
        //                 int quantity = bulk ? matching.StackSize : 1;
        //                 int amountPut = matching.TryPutInto(__instance.Api.World, bestSuitedSlot.slot, quantity);
        //                 matching.MarkDirty();
        //                 bestSuitedSlot.slot.MarkDirty();
        //                 
        //                 if (amountPut > 0)
        //                 {
        //                     // ___didMoveItems(bestSuitedSlot.slot.Itemstack, byPlayer);
        //                     ___didMoveItems!.Invoke(__instance, new object[]{ bestSuitedSlot.slot.Itemstack, byPlayer });
        //                     
        //                     amountPutTotal += amountPut;
        //                     auditCode = bestSuitedSlot.slot.Itemstack?.Collectible.Code;
        //                     
        //                     if (!bulk)
        //                     {
        //                         break;
        //                     }
        //                 }
        //                 skipSlots.Add(bestSuitedSlot.slot);
        //             }
        //             else
        //             {
        //                 break;
        //             }
        //         }
        //     }
        //     
        //     __instance.MarkDirty();
        // }
        //
        // if (amountTakeTotal > 0 && auditCode != null)
        // {
        //     __instance.Api.World.Logger.Audit(
        //         "{0} Took {1}x{2} from Crate at {3}.",
        //         byPlayer.PlayerName,
        //         amountTakeTotal,
        //         auditCode,
        //         __instance.Pos);
        // }
        //
        // if (amountPutTotal > 0 && auditCode != null)
        // {
        //     __instance.Api.World.Logger.Audit(
        //         "{0} Put {1}x{2} into Crate at {3}.", 
        //         byPlayer.PlayerName, 
        //         amountPutTotal,
        //         auditCode, 
        //         __instance.Pos);
        // }
        //
        // __result = true;
        // return false; // we did our thing and we skip any other prefixes and the original
    }
    
}

