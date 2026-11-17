using GorillaLibrary.Extensions;
using GorillaNetworking;
using GorillaTag.Cosmetics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaHoldableHide.Behaviours;

[RequireComponent(typeof(VRRig)), DisallowMultipleComponent]
internal class HiddenHoldableManager : MonoBehaviour
{
    public VRRig Rig => _rig;

    private VRRig _rig;

    private readonly List<CosmeticItemInstance> cosmetics = [];

    private readonly Dictionary<HiddenHoldableInstance, bool> holdables = [];

    private bool hasHoldables;

    public void Awake()
    {
        _rig = GetComponent<VRRig>();
    }

    public void Update()
    {
        if (!hasHoldables) return;

        for (int i = 0; i < holdables.Count; i++)
        {
            var element = holdables.ElementAt(i);
            if (!element.Value) continue;

            if (element.Key.gameObject.IsObjectNull())
            {
                holdables.Remove(element.Key);
                continue;
            }

            if (element.Key.gameObject.activeSelf)
            {
                element.Key.CheckState();
                element.Key.CheckState(); // TODO: improve visibility logic so double call to prevent flashing is not neccessary
                continue;
            }

            holdables[element.Key] = false;
            hasHoldables = holdables.Any(holdable => holdable.Value);
        }
    }

    public void CheckItem(CosmeticItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.isHoldableItem || cosmetics.Contains(itemInstance)) return;
        cosmetics.Add(itemInstance);

        TransferrableObject transferrableObject = itemInstance.holdableObjects.Find(obj => obj.GetComponent<TransferrableObject>())?.GetComponent<TransferrableObject>();
        if (transferrableObject == null)
        {
            Plugin.Logger.LogWarning($"Item {itemInstance.dbgname} omits any form of TransferrableObject component");
            return;
        }

        if (transferrableObject is ThrowablePickupableCosmetic)
        {
            Plugin.Logger.LogWarning($"Item {itemInstance.dbgname} is build off of a currently unsupported component of holdable (ThrowablePickupableCosmetic)");
            return;
        }

        HiddenHoldableInstance holdableInstance = transferrableObject.gameObject.AddComponent<HiddenHoldableInstance>();
        holdableInstance.HoldableManager = this;
        holdableInstance.ItemInstance = itemInstance;

        hasHoldables = hasHoldables || holdableInstance.gameObject.activeSelf;
        holdables.Add(holdableInstance, true);
    }

    public void SubscribeHoldable(HiddenHoldableInstance holdable)
    {
        if (!holdables.ContainsKey(holdable)) return;
        holdables[holdable] = true;
        hasHoldables = true;
    }

    public void UnsubscribeHoldable(HiddenHoldableInstance holdable)
    {
        if (!holdables.ContainsKey(holdable)) return;
        holdables[holdable] = false;
        hasHoldables = holdables.Any(holdable => holdable.Value);
    }
}
