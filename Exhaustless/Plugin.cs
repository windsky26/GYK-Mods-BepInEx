using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Exhaustless
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.Exhaustless";
        private const string PluginName = "Add Straight to Table!";
        private const string PluginVer = "0.0.1";

        private static ManualLogSource _log = null!;
        private static readonly Harmony Harmony = new(PluginGuid);

        private void Awake()

        {
            _log = new ManualLogSource(PluginName);
            BepInEx.Logging.Logger.Sources.Add(_log);
        }


        private void OnEnable()
        {
            Harmony.PatchAll();
            _log.LogWarning($"Plugin {PluginName} is loaded!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            _log.LogWarning($"Unloaded {PluginName}!");
        }
    }
}