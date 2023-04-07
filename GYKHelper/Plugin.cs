using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GYKHelper
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.gykhelper";
        private const string PluginName = "GYK Helper Library";
        private const string PluginVer = "3.0";

        internal static ManualLogSource Log { get; private set; }
        private static ConfigEntry<bool> DisableUnityLogging { get; set; }

        private void Awake()
        {
            InitializeDisableUnityLogging();
            Log = Logger;
            RegisterEventHandlers();
            PatchWithHarmony();
        }

        private void InitializeDisableUnityLogging()
        {
            DisableUnityLogging = Config.Bind("General", "Unity Logging", false, new ConfigDescription("Toggle Unity Logging", null, new ConfigurationManagerAttributes {Order = 1}));
            DisableUnityLogging.SettingChanged += (_, _) => Debug.unityLogger.logEnabled = DisableUnityLogging.Value;
            Debug.unityLogger.logEnabled = DisableUnityLogging.Value;
        }

        private static void RegisterEventHandlers()
        {
            Actions.WorldGameObjectInteractPrefix += Actions.WorldGameObject_Interact;
            Actions.GameStartedPlaying += Actions.CleanGerries;
        }

        private static void PatchWithHarmony()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
        }
    }
}