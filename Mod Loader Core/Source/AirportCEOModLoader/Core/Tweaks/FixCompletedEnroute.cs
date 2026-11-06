using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixCompletedEnroute
{
    private static readonly FieldInfo ActivatedField = AccessTools.Field(typeof(FlightModel), nameof(FlightModel.isActivated));

    private static readonly FieldInfo CompletedField = AccessTools.Field(typeof(FlightModel), nameof(FlightModel.isCompleted));

    [HarmonyPatch(typeof(AirTrafficController), nameof(AirTrafficController.GetAircraftEnroute))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> ExcludeCompletedFlights(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        int activatedIndex = codes.FindIndex((CodeInstruction instruction) => instruction.LoadsField(ActivatedField));
        if (activatedIndex < 1 || activatedIndex + 1 >= codes.Count || !IsLoadLocal(codes[activatedIndex - 1]) || !IsBranchIfFalse(codes[activatedIndex + 1]))
        {
            AirportCEOModLoader.ModLoaderLogger.LogWarning("GetAircraftEnroute IL changed. Completed flights are not excluded from the en route count.");
            return codes;
        }
        CodeInstruction loadFlight = codes[activatedIndex - 1];
        object skipTarget = codes[activatedIndex + 1].operand;
        codes.InsertRange(activatedIndex + 2, new[]
        {
            new CodeInstruction(loadFlight.opcode, loadFlight.operand),
            new CodeInstruction(OpCodes.Ldfld, CompletedField),
            new CodeInstruction(OpCodes.Brtrue, skipTarget)
        });
        return codes;
    }

    private static bool IsLoadLocal(CodeInstruction instruction)
    {
        return instruction.opcode == OpCodes.Ldloc || instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Ldloc_1 || instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Ldloc_3;
    }

    private static bool IsBranchIfFalse(CodeInstruction instruction)
    {
        return instruction.opcode == OpCodes.Brfalse || instruction.opcode == OpCodes.Brfalse_S;
    }
}
