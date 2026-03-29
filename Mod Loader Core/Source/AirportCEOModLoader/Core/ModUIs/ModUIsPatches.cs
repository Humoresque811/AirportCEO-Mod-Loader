using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core.ModUIs;

[HarmonyPatch]
internal class ModUIsPatches
{
    [HarmonyPatch(typeof(WorkshopModContainer), nameof(WorkshopModContainer.SetModContainer))]
    [HarmonyPostfix]
    internal static void SetModContainerPost(WorkshopModContainer __instance)
    {
        if (!AirportCEOModLoader.UpdateModdingMenu.Value)
        {
            return;
        }

        if (__instance.TryGetComponent<ModUIsContainerComp>(out var comp))
        {
            comp.TrySetToCodeBased();
        }
    }
}
