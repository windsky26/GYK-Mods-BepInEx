using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace WheresMaPoints
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.wheresmapoints";
        private const string PluginName = "Where's Ma' Points!";
        private const string PluginVer = "0.2.4";

        internal static ConfigEntry<bool> ShowPointGainAboveKeeper;
        internal static ConfigEntry<bool> StillPlayCollectAudio;

        internal static ConfigEntry<bool> AlwaysShowXpBar;

        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        internal static ConfigEntry<bool> Debug;
        private static  ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod,Order = 5}));
            Debug = Config.Bind("General", "Debug", false, new ConfigDescription("Enable debug logging", null, new ConfigurationManagerAttributes {Order = 4}));

            ShowPointGainAboveKeeper = Config.Bind("Visual Feedback", "Show Point Gain Above Keeper", true, new ConfigDescription("Show the point gain above the keeper's head when points are earned.", null, new ConfigurationManagerAttributes {Order = 3}));
            StillPlayCollectAudio = Config.Bind("Audio Feedback", "Still Play Collect Audio", false, new ConfigDescription("Play the collect audio even when the point gain is displayed above the keeper's head.", null, new ConfigurationManagerAttributes {Order = 2}));
            AlwaysShowXpBar = Config.Bind("User Interface", "Always Show Xp Bar", true, new ConfigDescription("Always show the experience bar, even if the player is not gaining experience.", null, new ConfigurationManagerAttributes {Order = 1}));

            Log = Logger;
            _harmony = new Harmony(PluginGuid);

            if (_modEnabled.Value)
            {
               Log.LogWarning($"Applying patches for {PluginName}!");
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
    }
}