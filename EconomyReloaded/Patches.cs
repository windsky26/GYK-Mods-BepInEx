using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace EconomyReloaded;

public static class Patches
{
    private static bool _gameBalanceAlreadyRun;
    private static readonly Dictionary<string, bool> BackedUpIsStaticCost = new();
    private static readonly HashSet<string> StaticCostItemIds = new();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Trading), nameof(Trading.GetSingleItemCostInTraderInventory), typeof(Item), typeof(int))]
    public static void Trading_GetSingleItemCostInTraderInventory(ref float __result, Item item)
    {
        if (!Plugin.OldSchoolModeConfig.Value) return;
        if (!Plugin.DisableInflationConfig.Value) return;
        if (__result != 0.0)
        {
            __result = item.definition.base_price;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Trading), nameof(Trading.GetSingleItemCostInPlayerInventory), typeof(Item), typeof(int))]
    public static void Trading_GetSingleItemCostInPlayerInventory(ref float __result, Item item)
    {
        if (!Plugin.OldSchoolModeConfig.Value) return;
        if (!Plugin.DisableDeflationConfig.Value) return;
        if (__result != 0.0)
        {
            __result = item.definition.base_price;
        }
    }

    public static void RestoreIsStaticCost()
    {
        foreach (var itemDef in GameBalance.me.items_data.Where(itemDef => StaticCostItemIds.Contains(itemDef.id)))
        {
            itemDef.is_static_cost = BackedUpIsStaticCost[itemDef.id];
        }
    }
    
    public static void GameBalance_LoadGameBalance(GameBalance obj)
    {
        if (Plugin.OldSchoolModeConfig.Value || _gameBalanceAlreadyRun) return;
        _gameBalanceAlreadyRun = true;

        var itemsWithBasePrice = GameBalance.me.items_data.Where(itemDef => itemDef.base_price > 0).ToList();
        BackedUpIsStaticCost.Clear();
        StaticCostItemIds.Clear();

        foreach (var itemDef in itemsWithBasePrice)
        {
            BackedUpIsStaticCost[itemDef.id] = itemDef.is_static_cost;
            StaticCostItemIds.Add(itemDef.id);
            itemDef.is_static_cost = true;
        }
    }
}