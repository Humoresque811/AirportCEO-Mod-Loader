using System;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal class FixRestrictedTimeWindow
{
    [HarmonyPatch(typeof(AirportController), nameof(AirportController.IsTimeWithinRestrictedOperationTime))]
    [HarmonyPrefix]
    internal static bool UseWholeNightWindow(TimeSpan time, ref bool __result)
    {
        __result = time > AirportController.restrictedTimeStart || time < AirportController.restrictedTimeEnd;
        return false;
    }
}
