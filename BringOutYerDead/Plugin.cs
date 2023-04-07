﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace BringOutYerDead
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.bringoutyerdead";
        private const string PluginName = "Bring Out Yer Dead!";
        private const string PluginVer = "0.1.6";

        internal static ConfigEntry<bool> Debug;
        internal static ManualLogSource Log { get; private set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _modEnabled;
        private static ConfigEntry<bool> _morningDelivery;
        private static ConfigEntry<bool> _dayDelivery;
        private static ConfigEntry<bool> _nightDelivery;
        private static ConfigEntry<bool> _eveningDelivery;
        internal static ConfigEntry<float> DonkeySpeed;

        internal static ConfigEntry<bool> InternalMorningDelivery;
        internal static ConfigEntry<bool> InternalDayDelivery;
        internal static ConfigEntry<bool> InternalEveningDelivery;
        internal static ConfigEntry<bool> InternalNightDelivery;
        internal static ConfigEntry<bool> InternalDonkeySpawned;
        private static ConfigEntry<bool> _internalTutMessageShown;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            InitInternalConfiguration();
            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 7}));
            _modEnabled.SettingChanged += ApplyPatches;


            _morningDelivery = Config.Bind("2. Delivery Times", "Morning Delivery", true, new ConfigDescription("Enable deliveries during the morning hours", null, new ConfigurationManagerAttributes {Order = 6}));
            _dayDelivery = Config.Bind("2. Delivery Times", "Day Delivery", false, new ConfigDescription("Enable deliveries during the daytime hours", null, new ConfigurationManagerAttributes {Order = 5}));
            _nightDelivery = Config.Bind("2. Delivery Times", "Night Delivery", false, new ConfigDescription("Enable deliveries during the nighttime hours", null, new ConfigurationManagerAttributes {Order = 4}));
            _eveningDelivery = Config.Bind("2. Delivery Times", "Evening Delivery", true, new ConfigDescription("Enable deliveries during the evening hours", null, new ConfigurationManagerAttributes {Order = 3}));

            DonkeySpeed = Config.Bind("3. Donkey Settings", "Donkey Speed", 2f, new ConfigDescription("Adjust the donkey's speed for deliveries (minimum value is 2)", new AcceptableValueRange<float>(2f, 20f), new ConfigurationManagerAttributes {Order = 2}));
            Debug = Config.Bind("4. Advanced", "Debug Logging", false, new ConfigDescription("Enable detailed logging for debugging purposes", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 1}));
        }


        private void InitInternalConfiguration()
        {
            InternalMorningDelivery = Config.Bind("Internal (Dont Touch)", "Morning Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 6}));
            InternalDayDelivery = Config.Bind("Internal (Dont Touch)", "Day Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 5}));
            InternalEveningDelivery = Config.Bind("Internal (Dont Touch)", "Evening Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 4}));
            InternalNightDelivery = Config.Bind("Internal (Dont Touch)", "Night Delivery Done", false, new ConfigDescription("Internal use. Used for tracking a days delivery state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 3}));
            InternalDonkeySpawned = Config.Bind("Internal (Dont Touch)", "Donkey Spawned Done", false, new ConfigDescription("Internal use. Used for tracking donkey spawn state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 2}));
            _internalTutMessageShown = Config.Bind("Internal (Dont Touch)", "Tut Message Shown", false, new ConfigDescription("Internal use. Used for tracking tutorial message state.", null, new ConfigurationManagerAttributes {Browsable = false, HideDefaultButton = true, IsAdvanced = true, ReadOnly = true, Order = 1}));
        }

        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }
    }
}