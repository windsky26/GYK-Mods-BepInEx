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
    [BepInDependency("p1xel8ted.gyk.gykhelper", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.queueeverything";
        private const string PluginName = "Queue Everything!*";
        private const string PluginVer = "2.1.4";

        internal static ConfigEntry<bool> HalfFireRequirements;
        internal static ConfigEntry<bool> AutoMaxMultiQualCrafts;
        internal static ConfigEntry<bool> AutoMaxNormalCrafts;
        internal static ConfigEntry<bool> AutoSelectHighestQualRecipe;
        internal static ConfigEntry<bool> AutoSelectCraftButtonWithController;
        internal static ConfigEntry<bool> MakeEverythingAuto;
        internal static ConfigEntry<bool> MakeHandTasksAuto;
        internal static ConfigEntry<bool> DisableComeBackLaterThoughts;
        internal static ConfigEntry<bool> ForceMultiCraft;

        internal static ConfigEntry<bool> Debug;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 15}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 14}));

            HalfFireRequirements = Config.Bind("Options", "Half Fire Requirements", true, new ConfigDescription("Enable or disable half fire requirements.", null, new ConfigurationManagerAttributes {Order = 13}));

            AutoMaxMultiQualCrafts = Config.Bind("Options", "Auto Max Multi-Quality Crafts", true, new ConfigDescription("Enable or disable automatic max multi-quality crafts.", null, new ConfigurationManagerAttributes {Order = 12}));

            AutoMaxNormalCrafts = Config.Bind("Options", "Auto Max Normal Crafts", false, new ConfigDescription("Enable or disable automatic max normal crafts.", null, new ConfigurationManagerAttributes {Order = 11}));

            AutoSelectHighestQualRecipe = Config.Bind("Options", "Auto Select Highest Quality Recipe", true, new ConfigDescription("Enable or disable automatic selection of the highest quality recipe.", null, new ConfigurationManagerAttributes {Order = 10}));

            AutoSelectCraftButtonWithController = Config.Bind("Options", "Auto Select Craft Button With Controller", true, new ConfigDescription("Enable or disable automatic selection of the craft button with controller.", null, new ConfigurationManagerAttributes {Order = 9}));

            MakeEverythingAuto = Config.Bind("Options", "Make Everything Auto", true, new ConfigDescription("Enable or disable making everything automatic.", null, new ConfigurationManagerAttributes {Order = 8}));

            MakeHandTasksAuto = Config.Bind("Options", "Make Hand Tasks Auto", false, new ConfigDescription("Enable or disable making hand tasks automatic.", null, new ConfigurationManagerAttributes {Order = 7}));

            DisableComeBackLaterThoughts = Config.Bind("Options", "Disable Come Back Later Thoughts", false, new ConfigDescription("Enable or disable disabling come back later thoughts.", null, new ConfigurationManagerAttributes {Order = 6}));

            ForceMultiCraft = Config.Bind("Options", "Force Multi Craft", true, new ConfigDescription("Enable or disable forcing multi craft.", null, new ConfigurationManagerAttributes {Order = 5}));

            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                
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
                
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                
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

        private void Update()
        {
            if (!MainGame.game_started) return;
            if (!MakeEverythingAuto.Value) return;
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