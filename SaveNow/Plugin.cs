using System;
using System.Collections.Generic;
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

namespace SaveNow
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.savenow";
        private const string PluginName = "Save Now!";
        private const string PluginVer = "2.4.8";

        private static ConfigEntry<bool> _debug;
        private static ConfigEntry<int> _saveInterval;
        private static ConfigEntry<bool> _autoSaveConfig;
        private static ConfigEntry<bool> _newFileOnAutoSave;
        private static ConfigEntry<bool> _newFileOnManualSave;
        private static ConfigEntry<bool> _backupSavesOnSave;
        private static ConfigEntry<bool> _travelMessages;
        private static ConfigEntry<bool> _saveGameNotificationText;
        private static ConfigEntry<bool> _exitToDesktop;
        private static ConfigEntry<bool> _disableSaveOnExit;
        private static ConfigEntry<int> _maximumSavesVisible;
        private static ConfigEntry<bool> _sortByRealTime;
        private static ConfigEntry<bool> _ascendingSort;
        private static ConfigEntry<bool> _enableManualSaveControllerButton;
        private static ConfigEntry<KeyboardShortcut> _manualSaveKeyBind;
        private static ConfigEntry<string> _manualSaveControllerButton;
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 19}));
            _modEnabled.SettingChanged += ApplyPatches;

            _saveInterval = Config.Bind("2. Saving", "Save Interval", 600, new ConfigDescription("Interval between automatic saves in seconds.", null, new ConfigurationManagerAttributes {Order = 18}));

            _autoSaveConfig = Config.Bind("2. Saving", "Auto Save", true, new ConfigDescription("Enable or disable automatic saving.", null, new ConfigurationManagerAttributes {Order = 17}));

            _newFileOnAutoSave = Config.Bind("2. Saving", "New File On Auto Save", true, new ConfigDescription("Create a new save file for each auto save.", null, new ConfigurationManagerAttributes {Order = 16}));

            _newFileOnManualSave = Config.Bind("2. Saving", "New File On Manual Save", true, new ConfigDescription("Create a new save file for each manual save.", null, new ConfigurationManagerAttributes {Order = 15}));

            _backupSavesOnSave = Config.Bind("2. Saving", "Backup Saves On Save", true, new ConfigDescription("Backup saves when saving the game.", null, new ConfigurationManagerAttributes {Order = 14}));

            _travelMessages = Config.Bind("3. Notifications", "Travel Messages", false, new ConfigDescription("Toggle travel messages.", null, new ConfigurationManagerAttributes {Order = 13}));

            _saveGameNotificationText = Config.Bind("3. Notifications", "Save Game Notification Text", false, new ConfigDescription("Disable save game notification text.", null, new ConfigurationManagerAttributes {Order = 12}));

            _exitToDesktop = Config.Bind("4. Exiting", "Exit To Desktop", false, new ConfigDescription("Enable or disable exit to desktop.", null, new ConfigurationManagerAttributes {Order = 11}));

            _disableSaveOnExit = Config.Bind("4. Exiting", "Save On Exit", true, new ConfigDescription("Disable saving the game when exiting.", null, new ConfigurationManagerAttributes {Order = 10}));

            _maximumSavesVisible = Config.Bind("5. UI", "Maximum Saves Visible", 3, new ConfigDescription("Maximum number of save files visible in the UI.", null, new ConfigurationManagerAttributes {Order = 9}));

            _sortByRealTime = Config.Bind("5. UI", "Sort By Real Time", false, new ConfigDescription("Sort save files by real time instead of in-game time.", null, new ConfigurationManagerAttributes {Order = 8}));

            _ascendingSort = Config.Bind("5. UI", "Ascending Sort", false, new ConfigDescription("Sort save files in ascending order.", null, new ConfigurationManagerAttributes {Order = 7}));
            
            _manualSaveKeyBind = Config.Bind("6. Controls", "Manual Save Key Bind", new KeyboardShortcut(KeyCode.K), new ConfigDescription("Key bind for manually saving the game.", null, new ConfigurationManagerAttributes {Order = 6}));
            
            _enableManualSaveControllerButton = Config.Bind("6. Controls", "Enable Manual Save Controller Button", false, new ConfigDescription("Enable or disable the manual save controller button.", null, new ConfigurationManagerAttributes {Order = 5}));
            _manualSaveControllerButton = Config.Bind("6. Controls", "Manual Save Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.LT), new ConfigDescription("Controller button for manually saving the game.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 4}));

            _debug = Config.Bind("7. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 3}));
        }

        private static void UpdateSaveData()
        {
            _savePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "SaveBackup");


            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            _dataPath = Path.Combine(PlatformSpecific.GetSaveFolder(), "save-locations-savenow.dat");
            LoadSaveLocations();
        }

        private void Update()
        {
            if (!MainGame.game_started || !Tools.TutorialDone()) return;

            if (_manualSaveKeyBind.Value.IsUp() ||
                (_enableManualSaveControllerButton.Value && LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(_manualSaveControllerButton.Value)))
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

            if (_newFileOnManualSave.Value)
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
            if (_modEnabled.Value)
            {
                Actions.GameStartedPlaying += RestoreLocation;
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                UpdateSaveData();
            }
            else
            {
                Actions.GameStartedPlaying -= RestoreLocation;
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }
    }
}