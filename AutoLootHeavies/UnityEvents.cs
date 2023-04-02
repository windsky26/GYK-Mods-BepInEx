using AutoLootHeavies.lang;
using GYKHelper;

namespace AutoLootHeavies;

public partial class Plugin
{
    private static bool _initialFullUpdate;
    
    private void Update()
    {
        if (!MainGame.game_started) return;

        if (!_initialFullUpdate)
        {
            _initialFullUpdate = true;
            MainGame.me.StartCoroutine(RunFullUpdate());
        }

        CheckKeybinds();
    }
    
    private static void CheckKeybinds()
    {
        if (ToggleTeleportToDumpSiteKeybind.Value.IsUp())
        {
            TeleportToDumpSiteWhenAllStockPilesFull.Value = !TeleportToDumpSiteWhenAllStockPilesFull.Value;
            Tools.ShowMessage(TeleportToDumpSiteWhenAllStockPilesFull.Value ? strings.TeleOn : strings.TeleOff, MainGame.me.player_pos);
        }

        if (SetTimberLocationKeybind.Value.IsUp())
        {
            DesignatedTimberLocation.Value = MainGame.me.player_pos;
            Tools.ShowMessage(strings.DumpTimber, DesignatedTimberLocation.Value);
        }

        if (SetOreLocationKeybind.Value.IsUp())
        {
            DesignatedOreLocation.Value = MainGame.me.player_pos;
            Tools.ShowMessage(strings.DumpOre, DesignatedOreLocation.Value);
        }

        if (SetStoneLocationKeybind.Value.IsUp())
        {
            DesignatedStoneLocation.Value = MainGame.me.player_pos;
            Tools.ShowMessage(strings.DumpStone, DesignatedStoneLocation.Value);
        }
    }
}