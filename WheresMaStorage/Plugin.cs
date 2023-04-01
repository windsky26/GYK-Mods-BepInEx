using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace WheresMaStorage;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.WheresMaStorage";
    private const string PluginName = "Where's Ma' Storage!";
    private const string PluginVer = "0.0.1";

    internal static ManualLogSource Log { get; private set; }
    private static Harmony _harmony;

    private static ConfigEntry<bool> _modEnabled;
    internal static ConfigEntry<bool> ModifyInventorySize;
    internal static ConfigEntry<bool> EnableGraveItemStacking;
    internal static ConfigEntry<bool> EnablePenPaperInkStacking;
    internal static ConfigEntry<bool> EnableChiselStacking;
    internal static ConfigEntry<bool> EnableToolAndPrayerStacking;
    internal static ConfigEntry<bool> AllowHandToolDestroy;
    internal static ConfigEntry<bool> ModifyStackSize;
    internal static ConfigEntry<bool> Debug;
    internal static ConfigEntry<bool> SharedInventory;
    internal static ConfigEntry<bool> DontShowEmptyRowsInInventory;
    internal static ConfigEntry<bool> ShowUsedSpaceInTitles;
    internal static ConfigEntry<bool> DisableInventoryDimming;
    internal static ConfigEntry<bool> ShowWorldZoneInTitles;
    internal static ConfigEntry<bool> HideInvalidSelections;
    internal static ConfigEntry<bool> RemoveGapsBetweenSections;
    internal static ConfigEntry<bool> RemoveGapsBetweenSectionsVendor;
    internal static ConfigEntry<bool> ShowOnlyPersonalInventory;
    internal static ConfigEntry<int> AdditionalInventorySpace;
    internal static ConfigEntry<int> StackSizeForStackables;
    internal static ConfigEntry<bool> HideStockpileWidgets;
    internal static ConfigEntry<bool> HideTavernWidgets;
    internal static ConfigEntry<bool> HideSoulWidgets;
    internal static ConfigEntry<bool> HideWarehouseShopWidgets;
    internal static ConfigEntry<bool> CollectDropsOnGameLoad;

    private void Awake()
    {
        ModifyInventorySize = Config.Bind("Inventory", "Modify Inventory Size", true, new ConfigDescription("Enable or disable modifying the inventory size", null, new ConfigurationManagerAttributes {Order = 50}));
        EnableGraveItemStacking = Config.Bind("Item Stacking", "Enable Grave Item Stacking", false, new ConfigDescription("Allow grave items to stack", null, new ConfigurationManagerAttributes {Order = 49}));
        EnablePenPaperInkStacking = Config.Bind("Item Stacking", "Enable Pen Paper Ink Stacking", false, new ConfigDescription("Allow pen, paper, and ink items to stack", null, new ConfigurationManagerAttributes {Order = 48}));
        EnableChiselStacking = Config.Bind("Item Stacking", "Enable Chisel Stacking", false, new ConfigDescription("Allow chisel items to stack", null, new ConfigurationManagerAttributes {Order = 47}));
        EnableToolAndPrayerStacking = Config.Bind("Item Stacking", "Enable Tool And Prayer Stacking", true, new ConfigDescription("Allow tool and prayer items to stack", null, new ConfigurationManagerAttributes {Order = 46}));
        AllowHandToolDestroy = Config.Bind("Gameplay", "Allow Hand Tool Destroy", true, new ConfigDescription("Enable or disable destroying objects with hand tools", null, new ConfigurationManagerAttributes {Order = 45}));
        ModifyStackSize = Config.Bind("Inventory", "Modify Stack Size", true, new ConfigDescription("Enable or disable modifying the stack size of items", null, new ConfigurationManagerAttributes {Order = 44}));

        _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 43}));
        Debug = Config.Bind("General", "Debug", false, new ConfigDescription("Enable or disable debug mode for the plugin", null, new ConfigurationManagerAttributes {Order = 43}));
        SharedInventory = Config.Bind("Inventory", "Shared Inventory", true, new ConfigDescription("Enable or disable shared inventory between players", null, new ConfigurationManagerAttributes {Order = 42}));
        DontShowEmptyRowsInInventory = Config.Bind("Inventory", "Dont Show Empty Rows In Inventory", true, new ConfigDescription("Enable or disable displaying empty rows in the inventory", null, new ConfigurationManagerAttributes {Order = 41}));
        ShowUsedSpaceInTitles = Config.Bind("Inventory", "Show Used Space In Titles", true, new ConfigDescription("Enable or disable showing used space in inventory titles", null, new ConfigurationManagerAttributes {Order = 40}));
        DisableInventoryDimming = Config.Bind("Inventory", "Disable Inventory Dimming", true, new ConfigDescription("Enable or disable inventory dimming", null, new ConfigurationManagerAttributes {Order = 39}));
        ShowWorldZoneInTitles = Config.Bind("Inventory", "Show World Zone In Titles", true, new ConfigDescription("Enable or disable showing world zone information in inventory titles", null, new ConfigurationManagerAttributes {Order = 38}));
        HideInvalidSelections = Config.Bind("Inventory", "Hide Invalid Selections", true, new ConfigDescription("Enable or disable hiding invalid item selections in the inventory", null, new ConfigurationManagerAttributes {Order = 37}));

        RemoveGapsBetweenSections = Config.Bind("Inventory", "Remove Gaps Between Sections", true, new ConfigDescription("Enable or disable removing gaps between inventory sections", null, new ConfigurationManagerAttributes {Order = 36}));
        RemoveGapsBetweenSectionsVendor = Config.Bind("Inventory", "Remove Gaps Between Sections Vendor", true, new ConfigDescription("Enable or disable removing gaps between sections in the vendor inventory", null, new ConfigurationManagerAttributes {Order = 35}));
        ShowOnlyPersonalInventory = Config.Bind("Inventory", "Show Only Personal Inventory", true, new ConfigDescription("Enable or disable showing only personal inventory", null, new ConfigurationManagerAttributes {Order = 34}));
        AdditionalInventorySpace = Config.Bind("Inventory", "Additional Inventory Space", 20, new ConfigDescription("Set the number of additional inventory spaces", null, new ConfigurationManagerAttributes {Order = 33}));
        StackSizeForStackables = Config.Bind("Inventory", "Stack Size For Stackables", 999, new ConfigDescription("Set the maximum stack size for stackable items", new AcceptableValueRange<int>(1, 999), new ConfigurationManagerAttributes {Order = 32}));

        HideStockpileWidgets = Config.Bind("UI", "Hide Stockpile Widgets", true, new ConfigDescription("Enable or disable hiding stockpile widgets", null, new ConfigurationManagerAttributes {Order = 31}));
        HideTavernWidgets = Config.Bind("UI", "Hide Tavern Widgets", true, new ConfigDescription("Enable or disable hiding tavern widgets", null, new ConfigurationManagerAttributes {Order = 30}));
        HideSoulWidgets = Config.Bind("UI", "Hide Soul Widgets", true, new ConfigDescription("Enable or disable hiding soul widgets", null, new ConfigurationManagerAttributes {Order = 29}));
        HideWarehouseShopWidgets = Config.Bind("UI", "Hide Warehouse Shop Widgets", true, new ConfigDescription("Enable or disable hiding warehouse shop widgets", null, new ConfigurationManagerAttributes {Order = 28}));
        CollectDropsOnGameLoad = Config.Bind("Gameplay", "Collect Drops On Game Load", true, new ConfigDescription("Enable or disable collecting drops on game load", null, new ConfigurationManagerAttributes {Order = 27}));


        Fields.GameBalanceAlreadyRun = false;


        Fields.InvSize = 20 + AdditionalInventorySpace.Value;

        Log = Logger;
        _harmony = new Harmony(PluginGuid);
        if (_modEnabled.Value)
        {
            Actions.SpawnPlayer += Helpers.RunWmsTasks;
            Actions.GameBalanceLoad += Helpers.GameBalanceLoad;
            Log.LogMessage($"Applying patches for {PluginName}");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    private void OnEnable()
    {
        Log.LogInfo($"Plugin {PluginName} has been enabled!");
    }

    private void OnDisable()
    {
        Log.LogWarning($"Plugin {PluginName} has been disabled!");
    }
}