using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using NodeCanvas.Framework;
using UnityEngine;

namespace EconomyReloaded
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.economyreloaded";
        private const string PluginName = "Economy Reloaded";
        private const string PluginVer = "1.3.3";
        
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> OldSchoolModeConfig;
        internal static ConfigEntry<bool> DisableInflationConfig;
        internal static ConfigEntry<bool> DisableDeflationConfig;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 8 ,CustomDrawer = ToggleMod}));
            OldSchoolModeConfig = Config.Bind("Gameplay", "Old School Mode", false, new ConfigDescription("Enable or disable Old School Mode", null, new ConfigurationManagerAttributes { Order = 7 }));
            DisableInflationConfig = Config.Bind("Economy", "Disable Inflation", true, new ConfigDescription("Enable or disable inflation", null, new ConfigurationManagerAttributes { Order = 6 }));
            DisableDeflationConfig = Config.Bind("Economy", "Disable Deflation", true, new ConfigDescription("Enable or disable deflation", null, new ConfigurationManagerAttributes { Order = 5 }));
            
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.GameBalanceLoad += Patches.GameBalance_LoadGameBalance;
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
                Actions.GameBalanceLoad += Patches.GameBalance_LoadGameBalance;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());   
            }
            else
            {
                Actions.GameBalanceLoad -= Patches.GameBalance_LoadGameBalance;
                Patches.RestoreIsStaticCost();
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