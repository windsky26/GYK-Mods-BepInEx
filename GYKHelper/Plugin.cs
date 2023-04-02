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
        private const string PluginVer = "2.1";

        internal static ManualLogSource Log { get; private set; }
        private static ConfigEntry<bool> DisableUnityLogging { get; set; }

        private void Awake()
        {
            DisableUnityLogging = Config.Bind("General", "Unity Logging", false, new ConfigDescription("Toggle Unity Logging", null, new ConfigurationManagerAttributes {Order = 1}));
            DisableUnityLogging.SettingChanged += (_, args) =>
            {
                var eventArgs = (SettingChangedEventArgs) args;
                var setting = Convert.ToBoolean(eventArgs.ChangedSetting.GetSerializedValue());
                Debug.unityLogger.logEnabled = setting;
            };
            Debug.unityLogger.logEnabled = DisableUnityLogging.Value;
            Log = Logger;
            Actions.WorldGameObjectInteractPrefix += Actions.WorldGameObject_Interact;
            Actions.GameStartedPlaying += Actions.CleanGerries;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
        }
    }
}