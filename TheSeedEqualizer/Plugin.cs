using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;

namespace TheSeedEqualizer
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.theseedequalizer";
        private const string PluginName = "The Seed Equalizer!";
        private const string PluginVer = "1.3.4";

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
            Log = Logger;
            _harmony = new Harmony(PluginGuid);

            InitConfiguration();

            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 21}));
            _modEnabled.SettingChanged += ApplyPatches;

            ModifyZombieGardens = Config.Bind("2. Gardens", "Modify Zombie Gardens", true, new ConfigDescription("Enable or disable modifying zombie gardens", null, new ConfigurationManagerAttributes {Order = 20}));
            ModifyZombieVineyards = Config.Bind("3. Gardens", "Modify Zombie Vineyards", true, new ConfigDescription("Enable or disable modifying zombie vineyards", null, new ConfigurationManagerAttributes {Order = 19}));
            ModifyPlayerGardens = Config.Bind("4. Gardens", "Modify Player Gardens", false, new ConfigDescription("Enable or disable modifying player gardens", null, new ConfigurationManagerAttributes {Order = 18}));
            ModifyRefugeeGardens = Config.Bind("5. Gardens", "Modify Refugee Gardens", true, new ConfigDescription("Enable or disable modifying refugee gardens", null, new ConfigurationManagerAttributes {Order = 17}));
            AddWasteToZombieGardens = Config.Bind("6. Gardens", "Add Waste To Zombie Gardens", true, new ConfigDescription("Enable or disable adding waste to zombie gardens", null, new ConfigurationManagerAttributes {Order = 16}));
            AddWasteToZombieVineyards = Config.Bind("7. Gardens", "Add Waste To Zombie Vineyards", true, new ConfigDescription("Enable or disable adding waste to zombie vineyards", null, new ConfigurationManagerAttributes {Order = 15}));
            BoostPotentialSeedOutput = Config.Bind("8. Gardens", "Boost Potential Seed Output", true, new ConfigDescription("Enable or disable boosting potential seed output", null, new ConfigurationManagerAttributes {Order = 14}));
            BoostGrowSpeedWhenRaining = Config.Bind("9. Gardens", "Boost Grow Speed When Raining", true, new ConfigDescription("Enable or disable boosting grow speed when raining", null, new ConfigurationManagerAttributes {Order = 13}));
        }
        
        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Actions.GameBalanceLoad += Helpers.GameBalancePostfix;
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.GameBalanceLoad -= Helpers.GameBalancePostfix;
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }
    }
}