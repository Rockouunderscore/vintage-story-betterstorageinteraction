
using System.Reflection;
using betterstorageinteractions.Patches;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;
using betterstorageinteractions.Patches.InventoryManager;
using Vintagestory.API.Server;

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
        
        // Harmony.DEBUG = true;
        if (!Harmony.HasAnyPatches(ModID))
        {
            harmony = new Harmony(ModID);
            if (Global.Config.PatchCrate)
            {
                MethodInfo original = typeof(BlockEntityCrate).GetMethod(nameof(BlockEntityCrate.OnBlockInteractStart));
                HarmonyMethod prefix = typeof(BlockEntityCratePatch).GetMethod(nameof(BlockEntityCratePatch.BlockEntityCrate_OnBlockInteractStart_Prefix));
                harmony.Patch(original, prefix: prefix);
            }

            if (Global.Config.FillHandFirst)
            {
                HarmonyMethod transpilerHandFirst = typeof(TryGiveItemstackPatch).GetMethod(nameof(TryGiveItemstackPatch.TryGiveItemstack_Replacer));

                if (Global.Config.PatchPlayerInventoryManager)
                {
                    MethodInfo original = typeof(PlayerInventoryManager).GetMethod(nameof(PlayerInventoryManager.TryGiveItemstack));
                    harmony.Patch(original, transpiler: transpilerHandFirst);
                }
                if (Global.Config.PatchShelf)
                {
                    MethodInfo original = typeof(BlockEntityShelf).GetMethod("TryTake", BindingFlags.NonPublic | BindingFlags.Instance);
                    harmony.Patch(original, transpiler: transpilerHandFirst);
                }
                if (Global.Config.PatchGroundStorage)
                {
                    MethodInfo stack = typeof(BlockEntityGroundStorage).GetMethod(nameof(BlockEntityGroundStorage.TryTakeItem));
                    MethodInfo singleItem = typeof(BlockEntityGroundStorage).GetMethod(nameof(BlockEntityGroundStorage.putOrGetItemSingle));
                    harmony.Patch(stack, transpiler: transpilerHandFirst);
                    harmony.Patch(singleItem, transpiler: transpilerHandFirst);
                }
                if (Global.Config.PatchItemPile)
                {
                    MethodInfo original = typeof(BlockEntityItemPile).GetMethod(nameof(BlockEntityItemPile.TryTakeItem));
                    harmony.Patch(original, transpiler: transpilerHandFirst);
                }
                if (Global.Config.PatchToolRack)
                {
                    MethodInfo original = typeof(BlockEntityToolrack).GetMethod("TakeFromSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                    harmony.Patch(original, transpiler: transpilerHandFirst);
                }
                if (Global.Config.PatchTorchHolder)
                {
                    MethodInfo original = typeof(BlockTorchHolder).GetMethod(nameof(BlockTorchHolder.OnBlockInteractStart));
                    harmony.Patch(original, transpiler: transpilerHandFirst);
                }
                if (Global.Config.PatchBlockBehaviorRightClickPickUp)
                {
                    MethodInfo original = typeof(BlockBehaviorRightClickPickup).GetMethod(nameof(BlockBehaviorRightClickPickup.OnBlockInteractStart));
                    harmony.Patch(original, transpiler: transpilerHandFirst);
                }
            }
        }
    }
    
    public override void Start(ICoreAPI api)
    {
        Mod.Logger.Notification($"{nameof(BetterStorageInteractionsModSystem)}.{nameof(Start)} {api.Side} {ModID}");

        Init(api);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        Mod.Logger.Notification($"{nameof(BetterStorageInteractionsModSystem)}.{nameof(StartServerSide)} {ModID}");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        Mod.Logger.Notification($"{nameof(BetterStorageInteractionsModSystem)}.{nameof(StartClientSide)} {ModID}");
    }
    
    public override void Dispose()
    {
        harmony?.UnpatchAll(ModID);
    }
}