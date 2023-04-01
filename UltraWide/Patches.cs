using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace UltraWide;

[HarmonyPatch]
public static class Patches
{
    internal static float NewValue;
    internal static float OtherNewValue;

    [HarmonyPostfix]
    [HarmonyBefore("p1xel8ted.gyk.largerscale")]
    [HarmonyPatch(typeof(ResolutionConfig), nameof(ResolutionConfig.GetResolutionConfigOrNull))]
    public static void Postfix(int width, int height, ref ResolutionConfig __result)
    {
        Helpers.Log("New Resolution: " + width + "x" + height);
        __result ??= new ResolutionConfig(width, height);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(FogObject), nameof(FogObject.Update))]
    private static IEnumerable<CodeInstruction> FogObject_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        NewValue = Screen.width > 3440 ? 72f : 48f;
        OtherNewValue = Screen.width > 3440 ? 12f : 8f;
        var code = new List<CodeInstruction>(instructions);
        var index = -1;
        for (var i = 0; i < code.Count; i++)
        {
            if (code[i].opcode == OpCodes.Ldarg_0 &&
                code[i + 1].opcode == OpCodes.Ldflda &&
                code[i + 2].opcode == OpCodes.Ldfld &&
                code[i + 3].opcode == OpCodes.Ldc_R4 && code[i + 3].OperandIs(6) &&
                code[i + 4].opcode == OpCodes.Ldsfld &&
                code[i + 5].opcode == OpCodes.Sub)
            {
                index = i + 3;
            }
        }
    
        if (index != -1)
        {
            code[index].operand = OtherNewValue;
            Helpers.Log($"Patched 6 to {OtherNewValue} in FogUpdate.");
        }
        else
        {
            Helpers.Log($"Could not patch 6 to {OtherNewValue} in FogUpdate.",true);
        }
    
        return code.AsEnumerable();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(FogObject), nameof(FogObject.Update))]
    [HarmonyPatch(typeof(FogObject), nameof(FogObject.InitFog))]
    public static void FogObject_InitFog_Prefix(ref Vector3 ___TILES_X_VECTOR)
    {
        NewValue = Screen.width > 3440 ? 72f : 48f;
        OtherNewValue = Screen.width > 3440 ? 12f : 8f;
        ___TILES_X_VECTOR = new Vector2(NewValue, 0f);
    }

    // [HarmonyTranspiler]
    [HarmonyPatch(typeof(FogObject), nameof(FogObject.InitFog))]
    private static IEnumerable<CodeInstruction> FogObject_InitFog_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        NewValue = Screen.width > 3440 ? 72f : 48f;
        OtherNewValue = Screen.width > 3440 ? 12f : 8f;
        var code = new List<CodeInstruction>(instructions);
    
        var index = -1;
        for (var i = 0; i < code.Count; i++)
        {
            if (code[i].opcode == OpCodes.Stloc_1 &&
                code[i + 1].opcode == OpCodes.Ldloc_1 &&
                code[i + 2].opcode == OpCodes.Ldc_I4_6 &&
                code[i + 3].opcode == OpCodes.Blt_S &&
                code[i + 4].opcode == OpCodes.Ret)
            {
                index = i + 2;
            }
        }
    
        if (index != -1)
        {
            code[index] = new CodeInstruction(OpCodes.Ldc_I4_S, Convert.ToInt32(OtherNewValue));
            Helpers.Log($"Patched 6 to {OtherNewValue} in InitFog.");
        }
        else
        {
            Helpers.Log($"Could not patch 6 to {OtherNewValue} in InitFog.",true);
        }
    
        return code.AsEnumerable();
    }
}