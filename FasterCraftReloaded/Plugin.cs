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
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 501, CustomDrawer = ToggleMod}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 500}));
            IncreaseBuildAndDestroySpeed = Config.Bind("Speed Settings", "Increase Build And Destroy Speed", true, new ConfigDescription("Increase the speed of building and destroying.", null, new ConfigurationManagerAttributes {Order = 499}));
            CraftSpeedMultiplier = Config.Bind("Speed Settings", "Craft Speed Multiplier", 2f, new ConfigDescription("Multiplier for crafting speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 498}));
            ModifyPlayerGardenSpeed = Config.Bind("Garden Settings", "Modify Player Garden Speed", false, new ConfigDescription("Enable or disable modification of player garden speed.", null, new ConfigurationManagerAttributes {Order = 497}));
            PlayerGardenSpeedMultiplier = Config.Bind("Garden Settings", "Player Garden Speed Multiplier", 2f, new ConfigDescription("Multiplier for player garden speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 496}));
            ModifyZombieGardenSpeed = Config.Bind("Garden Settings", "Modify Zombie Garden Speed", false, new ConfigDescription("Enable or disable modification of zombie garden speed.", null, new ConfigurationManagerAttributes {Order = 495}));
            ZombieGardenSpeedMultiplier = Config.Bind("Garden Settings", "Zombie Garden Speed Multiplier", 2f, new ConfigDescription("Multiplier for zombie garden speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 494}));
            ModifyRefugeeGardenSpeed = Config.Bind("Garden Settings", "Modify Refugee Garden Speed", false, new ConfigDescription("Enable or disable modification of refugee garden speed.", null, new ConfigurationManagerAttributes {Order = 493}));
            RefugeeGardenSpeedMultiplier = Config.Bind("Garden Settings", "Refugee Garden Speed Multiplier", 2f, new ConfigDescription("Multiplier for refugee garden speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 492}));
            ModifyZombieVineyardSpeed = Config.Bind("Production Settings", "Modify Zombie Vineyard Speed", false, new ConfigDescription("Enable or disable modification of zombie vineyard speed.", null, new ConfigurationManagerAttributes {Order = 491}));
            ZombieVineyardSpeedMultiplier = Config.Bind("Production Settings", "Zombie Vineyard Speed Multiplier", 2f, new ConfigDescription("Multiplier for zombie vineyard speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 490}));
            ModifyZombieSawmillSpeed = Config.Bind("Production Settings", "Modify Zombie Sawmill Speed", false, new ConfigDescription("Enable or disable modification of zombie sawmill speed.", null, new ConfigurationManagerAttributes {Order = 489}));
            ZombieSawmillSpeedMultiplier = Config.Bind("Production Settings", "Zombie Sawmill Speed Multiplier", 2f, new ConfigDescription("Multiplier for zombie sawmill speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 488}));
            ModifyZombieMinesSpeed = Config.Bind("Production Settings", "Modify Zombie Mines Speed", false, new ConfigDescription("Enable or disable modification of zombie mines speed.", null, new ConfigurationManagerAttributes {Order = 487}));
            ZombieMinesSpeedMultiplier = Config.Bind("Production Settings", "Zombie Mines Speed Multiplier", 2f, new ConfigDescription("Multiplier for zombie mines speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 486}));
            ModifyCompostSpeed = Config.Bind("Composting Settings", "Modify Compost Speed", false, new ConfigDescription("Enable or disable modification of composting speed.", null, new ConfigurationManagerAttributes {Order = 485}));
            CompostSpeedMultiplier = Config.Bind("Composting Settings", "Compost Speed Multiplier", 2f, new ConfigDescription("Multiplier for composting speed.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 484}));

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
    }
}