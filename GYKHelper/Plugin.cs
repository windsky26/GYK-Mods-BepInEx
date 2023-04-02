using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace GYKHelper
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.gykhelper";
        private const string PluginName = "GYK Helper Library";
        private const string PluginVer = "2.1";

        internal static ManualLogSource Log { get; set; }

        private void Awake()
        {
            Log = Logger;
            Actions.WorldGameObjectInteractPrefix += Actions.WorldGameObject_Interact;
            Actions.GameStartedPlaying += Actions.CleanGerries;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
        }
    }
}