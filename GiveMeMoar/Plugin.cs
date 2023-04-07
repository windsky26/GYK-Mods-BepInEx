using System;
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
        internal static ConfigEntry<bool> MultiplySticks;
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
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 18}));
            _modEnabled.SettingChanged += ApplyPatches;

            BlueTechPointMultiplier = Config.Bind("2. Multipliers", "Blue Tech Point Multiplier", 1f, new ConfigDescription("Adjust the multiplier for blue tech points", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 17}));

            DonationMultiplier = Config.Bind("2. Multipliers", "Donation Multiplier", 1f, new ConfigDescription("Adjust the multiplier for donations", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 16}));

            FaithMultiplier = Config.Bind("2. Multipliers", "Faith Multiplier", 1f, new ConfigDescription("Adjust the multiplier for faith", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 15}));

            GratitudeMultiplier = Config.Bind("2. Multipliers", "Gratitude Multiplier", 1f, new ConfigDescription("Adjust the multiplier for gratitude", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 14}));

            GreenTechPointMultiplier = Config.Bind("2. Multipliers", "Green Tech Point Multiplier", 1f, new ConfigDescription("Adjust the multiplier for green tech points", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 13}));

            HappinessMultiplier = Config.Bind("2. Multipliers", "Happiness Multiplier", 1f, new ConfigDescription("Adjust the multiplier for happiness", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 12}));

            RedTechPointMultiplier = Config.Bind("2. Multipliers", "Red Tech Point Multiplier", 1f, new ConfigDescription("Adjust the multiplier for red tech points", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 11}));

            ResourceMultiplier = Config.Bind("2. Multipliers", "Resource Multiplier", 1f, new ConfigDescription("Adjust the multiplier for resources", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 10}));

            SinShardMultiplier = Config.Bind("2. Multipliers", "Sin Shard Multiplier", 1f, new ConfigDescription("Adjust the multiplier for sin shards", new AcceptableValueRange<float>(1f, 50f), new ConfigurationManagerAttributes {Order = 9}));

            MultiplySticks = Config.Bind("3. Miscellaneous", "Multiply Sticks", false, new ConfigDescription("Sticks get multiplied endlessly when used in the garden. Enable this to exclude them.", null, new ConfigurationManagerAttributes {Order = 8}));

            Debug = Config.Bind("4. Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 7}));
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