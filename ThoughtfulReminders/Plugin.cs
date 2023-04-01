using System.Reflection;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using ThoughtfulReminders.lang;
using UnityEngine;

namespace ThoughtfulReminders
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.thoughtfulreminders";
        private const string PluginName = "Thoughtful Reminders";
        private const string PluginVer = "2.2.3";

        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;
        private static int _prevDayOfWeek;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> SpeechBubblesConfig;
        private static ConfigEntry<bool> _daysOnlyConfig;


        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 3, CustomDrawer = ToggleMod}));
            SpeechBubblesConfig = Config.Bind("General", "Speech Bubbles", true, new ConfigDescription("Enable or disable speech bubbles", null, new ConfigurationManagerAttributes {Order = 2}));
            _daysOnlyConfig = Config.Bind("General", "Days Only", false, new ConfigDescription("Enable or disable days only mode", null, new ConfigurationManagerAttributes {Order = 1}));
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
            Log.LogWarning($"Plugin {PluginName} has been disabled!");
        }

        private void Update()
        {
            if (!_modEnabled.Value || !MainGame.game_started || MainGame.me.player.is_dead || !Application.isFocused) return;

            var newDayOfWeek = MainGame.me.save.day_of_week;
            if (_prevDayOfWeek == newDayOfWeek || CrossModFields.TimeOfDayFloat is <= 0.22f or >= 0.25f) return;

            Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
            var localizedStrings = _daysOnlyConfig.Value ? new[] {strings.dSloth, strings.dPride, strings.dLust, strings.dGluttony, strings.dEnvy, strings.dWrath, strings._default} : new[] {strings.dhSloth, MainGame.me.save.unlocked_perks.Contains("p_preacher") ? strings.dhPrideSermon : strings.dhPride, strings.dhLust, strings.dhGluttony, strings.dhEnvy, strings.dhWrath, strings._default};

            Helpers.SayMessage(Helpers.GetLocalizedString(localizedStrings[newDayOfWeek]));
            _prevDayOfWeek = newDayOfWeek;
        }
    }
}