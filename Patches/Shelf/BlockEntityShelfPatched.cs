
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.Shelf;

public class BlockEntityShelfPatched
{
    
    private BlockEntityShelf orig;

    public ICoreAPI Api => orig.Api;
    public BlockPos Pos => orig.Pos;

    private InventoryGeneric inv;
    
    public bool dontSkipOriginal;
    
    public BlockEntityShelfPatched(BlockEntityShelf __instance, ref bool __result, ref InventoryGeneric ___inv)
    {
        orig = __instance;
        inv = ___inv;
        dontSkipOriginal = false;
    }

    public bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        
        
        bool up = blockSel.SelectionBoxIndex > 1;
        bool left = (blockSel.SelectionBoxIndex % 2) == 0;

        int start = (up ? 4 : 0) + (left ? 0 : 2);
        int end = start + 2;

        for (int i = end - 1; i >= start; i--)
        {
            if (!inv[i].Empty)
            {
                ItemStack stack = inv[i].TakeOut(1);

                ItemSlot handSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
                new DummySlot(stack).TryPutInto(handSlot);
                handSlot.MarkDirty();
                
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos);
                }
                Api.World.Logger.Audit("{0} Took 1x{1} from Shelf at {2}.",
                    byPlayer.PlayerName,
                    stack.Collectible.Code,
                    Pos
                );

                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                MarkDirty();
                return true;
            }
        }

        return false;
    }

    public void MarkDirty()
    {
        orig.MarkDirty();
    }
    
}