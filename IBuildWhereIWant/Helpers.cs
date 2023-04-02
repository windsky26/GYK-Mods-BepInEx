using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GYKHelper;
using UnityEngine;

namespace IBuildWhereIWant;

public partial class Plugin
{
    private static WorldGameObject _buildDesk;
    private static WorldGameObject _buildDeskClone;

    private static CraftsInventory _craftsInventory;

    private static Dictionary<string, string> _craftDictionary;
    private const string Zone = "mf_wood";

    private const string BuildDesk = "buildanywhere_desk";

    private static int _unlockedCraftListCount;

    private static string GetLocalizedString(string content)
    {
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        return content;
    }

    private static void OpenCraftAnywhere()
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains("refugee")) return;
        if (MainGame.me.player.GetParamInt("in_tutorial") == 1 &&
            MainGame.me.player.GetParamInt("tut_shown_tut_1") == 0)
        {
            MainGame.me.player.Say("cant_do_it_now");
            return;
        }

        _craftsInventory ??= new CraftsInventory();

        _craftDictionary ??= new Dictionary<string, string>();

        if (_buildDesk == null)
        {
            _buildDesk = Object.FindObjectsOfType<WorldGameObject>(true)
                .FirstOrDefault(x => string.Equals(x.obj_id, "mf_wood_builddesk"));
        }

        WriteLog(
            _buildDesk != null
                ? $"Found Build Desk: {_buildDesk}, Zone: {_buildDesk.GetMyWorldZone()}"
                : "Unable to locate a build desk.",true);

        if (_buildDeskClone != null)
        {
            Object.Destroy(_buildDeskClone);
        }

        _buildDeskClone = GameObject.Instantiate(_buildDesk);

        _buildDeskClone.name = BuildDesk;

        var needsRefresh = false;
        if (MainGame.me.save.unlocked_crafts.Count > _unlockedCraftListCount)
        {
            _unlockedCraftListCount = MainGame.me.save.unlocked_crafts.Count;
            needsRefresh = true;
        }

        if (needsRefresh)
        {
            foreach (var objectCraftDefinition in GameBalance.me.craft_obj_data.Where(x =>
                             x.build_type == ObjectCraftDefinition.BuildType.Put)
                         .Where(a => a.icon.Length > 0)
                         .Where(b => !b.id.Contains("refugee"))
                         .Where(d => MainGame.me.save.IsCraftVisible(d))
                         .Where(e => !_craftDictionary.TryGetValue(GJL.L(e.GetNameNonLocalized()), out _)))

            {
                var itemName = GJL.L(objectCraftDefinition.GetNameNonLocalized());
                _craftDictionary.Add(itemName, objectCraftDefinition.id);
            }


            var craftList = _craftDictionary.ToList();
            craftList.Sort((pair1, pair2) => string.CompareOrdinal(pair1.Key, pair2.Key));

            craftList.ForEach(craft => { _craftsInventory.AddCraft(craft.Value); });
        }

        CrossModFields.CraftAnywhere = true;

        BuildModeLogics.last_build_desk = _buildDeskClone;

        MainGame.me.build_mode_logics.SetCurrentBuildZone(_buildDeskClone.obj_def.zone_id, "");
        GUIElements.me.craft.OpenAsBuild(_buildDeskClone, _craftsInventory);
        MainGame.paused = false;
    }

    private static void WriteLog(string message, bool error = false)
    {
        if (error)
        {
            Plugin.Log.LogError($"{message}");
        }
        else
        {
            if (Plugin.Debug.Value)
            {
                Plugin.Log.LogInfo($"{message}");
            }
        }
    }
}