using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace BringOutYerDead
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.bringoutyerdead";
        private const string PluginName = "Bring Out Yer Dead!";
        private const string PluginVer = "0.1.7";

        internal static ConfigEntry<bool> Debug;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        private static ConfigEntry<bool> _morningDelivery;
        private static ConfigEntry<bool> _dayDelivery;
        private static ConfigEntry<bool> _nightDelivery;
        private static ConfigEntry<bool> _eveningDelivery;
        internal static ConfigEntry<float> DonkeySpeed;

        internal static ConfigEntry<bool> InternalMorningDelivery;
        internal static ConfigEntry<bool> InternalDayDelivery;
        internal static ConfigEntry<bool> InternalEveningDelivery;
        internal static ConfigEntry<bool> InternalNightDelivery;
        internal static ConfigEntry<bool> InternalDonkeySpawned;
        private static ConfigEntry<bool> _internalTutMessageShown;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 502}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 501}));
            _morningDelivery = Config.Bind("Delivery Times", "Morning Delivery", true, new ConfigDescription("Enable or disable morning delivery.", null, new ConfigurationManagerAttributes {Order = 500}));
            _dayDelivery = Config.Bind("Delivery Times", "Day Delivery", false, new ConfigDescription("Enable or disable day delivery.", null, new ConfigurationManagerAttributes {Order = 499}));
            _nightDelivery = Config.Bind("Delivery Times", "Night Delivery", false, new ConfigDescription("Enable or disable night delivery.", null, new ConfigurationManagerAttributes {Order = 498}));
            _eveningDelivery = Config.Bind("Delivery Times", "Evening Delivery", true, new ConfigDescription("Enable or disable evening delivery.", null, new ConfigurationManagerAttributes {Order = 497}));
            DonkeySpeed = Config.Bind("Donkey Settings", "Donkey Speed", 2f, new ConfigDescription("Set the speed of the donkey (minimum value is 2).", new AcceptableValueRange<float>(2f, 20f), new ConfigurationManagerAttributes {Order = 496}));

            InternalMorningDelivery = Config.Bind("Internal (Dont Touch)", "Morning Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 500}));
            InternalDayDelivery = Config.Bind("Internal (Dont Touch)", "Day Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 499}));
            InternalEveningDelivery = Config.Bind("Internal (Dont Touch)", "Evening Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 498}));
            InternalNightDelivery = Config.Bind("Internal (Dont Touch)", "Night Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 497}));
            InternalDonkeySpawned = Config.Bind("Internal (Dont Touch)", "Donkey Spawned Done", false, new ConfigDescription("Internal use. Used for tracking donkey spawn state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 496}));
            _internalTutMessageShown = Config.Bind("Internal (Dont Touch)", "Tut Message Shown", false, new ConfigDescription("Internal use. Used for tracking tutorial message state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 495}));

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
    }
}