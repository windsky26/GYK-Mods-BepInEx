using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace TheSeedEqualizer
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.theseedequalizer";
        private const string PluginName = "The Seed Equalizer!";
        private const string PluginVer = "1.1.4";

        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> ModifyZombieGardens;
        internal static ConfigEntry<bool> ModifyZombieVineyards;
        internal static ConfigEntry<bool> ModifyPlayerGardens;
        internal static ConfigEntry<bool> ModifyRefugeeGardens;
        internal static ConfigEntry<bool> AddWasteToZombieGardens;
        internal static ConfigEntry<bool> AddWasteToZombieVineyards;
        internal static ConfigEntry<bool> BoostPotentialSeedOutput;
        internal static ConfigEntry<bool> BoostGrowSpeedWhenRaining;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
            ModifyZombieGardens = Config.Bind("Gardens", "Modify Zombie Gardens", true, new ConfigDescription("Enable or disable modifying zombie gardens", null, new ConfigurationManagerAttributes {Order = 20}));
            ModifyZombieVineyards = Config.Bind("Gardens", "Modify Zombie Vineyards", true, new ConfigDescription("Enable or disable modifying zombie vineyards", null, new ConfigurationManagerAttributes {Order = 19}));
            ModifyPlayerGardens = Config.Bind("Gardens", "Modify Player Gardens", false, new ConfigDescription("Enable or disable modifying player gardens", null, new ConfigurationManagerAttributes {Order = 18}));
            ModifyRefugeeGardens = Config.Bind("Gardens", "Modify Refugee Gardens", true, new ConfigDescription("Enable or disable modifying refugee gardens", null, new ConfigurationManagerAttributes {Order = 17}));
            AddWasteToZombieGardens = Config.Bind("Gardens", "Add Waste To Zombie Gardens", true, new ConfigDescription("Enable or disable adding waste to zombie gardens", null, new ConfigurationManagerAttributes {Order = 16}));
            AddWasteToZombieVineyards = Config.Bind("Gardens", "Add Waste To Zombie Vineyards", true, new ConfigDescription("Enable or disable adding waste to zombie vineyards", null, new ConfigurationManagerAttributes {Order = 15}));
            BoostPotentialSeedOutput = Config.Bind("Gardens", "Boost Potential Seed Output", true, new ConfigDescription("Enable or disable boosting potential seed output", null, new ConfigurationManagerAttributes {Order = 14}));
            BoostGrowSpeedWhenRaining = Config.Bind("Gardens", "Boost Grow Speed When Raining", true, new ConfigDescription("Enable or disable boosting grow speed when raining", null, new ConfigurationManagerAttributes {Order = 13}));
            
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                GYKHelper.Actions.GameBalanceLoad += Helpers.GameBalancePostfix;
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
                GYKHelper.Actions.GameBalanceLoad += Helpers.GameBalancePostfix;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                GYKHelper.Actions.GameBalanceLoad -= Helpers.GameBalancePostfix;
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