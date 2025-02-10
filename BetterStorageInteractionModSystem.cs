using System.Reflection;
using betterstorageinteraction.Patches.InventoryManager;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace betterstorageinteraction;

public class BetterStorageInteractionModSystem : ModSystem
{
    public const string ModID = "betterstorageinteraction";
    private const string ConfigFileName = ModID + "/config.json";
    
    private Harmony harmony;

    private void Init(ICoreAPI api)
    {
        Global.Api = api;
        try
        {
            Global.Config = Global.Api.LoadModConfig<Config>(ConfigFileName);
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
            if (Global.Config.EnableCratePatch)
            {
                MethodInfo original = typeof(BlockEntityCrate).GetMethod(nameof(BlockEntityCrate.OnBlockInteractStart));
                HarmonyMethod prefix = typeof(BlockEntityCratePatches).GetMethod(nameof(BlockEntityCratePatches.BlockEntityCrate_OnBlockInteractStart_Prefix));
                harmony.Patch(original, prefix: prefix);
            }

            if (Global.Config.EnablePlayerInventoryManagerPatch && Global.Config.FillHandFirst)
            {
                MethodInfo original = typeof(PlayerInventoryManager).GetMethod(nameof(PlayerInventoryManager.TryGiveItemstack));
                HarmonyMethod prefix = typeof(PlayerInventoryManagerPatches).GetMethod(nameof(PlayerInventoryManagerPatches.PlayerInventoryManager_TryGiveItemstack_Prefix));
                harmony.Patch(original, prefix: prefix);
            }
        }
    }
    
    public override void Start(ICoreAPI api)
    {
        api.Logger.Notification($"{nameof(BetterStorageInteractionModSystem)}.{nameof(Start)} {api.Side} {ModID}");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Logger.Notification($"{nameof(BetterStorageInteractionModSystem)}.{nameof(StartServerSide)} {ModID}");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Logger.Notification($"{nameof(BetterStorageInteractionModSystem)}.{nameof(StartClientSide)} {ModID}");
        
        Init(api);
    }
    
    public override void Dispose()
    {
        harmony?.UnpatchAll(ModID);
    }
}