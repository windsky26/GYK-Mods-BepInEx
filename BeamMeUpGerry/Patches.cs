using System.Collections.Generic;
using System.Linq;
using BeamMeUpGerry.lang;
using FlowCanvas.Nodes;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace BeamMeUpGerry;

[HarmonyPatch]
public static class Patches
{
    internal static readonly List<Location> LocationsPartOne = new()
    {
        new Location("zone_witch_hut", "", new Vector3(-4964, -1772, -370)),
        new Location("zone_cellar", "mortuary", new Vector3(10841, -9241, -1923), EnvironmentEngine.State.Inside),
        new Location("zone_alchemy", "mortuary", new Vector3(8249, -10180, -2119), EnvironmentEngine.State.Inside),
        new Location("zone_morgue", "mortuary", new Vector3(9744, -11327, -2357), EnvironmentEngine.State.Inside),
        new Location("zone_beegarden", "", new Vector3(3234, 1815, 378)),
        new Location("zone_hill", "", new Vector3(8292, 1396, 292)),
        new Location("zone_sacrifice", "", new Vector3(9529, -8427, -1753), EnvironmentEngine.State.Inside),
        new Location("zone_beatch", "", new Vector3(22507, 314, 70)),
        new Location("zone_vineyard", "", new Vector3(6712, 42, 10)),
        new Location("zone_camp", "", new Vector3(20690, 2818, 591)),
        new Location("....", "", Vector3.zero),
        new Location("cancel", "", Vector3.zero),
    };

    internal static readonly List<Location> LocationsPartTwo = new()
    {
        new Location("zone_souls", "mortuary", new Vector3(11050, -10807, -2249), EnvironmentEngine.State.Inside),
        new Location("zone_graveyard", "", new Vector3(1635, -1506, -313)),
        new Location("zone_euric_room", "euric", new Vector3(20108, -11599, -2412), EnvironmentEngine.State.Inside),
        new Location("zone_church", "church", new Vector3(182, -8218, -1712), EnvironmentEngine.State.Inside),
        new Location("zone_zombie_sawmill", "", new Vector3(2204, 3409, 710)),
        new Location(strings.Coal, "", new Vector3(-505, 6098, 1270)),
        new Location(strings.Clay, "", new Vector3(595, -3185, -663)),
        new Location(strings.Sand, "", new Vector3(334, 875, 182)),
        new Location(strings.Mill, "", new Vector3(11805, -768, -157)),
        new Location(strings.Farmer, "", new Vector3(11800, -3251, -675)),
        new Location("cancel", "", Vector3.zero),
    };


    internal static bool DotSelection;
    internal static MultiAnswerGUI MaGui;
    internal static bool UsingStone;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MultiAnswerGUI), nameof(MultiAnswerGUI.OnChosen))]
    public static void MultiAnswerGUI_OnChosen_Prefix(ref string answer)
    {
        if (Tools.PlayerDisabled())
        {
            return;
        }

        if (Helpers.InTutorial()) return;
        if (!Plugin.EnableListExpansion.Value) return;
        var canUseStone = Helpers.CanUseStone();
        if (!canUseStone) return;
        // if (_isNpc) return;
        List<AnswerVisualData> answers;

        DotSelection = false;

        if (answer == "...")
        {
            // answers = LocationByVectorPartOne.Select(location => new AnswerVisualData() { id = location.Key }).ToList();
            answers = LocationsPartOne.Select(location => new AnswerVisualData() {id = location.Zone}).ToList();
            Show(out answer);
            return;
        }

        if (answer == "....")
        {
            //answers = LocationByVectorPartTwo.Select(location => new AnswerVisualData() { id = location.Key }).ToList();
            answers = LocationsPartTwo.Select(location => new AnswerVisualData() {id = location.Zone}).ToList();
            Show(out answer);
            return;
        }

        void Show(out string answer)
        {
            CrossModFields.TalkingToNpc(false);
            var cleanedAnswers = Helpers.ValidateAnswerList(answers);
            answer = "cancel";
            DotSelection = true;
            UsingStone = true;
            MainGame.me.player.components.character.control_enabled = false;
            MainGame.me.player.ShowMultianswer(cleanedAnswers, Helpers.BeamGerryOnChosen);
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(MultiAnswerGUI), nameof(MultiAnswerGUI.OnChosen))]
    public static void MultiAnswerGUI_OnChosen_Postfix(string answer)
    {
        if (Tools.PlayerDisabled())
        {
            return;
        }

        if (Helpers.InTutorial()) return;
        if (!Plugin.EnableListExpansion.Value)
        {
            UsingStone = false;
            DotSelection = false;
            CrossModFields.TalkingToNpc(false);
            return;
        }

        if (!Helpers.CanUseStone()) return;

        Helpers.Log($"[Answer]: {answer}");

        if (string.Equals("cancel", answer) && !DotSelection)
        {
            //real cancel
            Helpers.ShowHud(null, true);
            UsingStone = false;
            DotSelection = false;
            CrossModFields.TalkingToNpc(false);

            MainGame.me.player.components.character.control_enabled = true;
            return;
        }

        if (string.Equals("cancel", answer) && DotSelection)
        {
            //fake cancel to close the old menu and open a new one
            UsingStone = true;
            DotSelection = true;
            MainGame.me.player.components.character.control_enabled = false;
            return;
        }

        if (string.Equals("leave", answer.ToLowerInvariant()))
        {
            //leave option for npcs
            UsingStone = false;
            DotSelection = false;
            CrossModFields.TalkingToNpc(false);
            MainGame.me.player.components.character.control_enabled = true;

            return;
        }

        //if (CrossModFields.TalkingToNpc) return;

        UsingStone = false;
        DotSelection = false;

        MainGame.me.player.components.character.control_enabled = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MultiAnswerGUI), nameof(MultiAnswerGUI.ShowAnswers), typeof(List<AnswerVisualData>), typeof(bool))]
    public static void MultiAnswerGUI_ShowAnswers(ref MultiAnswerGUI __instance)
    {
        if (Tools.PlayerDisabled() || Helpers.InTutorial() || __instance == null)
        {
            return;
        }

        MaGui = __instance;

        if (UsingStone && Plugin.IncreaseMenuAnimationSpeed.Value)
        {
            __instance.anim_delay /= 3f;
            __instance.anim_time /= 3f;
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Item), nameof(Item.GetGrayedCooldownPercent))]
    public static void Item_GetGrayedCooldownPercent(ref Item __instance, ref int __result)
    {
        if (__instance is not {id: "hearthstone"}) return;

        __result = 0;
    }


    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.Interact))]
    public static void WorldGameObject_Interact(WorldGameObject __instance)
    {
        if (__instance.custom_tag != CrossModFields.ModGerryTag) return;
        
        if (Tools.PlayerDisabled())
        {
            return;
        }

        if (Helpers.InTutorial()) return;
        
        var multiAnswer = __instance.gameObject.AddComponent<MultiAnswerGUI>();
        var gui = multiAnswer.gameObject.AddComponent<MultiAnswerOptionGUI>();
        multiAnswer.Init();
        gui.Init();
        var answers = new List<AnswerVisualData> {new() {id = "Please leave..."}};
        multiAnswer.ShowAnswers(answers, false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Flow_MultiAnswer), nameof(Flow_MultiAnswer.RegisterPorts))]
    public static void Flow_MultiAnswer_RegisterPorts(ref Flow_MultiAnswer __instance)
    {
        if (Tools.PlayerDisabled() || Helpers.InTutorial() || __instance == null || !UsingStone || !Plugin.EnableListExpansion.Value || DotSelection)
        {
            return;
        }

        __instance.answers.Insert(__instance.answers.Count - 1, @"...");
    }
}