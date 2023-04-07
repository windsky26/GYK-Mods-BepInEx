using System;
using GYKHelper;

namespace BringOutYerDead;

public partial class Plugin
{
    internal static bool PrideDayLogged;
    private static WorldGameObject _donkey;
    private static bool _strikeDone;
    
    private void OnEnable()
    {
        Log.LogInfo($"Plugin {PluginName} has been enabled!");
    }

    private void OnDisable()
    {
        Log.LogError($"Plugin {PluginName} has been disabled!");
    }
    
    private void Update()
    {
        if (!MainGame.game_started) return;
        if (MainGame.paused) return;

        if (!Tools.TutorialDone() && !_internalTutMessageShown.Value)
        {
            _internalTutMessageShown.Value = true;
            Helpers.Log("Need to complete all 'tutorial' quests first, upto and including the repair the sword quest.");
            return;
        }
        
        Patches.Ld = new LogicData("donkey")
        {
            _started = false
        };


        if (_donkey == null)
        {
            _donkey = WorldMap.GetWorldGameObjectByCustomTag("donkey", true);
        }


        if (_donkey != null)
        {
            var dataGetParam = _donkey.data.GetParam("speed");
            var getParam = _donkey.GetParam("speed");

            _strikeDone = _donkey.GetParam("strike_completed") > 0f;

            if (dataGetParam < DonkeySpeed.Value || getParam < DonkeySpeed.Value)
            {
                Helpers.Log($"TDU: Donkey old speeds: DataGetParam: {dataGetParam}, GetParam: {getParam}");
                _donkey.components.character.SetSpeed(DonkeySpeed.Value);
                Helpers.Log($"TDU: Donkey new speeds: DataGetParam: {dataGetParam}, GetParam: {getParam}");
            }

            if (!_strikeDone)
            {
                Helpers.Log($"Must complete the donkey strike first! Pay him 10 carrots, grease his wheels etc.");
                return;
            }
        }
        else
        {
            Helpers.Log($"Donkey is null!?!?!");
            return;
        }


        if (MainGame.me.save.day_of_week == 1)
        {
            if (!PrideDayLogged)
            {
                Helpers.Log($"Pride day! Skipping donkey as he doesnt come anyway when asked if its Pride day!");
                PrideDayLogged = true;
            }

            return;
        }


        switch (TimeOfDay.me.time_of_day_enum)
        {
            case TimeOfDay.TimeOfDayEnum.Night:
                if (!_nightDelivery.Value)
                {
                    Helpers.Log("Night delivery is disabled in config!");
                    break;
                }

                if (!InternalNightDelivery.Value)
                {
                    //Tools.ShowMessage("Night Delivery!", MainGame.me.player_pos);

                    if (Patches.ForceDonkey(_donkey))
                    {
                        Helpers.Log($"It's night! Beginning night time delivery!");
                        InternalNightDelivery.Value = true;
                    }
                    else
                    {
                        InternalNightDelivery.Value = false;
                        Helpers.Log($"It's night! But we failed to force the donkey to deliver!");
                    }
                }

                break;
            case TimeOfDay.TimeOfDayEnum.Morning:
                if (!_morningDelivery.Value)
                {
                    Helpers.Log("Morning delivery is disabled in config!");
                    break;
                }

                if (!InternalMorningDelivery.Value)
                {
                    //Tools.ShowMessage("Morning Delivery!", MainGame.me.player_pos);
                    Helpers.Log($"It's morning! Beginning morning delivery!");
                    if (Patches.ForceDonkey(_donkey))
                    {
                        InternalMorningDelivery.Value = true;
                    }
                    else
                    {
                        InternalMorningDelivery.Value = false;
                        Helpers.Log($"It's morning! But we failed to force the donkey to deliver!");
                    }
                }

                break;
            case TimeOfDay.TimeOfDayEnum.Day:
                if (!_dayDelivery.Value)
                {
                    Helpers.Log("Day delivery is disabled in config!");
                    return;
                }

                if (!InternalDayDelivery.Value)
                {
                    // Tools.ShowMessage("Day Delivery!", MainGame.me.player_pos);
                    Helpers.Log($"It's Day! Beginning midday delivery!");
                    if (Patches.ForceDonkey(_donkey))
                    {
                        InternalDayDelivery.Value = true;
                    }
                    else
                    {
                        InternalDayDelivery.Value = false;
                        Helpers.Log($"It's midday! But we failed to force the donkey to deliver!");
                    }
                }

                break;
            case TimeOfDay.TimeOfDayEnum.Evening:
                if (!_eveningDelivery.Value)
                {
                    Helpers.Log("Evening delivery is disabled in config!");
                    return;
                }

                if (!InternalEveningDelivery.Value)
                {
                    if (Patches.ForceDonkey(_donkey))
                    {
                        Helpers.Log($"It's evening! Beginning evening delivery!");
                        InternalEveningDelivery.Value = true;
                    }
                    else
                    {
                        InternalEveningDelivery.Value = false;
                        Helpers.Log($"It's evening! But we failed to force the donkey to deliver!");
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}