using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixDeskCandidates
{
    [HarmonyPatch(typeof(AirportController), nameof(AirportController.GetPreferredDeskByDistanceAndQueueLoad))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> GetPreferredDeskByDistanceAndQueueLoadTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .Start()
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(ci => ci.opcode == OpCodes.Bne_Un || ci.opcode == OpCodes.Bne_Un_S),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ret))
            .ThrowIfInvalid("singleton desk return not found");
        List<Label> blockLabels = new List<Label>(matcher.Instruction.labels);
        matcher.RemoveInstructions(8).AddLabels(blockLabels);

        matcher.Start()
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Stloc_2))
            .ThrowIfInvalid("best index initializer not found")
            .Set(OpCodes.Ldc_I4_M1, null);

        matcher.Start()
            .MatchForward(false, new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(ci => ci.opcode == OpCodes.Ldloc || ci.opcode == OpCodes.Ldloc_S), new CodeMatch(ci => ci.opcode == OpCodes.Blt || ci.opcode == OpCodes.Blt_S))
            .ThrowIfInvalid("best index guard not found");
        object guardTarget = matcher.InstructionAt(2).operand;
        matcher.Advance(1)
            .Set(OpCodes.Ldc_I4_0, null)
            .Advance(1)
            .Set(OpCodes.Bge, guardTarget);

        return matcher.InstructionEnumeration();
    }
}
