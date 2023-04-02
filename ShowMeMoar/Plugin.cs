using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace ShowMeMoar
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.showmemoar";
        private const string PluginName = "Show Me Moar!";
        private const string PluginVer = "0.0.1";
        private static Harmony _harmony;
        private static ConfigEntry<bool> _modEnabled;
        private static ConfigEntry<KeyboardShortcut> _zoomIn;
        private static ConfigEntry<KeyboardShortcut> _zoomOut;

        private static ConfigEntry<float> _zoom;
        private static ConfigEntry<float> _craftIconAboveStations;

        private static GameObject _icons;
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            var defaultZoom = Screen.currentResolution.height / 2f;
            var min = 0 - defaultZoom;

            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 4, CustomDrawer = ToggleMod}));

            _craftIconAboveStations = Config.Bind("General", "Interaction Bubble Scale", 1f, new ConfigDescription("Changes the scale of the icons that appear above crafting stations and interaction icons.", new AcceptableValueRange<float>(0.1f, 10f), new ConfigurationManagerAttributes {Order = 3}));
            _craftIconAboveStations.SettingChanged += (_, args) =>
            {
                if (!MainGame.game_started) return;

                var eventArgs = (SettingChangedEventArgs) args;
                var setting = Convert.ToSingle(eventArgs.ChangedSetting.GetSerializedValue());
                UpdateCraftIconScale(setting);
            };

            _zoom = Config.Bind("General", "Zoom", 0f, new ConfigDescription("Zoom", new AcceptableValueRange<float>(min + 10, defaultZoom * 2), new ConfigurationManagerAttributes {Order = 2}));
            _zoom.SettingChanged += (_, args) =>
            {
                if (!MainGame.game_started) return;
                var eventArgs = (SettingChangedEventArgs) args;
                var setting = Convert.ToSingle(eventArgs.ChangedSetting.GetSerializedValue());
                Camera.main!.orthographicSize = defaultZoom + setting;
            };

            _zoomIn = Config.Bind("General", "Zoom In", new KeyboardShortcut(KeyCode.KeypadPlus), new ConfigDescription("Zoom In", null, new ConfigurationManagerAttributes {Order = 1}));
            _zoomOut = Config.Bind("General", "Zoom Out", new KeyboardShortcut(KeyCode.KeypadMinus), new ConfigDescription("Zoom Out", null, new ConfigurationManagerAttributes {Order = 0}));

            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.GameStartedPlaying += GameStartedPlaying;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }


        private void Update()
        {
            if (!MainGame.game_started) return;

            if (_zoomIn.Value.IsPressed())
            {
                _zoom.Value -= 5f;
            }

            if (_zoomOut.Value.IsPressed())
            {
                _zoom.Value += 5f;
            }
        }

        private void OnEnable()
        {
            Log.LogInfo($"Plugin {PluginName} has been enabled!");
        }

        private void OnDisable()
        {
            Log.LogWarning($"Plugin {PluginName} has been disabled!");
        }

        private static void UpdateCraftIconScale(float scale)
        {
            _icons ??= GameObject.Find("UI Root/Interaction bubbles");
            if (_icons != null)
            {
                _icons.transform.localScale = new Vector3(scale, scale, 1);
            }
        }

        private static void GameStartedPlaying(MainGame obj)
        {
            if (!MainGame.game_started) return;
            var setting = _zoom.Value;
            var defaultZoom = GameSettings.current_resolution.y / 2f;
            Camera.main!.orthographicSize = defaultZoom + setting;
        }

        private static void ToggleMod(ConfigEntryBase entry)
        {
            var ticked = GUILayout.Toggle(_modEnabled.Value, "Enabled");

            if (ticked == _modEnabled.Value) return;
            _modEnabled.Value = ticked;

            if (ticked)
            {
                Actions.GameStartedPlaying += GameStartedPlaying;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.GameStartedPlaying -= GameStartedPlaying;
                Camera.main!.orthographicSize = Screen.currentResolution.height / 2f;
                Log.LogWarning($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }
    }
}