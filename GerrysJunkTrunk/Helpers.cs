using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GerrysJunkTrunk.lang;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace GerrysJunkTrunk;

public partial class Plugin
{
    private static readonly ItemDefinition.ItemType[] ExcludeItems =
    {
        ItemDefinition.ItemType.Axe, ItemDefinition.ItemType.Shovel, ItemDefinition.ItemType.Hammer,
        ItemDefinition.ItemType.Pickaxe, ItemDefinition.ItemType.FishingRod, ItemDefinition.ItemType.BodyArmor,
        ItemDefinition.ItemType.HeadArmor, ItemDefinition.ItemType.Sword, ItemDefinition.ItemType.Preach,
        ItemDefinition.ItemType.GraveStone, ItemDefinition.ItemType.GraveFence, ItemDefinition.ItemType.GraveCover,
        ItemDefinition.ItemType.GraveStoneReq, ItemDefinition.ItemType.GraveFenceReq, ItemDefinition.ItemType.GraveCoverReq,
    };

    private static string GetLocalizedString(string content)
    {
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        return content;
    }

    private static void WriteLog(string message, bool error = false)
    {
        if (error)
        {
            Log.LogError($"{message}");
        }
        else
        {
            if (_debug.Value)
            {
                Log.LogInfo($"{message}");
            }
        }
    }

    private static async void ShowSummary(string money)
    {
        if (!MainGame.game_started) return;

        var salesByVendor = new List<List<VendorSale.Sale>>();

        await Task.Run(() =>
        {
            Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;

            salesByVendor.AddRange(_vendorSales.Select(vendor => vendor.GetSales().OrderBy(a => a.GetItem().id).ToList()));
        });

        var resultBuilder = new StringBuilder();

        foreach (var sale in salesByVendor.SelectMany(sales => sales))
        {
            resultBuilder.AppendFormat("{0} {1} {2}\n",
                sale.GetItem().GetItemName(), strings.For, Trading.FormatMoney(sale.GetPrice()));
        }

        await Task.Run(() => { GUIElements.me.dialog.OpenOK($"[37ff00]{strings.Header}[-]", null, $"{resultBuilder}", true, $"{money}"); });
    }

    private static void ClearGerryFlag(ref ChestGUI chestGui)
    {
        if (chestGui == null || !_usingShippingBox || StackSizeBackups.Count <= 0) return;

        foreach (var inventory in new[] {chestGui.player_panel.multi_inventory.all[0].data.inventory, chestGui.chest_panel.multi_inventory.all[0].data.inventory})
        {
            foreach (var item in inventory)
            {
                if (StackSizeBackups.TryGetValue(item.id, out var value))
                {
                    item.definition.stack_count = value;
                }
            }
        }

        _usingShippingBox = false;
    }

    private static float GetBoxEarnings(WorldGameObject shippingBox)
    {
        return shippingBox.data.inventory.Sum(GetItemEarnings);
    }

    private static float GetItemEarnings(Item selectedItem)
    {
        var itemCache = PriceCache.Find(a =>
            string.Equals(a.GetItem().id, selectedItem.id) && a.GetQty() == selectedItem.value);

        float ApplyPriceModifier(float price)
        {
            return UnlockedFullPrice() ? price * FullPriceModifier : price * PriceModifier;
        }

        if (itemCache != null)
        {
            var price = itemCache.GetPrice() == 0
                ? PityPrice * itemCache.GetQty()
                : itemCache.GetPrice();

            return ApplyPriceModifier(price);
        }

        var totalSalePrice = 0f;
        _vendorSales.Clear();
        var totalCount = selectedItem.value;

        List<Vendor> vendorList = new();
        List<float> priceList = new();

        var vendors = WorldMap._vendors;

        foreach (var vendor in vendors)
        {
            var myVendor = WorldMap.GetNPCByObjID(vendor.id, true);

            if (!KnownVendors.Contains(myVendor)) continue;

            float num = 0;
            var myTrader = new Trading(myVendor);
            _myVendor = myVendor;

            if (selectedItem.definition.base_price <= 0)
            {
                var lastChar = selectedItem.id[selectedItem.id.Length - 1];
                var multiplier = lastChar switch
                {
                    '3' => 0.75f,
                    '2' => 0.60f,
                    '1' => 0.45f,
                    _ => 0.25f
                };

                num += multiplier * totalCount;
            }
            else
            {
                for (var i = 0; i < totalCount; i++)
                {
                    var itemCost = Mathf.Round(myTrader.GetSingleItemCostInPlayerInventory(selectedItem, -i) * 100f) / 100f;
                    num += itemCost;
                }
            }

            vendorList.Add(vendor);
            priceList.Add(num);
        }

        var maxSaleIndex = priceList.IndexOf(priceList.Max());
        var newSale = new VendorSale(vendorList[maxSaleIndex]);
        newSale.AddSale(selectedItem, totalCount, priceList[maxSaleIndex]);
        _vendorSales.Add(newSale);
        _vendorSales = _vendorSales.OrderBy(a => a.GetVendor().id).ToList();
        totalSalePrice += priceList[maxSaleIndex];

        PriceCache.Add(new ItemPrice(selectedItem, totalCount, priceList[maxSaleIndex]));

        if (totalSalePrice <= 0)
        {
            var price = PityPrice * totalCount;
            return ApplyPriceModifier(price);
        }

        return ApplyPriceModifier(totalSalePrice);
    }

    private static void ShowIntroMessage()
    {
        GUIElements.me.dialog.OpenOK(strings.Message1, null, $"{strings.Message2}\n{strings.Message3}\n{strings.Message4}\n{strings.Message5}\n{strings.Message6}\n{strings.Message7}", true, strings.Message8);
    }

    private static void StartGerryRoutine(float num)
    {
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        var noSales = num <= 0;
        var money = Trading.FormatMoney(num, true);
        //var gerry = WorldMap.GetNPCByObjID("talking_skull");
        var gerry = WorldMap.SpawnWGO(_shippingBox.transform, "talking_skull", new Vector3(_shippingBox.pos3.x, _shippingBox.pos3.y + 43f, _shippingBox.pos3.z));
        Tools.NameSpawnedGerry(gerry);
        gerry.ReplaceWithObject("talking_skull", true);
        Tools.NameSpawnedGerry(gerry);

        GJTimer.AddTimer(2f, delegate
        {
            gerry.Say(noSales ? strings.Nothing : strings.WorkWork, delegate
            {
                GJTimer.AddTimer(1f, delegate
                {
                    gerry.ReplaceWithObject("talking_skull", true);
                    Tools.NameSpawnedGerry(gerry);
                    gerry.DestroyMe();
                });
            }, null, SpeechBubbleGUI.SpeechBubbleType.Talk, SmartSpeechEngine.VoiceID.Skull);
        });

        if (noSales) return;
        GJTimer.AddTimer(8f, delegate
        {
            var gerry2 = WorldMap.SpawnWGO(_shippingBox.transform, "talking_skull", new Vector3(_shippingBox.pos3.x, _shippingBox.pos3.y + 43f, _shippingBox.pos3.z));
            Tools.NameSpawnedGerry(gerry2);
            gerry2.ReplaceWithObject("talking_skull", true);
            Tools.NameSpawnedGerry(gerry2);

            GJTimer.AddTimer(2f, delegate
            {
                gerry2.Say($"{money}", delegate
                    {
                        _shippingBox.data.inventory.Clear();
                        if (_showSoldMessagesOnPlayer.Value)
                        {
                            Sounds.PlaySound("coins_sound", MainGame.me.player_pos, true);
                            var pos = MainGame.me.player_pos;
                            pos.y += 125f;
                            EffectBubblesManager.ShowImmediately(pos, $"{money}",
                                num > 0 ? EffectBubblesManager.BubbleColor.Green : EffectBubblesManager.BubbleColor.Red,
                                true, 4f);
                        }
                        else
                        {
                            Sounds.PlaySound("coins_sound", gerry2.pos3, true);
                        }

                        GJTimer.AddTimer(2f, delegate
                        {
                            gerry2.Say(strings.Bye, delegate
                            {
                                GJTimer.AddTimer(1f, delegate
                                {
                                    gerry2.ReplaceWithObject("talking_skull", true);
                                    Tools.NameSpawnedGerry(gerry2);
                                    gerry2.DestroyMe();

                                    GJTimer.AddTimer(1f, delegate { ShowSummary(money); });
                                });
                            }, null, SpeechBubbleGUI.SpeechBubbleType.Talk, SmartSpeechEngine.VoiceID.Skull);
                        });
                    }, null, SpeechBubbleGUI.SpeechBubbleType.Talk,
                    SmartSpeechEngine.VoiceID.Skull);
            });
        });
    }

    private static void TryAdd<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key)) return;
        dictionary.Add(key, value);
    }

    private static bool UnlockedFullPrice()
    {
        return UnlockedShippingBoxExpansion() && MainGame.me.save.unlocked_techs.Exists(a => a.ToLowerInvariant().Equals("Best friend".ToLowerInvariant()));
    }

    private static bool UnlockedShippingBox()
    {
        return MainGame.me.save.unlocked_techs.Exists(a =>
            a.ToLowerInvariant().Equals("Wood processing".ToLowerInvariant()));
    }

    private static bool UnlockedShippingBoxExpansion()
    {
        return UnlockedShippingBox() && MainGame.me.save.unlocked_techs.Exists(a => a.ToLowerInvariant().Equals("Engineer".ToLowerInvariant()));
    }

    private static void UpdateItemStates(ref ChestGUI instance)
    {
        foreach (var inventory in instance.player_panel.multi_inventory.all.Where(i => i.data.inventory.Count > 0))
        {
            // reset status
            foreach (var item in inventory.data.inventory)
            {
                instance.player_panel.GetItemCellGuiForItem(item).SetInactiveState(false);
            }

            // disable quest item selling
            foreach (var item in inventory.data.inventory.Where(item => item.definition.player_cant_throw_out && !ExcludeItems.Contains(item.definition.type)))
            {
                instance.player_panel.GetItemCellGuiForItem(item).SetInactiveState();
            }
        }

        // disable items in the chest inventory
        foreach (var inventory in instance.chest_panel.multi_inventory.all.Where(i => i.data.inventory.Count > 0))
        {
            inventory.is_locked = true;
            foreach (var item in inventory.data.inventory)
            {
                instance.chest_panel.GetItemCellGuiForItem(item)?.SetInactiveState();
            }
        }
    }

    private static int GetTrunkTier()
    {
        var fullPriceUnlocked = UnlockedFullPrice();
        var shippingBoxExpansionUnlocked = UnlockedShippingBoxExpansion();

        if (fullPriceUnlocked) return 3;
        return shippingBoxExpansionUnlocked ? 2 : 1;
    }


    private static void CheckShippingBox()
    {
        if (UnlockedShippingBox())
        {
            MainGame.me.save.UnlockCraft(ShippingBoxId);
            WriteLog($"Tech requirements met, unlocking shipping box craft!");
        }
        else
        {
            MainGame.me.save.LockCraft(ShippingBoxId);
            WriteLog($"Tech requirements not met, locking shipping box craft!");
        }
    }
}