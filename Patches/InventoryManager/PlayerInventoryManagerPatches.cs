using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.InventoryManager;

public class PlayerInventoryManagerPatches
{

    public static bool PlayerInventoryManager_TryGiveItemstack_Prefix(PlayerInventoryManager __instance, ItemStack itemstack, bool slotNotifyEffect = false)
    {
        Global.Api.Logger.Debug("PlayerInventoryManager_TryGiveItemstack_Prefix");
        
        ItemSlot dummySlot = new DummySlot(itemstack);
        ItemSlot handSlot = __instance.ActiveHotbarSlot;
        if (handSlot != null && handSlot.CanHold(dummySlot))
        {
            dummySlot.TryPutInto(handSlot);
            handSlot.MarkDirty();
        }
        
        return true;
    }
    
}