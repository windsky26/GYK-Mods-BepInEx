using UnityEngine;
using HarmonyLib;

namespace RegenerationReloaded;

[HarmonyPatch]
public static class Patches
{
    internal static float Delay;
    private static float _energyRegen;
    private static float _lifeRegen;
    private static bool _showRegenUpdates;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerComponent), nameof(PlayerComponent.Update))]
    public static void PlayerComponent_Update()
    {
        _energyRegen = Mathf.Abs(Plugin.EnergyRegen.Value);
        _lifeRegen = Mathf.Abs(Plugin.LifeRegen.Value);
        var player = MainGame.me.player;
        var save = MainGame.me.save;
        _showRegenUpdates = Plugin.ShowRegenUpdates.Value;
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
                player.energy += _energyRegen;
                if (_showRegenUpdates && player.energy < save.max_energy)
                    EffectBubblesManager.ShowStackedEnergy(player, _energyRegen);
                break;
            case var _ when player.hp < save.max_hp:
                player.hp += _lifeRegen;
                if (_showRegenUpdates && player.hp < save.max_hp)
                    EffectBubblesManager.ShowStackedHP(player, _lifeRegen);
                break;
        }
    }
}