using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using GYKHelper;
using ThoughtfulReminders.lang;
using UnityEngine;

namespace ThoughtfulReminders;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.thoughtfulreminders";
    private const string PluginName = "Thoughtful Reminders";
    private const string PluginVer = "2.2.3";

    private static int PrevDayOfWeek { get; set; }

    private static ConfigEntry<bool> ModEnabled { get; set; }
    internal static ConfigEntry<bool> SpeechBubblesConfig { get; private set; }
    private static ConfigEntry<bool> DaysOnlyConfig { get; set; }


    private void Awake()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 3}));

        SpeechBubblesConfig = Config.Bind("1. General", "Speech Bubbles", true, new ConfigDescription("Enable or disable speech bubbles", null, new ConfigurationManagerAttributes {Order = 2}));
        DaysOnlyConfig = Config.Bind("1. General", "Days Only", false, new ConfigDescription("Enable or disable days only mode", null, new ConfigurationManagerAttributes {Order = 1}));

    }

    private void Update()
    {
        if (!ModEnabled.Value || !MainGame.game_started || MainGame.me.player.is_dead || !Application.isFocused) return;

        var newDayOfWeek = MainGame.me.save.day_of_week;
        if (PrevDayOfWeek == newDayOfWeek || CrossModFields.TimeOfDayFloat is <= 0.22f or >= 0.25f) return;

        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        var localizedStrings = DaysOnlyConfig.Value ? new[] {strings.dSloth, strings.dPride, strings.dLust, strings.dGluttony, strings.dEnvy, strings.dWrath, strings._default} : new[] {strings.dhSloth, MainGame.me.save.unlocked_perks.Contains("p_preacher") ? strings.dhPrideSermon : strings.dhPride, strings.dhLust, strings.dhGluttony, strings.dhEnvy, strings.dhWrath, strings._default};

        Helpers.SayMessage(Helpers.GetLocalizedString(localizedStrings[newDayOfWeek]));
        PrevDayOfWeek = newDayOfWeek;
    }
}