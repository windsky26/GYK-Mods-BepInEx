using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace NoTimeForFishing
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.notimeforfishing";
        private const string PluginName = "No Time For Fishing!";
        private const string PluginVer = "3.2.2";
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            _modEnabled = Config.Bind("General", "Enabled", true, $"Toggle {PluginName}");
            _modEnabled.SettingChanged += ApplyPatches;

            ApplyPatches(this, null);
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
    }
}