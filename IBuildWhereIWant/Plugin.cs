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
        private const string PluginVer = "1.7.3";


        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        private static ConfigEntry<bool> _disableGrid;
        private static ConfigEntry<bool> _disableGreyRemoveOverlay;
        private static ConfigEntry<bool> _disableBuildingCollision;
        private static ConfigEntry<bool> _debug;
        private static ConfigEntry<KeyboardShortcut> _menuKeyBind;
        private static ConfigEntry<string> _menuControllerButton;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(null, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 605}));
            _modEnabled.SettingChanged += ApplyPatches;

            _disableBuildingCollision = Config.Bind("2. Collision", "Building Collision", true, new ConfigDescription("Toggle collision between buildings to place them closer together (or on top of each other...)", null, new ConfigurationManagerAttributes {Order = 604}));

            _disableGrid = Config.Bind("3. Display", "Grid", false, new ConfigDescription("Toggle the grid overlay from the building interface for a cleaner look.", null, new ConfigurationManagerAttributes {Order = 603}));

            _disableGreyRemoveOverlay = Config.Bind("3. Display", "Grey Overlay", false, new ConfigDescription("Toggle the grey overlay that appears when removing objects in the building interface.", null, new ConfigurationManagerAttributes {Order = 602}));

            _menuKeyBind = Config.Bind("4. Keybinds", "Menu Key Bind", new KeyboardShortcut(KeyCode.Q), new ConfigDescription("Define the key used to open the mod menu.", null, new ConfigurationManagerAttributes {Order = 601}));

            _menuControllerButton = Config.Bind("5. Controller", "Menu Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.RB), new ConfigDescription("Select the controller button used to open the mod menu.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 600}));

            _debug = Config.Bind("6. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging for troubleshooting purposes.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 599}));
        }


        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }

        private void Update()
        {
            if (!CanOpenCraftAnywhere()) return;

            if ((LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(_menuControllerButton.Value)) ||
                _menuKeyBind.Value.IsUp())
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
    }
}