using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOModLoader.Performance;

[HarmonyPatch]
public static class TranslateDirectionOptimization
{
    [HarmonyPatch(typeof(MovementManager), nameof(MovementManager.TranslateDirection))]
    [HarmonyPrefix]
    public static bool TranslateDirectionOptimizationPatch(this Transform currentTransform, Vector2 direction, float speed)
    {
        currentTransform.Translate(direction * (Time.deltaTime * speed), Space.Self);
        return false;
    }
}
