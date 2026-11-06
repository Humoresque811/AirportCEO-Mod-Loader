using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch(typeof(WeatherController), "GenerateWeatherDays", new Type[]
{
    typeof(int),
    typeof(bool),
    typeof(WorldTime),
    typeof(DateTime)
})]
internal static class FixRollingWeather
{
    private const int VanillaDayIndexGuards = 3;
    private const int GuardsCorrectedByWindyAndRainFix = 2;
    private const string FailureMessage = "GenerateWeatherDays day index guards did not match the expected pattern. The method was not changed.";

    [HarmonyTranspiler]
    [HarmonyPriority(Priority.Last)]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> source = instructions.ToList();
        List<int> uncorrected = new List<int>();
        List<int> corrected = new List<int>();
        for (int i = 0; i < source.Count - 1; i++)
        {
            if (source[i].opcode != OpCodes.Ldloc_3)
            {
                continue;
            }
            if (IsZeroTestBranch(source[i + 1]))
            {
                uncorrected.Add(i);
            }
            else if (IsCorrectedZeroTest(source, i))
            {
                corrected.Add(i);
            }
        }

        if (uncorrected.Count == 0)
        {
            if (corrected.Count < VanillaDayIndexGuards)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError(FailureMessage);
            }
            return source;
        }

        bool allVanilla = uncorrected.Count == VanillaDayIndexGuards;
        bool onlyRemainingTemperature = uncorrected.Count == 1 && corrected.Count >= GuardsCorrectedByWindyAndRainFix;
        if (!allVanilla && !onlyRemainingTemperature)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError(FailureMessage);
            return source;
        }

        HashSet<int> repairs = new HashSet<int>(uncorrected);
        List<CodeInstruction> patched = new List<CodeInstruction>(source.Count + repairs.Count * 4);
        for (int i = 0; i < source.Count; i++)
        {
            patched.Add(source[i]);
            if (repairs.Contains(i))
            {
                patched.Add(new CodeInstruction(OpCodes.Ldarg_2));
                patched.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                patched.Add(new CodeInstruction(OpCodes.Ceq));
                patched.Add(new CodeInstruction(OpCodes.Or));
            }
        }
        return patched;
    }

    private static bool IsCorrectedZeroTest(List<CodeInstruction> source, int index)
    {
        if (index + 5 >= source.Count)
        {
            return false;
        }
        if (source[index + 1].opcode != OpCodes.Ldarg_2 || source[index + 2].opcode != OpCodes.Ldc_I4_0 || source[index + 3].opcode != OpCodes.Ceq || source[index + 4].opcode != OpCodes.Or)
        {
            return false;
        }
        if (!IsZeroTestBranch(source[index + 5]))
        {
            return false;
        }
        return Enumerable.Range(index + 1, 5).All(k => source[k].labels.Count == 0);
    }

    private static bool IsZeroTestBranch(CodeInstruction instruction)
    {
        OpCode opcode = instruction.opcode;
        bool isZeroTest = opcode == OpCodes.Brtrue || opcode == OpCodes.Brtrue_S || opcode == OpCodes.Brfalse || opcode == OpCodes.Brfalse_S;
        return isZeroTest && instruction.labels.Count == 0;
    }
}
