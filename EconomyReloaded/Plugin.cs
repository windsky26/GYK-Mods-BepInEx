using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;

namespace EconomyReloaded
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.economyreloaded";
        private const string PluginName = "Economy Reloaded";
        private const string PluginVer = "1.3.3";

        internal static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> Inflation;
        internal static ConfigEntry<bool> Deflation;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 4}));
            _modEnabled.SettingChanged += ApplyPatches;

            Inflation = Config.Bind("2. Economy", "Inflation", true, new ConfigDescription("Control whether your trades experiences inflation (the more you buy, the more it cost's per unit.", null, new ConfigurationManagerAttributes {Order = 2}));
            Deflation = Config.Bind("2. Economy", "Deflation", true, new ConfigDescription("Control whether your trades experiences deflation (the more you sell, the less you get per unit.", null, new ConfigurationManagerAttributes {Order = 1}));
        }
        
        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Actions.GameBalanceLoad += Patches.GameBalance_LoadGameBalance;
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                Patches.MakeIsStaticCost();
            }
            else
            {
                Actions.GameBalanceLoad -= Patches.GameBalance_LoadGameBalance;
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
                Patches.RestoreIsStaticCost();
            }
        }
    }
}