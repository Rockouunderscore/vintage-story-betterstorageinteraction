
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.GroundStorage;

public class BlockEntityGroundStoragePatched
{
    
    private BlockEntityGroundStorage orig;

    public ICoreAPI Api => orig.Api;
    public BlockPos Pos => orig.Pos;

    private InventoryGeneric inventory;

    public int TotalStackSize => orig.TotalStackSize;
    
    public int TransferQuantity => orig.TransferQuantity;
    public int BulkTransferQuantity => orig.BulkTransferQuantity;
    public GroundStorageProperties StorageProps => orig.StorageProps;

    
    public bool dontSkipOriginal;
    
    public BlockEntityGroundStoragePatched(BlockEntityGroundStorage __instance, ref bool __result, ref InventoryGeneric ___inventory)
    {
        orig = __instance;
        inventory = ___inventory;
        dontSkipOriginal = false;
    }

    public void MarkDirty()
    {
        orig.MarkDirty();
    }

    public bool TryTakeItem(IPlayer player)
    {
        bool takeBulk = player.Entity.Controls.CtrlKey;
        int q = GameMath.Min(takeBulk ? BulkTransferQuantity : TransferQuantity, TotalStackSize);

        if (inventory[0]?.Itemstack != null)
        {
            ItemStack stack = inventory[0].TakeOut(q);
            
            ItemSlot handSlot = player.InventoryManager.ActiveHotbarSlot;
            new DummySlot(stack).TryPutInto(handSlot);
            handSlot.MarkDirty();
            
            player.InventoryManager.TryGiveItemstack(stack);

            if (stack.StackSize > 0)
            {
                Api.World.SpawnItemEntity(stack, Pos);
            }
            Api.World.Logger.Audit("{0} Took {1}x{2} from Ground storage at {3}.",
                player.PlayerName,
                q,
                stack.Collectible.Code,
                Pos
            );
        }

        if (TotalStackSize == 0)
        {
            Api.World.BlockAccessor.SetBlock(0, Pos);
        }

        Api.World.PlaySoundAt(StorageProps.PlaceRemoveSound, Pos.X + 0.5, Pos.InternalY, Pos.Z + 0.5, null, 0.88f + (float)Api.World.Rand.NextDouble() * 0.24f, 16);

        MarkDirty();

        (player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

        return true;
    }
}