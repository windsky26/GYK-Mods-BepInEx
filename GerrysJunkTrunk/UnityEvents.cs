using System;
using System.Linq;
using UnityEngine;

namespace GerrysJunkTrunk;

public partial class Plugin
{
    private void Update()
    {
        if (!MainGame.game_started) return;

        UpdateTechCount();
        ShowIntroMessageIfNeeded();

        if (!UnlockedShippingBox()) return;

        var sbCraft = GameBalance.me.GetData<ObjectCraftDefinition>(ShippingBoxId);
        UpdateShippingBox(sbCraft);
    }

    private static void UpdateTechCount()
    {
        _techCount = MainGame.me.save.unlocked_techs.Count;
        if (_techCount > _oldTechCount)
        {
            _oldTechCount = _techCount;
            CheckShippingBox();
        }
    }

    private static void ShowIntroMessageIfNeeded()
    {
        if (_internalShowIntroMessage.Value)
        {
            ShowIntroMessage();
        }
    }

    private static void UpdateShippingBox(CraftDefinition sbCraft, WorldGameObject shippingBoxInstance = null)
    {
        if (_internalShippingBoxBuilt.Value && _shippingBox == null)
        {
            _shippingBox = shippingBoxInstance ? shippingBoxInstance : FindObjectsOfType<WorldGameObject>(true)
                .FirstOrDefault(x => string.Equals(x.custom_tag, ShippingBoxTag));

            if (_shippingBox == null)
            {
                WriteLog("No Shipping Box Found!");
                _internalShippingBoxBuilt.Value = false;
                sbCraft.hidden = false;
            }
            else
            {
                WriteLog($"Found Shipping Box at {_shippingBox.pos3}");
                _internalShippingBoxBuilt.Value = true;
                _shippingBox.data.drop_zone_id = ShippingBoxTag;

                var invSize = UnlockedShippingBoxExpansion() ? LargeInvSize : SmallInvSize;
                _shippingBox.data.SetInventorySize(invSize);

                sbCraft.hidden = true;
            }
        }
    }
}