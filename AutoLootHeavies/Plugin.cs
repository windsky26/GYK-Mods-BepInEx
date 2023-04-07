using System;
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
        private const string PluginVer = "3.4.6";

        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;
        private static readonly List<Stockpile> SortedStockpiles = new();

        private static float _lastBubbleTime;
        private static List<WorldGameObject> _objects;
        private const float EnergyRequirement = 3f;

        private static ConfigEntry<bool> _teleportToDumpSiteWhenAllStockPilesFull;
        private static ConfigEntry<Vector3> _designatedTimberLocation;
        private static ConfigEntry<Vector3> _designatedOreLocation;
        private static ConfigEntry<Vector3> _designatedStoneLocation;
        private static ConfigEntry<bool> _immersionMode;
        private static ConfigEntry<bool> _debug;
        private static ConfigEntry<KeyboardShortcut> _setTimberLocationKeybind;
        private static ConfigEntry<KeyboardShortcut> _setOreLocationKeybind;
        private static ConfigEntry<KeyboardShortcut> _setStoneLocationKeybind;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            _harmony = new Harmony(PluginGuid);
            Log = Logger;

            InitConfiguration();

            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 10}));
            _modEnabled.SettingChanged += ApplyPatches;

            _teleportToDumpSiteWhenAllStockPilesFull = Config.Bind("2. Features", "Teleport To Dump Site When Full", true, new ConfigDescription("Teleport resources to a designated dump site when all stockpiles are full", null, new ConfigurationManagerAttributes {Order = 9}));
            _immersionMode = Config.Bind("2. Features", "Immersive Mode", true, new ConfigDescription("Disable immersive mode to remove energy requirements for teleportation", null, new ConfigurationManagerAttributes {Order = 8}));

            _designatedTimberLocation = Config.Bind("3. Locations", "Designated Timber Location", new Vector3(-3712.003f, 6144f, 1294.643f), new ConfigDescription("Set the designated location for dumping excess timber", null, new ConfigurationManagerAttributes {Order = 7}));
            _designatedOreLocation = Config.Bind("3. Locations", "Designated Ore Location", new Vector3(-3712.003f, 6144f, 1294.643f), new ConfigDescription("Set the designated location for dumping excess ore", null, new ConfigurationManagerAttributes {Order = 6}));
            _designatedStoneLocation = Config.Bind("3. Locations", "Designated Stone Location", new Vector3(-3712.003f, 6144f, 1294.643f), new ConfigDescription("Set the designated location for dumping excess stone and marble", null, new ConfigurationManagerAttributes {Order = 5}));

            _setTimberLocationKeybind = Config.Bind("4. Keybinds", "Set Timber Location Keybind", new KeyboardShortcut(KeyCode.Alpha7), new ConfigDescription("Define the keybind for setting the Timber Location", null, new ConfigurationManagerAttributes {Order = 4}));
            _setOreLocationKeybind = Config.Bind("4. Keybinds", "Set Ore Location Keybind", new KeyboardShortcut(KeyCode.Alpha8), new ConfigDescription("Define the keybind for setting the Ore Location", null, new ConfigurationManagerAttributes {Order = 3}));
            _setStoneLocationKeybind = Config.Bind("4. Keybinds", "Set Stone Location Keybind", new KeyboardShortcut(KeyCode.Alpha9), new ConfigDescription("Define the keybind for setting the Stone Location", null, new ConfigurationManagerAttributes {Order = 2}));

            _debug = Config.Bind("5. Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 1}));
        }


        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Actions.WorldGameObjectInteract += WorldGameObjectInteract;
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.WorldGameObjectInteract -= WorldGameObjectInteract;
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }
    }
}