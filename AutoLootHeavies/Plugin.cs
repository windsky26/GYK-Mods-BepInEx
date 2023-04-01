using System.Reflection;
using AutoLootHeavies.lang;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using GYKHelper;
using UnityEngine;

namespace AutoLootHeavies
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.autolootheavies";
        private const string PluginName = "Auto-Loot Heavies!";
        private const string PluginVer = "3.4.5";

        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;


        private void Awake()
        {
            Log = Logger;
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);

            TeleportToDumpSiteWhenAllStockPilesFull = Config.Bind("General", "TeleportToDumpSiteWhenAllStockPilesFull", false, "Teleport to dump site when all stock piles are full.");
            DesignatedTimberLocation = Config.Bind("General", "DesignatedTimberLocation", new Vector3(-3712.003f, 6144, 1294.643f), "Designated timber location.");
            DesignatedOreLocation = Config.Bind("General", "DesignatedOreLocation", new Vector3(-3712.003f, 6144, 1294.643f), "Designated ore location.");
            DesignatedStoneLocation = Config.Bind("General", "DesignatedStoneLocation", new Vector3(-3712.003f, 6144, 1294.643f), "Designated stone location.");
            DisableImmersionMode = Config.Bind("General", "DisableImmersionMode", false, "Disable immersion mode.");
            Debug = Config.Bind("General", "Debug", false, "Debug.");
            ToggleTeleportToDumpSiteKeybind = Config.Bind("Keybinds", "ToggleTeleportToDumpSiteKeybind", KeyCode.Alpha6, "Toggle teleport to dump site keybind.");
            SetTimberLocationKeybind = Config.Bind("Keybinds", "SetTimberLocationKeybind", KeyCode.Alpha7, "Set timber location keybind.");
            SetOreLocationKeybind = Config.Bind("Keybinds", "SetOreLocationKeybind", KeyCode.Alpha8, "Set ore location keybind.");
            SetStoneLocationKeybind = Config.Bind("Keybinds", "SetStoneLocationKeybind", KeyCode.Alpha9, "Set stone location keybind.");
        }


        private void OnEnable()
        {
            Log.LogInfo($"Plugin {PluginName} is enabled!");
        }

        private void OnDisable()
        {
            _harmony.UnpatchSelf();
            Log.LogInfo($"Disabled {PluginName}!");
        }

        private void Update()
        {
            if (!MainGame.game_started) return;

            if (!Fields.InitialFullUpdate)
            {
                Fields.InitialFullUpdate = true;
                MainGame.me.StartCoroutine(Helpers.RunFullUpdate());
            }

            if (Input.GetKeyUp(ToggleTeleportToDumpSiteKeybind.Value))
            {
                if (!TeleportToDumpSiteWhenAllStockPilesFull.Value)
                {
                    TeleportToDumpSiteWhenAllStockPilesFull.Value = true;
                    Helpers.Log($"Enabled teleport to dump site when stockpiles full.");
                    Tools.ShowMessage(strings.TeleOn, Vector3.zero);
                }
                else
                {
                    TeleportToDumpSiteWhenAllStockPilesFull.Value = false;
                    Helpers.Log($"Disabled teleport to dump site when stockpiles full.");
                    Tools.ShowMessage(strings.TeleOff, Vector3.zero);
                }
            }

            if (Input.GetKeyUp(SetTimberLocationKeybind.Value))
            {
                DesignatedTimberLocation.Value = MainGame.me.player_pos;
                Helpers.Log($"Set timber dump site to {DesignatedTimberLocation.Value}.");
                Tools.ShowMessage(strings.DumpTimber, DesignatedTimberLocation.Value);
            }

            if (Input.GetKeyUp(SetOreLocationKeybind.Value))
            {
                DesignatedOreLocation.Value = MainGame.me.player_pos;
                Helpers.Log($"Set ore dump site to {DesignatedOreLocation.Value}.");
                Tools.ShowMessage(strings.DumpOre, DesignatedOreLocation.Value);
            }

            if (Input.GetKeyUp(SetStoneLocationKeybind.Value))
            {
                DesignatedStoneLocation.Value = MainGame.me.player_pos;
                Helpers.Log($"Set stone dump site to {DesignatedStoneLocation.Value}.");
                Tools.ShowMessage(strings.DumpStone, DesignatedStoneLocation.Value);
            }
        }
    }
}