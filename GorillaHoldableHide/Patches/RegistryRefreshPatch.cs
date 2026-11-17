using GorillaHoldableHide.Behaviours;
using GorillaNetworking;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace GorillaHoldableHide.Patches;

[HarmonyPatch(typeof(CosmeticItemRegistry), nameof(CosmeticItemRegistry.RefreshRig)), HarmonyWrapSafe]
internal class RegistryRefreshPatch
{
    private static readonly Dictionary<CosmeticItemRegistry, int> _registryScan = [];

    public static void Postfix(CosmeticItemRegistry __instance)
    {
        if (!_registryScan.TryGetValue(__instance, out int scanStart))
        {
            scanStart = 0;
            _registryScan.Add(__instance, scanStart);
        }

        for (int i = scanStart; i < __instance._nameToCosmeticMap.Count; i++)
        {
            scanStart = i + 1;

            CosmeticItemInstance itemInstance = __instance._nameToCosmeticMap.ElementAt(i).Value;
            if (itemInstance == null || !itemInstance.isHoldableItem) continue;

            if (!__instance.Rig.TryGetComponent(out HiddenHoldableManager holdableManager)) holdableManager = __instance.Rig.gameObject.AddComponent<HiddenHoldableManager>();
            holdableManager.CheckItem(itemInstance);
        }

        _registryScan[__instance] = scanStart;
    }
}
