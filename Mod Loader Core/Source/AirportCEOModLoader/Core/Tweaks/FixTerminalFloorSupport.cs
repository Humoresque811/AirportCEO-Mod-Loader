using System;
using HarmonyLib;
using UnityEngine;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixTerminalFloorSupport
{
    [ThreadStatic]
    private static PlaceableTerminal checkingTerminal;

    [ThreadStatic]
    private static Vector3[] checkingPositions;

    private struct CheckState
    {
        public PlaceableTerminal Terminal;
        public Vector3[] Positions;
    }

    [HarmonyPatch(typeof(PlaceableTerminal), nameof(PlaceableTerminal.IsLegalPosition))]
    [HarmonyPrefix]
    private static void SaveCheckState(PlaceableTerminal __instance, Vector3[] positionArray, out CheckState __state)
    {
        __state = new CheckState { Terminal = checkingTerminal, Positions = checkingPositions };
        checkingTerminal = __instance;
        checkingPositions = positionArray;
    }

    [HarmonyPatch(typeof(PlaceableTerminal), nameof(PlaceableTerminal.IsLegalPosition))]
    [HarmonyFinalizer]
    private static Exception RestoreCheckState(Exception __exception, CheckState __state)
    {
        checkingTerminal = __state.Terminal;
        checkingPositions = __state.Positions;
        return __exception;
    }

    [HarmonyPatch(typeof(PlaceableTerminal), nameof(PlaceableTerminal.CanBuildOnFloor))]
    [HarmonyPostfix]
    private static void AcceptSupportAnywhereUnderArea(PlaceableTerminal __instance, int floor, ref bool __result)
    {
        if (__result || floor <= 0 || !ReferenceEquals(__instance, checkingTerminal) || checkingPositions == null)
        {
            return;
        }

        TiledObjectsManager tiles = Singleton<TiledObjectsManager>.Instance;
        for (int i = 0; i < checkingPositions.Length; i++)
        {
            if (!tiles.TryGetTileable(checkingPositions[i].RoundVectorToInt().SetZ(floor - 1), out ITileable tile))
            {
                continue;
            }

            if (!tile.IsPlanned || __instance.IsPlanned)
            {
                __result = true;
                return;
            }
        }
    }
}
