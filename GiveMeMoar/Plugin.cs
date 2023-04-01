using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace GiveMeMoar
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.givememoar";
        private const string PluginName = "Give Me Moar!";
        private const string PluginVer = "1.2.6";

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<bool> DisableSticks;
        internal static ConfigEntry<float> FaithMultiplier;
        internal static ConfigEntry<float> ResourceMultiplier;
        internal static ConfigEntry<float> GratitudeMultiplier;
        internal static ConfigEntry<float> SinShardMultiplier;
        internal static ConfigEntry<float> DonationMultiplier;
        internal static ConfigEntry<float> BlueTechPointMultiplier;
        internal static ConfigEntry<float> GreenTechPointMultiplier;
        internal static ConfigEntry<float> RedTechPointMultiplier;
        internal static ConfigEntry<float> HappinessMultiplier;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;


        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 501, CustomDrawer = ToggleMod}));

            FaithMultiplier = Config.Bind("Multipliers", "Faith Multiplier", 1f, new ConfigDescription("Multiplier for faith.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 500}));

            ResourceMultiplier = Config.Bind("Multipliers", "Resource Multiplier", 1f, new ConfigDescription("Multiplier for resources.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 499}));

            GratitudeMultiplier = Config.Bind("Multipliers", "Gratitude Multiplier", 1f, new ConfigDescription("Multiplier for gratitude.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 498}));

            SinShardMultiplier = Config.Bind("Multipliers", "Sin Shard Multiplier", 1f, new ConfigDescription("Multiplier for sin shards.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 497}));

            DonationMultiplier = Config.Bind("Multipliers", "Donation Multiplier", 1f, new ConfigDescription("Multiplier for donations.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 496}));

            BlueTechPointMultiplier = Config.Bind("Multipliers", "Blue Tech Point Multiplier", 1f, new ConfigDescription("Multiplier for blue tech points.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 495}));

            GreenTechPointMultiplier = Config.Bind("Multipliers", "Green Tech Point Multiplier", 1f, new ConfigDescription("Multiplier for green tech points.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 494}));

            RedTechPointMultiplier = Config.Bind("Multipliers", "Red Tech Point Multiplier", 1f, new ConfigDescription("Multiplier for red tech points.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 493}));

            HappinessMultiplier = Config.Bind("Multipliers", "Happiness Multiplier", 1f, new ConfigDescription("Multiplier for happiness.", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 492}));

            Debug = Config.Bind("Debug Settings", "Debug", false, new ConfigDescription("Enable or disable debugging features.", null, new ConfigurationManagerAttributes {Order = 491}));

            DisableSticks = Config.Bind("Miscellaneous", "Disable Sticks", false, new ConfigDescription("Enable or disable stick functionality.", null, new ConfigurationManagerAttributes {Order = 490}));

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