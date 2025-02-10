
using betterstorageinteractions.Patches.Shelf;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.GroundStorage;

public class BlockEntityGroundStoragePatches
{
    public static bool BlockEntityGroundStorage_TryTakeItem_Prefix(BlockEntityGroundStorage __instance, ref bool __result, ref InventoryGeneric ___inventory, IPlayer player)
    {
        BlockEntityGroundStoragePatched groundStorage = new BlockEntityGroundStoragePatched(__instance, ref __result, ref ___inventory);
        bool returnVal = groundStorage.TryTakeItem(player);
        bool dontSkipOriginal = groundStorage.dontSkipOriginal;
        
        __result = returnVal;
        return dontSkipOriginal;
    }
}