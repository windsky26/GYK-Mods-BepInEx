using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace IBuildWhereIWant
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.ibuildwhereiwant";
        private const string PluginName = "I Build Where I Want!";
        private const string PluginVer = "1.7.4";

    
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> DisableGrid;
        internal static ConfigEntry<bool> DisableGreyRemoveOverlay;
        internal static ConfigEntry<bool> DisableBuildingCollision;
        internal static ConfigEntry<bool> Debug;
        internal static ConfigEntry<KeyboardShortcut> MenuKeyBind;
        internal static ConfigEntry<string> MenuControllerButton;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 602}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 601}));

            DisableGrid = Config.Bind("Display", "Disable Grid", true, new ConfigDescription("Disable the grid in the building interface.", null, new ConfigurationManagerAttributes {Order = 600}));

            DisableGreyRemoveOverlay = Config.Bind("Display", "Disable Grey Remove Overlay", true, new ConfigDescription("Disable the grey overlay when removing objects.", null, new ConfigurationManagerAttributes {Order = 599}));

            DisableBuildingCollision = Config.Bind("Collision", "Disable Building Collision", false, new ConfigDescription("Disable collision between buildings.", null, new ConfigurationManagerAttributes {Order = 598}));
            
            MenuKeyBind = Config.Bind("Keybinds", "Menu Key Bind", new KeyboardShortcut(KeyCode.Q), new ConfigDescription("Key bind to open the menu.", null, new ConfigurationManagerAttributes {Order = 595}));

            MenuControllerButton = Config.Bind("Controller", "Menu Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.RB), new ConfigDescription("Controller button to open the menu.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 594}));
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

        private void Update()
        {
            if (!CanOpenCraftAnywhere()) return;

            if ((LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(MenuControllerButton.Value)) ||
                MenuKeyBind.Value.IsUp())
            {
                OpenCraftAnywhere();
            }
        }

        private static bool CanOpenCraftAnywhere()
        {
            return MainGame.game_started && !MainGame.me.player.is_dead && !MainGame.me.player.IsDisabled() &&
                   !MainGame.paused && BaseGUI.all_guis_closed &&
                   !MainGame.me.player.GetMyWorldZoneId().Contains("refugee");
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