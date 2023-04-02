using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace GerrysJunkTrunk
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.gerrysjunktrunk";
        private const string PluginName = "Gerry's Junk Trunk";
        private const string PluginVer = "1.8.6";


        
        internal static ConfigEntry<bool> Debug;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        internal static ConfigEntry<bool> ShowSoldMessagesOnPlayer;
        internal static ConfigEntry<bool> DisableSoldMessageWhenNoSale;
        internal static ConfigEntry<bool> EnableGerry;
        internal static ConfigEntry<bool> ShowSummaryMessage;
        internal static ConfigEntry<bool> ShowItemPriceTooltips;
        internal static ConfigEntry<bool> ShowKnownVendorCount;

        internal static ConfigEntry<bool> InternalShippingBoxBuilt;
        internal static ConfigEntry<bool> InternalShowIntroMessage;


        private void Awake()
        {
            _modEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {CustomDrawer = ToggleMod}));
            Debug = Config.Bind("Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 498}));

            InternalShippingBoxBuilt = Config.Bind("Internal (Dont Touch)", "Shipping Box Built", false, new ConfigDescription("Internal use.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 497}));
            InternalShowIntroMessage = Config.Bind("Internal (Dont Touch)", "Show Intro Message", false, new ConfigDescription("Internal use.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 496}));

            ShowSoldMessagesOnPlayer = Config.Bind("Messages", "Show Sold Messages On Player", true, new ConfigDescription("Display messages on the player when items are sold.", null, new ConfigurationManagerAttributes {Order = 500}));

            DisableSoldMessageWhenNoSale = Config.Bind("Messages", "Disable Sold Message When No Sale", false, new ConfigDescription("Disable the sold message when there is no sale.", null, new ConfigurationManagerAttributes {Order = 499}));

            EnableGerry = Config.Bind("General", "Enable Gerry", true, new ConfigDescription("Enable or disable Gerry in the game.", null, new ConfigurationManagerAttributes {Order = 498}));

            ShowSummaryMessage = Config.Bind("UI", "Show Summary", true, new ConfigDescription("Show a summary of transactions and other relevant information.", null, new ConfigurationManagerAttributes {Order = 497}));

            ShowItemPriceTooltips = Config.Bind("UI", "Show Item Price Tooltips", true, new ConfigDescription("Display tooltips with item prices in the user interface.", null, new ConfigurationManagerAttributes {Order = 496}));

            ShowKnownVendorCount = Config.Bind("UI", "Show Known Vendor Count", true, new ConfigDescription("Show the count of known vendors in the user interface.", null, new ConfigurationManagerAttributes {Order = 495}));

            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            if (_modEnabled.Value)
            {
                Actions.WorldGameObjectInteractPrefix += WorldGameObject_Interact;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        private static void ToggleMod(ConfigEntryBase entry)
        {
            var ticked = GUILayout.Toggle(_modEnabled.Value, "Enabled");

            if (ticked == _modEnabled.Value) return;
            _modEnabled.Value = ticked;

            if (ticked)
            {
                Actions.WorldGameObjectInteractPrefix += WorldGameObject_Interact;
                Log.LogWarning($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.WorldGameObjectInteractPrefix -= WorldGameObject_Interact;
                Log.LogWarning($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }

        private void OnEnable()
        {
            Log.LogInfo($"Plugin {PluginName} has been enabled!");
        }

        private void OnDisable()
        {
            Log.LogError($"Plugin {PluginName} has been disabled!");
        }
    }
}