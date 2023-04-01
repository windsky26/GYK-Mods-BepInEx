using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace Exhaustless
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.Exhaustless";
        private const string PluginName = "Exhaust-less!";
        private const string PluginVer = "3.4.4";

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> MakeToolsLastLonger;
        internal static ConfigEntry<bool> SpendHalfGratitude;
        internal static ConfigEntry<bool> AutoEquipNewTool;
        internal static ConfigEntry<bool> SpeedUpSleep;
        internal static ConfigEntry<bool> AutoWakeFromMeditation;
        internal static ConfigEntry<bool> SpendHalfSanity;
        internal static ConfigEntry<bool> SpeedUpMeditation;
        internal static ConfigEntry<bool> YawnMessage;
        internal static ConfigEntry<bool> SpendHalfEnergy;
        internal static ConfigEntry<int> EnergySpendBeforeSleepDebuff;

        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;


        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
            MakeToolsLastLonger = Config.Bind("Tools", "Make Tools Last Longer", true, new ConfigDescription("Enable or disable making tools last longer", null, new ConfigurationManagerAttributes {Order = 11}));
            SpendHalfGratitude = Config.Bind("Gameplay", "Spend Half Gratitude", true, new ConfigDescription("Enable or disable spending half gratitude", null, new ConfigurationManagerAttributes {Order = 10}));
            AutoEquipNewTool = Config.Bind("Tools", "Auto Equip New Tool", true, new ConfigDescription("Enable or disable auto equipping new tools", null, new ConfigurationManagerAttributes {Order = 9}));
            SpeedUpSleep = Config.Bind("Sleep", "Speed Up Sleep", true, new ConfigDescription("Enable or disable speeding up sleep", null, new ConfigurationManagerAttributes {Order = 8}));
            AutoWakeFromMeditation = Config.Bind("Meditation", "Auto Wake From Meditation", true, new ConfigDescription("Enable or disable auto waking from meditation", null, new ConfigurationManagerAttributes {Order = 7}));
            SpendHalfSanity = Config.Bind("Gameplay", "Spend Half Sanity", true, new ConfigDescription("Enable or disable spending half sanity", null, new ConfigurationManagerAttributes {Order = 6}));
            SpeedUpMeditation = Config.Bind("Meditation", "Speed Up Meditation", true, new ConfigDescription("Enable or disable speeding up meditation", null, new ConfigurationManagerAttributes {Order = 5}));
            YawnMessage = Config.Bind("Sleep", "Yawn Message", true, new ConfigDescription("Enable or disable yawn messages", null, new ConfigurationManagerAttributes {Order = 4}));
            SpendHalfEnergy = Config.Bind("Gameplay", "Spend Half Energy", true, new ConfigDescription("Enable or disable spending half energy", null, new ConfigurationManagerAttributes {Order = 3}));
            EnergySpendBeforeSleepDebuff = Config.Bind("Sleep", "Energy Spend Before Sleep Debuff", 1200, new ConfigDescription("Set the amount of energy spent before sleep debuff", new AcceptableValueRange<int>(350,50000), new ConfigurationManagerAttributes {Order = 2}));
           
            
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