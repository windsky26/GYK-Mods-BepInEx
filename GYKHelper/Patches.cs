using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace GYKHelper
{
    [HarmonyPriority(1)]
    public static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(BaseGUI), nameof(BaseGUI.Hide), typeof(bool))]
        public static void BaseGuiHidePostfix()
        {
            if (BaseGUI.all_guis_closed)
            {
                Tools.SetAllInteractionsFalse();
            }
        }


        [HarmonyPrefix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(SaveSlotsMenuGUI), nameof(SaveSlotsMenuGUI.Open))]
        public static void SaveSlotsMenuGUI_Open()
        {
            MainGame.game_started = false;
        }

        [HarmonyPostfix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.ApplyLanguageChange))]
        public static void GameSettingsApplyLanguageChangePostfix()
        {
            CrossModFields.Lang = GameSettings.me.language.Replace('_', '-').ToLower(CultureInfo.InvariantCulture).Trim();
            CrossModFields.Culture = CultureInfo.GetCultureInfo(CrossModFields.Lang);
            Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        }

        [HarmonyPrefix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(SmartAudioEngine), nameof(SmartAudioEngine.OnEndNPCInteraction))]
        public static void OnEndNPCInteractionPrefix()
        {
            CrossModFields.TalkingToNpc(false);
        }

        [HarmonyPrefix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(MovementComponent), nameof(MovementComponent.UpdateMovement), typeof(Vector2), typeof(float))]
        public static void Prefix(ref MovementComponent __instance)
        {
            if (__instance.wgo.is_player)
            {
                CrossModFields.PlayerIsDead = __instance.wgo.is_dead;
                CrossModFields.PlayerIsControlled = __instance.player_controlled_by_script;
                CrossModFields.PlayerFollowingTarget = __instance.is_following_target;
            }
        }


        [HarmonyPostfix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(MainMenuGUI), nameof(MainMenuGUI.Open))]
        public static void MainMenuGUI_Open_Postfix(ref MainMenuGUI __instance)
        {
            if (__instance == null) return;

            foreach (var comp in __instance.GetComponentsInChildren<UILabel>()
                         .Where(x => x.name.Contains("ver txt")))
            {
                comp.text =
                    $"[F7B000] BepInEx Modded[-] [F7B000]GYKHelper v{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}[-]";
                comp.overflowMethod = UILabel.Overflow.ResizeFreely;
                comp.multiLine = true;
                comp.MakePixelPerfect();
                //labelToMimic = comp;
            }
        }


        [HarmonyPrefix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(VendorGUI), nameof(VendorGUI.Open), typeof(WorldGameObject), typeof(GJCommons.VoidDelegate))]
        public static void VendorGUI_Open()
        {
            if (!MainGame.game_started) return;
            CrossModFields.TalkingToNpc(true);
        }

        [HarmonyPrefix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(VendorGUI), nameof(VendorGUI.Hide), typeof(bool))]
        public static void VendorGUI_Hide()
        {
            if (!MainGame.game_started) return;
            CrossModFields.TalkingToNpc(false);
        }

        [HarmonyPrefix]
        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(VendorGUI), nameof(VendorGUI.OnClosePressed))]
        public static void VendorGUI_OnClosePressed()
        {
            if (!MainGame.game_started) return;
            CrossModFields.TalkingToNpc(false);
        }
    }
}