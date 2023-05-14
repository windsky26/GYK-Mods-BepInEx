using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace ShowMeMoar;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.showmemoar";
    private const string PluginName = "Show Me Moar!";
    private const string PluginVer = "0.0.1";
    private static Harmony Harmony { get; set; }
    private static ConfigEntry<bool> ModEnabled { get; set; }
    private static ConfigEntry<KeyboardShortcut> ZoomIn { get; set; }
    private static ConfigEntry<KeyboardShortcut> ZoomOut { get; set; }

    private static ConfigEntry<float> Zoom { get; set; }
    private static ConfigEntry<float> CraftIconAboveStations { get; set; }

    private static GameObject Icons { get; set; }
    internal static ManualLogSource Log { get; private set; }

    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        var defaultZoom = Screen.currentResolution.height / 2f;
        var min = 0 - defaultZoom;

        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 4}));
        ModEnabled.SettingChanged += ApplyPatches;

        CraftIconAboveStations = Config.Bind("2. General", "Interaction Bubble Scale", 1f, new ConfigDescription("Changes the scale of the icons that appear above crafting stations and interaction icons.", new AcceptableValueRange<float>(0.1f, 10f), new ConfigurationManagerAttributes {Order = 3}));
        CraftIconAboveStations.SettingChanged += (_, _) =>
        {
            if (!MainGame.game_started) return;
            UpdateCraftIconScale(CraftIconAboveStations.Value);
        };

        Zoom = Config.Bind("3. General", "Zoom", 0f, new ConfigDescription("Zoom", new AcceptableValueRange<float>(min + 10, defaultZoom * 2), new ConfigurationManagerAttributes {Order = 2}));
        Zoom.SettingChanged += (_, _) =>
        {
            if (!MainGame.game_started) return;
            Camera.main!.orthographicSize = defaultZoom + Zoom.Value;
        };

        ZoomIn = Config.Bind("4. General", "Zoom In", new KeyboardShortcut(KeyCode.KeypadPlus), new ConfigDescription("Zoom In", null, new ConfigurationManagerAttributes {Order = 1}));
        ZoomOut = Config.Bind("5. General", "Zoom Out", new KeyboardShortcut(KeyCode.KeypadMinus), new ConfigDescription("Zoom Out", null, new ConfigurationManagerAttributes {Order = 0}));
    }



    private void Update()
    {
        if (!MainGame.game_started) return;

        if (ZoomIn.Value.IsPressed())
        {
            Zoom.Value -= 5f;
        }

        if (ZoomOut.Value.IsPressed())
        {
            Zoom.Value += 5f;
        }
    }

    private static void UpdateCraftIconScale(float scale)
    {
        Icons ??= GameObject.Find("UI Root/Interaction bubbles");
        if (Icons != null)
        {
            Icons.transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    private static void GameStartedPlaying(MainGame obj)
    {
        if (!MainGame.game_started) return;
        var setting = Zoom.Value;
        var defaultZoom = GameSettings.current_resolution.y / 2f;
        Camera.main!.orthographicSize = defaultZoom + setting;
    }

    private static void ApplyPatches(object sender, EventArgs eventArgs)
    {
        if (ModEnabled.Value)
        {
            Actions.GameStartedPlaying += GameStartedPlaying;
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Actions.GameStartedPlaying -= GameStartedPlaying;
            Camera.main!.orthographicSize = Screen.currentResolution.height / 2f;
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }
}