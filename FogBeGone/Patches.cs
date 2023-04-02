using System.Linq;
using FlowCanvas;
using HarmonyLib;
using UnityEngine;

namespace FogBeGone;

[HarmonyPatch]
public static class Patches
{
    private static bool _introPlaying;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CustomFlowScript), nameof(CustomFlowScript.Create), typeof(GameObject), typeof(FlowGraph), typeof(bool), typeof(CustomFlowScript.OnFinishedDelegate), typeof(string))]
    public static void CustomFlowScript_Create(ref FlowGraph g)
    {
        _introPlaying = string.Equals(g.name, "red_eye_talk_1");
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(SmartWeatherState), nameof(SmartWeatherState.Update))]
    public static void SmartWeatherState_Update(SmartWeatherState __instance)
    {
        if (!MainGame.game_started) return;
        if (__instance == null) return;
        if (_introPlaying) return;
        switch (__instance.type)
        {
            case SmartWeatherState.WeatherType.Fog:
                __instance._previously_enabled = false;
                __instance._enabled = true;
                // __instance._cur_amount = 0;
                // __instance.value = 0;
                break;

            case SmartWeatherState.WeatherType.Wind:
                __instance._previously_enabled = true;
                __instance._enabled = false;
                __instance._cur_amount = 0;
                __instance.value = 0;
                break;

            case SmartWeatherState.WeatherType.Rain:
                __instance._previously_enabled = true;
                __instance._enabled = false;
                __instance._cur_amount = 0;
                __instance.value = 0;
                break;

            case SmartWeatherState.WeatherType.LUT:
                break;
        }
    }
}