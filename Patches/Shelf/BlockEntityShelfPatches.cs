
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.Shelf;

public class BlockEntityShelfPatches
{
    public static bool BlockEntityShelf_TryTake_Prefix(BlockEntityShelf __instance, ref bool __result, ref InventoryGeneric ___inv, IPlayer byPlayer, BlockSelection blockSel)
    {
        BlockEntityShelfPatched shelf = new BlockEntityShelfPatched(__instance, ref __result, ref ___inv);
        bool returnVal = shelf.TryTake(byPlayer, blockSel);
        bool dontSkipOriginal = shelf.dontSkipOriginal;
        
        __result = returnVal;
        return dontSkipOriginal;
    }
}