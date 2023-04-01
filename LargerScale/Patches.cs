using HarmonyLib;

namespace LargerScale;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyAfter("p1xel8ted.gyk.ultrawide")]
    [HarmonyPatch(typeof(ResolutionConfig), nameof(ResolutionConfig.GetResolutionConfigOrNull))]
    public static void ResolutionConfig_GetResolutionConfigOrNull(int width, int height, ref ResolutionConfig __result)
    {
        var res = new ResolutionConfig(width, height);
        if (height < 900 || width < 1280)
        {
            res.large_gui_scale = height / 900f;
        }

        Plugin.Log.LogInfo($"ResolutionConfig_GetResolutionConfigOrNull: Width: {width}, Height: {height}, Pixel Size: {res.pixel_size}");
        __result = res;
    }
}