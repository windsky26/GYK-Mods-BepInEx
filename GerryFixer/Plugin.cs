using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace GerryFixer
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.gerryfixer";
        private const string PluginName = "Gerry Fixer...";
        private const string PluginVer = "0.1.3";

        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<bool> AttemptToFixCutsceneGerrys;
        internal static ConfigEntry<bool> SpawnTavernCellarGerry;
        internal static ConfigEntry<bool> SpawnTavernMorgueGerry;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 12,CustomDrawer = ToggleMod}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 11}));
            AttemptToFixCutsceneGerrys = Config.Bind("Gerry", "Attempt To Fix Cutscene Gerrys", false, new ConfigDescription("Enable or disable attempts to fix cutscene Gerrys", null, new ConfigurationManagerAttributes {Order = 10}));
            SpawnTavernCellarGerry = Config.Bind("Gerry", "Spawn Tavern Cellar Gerry", false, new ConfigDescription("Enable or disable spawning Gerry in the tavern cellar", null, new ConfigurationManagerAttributes {Order = 9}));
            SpawnTavernMorgueGerry = Config.Bind("Gerry", "Spawn Tavern Morgue Gerry", false, new ConfigDescription("Enable or disable spawning Gerry in the tavern morgue", null, new ConfigurationManagerAttributes {Order = 8}));
           
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.GameStartedPlaying += Patches.FixGerry;
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
                Actions.GameStartedPlaying += Patches.FixGerry;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());   
            }
            else
            {
                Actions.GameStartedPlaying -= Patches.FixGerry;
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