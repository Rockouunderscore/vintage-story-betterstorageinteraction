
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.ItemPile;

public class BlockEntityItemPilePatches
{
    public static bool BlockEntityItemPile_TryTakeItem_Prefix(BlockEntityItemPile __instance, ref bool __result, ref InventoryGeneric ___inventory, IPlayer player)
    {
        __instance.Api.Logger.Debug("BlockEntityItemPile_TryTakeItem_Prefix");
        BlockEntityItemPilePatched itemPile = new BlockEntityItemPilePatched(__instance, ref __result, ref ___inventory);
        bool returnVal = itemPile.TryTakeItem(player);
        bool dontSkipOriginal = itemPile.dontSkipOriginal;
        
        __result = returnVal;
        return dontSkipOriginal;
    }
}