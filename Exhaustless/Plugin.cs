using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;

namespace Exhaustless;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.exhaustless";
    private const string PluginName = "Exhaust-less!";
    private const string PluginVer = "3.4.5";

    private static ConfigEntry<bool> ModEnabled { get; set; }
    internal static ConfigEntry<bool> MakeToolsLastLonger { get;private set; }
    internal static ConfigEntry<bool> SpendHalfGratitude { get; private set; }
    internal static ConfigEntry<bool> AutoEquipNewTool { get; private set; }
    internal static ConfigEntry<bool> SpeedUpSleep { get; private set; }
    internal static ConfigEntry<bool> AutoWakeFromMeditationWhenStatsFull { get; private set; }
    internal static ConfigEntry<bool> SpendHalfSanity { get; private set; }
    internal static ConfigEntry<bool> SpeedUpMeditation { get; private set; }
    internal static ConfigEntry<bool> SpendHalfEnergy { get; private set; }
    internal static ConfigEntry<int> EnergySpendBeforeSleepDebuff { get; private set; }

    private static ManualLogSource Log { get; set; }
    private static Harmony Harmony { get; set; }


    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 11}));
        ModEnabled.SettingChanged += ApplyPatches;

        AutoEquipNewTool = Config.Bind("2. Tools", "Auto Equip New Tool", true, new ConfigDescription("Automatically equip a new tool if the current one breaks", null, new ConfigurationManagerAttributes {Order = 10}));
        MakeToolsLastLonger = Config.Bind("2. Tools", "Make Tools Last Longer", true, new ConfigDescription("Increase the durability of tools", null, new ConfigurationManagerAttributes {Order = 9}));

        AutoWakeFromMeditationWhenStatsFull = Config.Bind("3. Meditation", "Auto Wake From Meditation When Stats Full", true, new ConfigDescription("Automatically wake up when meditation is complete", null, new ConfigurationManagerAttributes {Order = 8}));
        SpeedUpMeditation = Config.Bind("3. Meditation", "Speed Up Meditation", true, new ConfigDescription("Reduce the time needed for meditation", null, new ConfigurationManagerAttributes {Order = 7}));

        EnergySpendBeforeSleepDebuff = Config.Bind("4. Sleep", "Energy Spend Before Sleep Debuff", 1200, new ConfigDescription("Set the total energy spent in a day required (game's default is 300) before sleep debuff is applied", new AcceptableValueRange<int>(350, 50000), new ConfigurationManagerAttributes {Order = 6}));
        SpeedUpSleep = Config.Bind("4. Sleep", "Speed Up Sleep", true, new ConfigDescription("Decrease the time needed for sleep", null, new ConfigurationManagerAttributes {Order = 5}));

        SpendHalfEnergy = Config.Bind("5. Gameplay", "Spend Half Energy", true, new ConfigDescription("Reduce energy consumption by half", null, new ConfigurationManagerAttributes {Order = 3}));
        SpendHalfGratitude = Config.Bind("5. Gameplay", "Spend Half Gratitude", true, new ConfigDescription("Reduce gratitude consumption by half", null, new ConfigurationManagerAttributes {Order = 2}));
        SpendHalfSanity = Config.Bind("5. Gameplay", "Spend Half Sanity", true, new ConfigDescription("Reduce sanity consumption by half", null, new ConfigurationManagerAttributes {Order = 1}));
    }


    private static void ApplyPatches(object sender, EventArgs eventArgs)
    {
        if (ModEnabled.Value)
        {
            Actions.GameBalanceLoad += Patches.GameBalance_LoadGameBalance;
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Actions.GameBalanceLoad -= Patches.GameBalance_LoadGameBalance;
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }
}