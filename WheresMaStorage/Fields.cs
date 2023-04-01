using System.Collections.Generic;

namespace WheresMaStorage;

public static class Fields
{
    internal const string Chest = "chest";
    internal const string Gerry = "gerry";
    internal const float LogGap = 3f;
    internal const string Multi = "multi";
    internal const string NpcBarman = "npc_tavern_barman";
    internal const string Player = "player";

    internal const string Storage = "storage";
    internal const string Tavern = "tavern";
    internal const string Vendor = "vendor";
    internal const string Writer = "writer";
    internal const string Soul = "soul_container";
    internal const string Bag = "bag";

    internal const string ShippingBoxTag = "shipping_box";
    internal static bool DebugMessageShown;

    internal static readonly string[] AlwaysHidePartials =
    {
       "refugee_camp_well", "refugee_camp_tent", "pump", "pallet", "refugee_camp_well_2"
    };

    internal static readonly string[] ChiselItems =
    {
        "chisel"
    };

    internal static readonly ItemDefinition.ItemType[] GraveItems =
    {
        ItemDefinition.ItemType.GraveStone, ItemDefinition.ItemType.GraveFence, ItemDefinition.ItemType.GraveCover,
        ItemDefinition.ItemType.GraveStoneReq, ItemDefinition.ItemType.GraveFenceReq, ItemDefinition.ItemType.GraveCoverReq
    };

    internal static readonly string[] PenPaperInkItems =
    {
        "book", "chapter", "ink", "pen"
    };


    internal static readonly string[] StockpileWidgetsPartials =
    {
        "mf_stones", "mf_ore", "mf_timber"
    };

    internal static readonly ItemDefinition.ItemType[] ToolItems =
    {
        ItemDefinition.ItemType.Axe, ItemDefinition.ItemType.Shovel, ItemDefinition.ItemType.Hammer,
        ItemDefinition.ItemType.Pickaxe, ItemDefinition.ItemType.FishingRod, ItemDefinition.ItemType.BodyArmor,
        ItemDefinition.ItemType.HeadArmor, ItemDefinition.ItemType.Sword, ItemDefinition.ItemType.Preach
    };

    internal static bool GameBalanceAlreadyRun;
    internal static bool GratitudeCraft;
    internal static int InvSize;

    internal static MultiInventory Mi = new();
    internal static MultiInventory RefugeeMi = new();
    internal static float TimeSix;
    internal static float TimeEight;
    internal static float TimeNine;
    internal static bool UsingBag;
    internal static bool ZombieWorker;
    internal static List<Item> OldDrops = new();

    internal static readonly string[] ExcludeTheseWildernessInventories =
    {
        "vendor", "npc", "donkey", "zombie", "worker", "refugee", "pile", "carrot", "cooking", "guard", "working", "obj_church"
    };

    internal static readonly Dictionary<WorldGameObject, MultiInventory> WildernessMultiInventories = new();
    internal static readonly List<Inventory> WildernessInventories = new();

    internal static bool InvsLoaded;
    public static bool DropsCleaned;
}