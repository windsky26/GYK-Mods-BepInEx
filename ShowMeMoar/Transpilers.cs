namespace ShowMeMoar;

public static class Transpilers

{
    private static float _newValue;
    private static float _otherNewValue;

    // [HarmonyTranspiler]
    // [HarmonyPatch(typeof(FogObject), nameof(FogObject.Update))]
    // private static IEnumerable<CodeInstruction> FogObject_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
    // {
    //     _newValue = Screen.width > 3440 ? 72f : 48f;
    //     _otherNewValue = Screen.width > 3440 ? 12f : 8f;
    //     var code = new List<CodeInstruction>(instructions);
    //     var index = -1;
    //     for (var i = 0; i < code.Count; i++)
    //     {
    //         if (code[i].opcode == OpCodes.Ldarg_0 &&
    //             code[i + 1].opcode == OpCodes.Ldflda &&
    //             code[i + 2].opcode == OpCodes.Ldfld &&
    //             code[i + 3].opcode == OpCodes.Ldc_R4 && code[i + 3].OperandIs(6) &&
    //             code[i + 4].opcode == OpCodes.Ldsfld &&
    //             code[i + 5].opcode == OpCodes.Sub)
    //         {
    //             index = i + 3;
    //         }
    //     }
    //
    //     if (index != -1)
    //     {
    //         code[index].operand = _otherNewValue;
    //         Helpers.Log($"Patched 6 to {_otherNewValue} in FogUpdate.");
    //     }
    //     else
    //     {
    //         Helpers.Log($"Could not patch 6 to {_otherNewValue} in FogUpdate.", true);
    //     }
    //
    //     return code.AsEnumerable();
    // }


    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(FogObject), nameof(FogObject.Update))]
    // [HarmonyPatch(typeof(FogObject), nameof(FogObject.InitFog))]
    // public static void FogObject_InitFog_Prefix(ref Vector3 ___TILES_X_VECTOR)
    // {
    //     _newValue = Screen.width > 3440 ? 72f : 48f;
    //     _otherNewValue = Screen.width > 3440 ? 12f : 8f;
    //     ___TILES_X_VECTOR = new Vector2(_newValue, 0f);
    // }

    // // [HarmonyTranspiler]
    // [HarmonyPatch(typeof(FogObject), nameof(FogObject.InitFog))]
    // private static IEnumerable<CodeInstruction> FogObject_InitFog_Transpiler(IEnumerable<CodeInstruction> instructions)
    // {
    //     _newValue = Screen.width > 3440 ? 72f : 48f;
    //     _otherNewValue = Screen.width > 3440 ? 12f : 8f;
    //     var code = new List<CodeInstruction>(instructions);
    //
    //     var index = -1;
    //     for (var i = 0; i < code.Count; i++)
    //     {
    //         if (code[i].opcode == OpCodes.Stloc_1 &&
    //             code[i + 1].opcode == OpCodes.Ldloc_1 &&
    //             code[i + 2].opcode == OpCodes.Ldc_I4_6 &&
    //             code[i + 3].opcode == OpCodes.Blt_S &&
    //             code[i + 4].opcode == OpCodes.Ret)
    //         {
    //             index = i + 2;
    //         }
    //     }
    //
    //     if (index != -1)
    //     {
    //         code[index] = new CodeInstruction(OpCodes.Ldc_I4_S, Convert.ToInt32(_otherNewValue));
    //         Helpers.Log($"Patched 6 to {_otherNewValue} in InitFog.");
    //     }
    //     else
    //     {
    //         Helpers.Log($"Could not patch 6 to {_otherNewValue} in InitFog.", true);
    //     }
    //
    //     return code.AsEnumerable();
    // }
}