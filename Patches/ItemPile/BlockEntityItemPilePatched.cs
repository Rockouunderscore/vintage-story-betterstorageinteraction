
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.ItemPile;

public class BlockEntityItemPilePatched
{
    
    private BlockEntityItemPile orig;

    public ICoreAPI Api => orig.Api;
    public BlockPos Pos => orig.Pos;

    private InventoryGeneric inventory;

    public int OwnStackSize => orig.OwnStackSize;
    public int DefaultTakeQuantity => orig.DefaultTakeQuantity;
    public int BulkTakeQuantity => orig.BulkTakeQuantity;
    public AssetLocation SoundLocation => orig.SoundLocation;
    public Block Block => orig.Block;

    
    public bool dontSkipOriginal;
    
    public BlockEntityItemPilePatched(BlockEntityItemPile __instance, ref bool __result, ref InventoryGeneric ___inventory)
    {
        orig = __instance;
        inventory = ___inventory;
        dontSkipOriginal = false;
    }

    public bool TryTakeItem(IPlayer player)
    {
        bool takeBulk = player.Entity.Controls.CtrlKey;
        int q = GameMath.Min(takeBulk ? BulkTakeQuantity : DefaultTakeQuantity, OwnStackSize);

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
            Api.World.Logger.Audit("{0} Took {1}x{2} from {3} at {4}.",
                player.PlayerName,
                q,
                stack.Collectible.Code,
                Block.Code,
                Pos
            );
        }

        if (OwnStackSize == 0)
        {
            Api.World.BlockAccessor.SetBlock(0, Pos);
        }

        Api.World.PlaySoundAt(SoundLocation, Pos.X + 0.5, Pos.InternalY, Pos.Z + 0.5, player, 0.88f + (float)Api.World.Rand.NextDouble() * 0.24f, 16);

        MarkDirty();

        (player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

        return true;
    }

    public void MarkDirty()
    {
        orig.MarkDirty();
    }
    
}