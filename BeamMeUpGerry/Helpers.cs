using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeamMeUpGerry.lang;
using GYKHelper;
using NodeCanvas.Tasks.Actions;
using UnityEngine;
using Random = System.Random;

namespace BeamMeUpGerry;

public static class Helpers
{
    private static string GetLocalizedString(string content)
    {
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        return content;
    }

    internal static void Log(string message, bool error = false)
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
    
      private static void Beam()
    {
        if (Tools.PlayerDisabled())
        {
            return;
        }

        if (InTutorial()) return;
        if (Patches.UsingStone || Patches.DotSelection || CrossModFields._talkingToNpc) return;

        var item = GetHearthstone();
        if (item != null)
        {
            if (CrossModFields.IsInDungeon)
            {
                Patches.UsingStone = false;
                SpawnGerry(strings.CantUseHere, Vector3.zero);
            }
            else
            {
                Patches.UsingStone = true;
                MainGame.me.player.UseItemFromInventory(item);
            }

            CrossModFields.TalkingToNpc(false);
        }
        else
        {
            SpawnGerry(strings.WhereIsIt, Vector3.zero);
        }
    }


    private static Item GetHearthstone()
    {
        return MainGame.me.player.data.GetItemWithID("hearthstone");
    }

    private static bool RemoveZone(AnswerVisualData answer)
    {
        var wheatExists = Tools.PlayerHasSeenZone("zone_wheat_land");
        var coalExists = Tools.PlayerHasSeenZone("zone_flat_under_waterflow");
        if (answer.id.Contains(strings.Farmer))
        {
            return wheatExists && Tools.PlayerKnowsNpcPartial("farmer");
        }

        if (answer.id.Contains(strings.Mill))
        {
            return wheatExists && Tools.PlayerKnowsNpcPartial("miller");
        }

        if (answer.id.Contains(strings.Coal))
        {
            return coalExists;
        }

        if (answer.id.Contains("Mystery") || answer.id.Contains(strings.Clay) || answer.id.Contains(strings.Sand) || answer.id.Contains("...") || answer.id.Contains("....") || answer.id.Contains("cancel")) return false;
        var zone = answer.id.Replace("zone_", "");
        if (MainGame.me.save.known_world_zones.Exists(a => string.Equals(a, zone)))
        {
            Log($"[RemoveZone]: Player knows {zone}. NOT removing.");
            return false;
        }

        Log($"[RemoveZone]: Player does not know {zone}. Removing.");
        return true;
    }

    private static string GetMoneyMessage()
    {
        var rng = new Random();
        var messageList = new List<string>
        {
            strings.M1, strings.M2, strings.M3, strings.M4, strings.M5, strings.M6, strings.M7,
            strings.M8, strings.M9, strings.M10
        };
        var shuffledList = messageList.OrderBy(_ => rng.Next()).ToList();
        return shuffledList[0];
    }

    private static string GetMessage()
    {
        var rng = new Random();
        var messageList = new List<string>
        {
            strings.M4, strings.M7,
            strings.M8, strings.M9
        };
        var shuffledList = messageList.OrderBy(_ => rng.Next()).ToList();
        return shuffledList[0];
    }

    private static WorldGameObject _gerry;
    private static bool _gerryRunning;

    private static void SpawnGerry(string message, Vector3 customPosition, bool money = false)
    {
        if (_gerryRunning) return;
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        var location = MainGame.me.player_pos;
        location.x -= 75f;
        //location.y += 50f;
        if (customPosition != Vector3.zero)
        {
            location = customPosition;
        }

        if (_gerry == null)
        {
            _gerry = WorldMap.SpawnWGO(MainGame.me.world_root.transform, "talking_skull", location);
            Tools.NameSpawnedGerry(_gerry);
            _gerry.ReplaceWithObject("talking_skull", true);
            Tools.NameSpawnedGerry(_gerry);
            _gerryRunning = true;
        }


        GJTimer.AddTimer(0.5f, delegate
        {
            if (_gerry == null) return;
            var newMessage = money == false ? GetMessage() : GetMoneyMessage();
            
            if(message != string.Empty)
            {
                newMessage = message;
            }

            _gerry.Say(newMessage, delegate
            {
                GJTimer.AddTimer(0.25f, delegate
                {
                    if (_gerry == null) return;
                    _gerry.ReplaceWithObject("talking_skull", true);
                    Tools.NameSpawnedGerry(_gerry);
                    _gerry.DestroyMe();
                    _gerry = null;
                    _gerryRunning = false;
                    if (!money) return;

                    TakeMoney(MainGame.me.player_pos);
                });
            }, null, SpeechBubbleGUI.SpeechBubbleType.Talk, SmartSpeechEngine.VoiceID.Skull);
        });
    }

    internal static List<AnswerVisualData> ValidateAnswerList(List<AnswerVisualData> answers)
    {
        return answers.Where(answer => !RemoveZone(answer)).ToList();
    }

    internal static void ShowHud(Location chosen, bool animate = false)
    {
        GUIElements.me.EnableHUD(true);
        GUIElements.ChangeHUDAlpha(true, animate);
        GUIElements.ChangeBubblesVisibility(true);
        GUIElements.me.overhead_panel.gameObject.SetActive(true);
        GUIElements.me.relation.ChangeHUDAlpha(true, animate);
        GUIElements.me.relation.Update();

        if (chosen == null) return;

        EnvironmentEngine.me.SetEngineGlobalState(chosen.State);
        Log($"[ApplyCurrentEnvironmentPreset, id] = {chosen.Preset}");
        var environmentPreset = EnvironmentPreset.Load(chosen.Preset);
        EnvironmentEngine.me.ApplyEnvironmentPreset(environmentPreset);
    }

    private static float GenerateFee()
    {
        var dynamicFee = (float) Math.Round((0.1f * MainGame.me.player.data.money) / 100f, 2);
        const float minimumFee = 0.01f;
        var feeToPay = dynamicFee switch
        {
            < minimumFee => minimumFee,
            > 5f => 5f,
            _ => dynamicFee
        };

        Log($"[Fee]: {Trading.FormatMoney(feeToPay, true)}\n[DynFee]: {Trading.FormatMoney(dynamicFee, true)}\nMoney: {Trading.FormatMoney(MainGame.me.player.data.money, true)}, Minimum: {Trading.FormatMoney(minimumFee, true)}");
        return feeToPay;
    }

    private static void TakeMoney(Vector3 vector)
    {
        if (Plugin.DisableCost.Value) return;
        vector.y += 125f;
        var feeToPay = GenerateFee();
        MainGame.me.player.data.money -= feeToPay;
        Sounds.PlaySound("coins_sound", vector, true);
        EffectBubblesManager.ShowImmediately(vector, $"-{Trading.FormatMoney(feeToPay, true)}", EffectBubblesManager.BubbleColor.Red, true, 3f);
    }

    internal static bool CanUseStone()
    {
        var inDungeon = CrossModFields.IsInDungeon;
        var talkingToNpc = CrossModFields._talkingToNpc;
        var inTutorial = !Tools.TutorialDone() || MainGame.me.save.IsInTutorial();
        var controlled = CrossModFields.PlayerIsControlled;
        if (inDungeon || talkingToNpc || inTutorial || controlled) return false;
        return true;
    }

    internal static void BeamGerryOnChosen(string chosen)
    {
        if (Tools.PlayerDisabled())
        {
            return;
        }

        if (InTutorial()) return;
        //
        //  ShowHud();
        if (!Plugin.EnableListExpansion.Value) return;
        if (!CanUseStone()) return;
        if (string.Equals("cancel", chosen))
        {
            // ShowHud();
            return;
        }

        if (MainGame.me.player.data.money < GenerateFee())
        {
            var location = MainGame.me.player_pos;
            location.x += 125f;
            location.y += 125f;
            SpawnGerry(strings.MoreCoin, Vector3.zero);
            return;
        }

        var partOne = Patches.LocationsPartOne.Exists(a => a.Zone == chosen);
        var partTwo = Patches.LocationsPartTwo.Exists(a => a.Zone == chosen);

        Vector3 vector;
        Location chosenLocation = null;
        if (partOne)
        {
            chosenLocation = Patches.LocationsPartOne.Find(a => a.Zone == chosen);
            vector = chosenLocation.Coords;
        }
        else if (partTwo)
        {
            chosenLocation = Patches.LocationsPartTwo.Find(a => a.Zone == chosen);
            vector = chosenLocation.Coords;
        }
        else
        {
            vector = MainGame.me.player_pos;
        }

        var found = partOne || partTwo;
        if (found)
        {
            if (Plugin.FadeForCustomLocations.Value)
            {
                CameraFader.current.FadeOut(0.15f);
                GJTimer.AddTimer(0.15f, delegate
                {
                    MainGame.me.player.PlaceAtPos(vector);
                    MainGame.me.player.components.character.control_enabled = true;
                    GJTimer.AddTimer(1.25f, delegate
                    {
                        ShowHud(chosenLocation, true);
                        CameraFader.current.FadeIn(0.15f);
                        GJTimer.AddTimer(0.20f, delegate
                        {
                            if (!Plugin.DisableGerry.Value)
                            {
                                SpawnGerry("", Vector3.zero, !Plugin.DisableCost.Value);
                            }
                            else
                            {
                                TakeMoney(vector);
                            }
                        });
                    });
                });
            }
            else
            {
                ShowHud(null, false);
                MainGame.me.player.PlaceAtPos(vector);
                MainGame.me.player.components.character.control_enabled = true;

                if (!Plugin.DisableGerry.Value)
                {
                    SpawnGerry("", Vector3.zero, !Plugin.DisableCost.Value);
                }
                else
                {
                    TakeMoney(vector);
                }
            }

            EnvironmentEngine.me.SetEngineGlobalState(EnvironmentEngine.State.Inside);
        }
        else
        {
            Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
            MainGame.me.player.Say(strings.DontKnow);
            MainGame.me.player.components.character.control_enabled = true;
            Patches.MaGui.DestroyBubble();
        }
    }


    internal static bool InTutorial()
    {
        return !Tools.TutorialDone() || MainGame.me.save.IsInTutorial();
    }


    internal static void DoLoggingAndBeam()
    {
        if (InTutorial() && !Tools.PlayerDisabled())
        {
            var item = GetHearthstone();
            Tools.SpawnGerry(GetLocalizedString(item == null ? strings.InTutorialNoStone : strings.InTutorial), Vector3.zero, CrossModFields.ModGerryTag);
            return;
        }

        if (Tools.PlayerDisabled())
        {
            return;
        }

        Log($"[ZONE]: {GJL.L("zone_" + MainGame.me.player.GetMyWorldZoneId())}, ID: {"zone_" + MainGame.me.player.GetMyWorldZoneId()}, Vector: {MainGame.me.player_pos}");
        var gdPoint = Util.FindNearestGdPoint();
        if (gdPoint != null)
        {
            if (Plugin.Debug.Value)
            {
                SpawnGerry("Here!", gdPoint.pos);
            }

            var distance = Vector3.Distance(MainGame.me.player_pos, gdPoint.pos);
            Log($"[GDPoint:] Nearest: {gdPoint.name}, Distance to player: {distance}, Vector: {gdPoint.pos}");
        }

        Beam();
    }
}