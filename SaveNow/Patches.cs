using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GYKHelper;
using HarmonyLib;
using SaveNow.lang;
using UnityEngine;

namespace SaveNow;

[HarmonyPatch]
public partial class Plugin
{
    private static Vector3 Pos { get; set; }
    private static string DataPath { get; set; }
    private static string SavePath { get; set; }
    private static readonly List<SaveSlotData> AllSaveGames = new();
    private static List<SaveSlotData> SortedTrimmedSaveGames  { get; set; } = new();
    private static bool CanSave { get; set; }
    private static string CurrentSave { get; set; }
    private static readonly Dictionary<string, Vector3> SaveLocationsDictionary = new();


    [HarmonyPrefix]
    [HarmonyPatch(typeof(SaveSlotsMenuGUI), nameof(SaveSlotsMenuGUI.RedrawSlots))]
    public static void SaveSlotsMenuGUI_RedrawSlots(ref List<SaveSlotData> slot_datas, ref bool focus_on_first)
    {
        slot_datas.Clear();
        AllSaveGames.Clear();
        SortedTrimmedSaveGames.Clear();

        LoadSaveGames();

        SortedTrimmedSaveGames = SortSaveGames();

        if (SortedTrimmedSaveGames.Count > MaximumSavesVisible.Value)
        {
            Resize(SortedTrimmedSaveGames, MaximumSavesVisible.Value);
        }

        slot_datas = SortedTrimmedSaveGames;
        focus_on_first = true;
    }

    private static void LoadSaveGames()
    {
        var saveFiles = Directory.GetFiles(PlatformSpecific.GetSaveFolder(), "*.info",
            SearchOption.TopDirectoryOnly);

        foreach (var text in saveFiles)
        {
            var data = SaveSlotData.FromJSON(File.ReadAllText(text));
            if (data == null) continue;
            data.filename_no_extension = Path.GetFileNameWithoutExtension(text);
            AllSaveGames.Add(data);
        }
    }

    private static List<SaveSlotData> SortSaveGames()
    {
        return SortByRealTime.Value
            ? (AscendingSort.Value
                ? AllSaveGames.OrderBy(o => o.real_time).ToList()
                : AllSaveGames.OrderByDescending(o => o.real_time).ToList())
            : (AscendingSort.Value
                ? AllSaveGames.OrderBy(o => o.game_time).ToList()
                : AllSaveGames.OrderByDescending(o => o.game_time).ToList());
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(InGameMenuGUI), nameof(InGameMenuGUI.OnPressedSaveAndExit))]
    public static bool InGameMenuGUI_OnPressedSaveAndExit(InGameMenuGUI __instance)
    {
        if (!Tools.TutorialDone()) return true;
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;

        __instance.SetControllsActive(false);
        __instance.OnClosePressed();

        var messageText = CreateMessageText();

        var dialog = GUIElements.me.dialog;
        dialog.OpenYesNo(messageText, SaveAndExit);

        return false;

        string CreateMessageText()
        {
            var baseMessage = ExitToDesktop.Value ? strings.SaveAreYouSureDesktop : strings.SaveAreYouSureMenu;
            var progressMessage = DisableSaveOnExit.Value || CrossModFields.IsInDungeon ? strings.SaveProgressNotSaved : strings.SaveProgressSaved;

            return $"{baseMessage}?\n\n{progressMessage}.";
        }

        void SaveAndExit()
        {
            if (DisableSaveOnExit.Value || CrossModFields.IsInDungeon)
            {
                PerformExit();
            }
            else
            {
                if (SaveLocation(true, string.Empty))
                {
                    PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save, delegate { PerformExit(); });
                }
            }
        }

        void PerformExit()
        {
            if (ExitToDesktop.Value)
            {
                GC.Collect();
                Resources.UnloadUnusedAssets();
                Application.Quit();
            }
            else
            {
                LoadingGUI.Show(__instance.ReturnToMainMenu);
            }
        }
    }


    // if this isn't here, when you sleep, it teleport you back to where the mod saved you last
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SleepGUI), nameof(SleepGUI.WakeUp))]
    public static void SleepGUI_WakeUp()
    {
        SaveLocation(false, string.Empty);
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(InGameMenuGUI), nameof(InGameMenuGUI.Open))]
    public static void InGameMenuGUI_Open(ref InGameMenuGUI __instance)
    {
        if (__instance == null || !ExitToDesktop.Value) return;

        var exitButtons = __instance.GetComponentsInChildren<UIButton>()
            .Where(x => x.name.Contains("exit"));

        foreach (var comp in exitButtons)
        {
            var exitLabels = comp.GetComponentsInChildren<UILabel>();
            foreach (var label in exitLabels)
            {
                label.text = strings.ExitButtonText;
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MovementComponent), nameof(MovementComponent.UpdateMovement), null)]
    public static void MovementComponent_UpdateMovement(MovementComponent __instance)
    {
        CanSave = !__instance.player_controlled_by_script;
    }
}