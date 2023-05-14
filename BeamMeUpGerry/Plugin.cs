using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace BeamMeUpGerry;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.beammeupgerry";
    private const string PluginName = "Beam Me Up Gerry!";
    private const string PluginVer = "2.0.7";

    internal static ConfigEntry<bool> Debug;
    internal static ManualLogSource Log { get; private set; }
    private static Harmony Harmony { get; set; }

    private static ConfigEntry<bool> ModEnabled { get; set; }
    internal static ConfigEntry<bool> IncreaseMenuAnimationSpeed { get; private set; }
    internal static ConfigEntry<bool> FadeForCustomLocations { get;  private set; }
    internal static ConfigEntry<bool> EnableListExpansion { get; private set; }
    internal static ConfigEntry<bool> Gerry { get; private set; }
    internal static ConfigEntry<bool> Cost { get; private set; }
    private static ConfigEntry<KeyboardShortcut> TeleportMenuKeyBind { get; set; }
    private static ConfigEntry<string> TeleportMenuControllerButton { get; set; }

    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 802}));
        ModEnabled.SettingChanged += ApplyPatches;

        IncreaseMenuAnimationSpeed = Config.Bind("2. Features", "Increase Menu Animation Speed", true, new ConfigDescription("Toggle increased menu animation speed", null, new ConfigurationManagerAttributes {Order = 801}));
        FadeForCustomLocations = Config.Bind("2. Features", "Fade For Custom Locations", true, new ConfigDescription("Toggle fade effect for custom locations", null, new ConfigurationManagerAttributes {Order = 800}));
        EnableListExpansion = Config.Bind("2. Features", "Enable List Expansion", true, new ConfigDescription("Toggle list expansion functionality", null, new ConfigurationManagerAttributes {Order = 799}));
        Gerry = Config.Bind("2. Features", "Gerry", false, new ConfigDescription("Toggle Gerry's presence", null, new ConfigurationManagerAttributes {Order = 798}));
        Cost = Config.Bind("2. Features", "Gerrys Fee", false, new ConfigDescription("Toggle the cost of teleporting", null, new ConfigurationManagerAttributes {Order = 797}));

        TeleportMenuKeyBind = Config.Bind("3. Keybinds", "Teleport Menu Keybind", new KeyboardShortcut(KeyCode.Z), new ConfigDescription("Set the keybind for opening the teleport menu", null, new ConfigurationManagerAttributes {Order = 796}));
        TeleportMenuControllerButton = Config.Bind("4. Controller", "Teleport Menu Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.RB), new ConfigDescription("Set the controller button for opening the teleport menu", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 795}));

        Debug = Config.Bind("5. Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 794}));
    }


    private static void ApplyPatches(object sender, EventArgs eventArgs)
    {
        if (ModEnabled.Value)
        {
            Actions.WorldGameObjectInteract += Patches.WorldGameObject_Interact;
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Actions.WorldGameObjectInteract -= Patches.WorldGameObject_Interact;
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }

    private void Update()
    {
        if (!IsUpdateConditionsMet()) return;

        HandleTeleportMenuInput();

        HandleEscapeInput();
    }

    private static bool IsUpdateConditionsMet()
    {
        return MainGame.game_started && !MainGame.me.player.is_dead && !MainGame.me.player.IsDisabled() && !MainGame.paused;
    }

    private static void HandleTeleportMenuInput()
    {
        if (BaseGUI.all_guis_closed && MainGame.me.player.components.character.control_enabled)
        {
            var player = ReInput.players.GetPlayer(0);
            if (LazyInput.gamepad_active && player.GetButtonDown(TeleportMenuControllerButton.Value) ||
                TeleportMenuKeyBind.Value.IsUp())
            {
                Helpers.DoLoggingAndBeam();
            }
        }
    }

    private void HandleEscapeInput()
    {
        var escapeKeyPressed = Input.GetKeyUp(KeyCode.Escape);
        var gamepadButtonPressed = LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(3);

        if (escapeKeyPressed || gamepadButtonPressed)
        {
            CloseMaGui();
        }
    }

    private static void CloseMaGui()
    {
        if (Patches.MaGui != null)
        {
            Helpers.ShowHud(null, true);
            Sounds.OnClosePressed();
            Patches.MaGui.DestroyBubble();
            Patches.UsingStone = false;
            Patches.DotSelection = false;
            CrossModFields.TalkingToNpc(false);
            MainGame.me.player.components.character.control_enabled = true;
            Patches.MaGui = null;
        }
    }
}