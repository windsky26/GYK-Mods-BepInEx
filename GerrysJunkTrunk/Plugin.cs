using System;
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


        private static ConfigEntry<bool> _debug;
        private static ManualLogSource Log { get;  set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;

        private static ConfigEntry<bool> _showSoldMessagesOnPlayer;
        private static ConfigEntry<bool> _disableSoldMessageWhenNoSale;
        private static ConfigEntry<bool> _enableGerry;
        private static ConfigEntry<bool> _showSummaryMessage;
        private static ConfigEntry<bool> _showItemPriceTooltips;
        private static ConfigEntry<bool> _showKnownVendorCount;

        private static ConfigEntry<bool> _internalShippingBoxBuilt;
        private static ConfigEntry<bool> _internalShowIntroMessage;


        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitInternalConfiguration();
            InitConfiguration();
            ApplyPatches(this, null);
        }


        private void InitInternalConfiguration()
        {
            _internalShippingBoxBuilt = Config.Bind("Internal (Dont Touch)", "Shipping Box Built", false, new ConfigDescription("Internal use.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 497}));
            _internalShowIntroMessage = Config.Bind("Internal (Dont Touch)", "Show Intro Message", false, new ConfigDescription("Internal use.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 496}));
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 7}));
            _modEnabled.SettingChanged += ApplyPatches;

            _enableGerry = Config.Bind("2. Gerry", "Gerry", true, new ConfigDescription("Toggle Gerry", null, new ConfigurationManagerAttributes {Order = 6}));

            _showSoldMessagesOnPlayer = Config.Bind("3. Messages", "Show Sold Messages On Player", true, new ConfigDescription("Display messages on the player when items are sold", null, new ConfigurationManagerAttributes {Order = 5}));

            _disableSoldMessageWhenNoSale = Config.Bind("3. Messages", "Show Sold Message When No Sale", false, new ConfigDescription("Disable the sold message when there is no sale", null, new ConfigurationManagerAttributes {Order = 4}));

            _showSummaryMessage = Config.Bind("4. UI", "Show Summary", false, new ConfigDescription("Display a summary of transactions and other relevant information", null, new ConfigurationManagerAttributes {Order = 3}));

            _showItemPriceTooltips = Config.Bind("4. UI", "Show Item Price Tooltips", true, new ConfigDescription("Display tooltips with item prices in the user interface", null, new ConfigurationManagerAttributes {Order = 2}));

            _showKnownVendorCount = Config.Bind("4. UI", "Show Known Vendor Count", false, new ConfigDescription("Display the count of known vendors in the user interface", null, new ConfigurationManagerAttributes {Order = 1}));
            _debug = Config.Bind("5. Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 0}));
        }


        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Actions.WorldGameObjectInteractPrefix += WorldGameObject_Interact;
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.WorldGameObjectInteractPrefix -= WorldGameObject_Interact;
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }
    }
}