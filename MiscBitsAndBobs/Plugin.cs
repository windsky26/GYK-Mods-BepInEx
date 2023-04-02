using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace MiscBitsAndBobs
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.miscbitsandbobs";
        private const string PluginName = "Misc. Bits & Bobs";
        private const string PluginVer = "2.2.5";


        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<bool> QuietMusicInGuiConfig;
        internal static ConfigEntry<bool> CondenseXpBarConfig;
        internal static ConfigEntry<bool> ModifyPlayerMovementSpeedConfig;
        internal static ConfigEntry<float> PlayerMovementSpeedConfig;
        internal static ConfigEntry<bool> ModifyPorterMovementSpeedConfig;
        internal static ConfigEntry<float> PorterMovementSpeedConfig;
        internal static ConfigEntry<bool> HalloweenNowConfig;
        internal static ConfigEntry<bool> HideCreditsButtonOnMainMenuConfig;
        internal static ConfigEntry<bool> SkipIntroVideoOnNewGameConfig;
        internal static ConfigEntry<bool> DisableCinematicLetterboxingConfig;
        internal static ConfigEntry<bool> KitsuneKitoModeConfig;
        internal static ConfigEntry<bool> LessenFootprintImpactConfig;
        internal static ConfigEntry<bool> RemovePrayerOnUseConfig;
        internal static ConfigEntry<bool> AddCoalToTavernOvenConfig;
        internal static ConfigEntry<bool> AddZombiesToPyreAndCrematoriumConfig;
        internal static ConfigEntry<bool> KeepGamingRunningInBackgroundConfig;


        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;


        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 31,CustomDrawer = ToggleMod}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 7}));

            QuietMusicInGuiConfig = Config.Bind("Audio", "Quiet Music In GUI", true, new ConfigDescription("Enable or disable quiet music in GUI", null, new ConfigurationManagerAttributes {Order = 30}));
            CondenseXpBarConfig = Config.Bind("UI", "Condense XP Bar", true, new ConfigDescription("Enable or disable condensing the XP bar", null, new ConfigurationManagerAttributes {Order = 29}));
            ModifyPlayerMovementSpeedConfig = Config.Bind("Movement", "Modify Player Movement Speed", true, new ConfigDescription("Enable or disable modifying the player's movement speed", null, new ConfigurationManagerAttributes {Order = 28}));
            PlayerMovementSpeedConfig = Config.Bind("Movement", "Player Movement Speed", 1.0f, new ConfigDescription("Set the player's movement speed", new AcceptableValueRange<float>(1.0f, 100f), new ConfigurationManagerAttributes {Order = 27}));
            ModifyPorterMovementSpeedConfig = Config.Bind("Movement", "Modify Porter Movement Speed", true, new ConfigDescription("Enable or disable modifying the porter's movement speed", null, new ConfigurationManagerAttributes {Order = 26}));
            PorterMovementSpeedConfig = Config.Bind("Movement", "Porter Movement Speed", 1.0f, new ConfigDescription("Set the porter's movement speed", new AcceptableValueRange<float>(1.0f, 100f), new ConfigurationManagerAttributes {Order = 25}));
            HalloweenNowConfig = Config.Bind("Gameplay", "Halloween Now", false, new ConfigDescription("Enable or disable Halloween mode", null, new ConfigurationManagerAttributes {Order = 24}));
            HideCreditsButtonOnMainMenuConfig = Config.Bind("UI", "Hide Credits Button On Main Menu", true, new ConfigDescription("Enable or disable hiding the credits button on the main menu", null, new ConfigurationManagerAttributes {Order = 23}));
            SkipIntroVideoOnNewGameConfig = Config.Bind("Gameplay", "Skip Intro Video On New Game", false, new ConfigDescription("Enable or disable skipping the intro video on a new game", null, new ConfigurationManagerAttributes {Order = 22}));
            DisableCinematicLetterboxingConfig = Config.Bind("UI", "Disable Cinematic Letterboxing", true, new ConfigDescription("Enable or disable disabling cinematic letterboxing", null, new ConfigurationManagerAttributes {Order = 21}));
            KitsuneKitoModeConfig = Config.Bind("Misc", "KitsuneKito Mode", false, new ConfigDescription("Enable or disable KitsuneKito mode", null, new ConfigurationManagerAttributes {Order = 20}));
            SkipIntroVideoOnNewGameConfig = Config.Bind("UI", "Skip Intro Video On New Game", false, new ConfigDescription("Enable or disable skipping the intro video on a new game", null, new ConfigurationManagerAttributes {Order = 15}));
            DisableCinematicLetterboxingConfig = Config.Bind("UI", "Disable Cinematic Letterboxing", true, new ConfigDescription("Enable or disable cinematic letterboxing", null, new ConfigurationManagerAttributes {Order = 14}));
            LessenFootprintImpactConfig = Config.Bind("Gameplay", "Lessen Footprint Impact", false, new ConfigDescription("Enable or disable lessening footprint impact", null, new ConfigurationManagerAttributes {Order = 12}));
            RemovePrayerOnUseConfig = Config.Bind("Gameplay", "Remove Prayer On Use", false, new ConfigDescription("Enable or disable removing prayer on use", null, new ConfigurationManagerAttributes {Order = 11}));
            AddCoalToTavernOvenConfig = Config.Bind("Gameplay", "Add Coal To Tavern Oven", true, new ConfigDescription("Enable or disable adding coal to the tavern oven", null, new ConfigurationManagerAttributes {Order = 10}));
            AddZombiesToPyreAndCrematoriumConfig = Config.Bind("Gameplay", "Add Zombies To Pyre And Crematorium", true, new ConfigDescription("Enable or disable adding zombies to pyre and crematorium", null, new ConfigurationManagerAttributes {Order = 9}));
            KeepGamingRunningInBackgroundConfig = Config.Bind("General", "Keep Gaming Running In Background", true, new ConfigDescription("Enable or disable keeping the game running in the background", null, new ConfigurationManagerAttributes {Order = 8}));

            Log = Logger;
            _harmony = new Harmony(PluginGuid);

            Application.runInBackground = KeepGamingRunningInBackgroundConfig.Value;

            if (_modEnabled.Value)
            {
                Actions.GameStartedPlaying += Helpers.ActionsOnSpawnPlayer;
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
                Application.runInBackground = KeepGamingRunningInBackgroundConfig.Value;
                Actions.GameStartedPlaying += Helpers.ActionsOnSpawnPlayer;
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Log.LogWarning($"Removing patches for {PluginName}");
                Application.runInBackground = false;
                Actions.GameStartedPlaying -= Helpers.ActionsOnSpawnPlayer;
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