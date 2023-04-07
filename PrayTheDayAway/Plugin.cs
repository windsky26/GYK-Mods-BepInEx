using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace PrayTheDayAway
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.praythedayaway";
        private const string PluginName = "Pray The Day Away!";
        private const string PluginVer = "0.2.8";

        private static ConfigEntry<bool> _debug;
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private static ConfigEntry<bool> _everydayIsSermonDay;
        private static ConfigEntry<bool> _sermonOverAndOver;
        private static ConfigEntry<bool> _notifyOnPrayerLoss;
        private static ConfigEntry<bool> _alternateMode;
        private static ConfigEntry<bool> _randomlyUpgradeBasicPrayer;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 606}));
            _modEnabled.SettingChanged += ApplyPatches;

            _everydayIsSermonDay = Config.Bind("1. General", "Everyday Is Sermon Day", true, new ConfigDescription("Allow sermons to be held every day.", null, new ConfigurationManagerAttributes {Order = 605}));

            _sermonOverAndOver = Config.Bind("1. General", "Sermon Over And Over", false, new ConfigDescription("Allow sermons to be repeated without limitation.", null, new ConfigurationManagerAttributes {Order = 604}));

            _alternateMode = Config.Bind("2. Mode", "Alternate Mode", true, new ConfigDescription("Chance to lower item level instead of chance to lose it on prayer.", null, new ConfigurationManagerAttributes {Order = 603}));

            _notifyOnPrayerLoss = Config.Bind("3. Notifications", "Notify On Prayer Loss", true, new ConfigDescription("Display notifications when prayer items are lost.", null, new ConfigurationManagerAttributes {Order = 602}));

            _randomlyUpgradeBasicPrayer = Config.Bind("4. Upgrades", "Randomly Upgrade Basic Prayer", true, new ConfigDescription("Allow basic prayers to be randomly upgraded (to a known starred prayer).", null, new ConfigurationManagerAttributes {Order = 601}));

            _debug = Config.Bind("5. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 600}));
        }


        private static void ApplyPatches(object sender, EventArgs eventArgs)
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