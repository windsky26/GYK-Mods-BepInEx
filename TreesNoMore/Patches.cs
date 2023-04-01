using System;
using HarmonyLib;

namespace TreesNoMore;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.InitNewObject))]
    private static void WorldGameObject_InitNewObject(ref WorldGameObject __instance)
    {
        if (__instance == null) return;
        if (__instance.obj_id.Contains("stump"))
        {
            UnityEngine.Object.Destroy(__instance.gameObject);
        }
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.InitNewObject))]
    public static Exception WorldGameObject_InitNewObject_Finalizer()
    {
        return null;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.SmartInstantiate))]
    public static void WorldGameObject_SmartInstantiate(ref WorldObjectPart prefab)
    {
        if (prefab == null) return;
        if ((!MainGame.game_started && !MainGame.game_starting) || !prefab.name.Contains("tree") || prefab.name.Contains("bees")) return;
        if (prefab.name.Contains("apple")) return;
        prefab = null;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.SmartInstantiate))]
    public static Exception WorldGameObject_SmartInstantiate_Finalizer()
    {
        return null;
    }
}