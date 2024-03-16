using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AirportCEOModLoader.Performance;

[HarmonyPatch]
public class HasJanitorOptimization
{
    private static bool hasJanitor = false;
    private static int checkedCounter = -1;
    private static readonly int checkedCounterMax = 50;

    [HarmonyPatch(typeof(AirportController), nameof(AirportController.HasHiredEmployeeType))]
    [HarmonyPrefix]
    public static bool Optimization(AirportController __instance, Enums.EmployeeType employeeType, ref bool __result)
    {
        if (employeeType != Enums.EmployeeType.Janitor)
        {
            return true;
        }

        if (checkedCounter < 0 || checkedCounter >= checkedCounterMax)
        {
            hasJanitor = AirportController.Instance.GetHiredEmployeesCount(employeeType) > 0;
        }

        checkedCounter++;

        return hasJanitor;
    }
}
