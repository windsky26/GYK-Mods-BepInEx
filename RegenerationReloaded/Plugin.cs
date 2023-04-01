using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Expressive.Functions.Mathematical;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace RegenerationReloaded
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.regenerationreloaded";
        private const string PluginName = "Regeneration Reloaded";
        private const string PluginVer = "1.1.4";

        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> ShowRegenUpdates;
        internal static ConfigEntry<float> LifeRegen;
        internal static ConfigEntry<float> EnergyRegen;
        internal static ConfigEntry<float> RegenDelay;
        
        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
            ShowRegenUpdates = Config.Bind("Regeneration", "Show Regeneration Updates", true, new ConfigDescription("Enable or disable displaying updates when life and energy regenerate", null, new ConfigurationManagerAttributes {Order = 24}));
            LifeRegen = Config.Bind("Regeneration", "Life Regeneration Rate", 2f, new ConfigDescription("Set the rate at which life regenerates per tick.", new AcceptableValueRange<float>(1f,10f), new ConfigurationManagerAttributes {Order = 23}));
            EnergyRegen = Config.Bind("Regeneration", "Energy Regeneration Rate", 1f, new ConfigDescription("Set the rate at which energy regenerates per tick.", new AcceptableValueRange<float>(1f,10f), new ConfigurationManagerAttributes {Order = 22}));
            RegenDelay = Config.Bind("Regeneration", "Regeneration Delay", 5f, new ConfigDescription("Set the delay in seconds between each regeneration tick.", new AcceptableValueRange<float>(0f,10f), new ConfigurationManagerAttributes {Order = 21}));
            Patches.Delay = RegenDelay.Value;
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
            Log.LogWarning($"Plugin {PluginName} has been disabled!");
        }
    }
}