using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppleTreesEnhanced;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public partial class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.appletreesenhanced";
    private const string PluginName = "Apple Tree's Enhanced!";
    private const string PluginVer = "2.7.3";
    private static ManualLogSource Log { get; set; }
    private static Harmony _harmony;

    private static ConfigEntry<bool> _debug;

    private static ConfigEntry<bool> _modEnabled;

    private void Awake()
    {
        _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 8}));
        _debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 7}));

        IncludeGardenBerryBushes = Config.Bind("Player Garden", "Include Garden Berry Bushes", true, new ConfigDescription("Enhance player garden berry bushes.", null, new ConfigurationManagerAttributes {Order = 6}));
        IncludeGardenTrees = Config.Bind("Player Garden", "Include Garden Trees", true, new ConfigDescription("Enhance player garden trees.", null, new ConfigurationManagerAttributes {Order = 5}));
        IncludeGardenBeeHives = Config.Bind("Player Garden", "Include Garden Bee Hives", false, new ConfigDescription("Enhance player garden bee hives.", null, new ConfigurationManagerAttributes {Order = 4}));

        RealisticHarvest = Config.Bind("Harvesting", "Realistic Harvest", true, new ConfigDescription("Enable realistic harvest.", null, new ConfigurationManagerAttributes {Order = 3}));
        ShowHarvestReadyMessages = Config.Bind("Harvesting", "Show Harvest Ready Messages", true, new ConfigDescription("Show harvest ready messages.", null, new ConfigurationManagerAttributes {Order = 2}));

        IncludeWorldBerryBushes = Config.Bind("World Environment", "Include World Berry Bushes", false, new ConfigDescription("Enhance world berry bushes.", null, new ConfigurationManagerAttributes {Order = 1}));

        BeeKeeperBuyback = Config.Bind("Economy", "Bee Keeper Buyback", false, new ConfigDescription("Allow bee keeper to buy back bees.", null, new ConfigurationManagerAttributes {Order = 0}));

        Log = Logger;
        _harmony = new Harmony(PluginGuid);
        if (_modEnabled.Value)
        {
            Log.LogWarning($"Applying patches for {PluginName}");
            Actions.GameStartedPlaying += CleanUpTrees;
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    private static void ToggleMod(ConfigEntryBase entry)
    {
        var ticked = GUILayout.Toggle(_modEnabled.Value, "Enabled");

        if (ticked == _modEnabled.Value) return;
        _modEnabled.Value = ticked;

        if (ticked)
        {
            Log.LogWarning($"Applying patches for {PluginName}");
            Actions.GameStartedPlaying += CleanUpTrees;
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Log.LogWarning($"Removing patches for {PluginName}");
            Actions.GameStartedPlaying -= CleanUpTrees;
            _harmony.UnpatchSelf();
        }
    }

    private void OnEnable()
    {
        Log.LogInfo($"Plugin {PluginName} has been enabled!");
    }

    private void OnDisable()
    {
        Log.LogError($"Plugin {PluginName} has been disabled!");
    }

    private static void CleanUpTrees(MainGame mainGame)
    {
        Plugin.Log.LogWarning($"Running CleanUpTrees as Player has spawned in.");
        if (!MainGame.game_started) return;

        var dudBees = FindObjectsOfType<WorldGameObject>(true)
            .Where(a => a.obj_id == Helpers.Constants.HarvestGrowing.BeeHouse).Where(b => b.progress <= 0)
            .Where(Helpers.IsPlayerBeeHive);
        var dudBeesCount = 0;
        foreach (var dudBee in dudBees)
        {
            dudBeesCount++;
            Helpers.ProcessBeeRespawn(dudBee);
            if (_debug.Value)
            {
                Log.LogMessage($"Fixed DudBee {dudBeesCount}");
            }
        }

        var dudTrees = FindObjectsOfType<WorldGameObject>(true)
            .Where(a => a.obj_id == Helpers.Constants.HarvestGrowing.GardenAppleTree).Where(b => b.progress <= 0);
        var dudTreeCount = 0;
        foreach (var dudTree in dudTrees)
        {
            dudTreeCount++;
            Helpers.ProcessRespawn(dudTree, Helpers.Constants.HarvestGrowing.GardenAppleTree,
                Helpers.Constants.HarvestSpawner.GardenAppleTree);
            if (_debug.Value)
            {
                Log.LogMessage($"Fixed DudGardenTree {dudTreeCount}");
            }
        }

        var dudBushes = FindObjectsOfType<WorldGameObject>(true)
            .Where(a => a.obj_id == Helpers.Constants.HarvestGrowing.GardenBerryBush).Where(b => b.progress <= 0);
        var dudBushCount = 0;
        foreach (var dudBush in dudBushes)
        {
            dudBushCount++;
            Helpers.ProcessRespawn(dudBush, Helpers.Constants.HarvestGrowing.GardenBerryBush,
                Helpers.Constants.HarvestSpawner.GardenBerryBush);

            if (_debug.Value)
            {
                Log.LogMessage($"Fixed DudGardenBush {dudBushCount}");
            }
        }

        var readyBees = FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id == Helpers.Constants.HarvestReady.BeeHouse)
            .Where(Helpers.IsPlayerBeeHive);
        var readyGardenTrees = FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id == Helpers.Constants.HarvestReady.GardenAppleTree);
        var readyGardenBushes = FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id == Helpers.Constants.HarvestReady.GardenBerryBush);
        var readyWorldBushes = FindObjectsOfType<WorldGameObject>(true).Where(a => Helpers.WorldReadyHarvests.Contains(a.obj_id));

        foreach (var item in readyBees)
        {
            Helpers.ProcessGardenBeeHive(item);
        }

        foreach (var item in readyGardenTrees)
        {
            Helpers.ProcessGardenAppleTree(item);
        }

        foreach (var item in readyGardenBushes)
        {
            Helpers.ProcessGardenBerryBush(item);
        }

        foreach (var item in readyWorldBushes)
        {
            switch (item.obj_id)
            {
                case Helpers.Constants.HarvestReady.WorldBerryBush1:
                    Helpers.ProcessBerryBush1(item);
                    break;

                case Helpers.Constants.HarvestReady.WorldBerryBush2:
                    Helpers.ProcessBerryBush2(item);
                    break;

                case Helpers.Constants.HarvestReady.WorldBerryBush3:
                    Helpers.ProcessBerryBush3(item);
                    break;
            }
        }
    }
}