using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using MaxButton;
using Rewired;
using UnityEngine;

namespace MaxButtonControllerSupport;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public partial class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.maxbuttoncontrollersupport";
    private const string PluginName = "Max Button Controller Support";
    private const string PluginVer = "1.3.1";
    
    private static ManualLogSource Log { get; set; }
    private static Harmony _harmony;

    private static ConfigEntry<bool> _modEnabled;

    private void Awake()
    {
        _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
        
        Log = Logger;
        _harmony = new Harmony(PluginGuid);
        if (_modEnabled.Value)
        {
            Actions.WorldGameObjectInteractPrefix += WorldGameObject_Interact;
            Log.LogWarning($"Applying patches for {PluginName}");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    private static void ToggleMod(ConfigEntryBase entry)
    {
        var ticked = GUILayout.Toggle(_modEnabled.Value, "Enabled");

        if (ticked == _modEnabled.Value) return;
        _modEnabled.Value = ticked;

        if (ticked)
        {
            Actions.WorldGameObjectInteractPrefix += WorldGameObject_Interact;
            Log.LogWarning($"Applying patches for {PluginName}");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Actions.WorldGameObjectInteractPrefix -= WorldGameObject_Interact;
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
        Log.LogError($"Plugin {PluginName} has been disabled!");
    }

    private void Update()
    {
        if (!MainGame.game_started || MainGame.me.player.is_dead || MainGame.me.player.IsDisabled()) return;
        if (FloatingWorldGameObject.cur_floating != null) return;

        //RT = 19
        if (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(19) && _itemCountGuiOpen)
        {
            typeof(MaxButtonVendor).GetMethod("SetMaxPrice", AccessTools.all)
                ?.Invoke(typeof(MaxButtonVendor), new object[]
                {
                    _slider
                });
        }

        //LT = 20
        if (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(20) && _itemCountGuiOpen)
        {
            typeof(MaxButtonVendor).GetMethod("SetSliderValue", AccessTools.all)
                ?.Invoke(typeof(MaxButtonVendor), new object[]
                {
                    _slider,
                    1
                });
        }

        //RT = 19
        if (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(19) && _craftGuiOpen && !_unsafeInteraction)
        {
            if (_craftItemGui.current_craft.needs.Any(need => need.is_multiquality)) return;
            if (_craftItemGui.current_craft.one_time_craft) return;
            typeof(MaxButtonCrafting).GetMethod("SetMaximumAmount", AccessTools.all)
                ?.Invoke(typeof(MaxButtonCrafting), new object[]
                {
                    _craftItemGui,
                    _crafteryWgo
                });
        }

        //LT = 20
        if (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(20) && _craftGuiOpen && !_unsafeInteraction)
        {
            if (_craftItemGui.current_craft.needs.Any(need => need.is_multiquality)) return;
            if (_craftItemGui.current_craft.one_time_craft) return;
            typeof(MaxButtonCrafting).GetMethod("SetMinimumAmount", AccessTools.all)
                ?.Invoke(typeof(MaxButtonCrafting), new object[]
                {
                    _craftItemGui
                });
        }
    }
}