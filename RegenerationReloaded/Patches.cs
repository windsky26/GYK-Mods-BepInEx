using UnityEngine;
using HarmonyLib;

namespace RegenerationReloaded;

[HarmonyPatch]
public static class Patches
{
    internal static float Delay { get; set; }
    private static float EnergyRegen { get; set; }
    private static float LifeRegen { get; set; }
    private static bool ShowRegenUpdates { get; set; }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerComponent), nameof(PlayerComponent.Update))]
    public static void PlayerComponent_Update()
    {
        EnergyRegen = Mathf.Abs(Plugin.EnergyRegen.Value);
        LifeRegen = Mathf.Abs(Plugin.LifeRegen.Value);
        var player = MainGame.me.player;
        var save = MainGame.me.save;
        ShowRegenUpdates = Plugin.ShowRegenUpdates.Value;
        var regenDelay = Plugin.RegenDelay.Value;

        if (Delay > 0f)
        {
            Delay -= Time.deltaTime;
            return;
        }

        Delay = regenDelay;

        switch (true)
        {
            case var _ when player.energy < save.max_energy:
                player.energy += EnergyRegen;
                if (ShowRegenUpdates && player.energy < save.max_energy)
                    EffectBubblesManager.ShowStackedEnergy(player, EnergyRegen);
                break;
            case var _ when player.hp < save.max_hp:
                player.hp += LifeRegen;
                if (ShowRegenUpdates && player.hp < save.max_hp)
                    EffectBubblesManager.ShowStackedHP(player, LifeRegen);
                break;
        }
    }
}