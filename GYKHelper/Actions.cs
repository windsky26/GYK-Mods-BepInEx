using System;
using System.Linq;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace GYKHelper;

[HarmonyPatch]
public static class Actions
{
    internal static readonly string[] SafeGerryTags =
    {
        "tavern_skull",
        "talking_skull",
        "crafting_skull",
        "crafting_skull_1",
        "crafting_skull_2",
        "crafting_skull_3",
        "crafting_skull_4",
        "crafting_skull_5",
        "crafting_skull_6",
        "crafting_skull_7",
        "crafting_skull_8",
        "crafting_skull_9",
        "crafting_skull_10",
        "talking_skull_in_tavern"
    };

    //public static Action GameStatusInGame;
    public static Action PlayerSpawnedIn;
    public static Action GameStatusInMenu;
    public static Action GameStatusUndefined;
    public static Action<MainGame> GameStartedPlaying;

    public static Action<GameBalance> GameBalanceLoad;
    public static Action<WorldGameObject> WorldGameObjectInteract;

    public static Action<WorldGameObject, WorldGameObject> WorldGameObjectInteractPrefix;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerComponent), nameof(PlayerComponent.SpawnPlayer), typeof(bool), typeof(Item))]
    public static void PlayerComponent_SpawnPlayer(bool is_local_player)
    {
        if (is_local_player)
        {
            Plugin.Log.LogWarning($"Player spawned in. Invoking PlayerSpawnedIn Action for attached mods.");
            PlayerSpawnedIn?.Invoke();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSave), nameof(GameSave.GlobalEventsCheck))]
    public static void GameSave_GlobalEventsCheck()
    {
        Plugin.Log.LogWarning($"Final load task complete. Game starting. Invoking GameStartedPlaying Action for attached mods.");
        GameStartedPlaying?.Invoke(MainGame.me);
    }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(PlatformSpecific), nameof(PlatformSpecific.SetGameStatus), typeof(GameEvents.GameStatus))]
    // public static void PlatformSpecific_SetGameStatus(GameEvents.GameStatus status)
    // {
    //     switch (status)
    //     {
    //         case GameEvents.GameStatus.InMenu:
    //             GameStatusInMenu?.Invoke();
    //             break;
    //         case GameEvents.GameStatus.InGame:
    //             GameStatusInGame?.Invoke();
    //             break;
    //         case GameEvents.GameStatus.Undefined:
    //             GameStatusUndefined?.Invoke();
    //             break;
    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(status), status, null);
    //     }
    // }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameBalance), nameof(GameBalance.LoadGameBalance))]
    private static void GameBalance_LoadGameBalance_Postfix()
    {
        Plugin.Log.LogWarning($"Game balance loaded. Invoking GameBalanceLoad Action for attached mods.");
        GameBalanceLoad?.Invoke(GameBalance.me);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.Interact))]
    private static void WorldGameObject_Interact_Postfix(ref WorldGameObject __instance)
    {
        Plugin.Log.LogWarning($"WGO interacted with (postfix). Invoking WorldGameObjectInteract Action for attached mods.");
        WorldGameObjectInteract?.Invoke(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.Interact))]
    private static void WorldGameObject_Interact_Prefix(ref WorldGameObject __instance, ref WorldGameObject other_obj)
    {
        Plugin.Log.LogWarning($"WGO interacted with (prefix). Invoking WorldGameObjectInteractPrefix Action for attached mods.");
        WorldGameObjectInteractPrefix?.Invoke(__instance, other_obj);
    }

    public static void WorldGameObject_Interact(WorldGameObject instance, WorldGameObject other_obj)
    {
        if (!MainGame.game_started || instance == null) return;

        Plugin.Log.LogWarning($"Object: {instance.obj_id}, CustomTag: {instance.custom_tag}, Location: {instance.pos3}, Zone:{instance.GetMyWorldZoneId()}");
        //Where's Ma Storage
        CrossModFields.PreviousWgoInteraction = CrossModFields.CurrentWgoInteraction;
        CrossModFields.CurrentWgoInteraction = instance;
        CrossModFields.IsVendor = instance.vendor != null;
        CrossModFields.IsCraft = other_obj.is_player && instance.obj_def.interaction_type != ObjectDefinition.InteractionType.Chest && instance.obj_def.has_craft;
        //Log($"IsCraft: {CrossModFields.IsCraft}",true);
        CrossModFields.IsChest = instance.obj_def.interaction_type == ObjectDefinition.InteractionType.Chest;
        CrossModFields.IsBarman = instance.obj_id.ToLowerInvariant().Contains("barman");
        CrossModFields.IsTavernCellarRack = instance.obj_id.ToLowerInvariant().Contains("tavern_cellar_rack");
        CrossModFields.IsRefugee = instance.obj_id.ToLowerInvariant().Contains("refugee");
        CrossModFields.IsWritersTable = instance.obj_id.ToLowerInvariant().Contains("writer");
        CrossModFields.IsSoulBox = instance.obj_id.ToLowerInvariant().Contains("soul_container");
        CrossModFields.IsChurchPulpit = instance.obj_id.ToLowerInvariant().Contains("pulpit");
        CrossModFields.IsMoneyLender = instance.obj_id.ToLowerInvariant().Contains("lender");

        if (instance.obj_def.inventory_size > 0)
        {
            if (instance.obj_id.Length <= 0)
            {
                instance.data.sub_name = "Unknown#" + instance.GetMyWorldZoneId();
            }
            else
            {
                instance.data.sub_name = instance.obj_id + "#" + instance.GetMyWorldZoneId();
            }
        }

        //Beam Me Up & Save Now
        CrossModFields.IsInDungeon = instance.obj_id.ToLowerInvariant().Contains("dungeon_enter");
        // Log($"[InDungeon]: {CrossModFields.IsInDungeon}");

        //I Build Where I Want
        if (instance.obj_def.interaction_type is not ObjectDefinition.InteractionType.None)
        {
            CrossModFields.CraftAnywhere = false;
        }

        //Beam Me Up Gerry
        CrossModFields.TalkingToNpc(instance.obj_def.IsNPC() && !instance.obj_id.Contains("zombie"));

        //Log($"[WorldGameObject.Interact]: Instance: {__instance.obj_id}, InstanceIsPlayer: {__instance.is_player},  Other: {other_obj.obj_id}, OtherIsPlayer: {other_obj.is_player}");
    }

    public static void CleanGerries(MainGame mainGame)
    {
        if (!MainGame.game_started) return;

        if (!Tools.TutorialDone()) return;
        //get all gerry objects + a few extras
        var otherGerrys = Object.FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id.ToLowerInvariant().Contains("skull")).ToList();

        //removes the extras - mainly the skull objects in the dungeon
        otherGerrys.RemoveAll(a => a.obj_id.ToLowerInvariant().StartsWith("skulls"));

        //log each gerry object found
        foreach (var g in otherGerrys.Where(g => g != null))
        {
            Plugin.Log.LogWarning($"AllGerries: Gerry: {g.obj_id}, CustomTag: {g.custom_tag}, POS: {g.pos3}, Location: {g.GetMyWorldZoneId()}");
        }

        //find gerrys that match any gerrys made by mods
        var gerrys = WorldMap.GetWorldGameObjectsByCustomTag(CrossModFields.ModGerryTag, false);

        //log each mod_gerry object found
        foreach (var g in gerrys.Where(g => g != null))
        {
            Plugin.Log.LogWarning($"AllModGerries: Gerry: {g.obj_id}, CustomTag: {g.custom_tag}, POS: {g.pos3}, Location: {g.GetMyWorldZoneId()}");
        }

        //remove each gerry, ensuring no gerrys on the safeTag list are removed
        foreach (var gerry in gerrys.Where(gerry => gerry != null))
        {
            if (SafeGerryTags.Contains(gerry.custom_tag)) continue;
            Plugin.Log.LogWarning($"Destroyed Gerry: {gerry.obj_id}, CustomTag: {gerry.custom_tag}, POS: {gerry.pos3}, Location: {gerry.GetMyWorldZoneId()}");
            gerry.DestroyMe();
        }

        //remove each mod gerry, ensuring no gerrys on the safeTag list are removed
        foreach (var gerry in otherGerrys.Where(gerry => gerry != null))
        {
            if (SafeGerryTags.Contains(gerry.custom_tag)) continue;
            Plugin.Log.LogWarning($"Destroyed Gerry: {gerry.obj_id}, CustomTag: {gerry.custom_tag}, POS: {gerry.pos3}, Location: {gerry.GetMyWorldZoneId()}");
            gerry.DestroyMe();
        }


        CrossModFields.TimeOfDayFloat = TimeOfDay.me.GetTimeK();
    }
}