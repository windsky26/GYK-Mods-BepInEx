using System;
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
        private const string PluginVer = "2.2.4";


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
        internal static ConfigEntry<bool> CinematicLetterboxingConfig;
        internal static ConfigEntry<bool> KitsuneKitoModeConfig;
        internal static ConfigEntry<bool> LessenFootprintImpactConfig;
        internal static ConfigEntry<bool> RemovePrayerOnUseConfig;
        internal static ConfigEntry<bool> AddCoalToTavernOvenConfig;
        internal static ConfigEntry<bool> AddZombiesToPyreAndCrematoriumConfig;
        private static ConfigEntry<bool> _keepGamingRunningInBackgroundConfig;

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
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 31}));
            _modEnabled.SettingChanged += ApplyPatches;

            _keepGamingRunningInBackgroundConfig = Config.Bind("1. General", "Keep Game Running In Background", true, new ConfigDescription("Keep the game running when it is in the background.", null, new ConfigurationManagerAttributes {Order = 30}));

            QuietMusicInGuiConfig = Config.Bind("2. Audio", "Quiet Music In GUI", true, new ConfigDescription("Lower the music volume when in-game menus are open.", null, new ConfigurationManagerAttributes {Order = 29}));

            CondenseXpBarConfig = Config.Bind("3. UI", "Condense XP Bar", true, new ConfigDescription("Reduce the size of the XP bar in the user interface.", null, new ConfigurationManagerAttributes {Order = 28}));
            HideCreditsButtonOnMainMenuConfig = Config.Bind("3. UI", "Hide Credits Button On Main Menu", true, new ConfigDescription("Remove the credits button from the main menu.", null, new ConfigurationManagerAttributes {Order = 27}));
            CinematicLetterboxingConfig = Config.Bind("3. UI", "Remove Cinematic Letterboxing", true, new ConfigDescription("Remove black bars during cinematic cutscenes.", null, new ConfigurationManagerAttributes {Order = 25}));

            HalloweenNowConfig = Config.Bind("4. Gameplay", "Halloween Now", false, new ConfigDescription("Activate Halloween mode at any time.", null, new ConfigurationManagerAttributes {Order = 24}));
            SkipIntroVideoOnNewGameConfig = Config.Bind("4. Gameplay", "Skip Intro Video On New Game", false, new ConfigDescription("Skip the intro video when starting a new game.", null, new ConfigurationManagerAttributes {Order = 23}));
            LessenFootprintImpactConfig = Config.Bind("4. Gameplay", "Lessen Footprint Impact", false, new ConfigDescription("Reduce the impact of footprints on the environment.", null, new ConfigurationManagerAttributes {Order = 22}));
            RemovePrayerOnUseConfig = Config.Bind("4. Gameplay", "Remove Prayer On Use", false, new ConfigDescription("Prayers are removed after use.", null, new ConfigurationManagerAttributes {Order = 21}));
            AddCoalToTavernOvenConfig = Config.Bind("4. Gameplay", "Add Coal To Tavern Oven", true, new ConfigDescription("Allow coal to be used as fuel in the tavern oven.", null, new ConfigurationManagerAttributes {Order = 20}));
            AddZombiesToPyreAndCrematoriumConfig = Config.Bind("4. Gameplay", "Add Zombies To Pyre And Crematorium", true, new ConfigDescription("Enable the option to burn zombies at the pyre and crematorium.", null, new ConfigurationManagerAttributes {Order = 19}));

            ModifyPlayerMovementSpeedConfig = Config.Bind("5. Movement", "Modify Player Movement Speed", true, new ConfigDescription("Allow modification of the player's movement speed.", null, new ConfigurationManagerAttributes {Order = 18}));
            PlayerMovementSpeedConfig = Config.Bind("5. Movement", "Player Movement Speed", 1.0f, new ConfigDescription("Set the player's movement speed.", new AcceptableValueRange<float>(1.0f, 100f), new ConfigurationManagerAttributes {Order = 17}));
            ModifyPorterMovementSpeedConfig = Config.Bind("5. Movement", "Modify Porter Movement Speed", true, new ConfigDescription("Allow modification of the porter's movement speed.", null, new ConfigurationManagerAttributes {Order = 16}));
            PorterMovementSpeedConfig = Config.Bind("5. Movement", "Porter Movement Speed", 1.0f, new ConfigDescription("Set the porter's movement speed.", new AcceptableValueRange<float>(1.0f, 100f), new ConfigurationManagerAttributes {Order = 15}));
            KitsuneKitoModeConfig = Config.Bind("6. Misc", "KitsuneKito Mode", false, new ConfigDescription("Discord user request. Drops a blue xp point when adding a basic fence to a grave.", null, new ConfigurationManagerAttributes {Order = 14}));

            Debug = Config.Bind("7. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 13}));
        }

        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Log.LogInfo($"Applying patches for {PluginName}");
                Application.runInBackground = _keepGamingRunningInBackgroundConfig.Value;
                Actions.GameStartedPlaying += Helpers.ActionsOnSpawnPlayer;
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Log.LogInfo($"Removing patches for {PluginName}");
                Application.runInBackground = false;
                Actions.GameStartedPlaying -= Helpers.ActionsOnSpawnPlayer;
                _harmony.UnpatchSelf();
            }
        }
    }
}