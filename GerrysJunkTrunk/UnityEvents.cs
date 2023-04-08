using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GerrysJunkTrunk;

public partial class Plugin
{
    private void Update()
    {
        if (!_modEnabled.Value) return;
        if (!MainGame.game_started) return;

        _techCount = MainGame.me.save.unlocked_techs.Count;
        if (_techCount > _oldTechCount)
        {
            _oldTechCount = _techCount;
            CheckShippingBox();
        }

        if (_internalShowIntroMessage.Value)
        {
            ShowIntroMessage();
            _internalShowIntroMessage.Value = false;
        }

        if (!UnlockedShippingBox()) return;
        var sbCraft = GameBalance.me.GetData<ObjectCraftDefinition>(ShippingBoxId);
        if (_internalShippingBoxBuilt.Value && _shippingBox == null)
        {
            _shippingBox = Object.FindObjectsOfType<WorldGameObject>(true)
                .FirstOrDefault(x => string.Equals(x.custom_tag, ShippingBoxTag));
            if (_shippingBox == null)
            {
                Plugin.Log.LogWarning("Update: No Shipping Box Found!");
                _internalShippingBoxBuilt.Value = false;
                sbCraft.hidden = false;
            }
            else
            {
                Plugin.Log.LogWarning($"Update: Found Shipping Box at {_shippingBox.pos3}");
                _internalShippingBoxBuilt.Value = true;
                _shippingBox.data.drop_zone_id = ShippingBoxTag;

                var invSize = SmallInvSize;
                if (UnlockedShippingBoxExpansion())
                {
                    invSize = LargeInvSize;
                }

                _shippingBox.data.SetInventorySize(invSize);


                sbCraft.hidden = true;
            }
        }
    }

    private static void UpdateShippingBox(CraftDefinition sbCraft, WorldGameObject shippingBoxInstance = null)
    {
        if (!_internalShippingBoxBuilt.Value || _shippingBox != null) return;

        _shippingBox = shippingBoxInstance ? shippingBoxInstance : FindObjectsOfType<WorldGameObject>(true)
            .FirstOrDefault(x => string.Equals(x.custom_tag, ShippingBoxTag));

        if (_shippingBox == null)
        {
            Log.LogWarning("UpdateShippingBox: No Shipping Box Found!");
            _internalShippingBoxBuilt.Value = false;
            sbCraft.hidden = false;
        }
        else
        {
            Log.LogWarning($"UpdateShippingBox: Found Shipping Box at {_shippingBox.pos3}");
            _internalShippingBoxBuilt.Value = true;
            _shippingBox.data.drop_zone_id = ShippingBoxTag;

            var invSize = UnlockedShippingBoxExpansion() ? LargeInvSize : SmallInvSize;
            _shippingBox.data.SetInventorySize(invSize);

            sbCraft.hidden = true;
        }
    }

}