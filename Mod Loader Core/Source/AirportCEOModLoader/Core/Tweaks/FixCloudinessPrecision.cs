using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal class FixCloudinessPrecision
{
    [HarmonyPatch(typeof(WeatherController), "GenerateWeatherDay", new Type[]
    {
        typeof(WeatherType),
        typeof(WorldTime),
        typeof(DateTime)
    })]
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.Last)]
    internal static IEnumerable<CodeInstruction> KeepCloudinessFractional(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> list = instructions.ToList();
        List<int> roundTrips = new List<int>();
        List<int> corrected = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (IsCloudinessStore(list, i))
            {
                if (IsIntegerRoundTrip(list, i))
                {
                    roundTrips.Add(i);
                }
                else if (IsClampThenStore(list, i))
                {
                    corrected.Add(i);
                }
            }
        }

        if (roundTrips.Count == 1 && corrected.Count == 0)
        {
            int storeIndex = roundTrips[0];
            NopInstruction(list[storeIndex - 2]);
            NopInstruction(list[storeIndex - 1]);
            AirportCEOModLoader.ModLoaderLogger.LogInfo("Applied cloudiness precision fix to GenerateWeatherDay.");
            return list;
        }

        if (roundTrips.Count == 0 && corrected.Count == 1)
        {
            return list;
        }

        AirportCEOModLoader.ModLoaderLogger.LogError($"GenerateWeatherDay did not match the expected cloudiness assignment shape (integer round trips: {roundTrips.Count}, already corrected: {corrected.Count}). The cloudiness precision fix was not applied.");
        return list;
    }

    private static bool IsCloudinessStore(List<CodeInstruction> list, int storeIndex)
    {
        return list[storeIndex].opcode == OpCodes.Stfld
            && list[storeIndex].operand is FieldInfo field
            && field.DeclaringType == typeof(WeatherData)
            && field.Name == nameof(WeatherData.cloudiness);
    }

    private static bool IsIntegerRoundTrip(List<CodeInstruction> list, int storeIndex)
    {
        return storeIndex >= 3
            && list[storeIndex - 1].opcode == OpCodes.Conv_R4
            && list[storeIndex - 2].opcode == OpCodes.Conv_I4
            && IsFloatClamp(list[storeIndex - 3]);
    }

    private static bool IsClampThenStore(List<CodeInstruction> list, int storeIndex)
    {
        int index = storeIndex - 1;
        while (index >= 0 && list[index].opcode == OpCodes.Nop)
        {
            index--;
        }

        return index >= 0 && IsFloatClamp(list[index]);
    }

    private static bool IsFloatClamp(CodeInstruction instruction)
    {
        return instruction.opcode == OpCodes.Call
            && instruction.operand is MethodInfo clamp
            && clamp.DeclaringType == typeof(Utils)
            && clamp.Name == nameof(Utils.Clamp)
            && clamp.ReturnType == typeof(float);
    }

    private static void NopInstruction(CodeInstruction instruction)
    {
        instruction.opcode = OpCodes.Nop;
        instruction.operand = null;
    }
}
