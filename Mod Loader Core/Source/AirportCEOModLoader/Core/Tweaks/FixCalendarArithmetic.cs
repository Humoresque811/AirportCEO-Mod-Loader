using System;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixCalendarArithmetic
{
    [HarmonyPatch(typeof(WorldTime), nameof(WorldTime.Add))]
    [HarmonyPrefix]
    private static bool Add(WorldTime __instance, TimeSpan timeSpan, ref WorldTime __result)
    {
        __result = AtTime(__instance, __instance.Time.Add(timeSpan));
        return false;
    }

    [HarmonyPatch(typeof(WorldTime), nameof(WorldTime.Subtract))]
    [HarmonyPrefix]
    private static bool Subtract(WorldTime __instance, TimeSpan timeSpan, ref WorldTime __result)
    {
        __result = AtTime(__instance, __instance.Time.Subtract(timeSpan));
        return false;
    }

    [HarmonyPatch(typeof(WorldTime), nameof(WorldTime.AddDays))]
    [HarmonyPrefix]
    private static bool AddDays(WorldTime __instance, int nbrOfDays, ref WorldTime __result)
    {
        __result = AtTime(__instance, __instance.Time.AddDays(nbrOfDays));
        return false;
    }

    [HarmonyPatch(typeof(WorldTime), nameof(WorldTime.SubtractDays))]
    [HarmonyPrefix]
    private static bool SubtractDays(WorldTime __instance, int nbrOfDays, ref WorldTime __result)
    {
        __result = AtTime(__instance, __instance.Time.AddDays(-nbrOfDays));
        return false;
    }

    private static WorldTime AtTime(WorldTime source, DateTime time)
    {
        int day = source.Day - 1 + (time.Date - source.Time.Date).Days;
        int year = source.Year + day / 12;
        day %= 12;
        if (day < 0)
        {
            day += 12;
            year--;
        }
        return new WorldTime(time, year, day + 1);
    }
}
