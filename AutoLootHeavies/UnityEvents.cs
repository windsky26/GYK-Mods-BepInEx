using AutoLootHeavies.lang;
using BepInEx;
using GYKHelper;


namespace AutoLootHeavies
{
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
            if (_setTimberLocationKeybind.Value.IsUp())
            {
                _designatedTimberLocation.Value = MainGame.me.player_pos;
                Tools.ShowMessage(strings.DumpTimber, _designatedTimberLocation.Value);
            }

            if (_setOreLocationKeybind.Value.IsUp())
            {
                _designatedOreLocation.Value = MainGame.me.player_pos;
                Tools.ShowMessage(strings.DumpOre, _designatedOreLocation.Value);
            }

            if (_setStoneLocationKeybind.Value.IsUp())
            {
                _designatedStoneLocation.Value = MainGame.me.player_pos;
                Tools.ShowMessage(strings.DumpStone, _designatedStoneLocation.Value);
            }
        }
    }
}