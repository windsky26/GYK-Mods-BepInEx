using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace QueueEverything
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.queueeverything";
        private const string PluginName = "Queue Everything!*";
        private const string PluginVer = "2.1.4";

        private static ConfigEntry<bool> _halfFireRequirements;
        private static ConfigEntry<bool> _autoMaxMultiQualCrafts;
        private static ConfigEntry<bool> _autoMaxNormalCrafts;
        private static ConfigEntry<bool> _autoSelectHighestQualRecipe;
        private static ConfigEntry<bool> _autoSelectCraftButtonWithController;
        private static ConfigEntry<bool> _makeEverythingAuto;
        private static ConfigEntry<bool> _makeHandTasksAuto;
        private static ConfigEntry<bool> _forceMultiCraft;

        
        private static ConfigEntry<float> _fcTimeAdjustment;
        
        private static ConfigEntry<bool> _debug;
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            const string fcGuid = "p1xel8ted.gyk.fastercraftreloaded";
            if (Harmony.HasAnyPatches(fcGuid))
            {
                //get fc config
                var config = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{fcGuid}.cfg"), true);
                var cg = new ConfigDefinition("3. Speed Settings", "Craft Speed Multiplier");
                _fcTimeAdjustment = config.Bind(cg, 2f);
                Log.LogInfo("Loading FasterCraft Reloaded Config");
            }

            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 16}));
            _modEnabled.SettingChanged += ApplyPatches;

            _halfFireRequirements = Config.Bind("2. Options", "Half Fire Requirements", true, new ConfigDescription("Reduce fire requirements by 50%.", null, new ConfigurationManagerAttributes {Order = 15}));

            _autoMaxMultiQualCrafts = Config.Bind("2. Options", "Auto Max Multi-Quality Crafts", true, new ConfigDescription("Automatically choose maximum craft amount multi-quality crafts.", null, new ConfigurationManagerAttributes {Order = 14}));

            _autoMaxNormalCrafts = Config.Bind("2. Options", "Auto Max Normal Crafts", false, new ConfigDescription("Automatically choose maximum craft amount for normal crafts.", null, new ConfigurationManagerAttributes {Order = 13}));

            _autoSelectHighestQualRecipe = Config.Bind("2. Options", "Auto Select Highest Quality Recipe", true, new ConfigDescription("Automatically select the highest quality recipe available.", null, new ConfigurationManagerAttributes {Order = 12}));

            _autoSelectCraftButtonWithController = Config.Bind("2. Options", "Auto Select Craft Button With Controller", true, new ConfigDescription("Automatically select the craft button when using a controller.", null, new ConfigurationManagerAttributes {Order = 11}));

            _makeEverythingAuto = Config.Bind("2. Options", "Make Everything Auto", true, new ConfigDescription("Automate all possible crafts.", null, new ConfigurationManagerAttributes {Order = 10}));

            _makeHandTasksAuto = Config.Bind("2. Options", "Make Hand Tasks Auto", false, new ConfigDescription("Automate manual crafts (i.e. cooking table).", null, new ConfigurationManagerAttributes {Order = 9}));

            _forceMultiCraft = Config.Bind("2. Options", "Force Multi Craft", true, new ConfigDescription("Makes almost all crafting items able to be queued.", null, new ConfigurationManagerAttributes {Order = 7}));

            _debug = Config.Bind("3. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 6}));
        }


        private static void ApplyPatches(object sender, EventArgs e)
        {
            if (_modEnabled.Value)
            {
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }

        private void Update()
        {
            if (!MainGame.game_started) return;
            if (!_makeEverythingAuto.Value) return;
            if (_craftsStarted) return;

            foreach (var wgo in MainGame.me.world.GetComponentsInChildren<WorldGameObject>(true))
            {
                if (wgo != null && wgo.components.craft.is_crafting && !wgo.has_linked_worker)
                {
                    currentlyCrafting.Add(wgo);
                }
            }

            _craftsStarted = true;
        }
    }
}