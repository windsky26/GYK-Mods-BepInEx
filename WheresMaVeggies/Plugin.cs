using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace WheresMaVeggies
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.wheresmaveggies";
        private const string PluginName = "Where's Ma' Veggies!";
        private const string PluginVer = "0.1.1";

        internal static ConfigEntry<bool> Debug;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order=2}));
            _modEnabled.SettingChanged += ApplyPatches;
            Debug = Config.Bind("2. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 1}));
            ApplyPatches(this, null);
        }

        private static void ApplyPatches(object sender, EventArgs e)
        {
        
            if ( _modEnabled.Value)
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