
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;
using betterstorageinteractions.Patches.Crate;
using betterstorageinteractions.Patches.GroundStorage;
using betterstorageinteractions.Patches.InventoryManager;
using betterstorageinteractions.Patches.ItemPile;
using betterstorageinteractions.Patches.Shelf;

namespace betterstorageinteractions;

public class BetterStorageInteractionsModSystem : ModSystem
{
    public const string ModID = "betterstorageinteractions";
    private const string ConfigFileName = ModID + "/config.json";
    
    private Harmony harmony;

    private void Init(ICoreAPI api)
    {
        Global.Api = api;
        try
        {
            Global.Config = Global.Api.LoadModConfig<Config>(ConfigFileName);
            Global.Api.StoreModConfig(Global.Config, ConfigFileName); // update the config file
        }
        finally
        {
            if (Global.Config == null)
            {
                Global.Config = new Config();
                Global.Api.StoreModConfig(Global.Config, ConfigFileName);
            }
        }
        
        Harmony.DEBUG = true;
        if (!Harmony.HasAnyPatches(ModID))
        {
            harmony = new Harmony(ModID);
            if (Global.Config.PatchCrate)
            {
                MethodInfo original = typeof(BlockEntityCrate).GetMethod(nameof(BlockEntityCrate.OnBlockInteractStart));
                HarmonyMethod prefix = typeof(BlockEntityCratePatches).GetMethod(nameof(BlockEntityCratePatches.BlockEntityCrate_OnBlockInteractStart_Prefix));
                harmony.Patch(original, prefix: prefix);
            }

            if (Global.Config.PatchPlayerInventoryManager && Global.Config.FillHandFirst)
            {
                MethodInfo original = typeof(PlayerInventoryManager).GetMethod(nameof(PlayerInventoryManager.TryGiveItemstack));
                HarmonyMethod prefix = typeof(PlayerInventoryManagerPatches).GetMethod(nameof(PlayerInventoryManagerPatches.PlayerInventoryManager_TryGiveItemstack_Prefix));
                harmony.Patch(original, prefix: prefix);
            }

            if (Global.Config.PatchShelf && Global.Config.FillHandFirst)
            {
                MethodInfo original = typeof(BlockEntityShelf).GetMethod("TryTake", BindingFlags.NonPublic | BindingFlags.Instance);
                HarmonyMethod prefix = typeof(BlockEntityShelfPatches).GetMethod(nameof(BlockEntityShelfPatches.BlockEntityShelf_TryTake_Prefix));
                harmony.Patch(original, prefix: prefix);
            }

            if (Global.Config.PatchGroundStorage && Global.Config.FillHandFirst)
            {
                MethodInfo original = typeof(BlockEntityGroundStorage).GetMethod(nameof(BlockEntityGroundStorage.TryTakeItem));
                HarmonyMethod prefix = typeof(BlockEntityGroundStoragePatches).GetMethod(nameof(BlockEntityGroundStoragePatches.BlockEntityGroundStorage_TryTakeItem_Prefix));
                harmony.Patch(original, prefix: prefix);
            }

            if (Global.Config.PatchItemPile && Global.Config.FillHandFirst)
            {
                MethodInfo original = typeof(BlockEntityItemPile).GetMethod(nameof(BlockEntityItemPile.TryTakeItem));
                HarmonyMethod prefix = typeof(BlockEntityItemPilePatches).GetMethod(nameof(BlockEntityItemPilePatches.BlockEntityItemPile_TryTakeItem_Prefix));
                harmony.Patch(original, prefix: prefix);
                
                // MethodInfo originalFirewood = typeof(BlockEntityPeatPile).GetMethod("TryTakeItem", BindingFlags.NonPublic | BindingFlags.Instance);
                // harmony.Patch(originalFirewood, prefix: prefix);
            }
        }
    }
    
    // public override void Start(ICoreAPI api)
    // {
    //     api.Logger.Notification($"{nameof(BetterStorageInteractionsModSystem)}.{nameof(Start)} {api.Side} {ModID}");
    // }

    // public override void StartServerSide(ICoreServerAPI api)
    // {
    //     api.Logger.Notification($"{nameof(BetterStorageInteractionsModSystem)}.{nameof(StartServerSide)} {ModID}");
    // }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Logger.Notification($"{nameof(BetterStorageInteractionsModSystem)}.{nameof(StartClientSide)} {ModID}");
        
        Init(api);
    }
    
    public override void Dispose()
    {
        harmony?.UnpatchAll(ModID);
    }
}