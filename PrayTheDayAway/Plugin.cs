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
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 602}));
            _debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 601}));

            _everydayIsSermonDay = Config.Bind("General", "Everyday Is Sermon Day", true, new ConfigDescription("Enable or disable sermons every day.", null, new ConfigurationManagerAttributes { Order = 600 }));

            _sermonOverAndOver = Config.Bind("General", "Sermon Over And Over", false, new ConfigDescription("Enable or disable repeating sermons.", null, new ConfigurationManagerAttributes { Order = 599 }));

            _notifyOnPrayerLoss = Config.Bind("Notifications", "Notify On Prayer Loss", true, new ConfigDescription("Enable or disable notifications for prayer loss.", null, new ConfigurationManagerAttributes { Order = 598 }));

            _alternateMode = Config.Bind("Mode", "Alternate Mode", true, new ConfigDescription("Enable or disable alternate mode.", null, new ConfigurationManagerAttributes { Order = 597 }));

            _randomlyUpgradeBasicPrayer = Config.Bind("Upgrades", "Randomly Upgrade Basic Prayer", true, new ConfigDescription("Enable or disable random upgrades for basic prayers.", null, new ConfigurationManagerAttributes { Order = 596 }));
            
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