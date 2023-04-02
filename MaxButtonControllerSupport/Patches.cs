using System.Linq;
using HarmonyLib;
namespace MaxButtonControllerSupport;

[HarmonyPatch]
public partial class Plugin
{
    private static bool _craftGuiOpen;
    private static bool _itemCountGuiOpen;
    private static CraftItemGUI _craftItemGui;
    private static WorldGameObject _crafteryWgo;
    private static SmartSlider _slider;

    private static bool _unsafeInteraction;

    private static readonly string[] UnSafeCraftObjects =
    {
        "mf_crematorium_corp", "garden_builddesk", "tree_garden_builddesk", "mf_crematorium", "grave_ground",
        "tile_church_semicircle_2floors", "mf_grindstone_1", "zombie_garden_desk_1", "zombie_garden_desk_2", "zombie_garden_desk_3",
        "zombie_vineyard_desk_1", "zombie_vineyard_desk_2", "zombie_vineyard_desk_3", "graveyard_builddesk", "blockage_H_low", "blockage_V_low",
        "blockage_H_high", "blockage_V_high", "wood_obstacle_v", "refugee_camp_garden_bed", "refugee_camp_garden_bed_1", "refugee_camp_garden_bed_2",
        "refugee_camp_garden_bed_3"
    };

    private static readonly string[] UnSafeCraftZones =
    {
        "church"
    };

    private static readonly string[] UnSafePartials =
    {
        "blockage", "obstacle", "builddesk", "fix", "broken"
    };


    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftGUI), nameof(CraftGUI.Open))]
    public static void CraftGUI_Open()
    {
        _craftGuiOpen = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftGUI), nameof(CraftGUI.OnClosePressed))]
    public static void CraftGUI_OnClosePressed()
    {
        _craftGuiOpen = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftItemGUI), nameof(CraftItemGUI.OnOver))]
    public static void CraftItemGUI_OnOver()
    {
        _craftItemGui = CraftItemGUI.current_overed;
        _crafteryWgo = GUIElements.me.craft.GetCrafteryWGO();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemCountGUI), nameof(ItemCountGUI.Open))]
    public static void OpenPostfix(ref ItemCountGUI __instance)
    {
        _itemCountGuiOpen = true;
        _slider = __instance.transform.Find("window/Container/smart slider").GetComponent<SmartSlider>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemCountGUI), nameof(ItemCountGUI.OnPressedBack))]
    [HarmonyPatch(typeof(ItemCountGUI), nameof(ItemCountGUI.OnConfirm))]
    public static void ItemCountGUI_OnPressedBack()
    {
        _itemCountGuiOpen = false;
    }


    public static void WorldGameObject_Interact(WorldGameObject instance, WorldGameObject other)
    {
        if (UnSafeCraftZones.Contains(instance.GetMyWorldZoneId()) || UnSafePartials.Any(instance.obj_id.Contains) || UnSafeCraftObjects.Contains(instance.obj_id))
        {
            _unsafeInteraction = true;
        }
        else
        {
            _unsafeInteraction = false;
        }
    }
}