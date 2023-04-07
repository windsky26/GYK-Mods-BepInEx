using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace FasterCraftReloaded
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.fastercraftreloaded";
        private const string PluginName = "FasterCraft Reloaded";
        private const string PluginVer = "1.4.4";

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<bool> IncreaseBuildAndDestroySpeed;
        internal static ConfigEntry<float> BuildAndDestroySpeed;
        internal static ConfigEntry<float> CraftSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyPlayerGardenSpeed;
        internal static ConfigEntry<float> PlayerGardenSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyZombieGardenSpeed;
        internal static ConfigEntry<float> ZombieGardenSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyRefugeeGardenSpeed;
        internal static ConfigEntry<float> RefugeeGardenSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyZombieVineyardSpeed;
        internal static ConfigEntry<float> ZombieVineyardSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyZombieSawmillSpeed;
        internal static ConfigEntry<float> ZombieSawmillSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyZombieMinesSpeed;
        internal static ConfigEntry<float> ZombieMinesSpeedMultiplier;
        internal static ConfigEntry<bool> ModifyCompostSpeed;
        internal static ConfigEntry<float> CompostSpeedMultiplier;

        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;


        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
        }


        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 502}));
            _modEnabled.SettingChanged += ApplyPatches;

            Debug = Config.Bind("2. Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 501}));

            CraftSpeedMultiplier = Config.Bind("3. Speed Settings", "Craft Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for crafting speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 500}));
            IncreaseBuildAndDestroySpeed = Config.Bind("3. Speed Settings", "Increase Build And Destroy Speed", true, new ConfigDescription("Toggle faster building and destruction speed.", null, new ConfigurationManagerAttributes {Order = 499}));
            BuildAndDestroySpeed = Config.Bind("3. Speed Settings", "Build And Destroy Speed", 4f, new ConfigDescription("Set the multiplier for building and destruction speed.", new AcceptableValueRange<float>(2f, 10f), new ConfigurationManagerAttributes {Order = 498}));
            
            ModifyCompostSpeed = Config.Bind("4. Composting Settings", "Modify Compost Speed", false, new ConfigDescription("Toggle composting speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 497}));
            CompostSpeedMultiplier = Config.Bind("4. Composting Settings", "Compost Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for composting speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 496}));
            
            ModifyPlayerGardenSpeed = Config.Bind("5. Garden Settings", "Modify Player Garden Speed", false, new ConfigDescription("Toggle player garden speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 495}));
            PlayerGardenSpeedMultiplier = Config.Bind("5. Garden Settings", "Player Garden Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for player garden speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 494}));
            ModifyRefugeeGardenSpeed = Config.Bind("5. Garden Settings", "Modify Refugee Garden Speed", false, new ConfigDescription("Toggle refugee garden speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 493}));
            RefugeeGardenSpeedMultiplier = Config.Bind("5. Garden Settings", "Refugee Garden Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for refugee garden speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 492}));
            ModifyZombieGardenSpeed = Config.Bind("5. Garden Settings", "Modify Zombie Garden Speed", false, new ConfigDescription("Toggle zombie garden speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 491}));
            ZombieGardenSpeedMultiplier = Config.Bind("5. Garden Settings", "Zombie Garden Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for zombie garden speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 490}));

            ModifyZombieMinesSpeed = Config.Bind("6. Production Settings", "Modify Zombie Mines Speed", false, new ConfigDescription("Toggle zombie mines speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 489}));
            ZombieMinesSpeedMultiplier = Config.Bind("6. Production Settings", "Zombie Mines Speed Multiplier", 2f
                , new ConfigDescription("Set the multiplier for zombie mines speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 488}));
            ModifyZombieSawmillSpeed = Config.Bind("6. Production Settings", "Modify Zombie Sawmill Speed", false, new ConfigDescription("Toggle zombie sawmill speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 487}));
            ZombieSawmillSpeedMultiplier = Config.Bind("6. Production Settings", "Zombie Sawmill Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for zombie sawmill speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 486}));
            ModifyZombieVineyardSpeed = Config.Bind("6. Production Settings", "Modify Zombie Vineyard Speed", false, new ConfigDescription("Toggle zombie vineyard speed modification on or off.", null, new ConfigurationManagerAttributes {Order = 485}));
            ZombieVineyardSpeedMultiplier = Config.Bind("6. Production Settings", "Zombie Vineyard Speed Multiplier", 2f, new ConfigDescription("Set the multiplier for zombie vineyard speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 484}));
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
    }
}