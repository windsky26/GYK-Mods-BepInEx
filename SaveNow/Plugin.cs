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
        private const string PluginVer = "2.4.9";

        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<int> SaveInterval;
        internal static ConfigEntry<bool> AutoSaveConfig;
        internal static ConfigEntry<bool> NewFileOnAutoSave;
        internal static ConfigEntry<bool> NewFileOnManualSave;
        internal static ConfigEntry<bool> BackupSavesOnSave;
        internal static ConfigEntry<bool> TurnOffTravelMessages;
        internal static ConfigEntry<bool> TurnOffSaveGameNotificationText;
        internal static ConfigEntry<bool> ExitToDesktop;
        internal static ConfigEntry<bool> DisableSaveOnExit;
        internal static ConfigEntry<int> MaximumSavesVisible;
        internal static ConfigEntry<bool> SortByRealTime;
        internal static ConfigEntry<bool> AscendingSort;
        internal static ConfigEntry<bool> EnableManualSaveControllerButton;
        internal static ConfigEntry<KeyboardShortcut> ManualSaveKeyBind;
        internal static ConfigEntry<string> ManualSaveControllerButton;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 802}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 801}));
            SaveInterval = Config.Bind("Saving", "Save Interval", 600, new ConfigDescription("Interval between automatic saves in seconds.", null, new ConfigurationManagerAttributes {Order = 800}));

            AutoSaveConfig = Config.Bind("Saving", "Auto Save", true, new ConfigDescription("Enable or disable automatic saving.", null, new ConfigurationManagerAttributes {Order = 799}));

            NewFileOnAutoSave = Config.Bind("Saving", "New File On Auto Save", true, new ConfigDescription("Create a new save file for each auto save.", null, new ConfigurationManagerAttributes {Order = 798}));

            NewFileOnManualSave = Config.Bind("Saving", "New File On Manual Save", true, new ConfigDescription("Create a new save file for each manual save.", null, new ConfigurationManagerAttributes {Order = 797}));

            BackupSavesOnSave = Config.Bind("Saving", "Backup Saves On Save", true, new ConfigDescription("Backup saves when saving the game.", null, new ConfigurationManagerAttributes {Order = 796}));

            TurnOffTravelMessages = Config.Bind("Notifications", "Turn Off Travel Messages", false, new ConfigDescription("Disable travel messages.", null, new ConfigurationManagerAttributes {Order = 795}));

            TurnOffSaveGameNotificationText = Config.Bind("Notifications", "Turn Off Save Game Notification Text", false, new ConfigDescription("Disable save game notification text.", null, new ConfigurationManagerAttributes {Order = 794}));

            ExitToDesktop = Config.Bind("Exiting", "Exit To Desktop", false, new ConfigDescription("Enable or disable exit to desktop.", null, new ConfigurationManagerAttributes {Order = 793}));

            DisableSaveOnExit = Config.Bind("Exiting", "Disable Save On Exit", false, new ConfigDescription("Disable saving the game when exiting.", null, new ConfigurationManagerAttributes {Order = 792}));

            MaximumSavesVisible = Config.Bind("UI", "Maximum Saves Visible", 3, new ConfigDescription("Maximum number of save files visible in the UI.", null, new ConfigurationManagerAttributes {Order = 791}));

            SortByRealTime = Config.Bind("UI", "Sort By Real Time", false, new ConfigDescription("Sort save files by real time instead of in-game time.", null, new ConfigurationManagerAttributes {Order = 789}));

            AscendingSort = Config.Bind("UI", "Ascending Sort", false, new ConfigDescription("Sort save files in ascending order.", null, new ConfigurationManagerAttributes {Order = 788}));

            EnableManualSaveControllerButton = Config.Bind("Controls", "Enable Manual Save Controller Button", false, new ConfigDescription("Enable or disable the manual save controller button.", null, new ConfigurationManagerAttributes {Order = 786}));

            ManualSaveKeyBind = Config.Bind("Controls", "Manual Save Key Bind", new KeyboardShortcut(KeyCode.K), new ConfigDescription("Key bind for manually saving the game.", null, new ConfigurationManagerAttributes {Order = 787}));

            ManualSaveControllerButton = Config.Bind("Controls", "Manual Save Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.LT), new ConfigDescription("Controller button for manually saving the game.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 784}));


            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.GameStartedPlaying += RestoreLocation;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());

                UpdateSaveData();
            }
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

        private static void ToggleMod(ConfigEntryBase entry)
        {
            var ticked = GUILayout.Toggle(_modEnabled.Value, "Enabled");

            if (ticked == _modEnabled.Value) return;
            _modEnabled.Value = ticked;

            if (ticked)
            {
                Actions.GameStartedPlaying += RestoreLocation;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                UpdateSaveData();
            }
            else
            {
                Actions.GameStartedPlaying -= RestoreLocation;
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