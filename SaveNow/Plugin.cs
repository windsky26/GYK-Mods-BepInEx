using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using Rewired;
using SaveNow.lang;
using UnityEngine;

namespace SaveNow;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper", "3.0.1")]
public partial class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.savenow";
    private const string PluginName = "Save Now!";
    private const string PluginVer = "2.4.9";

    private static ConfigEntry<bool> Debug { get; set; }
    private static ConfigEntry<int> SaveInterval { get; set; }
    private static ConfigEntry<bool> AutoSaveConfig { get; set; }
    private static ConfigEntry<bool> NewFileOnAutoSave { get; set; }
    private static ConfigEntry<bool> NewFileOnManualSave { get; set; }
    private static ConfigEntry<bool> BackupSavesOnSave { get; set; }
    private static ConfigEntry<bool> TravelMessages { get; set; }
    private static ConfigEntry<bool> SaveGameNotificationText { get; set; }
    private static ConfigEntry<bool> ExitToDesktop { get; set; }
    private static ConfigEntry<bool> DisableSaveOnExit { get; set; }
    private static ConfigEntry<int> MaximumSavesVisible { get; set; }
    private static ConfigEntry<bool> SortByRealTime { get; set; }
    private static ConfigEntry<bool> AscendingSort { get; set; }
    private static ConfigEntry<bool> EnableManualSaveControllerButton { get; set; }
    private static ConfigEntry<KeyboardShortcut> ManualSaveKeyBind { get; set; }
    private static ConfigEntry<string> ManualSaveControllerButton { get; set; }
    private static ManualLogSource Log { get; set; }
    private static Harmony Harmony { get; set; }

    private static ConfigEntry<bool> ModEnabled { get; set; }

    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 19}));
        ModEnabled.SettingChanged += ApplyPatches;

        SaveInterval = Config.Bind("2. Saving", "Save Interval", 600, new ConfigDescription("Interval between automatic saves in seconds.", null, new ConfigurationManagerAttributes {Order = 18}));

        AutoSaveConfig = Config.Bind("2. Saving", "Auto Save", true, new ConfigDescription("Enable or disable automatic saving.", null, new ConfigurationManagerAttributes {Order = 17}));

        NewFileOnAutoSave = Config.Bind("2. Saving", "New File On Auto Save", true, new ConfigDescription("Create a new save file for each auto save.", null, new ConfigurationManagerAttributes {Order = 16}));

        NewFileOnManualSave = Config.Bind("2. Saving", "New File On Manual Save", true, new ConfigDescription("Create a new save file for each manual save.", null, new ConfigurationManagerAttributes {Order = 15}));

        BackupSavesOnSave = Config.Bind("2. Saving", "Backup Saves On Save", true, new ConfigDescription("Backup saves when saving the game.", null, new ConfigurationManagerAttributes {Order = 14}));

        TravelMessages = Config.Bind("3. Notifications", "Travel Messages", false, new ConfigDescription("Toggle travel messages.", null, new ConfigurationManagerAttributes {Order = 13}));

        SaveGameNotificationText = Config.Bind("3. Notifications", "Save Game Notification Text", false, new ConfigDescription("Disable save game notification text.", null, new ConfigurationManagerAttributes {Order = 12}));

        ExitToDesktop = Config.Bind("4. Exiting", "Exit To Desktop", false, new ConfigDescription("Enable or disable exit to desktop.", null, new ConfigurationManagerAttributes {Order = 11}));

        DisableSaveOnExit = Config.Bind("4. Exiting", "Save On Exit", true, new ConfigDescription("Disable saving the game when exiting.", null, new ConfigurationManagerAttributes {Order = 10}));

        MaximumSavesVisible = Config.Bind("5. UI", "Maximum Saves Visible", 3, new ConfigDescription("Maximum number of save files visible in the UI.", null, new ConfigurationManagerAttributes {Order = 9}));

        SortByRealTime = Config.Bind("5. UI", "Sort By Real Time", false, new ConfigDescription("Sort save files by real time instead of in-game time.", null, new ConfigurationManagerAttributes {Order = 8}));

        AscendingSort = Config.Bind("5. UI", "Ascending Sort", false, new ConfigDescription("Sort save files in ascending order.", null, new ConfigurationManagerAttributes {Order = 7}));
            
        ManualSaveKeyBind = Config.Bind("6. Controls", "Manual Save Key Bind", new KeyboardShortcut(KeyCode.K), new ConfigDescription("Key bind for manually saving the game.", null, new ConfigurationManagerAttributes {Order = 6}));
            
        EnableManualSaveControllerButton = Config.Bind("6. Controls", "Enable Manual Save Controller Button", false, new ConfigDescription("Enable or disable the manual save controller button.", null, new ConfigurationManagerAttributes {Order = 5}));
        ManualSaveControllerButton = Config.Bind("6. Controls", "Manual Save Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.LT), new ConfigDescription("Controller button for manually saving the game.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 4}));

        Debug = Config.Bind("7. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 3}));
    }

    private static void UpdateSaveData()
    {
        SavePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SaveBackup");


        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

        DataPath = Path.Combine(PlatformSpecific.GetSaveFolder(), "save-locations-savenow.dat");
        LoadSaveLocations();
    }

    private void Update()
    {
        if (!MainGame.game_started || !Tools.TutorialDone()) return;

        if (ManualSaveKeyBind.Value.IsUp() ||
            (EnableManualSaveControllerButton.Value && LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(ManualSaveControllerButton.Value)))
        {
            PerformManualSave();
        }
    }

    private static void PerformManualSave()
    {
        if (CrossModFields.IsInDungeon)
        {
            Tools.SpawnGerry(GetLocalizedString(strings.CantSaveHere), Vector3.zero, CrossModFields.ModGerryTag);
            return;
        }

        GUIElements.me.ShowSavingStatus(true);

        void SaveCallback(SaveSlotData slot)
        {
            SaveLocation(false, string.Empty);
            GUIElements.me.ShowSavingStatus(false);
        }

        if (NewFileOnManualSave.Value)
        {
            var date = DateTime.Now.ToString("ddmmyyhhmmss");
            var newSlot = $"manualsave.{date}".Trim();

            MainGame.me.save_slot.filename_no_extension = newSlot;
            PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save, SaveCallback);
        }
        else
        {
            PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save, SaveCallback);
        }
    }

    private static void ApplyPatches(object sender, EventArgs eventArgs)
    {
        if (ModEnabled.Value)
        {
            Actions.GameStartedPlaying += RestoreLocation;
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            UpdateSaveData();
        }
        else
        {
            Actions.GameStartedPlaying -= RestoreLocation;
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }
}