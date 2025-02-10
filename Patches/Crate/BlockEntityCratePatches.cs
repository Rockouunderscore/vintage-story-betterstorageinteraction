using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace betterstorageinteractions.Patches.Crate;

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
    }
    
}

