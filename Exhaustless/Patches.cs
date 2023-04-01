﻿using System.Linq;
using System.Threading;
using Exhaustless.lang;
using GYKHelper;
using HarmonyLib;

namespace Exhaustless;

[HarmonyPatch]
public static class Patches
{
    private static readonly ItemDefinition.ItemType[] ToolItems =
    {
        ItemDefinition.ItemType.Axe, ItemDefinition.ItemType.Shovel, ItemDefinition.ItemType.Hammer,
        ItemDefinition.ItemType.Pickaxe, ItemDefinition.ItemType.FishingRod, ItemDefinition.ItemType.BodyArmor,
        ItemDefinition.ItemType.HeadArmor, ItemDefinition.ItemType.Sword,
    };

    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuffsLogics), nameof(BuffsLogics.AddBuff))]
    public static void BuffsLogics_AddBuff(ref string buff_id)
    {
        if (!Plugin.YawnMessage.Value) return;
        if (buff_id.Equals("buff_tired"))
            Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        MainGame.me.player.Say(strings.Yawn, null, null,
            SpeechBubbleGUI.SpeechBubbleType.Think, SmartSpeechEngine.VoiceID.None, true);
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.TrySpendPlayerGratitudePoints))]
    public static void CraftComponent_TrySpendPlayerGratitudePoints(ref float value)
    {
        if (Plugin.SpendHalfGratitude.Value) value /= 2f;
    }

    //TODO: Attach this to the Action
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameBalance), nameof(GameBalance.LoadGameBalance))]
    public static void GameBalance_LoadGameBalance()
    {
        if (!Plugin.MakeToolsLastLonger.Value) return;
        foreach (var itemDef in GameBalance.me.items_data.Where(a => ToolItems.Contains(a.type)))
        {
            if (itemDef.durability_decrease_on_use)
            {
                itemDef.durability_decrease_on_use_speed = 0.005f;
            }
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainGame), nameof(MainGame.OnEquippedToolBroken))]
    public static void MainGame_OnEquippedToolBroken()
    {
        if (!Plugin.AutoEquipNewTool.Value) return;
        var equippedTool = MainGame.me.player.GetEquippedTool();
        var save = MainGame.me.save;
        var playerInv = save.GetSavedPlayerInventory();

        foreach (var item in playerInv.inventory.Where(item =>
                     item.definition.type == equippedTool.definition.type))
        {
            if (item.durability_state is not (Item.DurabilityState.Full or Item.DurabilityState.Used))
                continue;
            MainGame.me.player.EquipItem(item, -1, playerInv.is_bag ? playerInv : null);
            MainGame.me.player.Say(
                $"{strings.LuckyHadAnotherPartOne} {item.definition.GetItemName()} {strings.LuckyHadAnotherPartTwo}",
                null, false,
                SpeechBubbleGUI.SpeechBubbleType.Think,
                SmartSpeechEngine.VoiceID.None, true);
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerComponent), nameof(PlayerComponent.SpendSanity))]
    public static void PlayerComponent_SpendSanity(ref float need_sanity)
    {
        if (Plugin.SpendHalfSanity.Value) need_sanity /= 2f;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerComponent), nameof(PlayerComponent.TrySpendEnergy))]
    public static void PlayerComponent_TrySpendEnergy(ref float need_energy)
    {
        if (Plugin.SpendHalfEnergy.Value) need_energy /= 2f;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(SleepGUI), nameof(SleepGUI.Update))]
    public static void SleepGUI_Update()
    {
        if (!Plugin.SpeedUpSleep.Value) return;
        MainGame.me.player.energy += 0.25f;
        MainGame.me.player.hp += 0.25f;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaitingGUI), nameof(WaitingGUI.Update))]
    public static void WaitingGUI_Update_Postfix(WaitingGUI __instance)
    {
        if (!Plugin.AutoWakeFromMeditation.Value) return;
        var save = MainGame.me.save;
        if (MainGame.me.player.energy.EqualsOrMore(save.max_hp) &&
            MainGame.me.player.hp.EqualsOrMore(save.max_energy))
            __instance.StopWaiting();
            // typeof(WaitingGUI).GetMethod("StopWaiting", AccessTools.all)
            //     ?.Invoke(__instance, null);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WaitingGUI), nameof(WaitingGUI.Update))]
    public static void WaitingGUI_Update_Prefix()
    {
        if (!Plugin.SpeedUpMeditation.Value) return;
        MainGame.me.player.energy += 0.25f;
        MainGame.me.player.hp += 0.25f;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.EquipItem))]
    public static void WorldGameObject_EquipItem(ref Item item)
    {
        if (!Plugin.MakeToolsLastLonger.Value) return;
        if (! ToolItems.Contains(item.definition.type)) return;
        if (item.definition.durability_decrease_on_use)
        {
            item.definition.durability_decrease_on_use_speed = 0.005f;
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.GetParam))]
    private static void WorldGameObject_GetParam(ref WorldGameObject __instance, ref string param_name,
        ref float __result)
    {
        if (!param_name.Contains("tiredness")) return;
        var tiredness = __instance._data.GetParam("tiredness");

        var newTirednessLimit = (float) Plugin.EnergySpendBeforeSleepDebuff.Value;
        __result = tiredness < newTirednessLimit ? 250 : 350;
    }
}