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
    private const string FailureMessage = "GenerateWeatherDays day index guards did not match the expected pattern. The method was not changed.";

    [HarmonyTranspiler]
    [HarmonyPriority(Priority.Last)]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> source = instructions.ToList();
        List<int> uncorrected = new List<int>();
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
        }

        if (uncorrected.Count == 0)
        {
            return source;
        }

        if (uncorrected.Count > VanillaDayIndexGuards)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError(FailureMessage);
            return source;
        }

        HashSet<int> repairs = new HashSet<int>(uncorrected);
        AirportCEOModLoader.ModLoaderLogger.LogInfo("Rolled forecast days: widened " + repairs.Count + " day index guards.");
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

    private static bool IsZeroTestBranch(CodeInstruction instruction)
    {
        OpCode opcode = instruction.opcode;
        bool isZeroTest = opcode == OpCodes.Brtrue || opcode == OpCodes.Brtrue_S || opcode == OpCodes.Brfalse || opcode == OpCodes.Brfalse_S;
        return isZeroTest && instruction.labels.Count == 0;
    }
}
