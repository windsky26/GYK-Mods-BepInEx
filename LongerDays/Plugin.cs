using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace LongerDays
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.longerdays";
        private const string PluginName = "Longer Days";
        private const string PluginVer = "1.6.3";

        internal const float MadnessSeconds = 1350f; //3 bodies in total
        internal const float EvenLongerSeconds = 1125f; //2 bodies in total
        internal const float DoubleLengthSeconds = 900f; //2 bodies in total
        internal const float DefaultIncreaseSeconds = 675f; //1 body in total

        internal static float Seconds;
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        private static ConfigEntry<float> _dayLength;

        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
            _dayLength = Config.Bind("General", "Day Length", 675f, new ConfigDescription($"Set the length of a day", new AcceptableValueList<float>(675f, 900f, 1125f, 1350f), new ConfigurationManagerAttributes {CustomDrawer = LengthSlider}));
            Seconds = _dayLength.Value;
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        private static void LengthSlider(ConfigEntryBase entry)
        {
            GUILayout.Label($"{Patches.GetTimeMulti()}x", GUILayout.Width(60));
            float[] steps = {675f, 900f, 1125f, 1350f};
            var selectedIndex = Mathf.RoundToInt((_dayLength.Value - steps[0]) / (steps[steps.Length - 1] - steps[0]) * (steps.Length - 1));
            var newSelectedIndex = Mathf.RoundToInt(GUILayout.HorizontalSlider(selectedIndex, 0, steps.Length - 1,GUILayout.ExpandWidth(true)));
            if (newSelectedIndex == selectedIndex) return;
            _dayLength.Value = steps[newSelectedIndex];
            Seconds = _dayLength.Value;
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

        private void OnEnable()
        {
            Log.LogInfo($"Plugin {PluginName} has been enabled!");
        }

        private void OnDisable()
        {
            Log.LogWarning($"Plugin {PluginName} has been disabled!");
        }
    }
}