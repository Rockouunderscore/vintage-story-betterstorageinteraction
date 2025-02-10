using Vintagestory.API.Common;
using Vintagestory.Common;

namespace betterstorageinteraction.Patches.InventoryManager;

public class PlayerInventoryManagerPatches
{

    public static bool PlayerInventoryManager_TryGiveItemstack_Prefix(PlayerInventoryManager __instance, ItemStack itemstack, bool slotNotifyEffect = false)
    {
        Global.Api.Logger.Debug("PlayerInventoryManager_TryGiveItemstack_Prefix");

        ItemSlot handSlot = __instance.ActiveHotbarSlot;
        handSlot?.TryTakeInto(ref itemstack);
        handSlot?.MarkDirty();
        
        return true;
    }
    
}