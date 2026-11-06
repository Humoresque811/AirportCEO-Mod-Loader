using System;
using HarmonyLib;
using Nodes;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch(typeof(AircraftController), "SetNextActivity")]
internal static class FixCancelledDeicingRoute
{
    private static readonly AccessTools.FieldRef<AircraftController, SmoothPath<TaxiwayNode>> PathOf = AccessTools.FieldRefAccess<AircraftController, SmoothPath<TaxiwayNode>>("path");

    private static readonly AccessTools.FieldRef<AircraftController, bool> PathPreOccupied = AccessTools.FieldRefAccess<AircraftController, bool>("pathPreOccupied");

    private static readonly Action<AircraftController, bool> ResetPathDelegate = AccessTools.MethodDelegate<Action<AircraftController, bool>>(AccessTools.Method(typeof(AircraftController), "ResetPath", new[] { typeof(bool) }));

    [HarmonyPrefix]
    private static void Prefix(AircraftController __instance, Enums.AircraftAction newAction)
    {
        if (newAction != Enums.AircraftAction.TaxiingToRunway)
        {
            return;
        }

        AircraftModel am = __instance.am;
        if (am == null || am.AircraftAction != Enums.AircraftAction.TaxiingToDeicingStand || PathOf(__instance) == null)
        {
            return;
        }

        ResetPathDelegate(__instance, true);
        PathPreOccupied(__instance) = false;
    }
}
