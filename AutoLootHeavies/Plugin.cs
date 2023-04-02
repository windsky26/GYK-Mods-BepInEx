using System.Collections.Generic;
using System.Reflection;
using AutoLootHeavies.lang;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using GYKHelper;
using UnityEngine;

namespace AutoLootHeavies
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.autolootheavies";
        private const string PluginName = "Auto-Loot Heavies!";
        private const string PluginVer = "3.4.5";

        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;
        private static readonly List<Stockpile> SortedStockpiles = new();

        private static float _lastBubbleTime;

        // private static bool _needScanning = true;
        private static List<WorldGameObject> _objects;

        private static float _xAdjustment;
        private const float EnergyRequirement = 3f;
    
        internal static ConfigEntry<bool> TeleportToDumpSiteWhenAllStockPilesFull;
        internal static ConfigEntry<Vector3> DesignatedTimberLocation;
        internal static ConfigEntry<Vector3> DesignatedOreLocation;
        internal static ConfigEntry<Vector3> DesignatedStoneLocation;
        internal static ConfigEntry<bool> DisableImmersionMode;
        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<KeyboardShortcut> ToggleTeleportToDumpSiteKeybind;
        internal static ConfigEntry<KeyboardShortcut> SetTimberLocationKeybind;
        internal static ConfigEntry<KeyboardShortcut> SetOreLocationKeybind;
        internal static ConfigEntry<KeyboardShortcut> SetStoneLocationKeybind;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
            TeleportToDumpSiteWhenAllStockPilesFull = Config.Bind("General", "Teleport To Dump Site When All Stock Piles Full", true, new ConfigDescription("Enable or disable teleporting to the dump site when all stockpiles are full.", null, new ConfigurationManagerAttributes {Order = 500}));

            DisableImmersionMode = Config.Bind("General", "Disable Immersion Mode", false, new ConfigDescription("Enable or disable Immersion Mode.", null, new ConfigurationManagerAttributes {Order = 499}));

            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 498}));

            DesignatedTimberLocation = Config.Bind("Locations", "Designated Timber Location", new Vector3(-3712.003f, 6144f, 1294.643f), new ConfigDescription("Set the designated Timber Location.", null, new ConfigurationManagerAttributes {Order = 497}));

            DesignatedOreLocation = Config.Bind("Locations", "Designated Ore Location", new Vector3(-3712.003f, 6144f, 1294.643f), new ConfigDescription("Set the designated Ore Location.", null, new ConfigurationManagerAttributes {Order = 496}));

            DesignatedStoneLocation = Config.Bind("Locations", "Designated Stone Location", new Vector3(-3712.003f, 6144f, 1294.643f), new ConfigDescription("Set the designated Stone Location.", null, new ConfigurationManagerAttributes {Order = 495}));

            ToggleTeleportToDumpSiteKeybind = Config.Bind("Keybinds", "Toggle Teleport To Dump Site Keybind", new KeyboardShortcut(KeyCode.Alpha6), new ConfigDescription("Set the keybind for toggling Teleport To Dump Site.", null, new ConfigurationManagerAttributes {Order = 494}));

            SetTimberLocationKeybind = Config.Bind("Keybinds", "Set Timber Location Keybind", new KeyboardShortcut(KeyCode.Alpha7), new ConfigDescription("Set the keybind for setting Timber Location.", null, new ConfigurationManagerAttributes {Order = 493}));

            SetOreLocationKeybind = Config.Bind("Keybinds", "Set Ore Location Keybind", new KeyboardShortcut(KeyCode.Alpha8), new ConfigDescription("Set the keybind for setting Ore Location.", null, new ConfigurationManagerAttributes {Order = 492}));

            SetStoneLocationKeybind = Config.Bind("Keybinds", "Set Stone Location Keybind", new KeyboardShortcut(KeyCode.Alpha9), new ConfigDescription("Set the keybind for setting Stone Location.", null, new ConfigurationManagerAttributes {Order = 491}));

            
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.WorldGameObjectInteract += WorldGameObjectInteract;
                Log.LogWarning($"Applying patches for {PluginName}");
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
                Actions.WorldGameObjectInteract += WorldGameObjectInteract;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.WorldGameObjectInteract -= WorldGameObjectInteract;
                Log.LogWarning($"Removing patches for {PluginName}");
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
    }
}