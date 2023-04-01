using BepInEx.Configuration;
using UnityEngine;

namespace AutoLootHeavies;

public partial class Plugin
{
    internal static ConfigEntry<KeyCode> ReloadConfigKeyBind;
    internal static ConfigEntry<bool> TeleportToDumpSiteWhenAllStockPilesFull;
    internal static ConfigEntry<Vector3> DesignatedTimberLocation;
    internal static ConfigEntry<Vector3> DesignatedOreLocation = null!; 
    internal static ConfigEntry<Vector3> DesignatedStoneLocation = null!; 
    internal static ConfigEntry<bool> DisableImmersionMode = null!; 
    internal static ConfigEntry<bool> Debug = null!; 
    internal static ConfigEntry<KeyCode> ToggleTeleportToDumpSiteKeybind = null!; 
    internal static ConfigEntry<KeyCode> SetTimberLocationKeybind = null!; 
    internal static ConfigEntry<KeyCode> SetOreLocationKeybind = null!; 
    internal static ConfigEntry<KeyCode> SetStoneLocationKeybind = null!; 
   
}