using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using Rewired;
using Rewired.Platforms.XboxOne;
using UnityEngine;

namespace BeamMeUpGerry
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.beammeupgerry";
        private const string PluginName = "Beam Me Up Gerry!";
        private const string PluginVer = "2.0.6";

        internal static ConfigEntry<bool> Debug;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<bool> IncreaseMenuAnimationSpeed;
        internal static ConfigEntry<bool> FadeForCustomLocations;
        internal static ConfigEntry<bool> EnableListExpansion;
        internal static ConfigEntry<bool> DisableGerry;
        internal static ConfigEntry<bool> DisableCost;
        private static ConfigEntry<KeyboardShortcut> _teleportMenuKeyBind;
        private static ConfigEntry<string> _teleportMenuControllerButton;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod, Order = 802}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 801}));

            IncreaseMenuAnimationSpeed = Config.Bind("General", "Increase Menu Animation Speed", true, new ConfigDescription("Enable or disable increasing menu animation speed.", null, new ConfigurationManagerAttributes {Order = 800}));

            FadeForCustomLocations = Config.Bind("General", "Fade For Custom Locations", true, new ConfigDescription("Enable or disable fade for custom locations.", null, new ConfigurationManagerAttributes {Order = 799}));

            EnableListExpansion = Config.Bind("General", "Enable List Expansion", true, new ConfigDescription("Enable or disable list expansion.", null, new ConfigurationManagerAttributes {Order = 798}));

            DisableGerry = Config.Bind("General", "Disable Gerry", false, new ConfigDescription("Enable or disable Gerry.", null, new ConfigurationManagerAttributes {Order = 797}));

            DisableCost = Config.Bind("General", "Disable Cost", false, new ConfigDescription("Enable or disable cost.", null, new ConfigurationManagerAttributes {Order = 796}));

            _teleportMenuKeyBind = Config.Bind("Keybinds", "Teleport Menu Keybind", new KeyboardShortcut(KeyCode.Z), new ConfigDescription("Set the keybind for opening the teleport menu.", null, new ConfigurationManagerAttributes {Order = 794}));

            _teleportMenuControllerButton = Config.Bind("Controller", "Teleport Menu Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.RB), new ConfigDescription("Set the controller button for opening the teleport menu.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 792}));

            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.WorldGameObjectInteract += Patches.WorldGameObject_Interact;
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
                Actions.WorldGameObjectInteract += Patches.WorldGameObject_Interact;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.WorldGameObjectInteract -= Patches.WorldGameObject_Interact;
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

        private void Update()
        {
            if (!MainGame.game_started || MainGame.me.player.is_dead || MainGame.me.player.IsDisabled() ||
                MainGame.paused) return;

            if (BaseGUI.all_guis_closed && MainGame.me.player.components.character.control_enabled)
            {
                if (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(_teleportMenuControllerButton.Value))
                {
                    Helpers.DoLoggingAndBeam();
                }

                if (Plugin._teleportMenuKeyBind.Value.IsUp())
                {
                    Helpers.DoLoggingAndBeam();
                }
            }

            if (Input.GetKeyUp(KeyCode.Escape) ||
                (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(3)))
            {
                if (Patches.MaGui != null)
                {
                    Helpers.ShowHud(null, true);
                    Sounds.OnClosePressed();
                    //_maGui.OnChosen("cancel");
                    //_maGui.OnChosen("leave");
                    Patches.MaGui.DestroyBubble();
                    Patches.UsingStone = false;
                    Patches.DotSelection = false;
                    CrossModFields.TalkingToNpc(false);
                    MainGame.me.player.components.character.control_enabled = true;
                    Patches.MaGui = null;
                }
            }
        }
    }
}