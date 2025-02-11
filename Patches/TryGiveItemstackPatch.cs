
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace betterstorageinteractions.Patches;

public class TryGiveItemstackPatch
{
    
    // https://github.com/anegostudios/vssurvivalmod/blob/master/BlockEntity/BEToolrack.cs

    private static bool PatchedTryGiveItemstack(PlayerInventoryManager instance, ItemStack itemstack, bool slotNotifyEffect = false)
    {
        Global.Api.Logger.Debug($"PatchedTryGiveItemstack called");
        ItemSlot handSlot = instance.ActiveHotbarSlot;
        
        ItemSlot tempSlot = new DummySlot(itemstack);
        tempSlot.TryPutInto(handSlot);
        handSlot.MarkDirty();
        // we still have items in our item stack so we call the original method and return its result
        if (tempSlot.StackSize > 0)
        {
            return instance.TryGiveItemstack(itemstack, slotNotifyEffect);
        }
        // we did everything successfully with no items remaining in the stack
        // we don't need to fallback to original
        return true;
    }
    
    // Harmony entry point
    public static IEnumerable<CodeInstruction> TryGiveItemstack_Replacer(IEnumerable<CodeInstruction> instructions)
    {
        bool done = false;
        MethodInfo dispatchMethodInfo = SymbolExtensions.GetMethodInfo(() => PatchedTryGiveItemstack(null, null, false));
        // MethodInfo dispatchMethodInfo = typeof(TryGiveItemstackPatch).GetMethod(nameof(PatchedTryGiveItemstack));
        foreach (var instruction in instructions)
        {
            // we arent done with what we were supposed to do
            // we are calling a method
            if (!done && instruction.opcode == OpCodes.Callvirt && instruction.operand != null)
            {
                // we are calling a method called TryGiveItemstack
                string strop = instruction.operand.ToString();
                if (strop != null && strop.Contains(nameof(PlayerInventoryManager.TryGiveItemstack)))
                {
                    // replace with our method
                    yield return new CodeInstruction(OpCodes.Call, dispatchMethodInfo);
                    // we are done
                    done = true;
                    
                    continue;
                }
            }

            yield return instruction;
        }
    }
    
}