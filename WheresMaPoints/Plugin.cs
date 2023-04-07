using System;
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
        private const string PluginVer = "0.2.5";

        internal static ConfigEntry<bool> ShowPointGainAboveKeeper;
        internal static ConfigEntry<bool> StillPlayCollectAudio;

        internal static ConfigEntry<bool> AlwaysShowXpBar;

        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        internal static ConfigEntry<bool> Debug;
        private static  ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            
            Log = Logger;
            _harmony = new Harmony(PluginGuid);

            
            InitConfiguration();

            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 0}));
            _modEnabled.SettingChanged += ApplyPatches;

            AlwaysShowXpBar = Config.Bind("User Interface", "Always Show XP Bar", true, new ConfigDescription("Display the experience bar constantly, even without active experience gain.", null, new ConfigurationManagerAttributes {Order = 6}));
            ShowPointGainAboveKeeper = Config.Bind("Visual Feedback", "Show Point Gain Above Keeper", true, new ConfigDescription("Display the points earned above the keeper's head.", null, new ConfigurationManagerAttributes {Order = 5}));
            StillPlayCollectAudio = Config.Bind("Audio Feedback", "Still Play Collect Audio", false, new ConfigDescription("Keep playing the collect audio when point gain is displayed above the keeper's head.", null, new ConfigurationManagerAttributes {Order = 4}));

            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 1}));

        }


        private static void ApplyPatches(object sender, EventArgs eventArgs)
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
    }
}