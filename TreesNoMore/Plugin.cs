using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using Newtonsoft.Json;
using NodeCanvas.Framework;
using UnityEngine;

namespace TreesNoMore
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.treesnomore";
        private const string PluginName = "Trees, No More!";
        private const string PluginVer = "2.5.3";
        internal static ManualLogSource Log { get; set; }
        private static Harmony _harmony;
        internal static List<Tree> Trees = new();
        private static ConfigEntry<bool> _modEnabled;
        internal static ConfigEntry<int> TreeSearchDistance;
        internal static ConfigEntry<bool> InstantStumpRemoval;
        private static string _filePath;

        private void Awake()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "trees.json");
            Log = Logger;
            _harmony = new Harmony(PluginGuid);
            InitConfiguration();
            ApplyPatches(this, null);
            LoadTrees();
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 3}));
            _modEnabled.SettingChanged += ApplyPatches;

            TreeSearchDistance = Config.Bind("2. Trees", "Tree Search Distance", 2, new ConfigDescription("The allowable distance to check if a tree shouldn't exist on load. The default value of 2 seems to work well. Setting this too large may cause trees surrounding the intended tree to also be removed.", null, new ConfigurationManagerAttributes {Order = 2}));

            InstantStumpRemoval = Config.Bind("2. Stumps", "Instant Stump Removal", true, new ConfigDescription("Instantly remove stumps when chopping down trees.", null, new ConfigurationManagerAttributes {Order = 1}));

            Config.Bind("3. Reset", "Reset Trees", true, new ConfigDescription("All felled trees will be restored on restart.", null, new ConfigurationManagerAttributes {HideDefaultButton = true, Order = 0, CustomDrawer = RestoreTrees}));
        }


        private static bool _showConfirmationDialog = false;

        private static void RestoreTrees(ConfigEntryBase entry)
        {
            if (_showConfirmationDialog)
            {
                Tools.DisplayConfirmationDialog("Are you sure you want to reset all trees?", () =>
                {
                    Log.LogWarning("All felled trees will be restored on restart.");
                    Trees.Clear();
                    File.Delete(_filePath);
                    _showConfirmationDialog = false;
                }, () => _showConfirmationDialog = false);
                
            }
            else
            {
                var button = GUILayout.Button("Reset Trees", GUILayout.ExpandWidth(true));
                if (button)
                {
                    _showConfirmationDialog = true;
                }
            }
        }
        
        private static void LoadTrees()
        {
            if (!File.Exists(_filePath)) return;
            var jsonString = File.ReadAllText(_filePath);
            Trees = JsonConvert.DeserializeObject<List<Tree>>(jsonString);
            Log.LogWarning($"Loaded {Trees.Count} trees from {_filePath}");
        }

        internal static void SaveTrees()
        {
            //remove near enough duplicates from Trees list
            var count = Trees.RemoveAll(x => Trees.FindAll(y => y.location == x.location).Count > 1);
            Log.LogWarning($"Removed {count} duplicate trees");
            var jsonString = JsonConvert.SerializeObject(Trees, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });

            File.WriteAllText(_filePath, jsonString);
            Log.LogWarning($"Saved {Trees.Count} trees to {_filePath}");
        }

        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Actions.GameStartedPlaying += Patches.DestroyTress;
                Actions.ReturnToMenu += SaveTrees;
                LoadTrees();
                Log.LogInfo($"Applying patches for {PluginName}");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Actions.GameStartedPlaying -= Patches.DestroyTress;
                Actions.ReturnToMenu -= SaveTrees;
                Log.LogInfo($"Removing patches for {PluginName}");
                _harmony.UnpatchSelf();
            }
        }

        private void OnDisable()
        {
            SaveTrees();
        }

        private void OnDestroy()
        {
            SaveTrees();
        }
    }
}