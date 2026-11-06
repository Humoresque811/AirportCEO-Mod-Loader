using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixDailyFlightCleanup
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private static readonly FieldInfo CompletedField = AccessTools.Field(typeof(FlightModel), nameof(FlightModel.isCompleted));

    private static readonly MethodInfo AircraftIsAssignedGetter = AccessTools.PropertyGetter(typeof(FlightModel), nameof(FlightModel.AircraftIsAssigned));

    internal static void Awake()
    {
        Harmony harmony = AirportCEOModLoader.Harmony;
        MethodInfo clearCompletedFlights = AccessTools.Method(typeof(AirTrafficController), nameof(AirTrafficController.ClearCompletedFlights));
        MethodInfo moveNext = AccessTools.Method(UpdateFlightsStateMachineType(), "MoveNext") ?? throw new InvalidOperationException("UpdateFlights MoveNext not found. Daily cleanup is not enabled.");
        MethodInfo initializeAirTraffic = AccessTools.Method(typeof(AirTrafficController), nameof(AirTrafficController.InitializeAirTraffic));
        harmony.Patch(clearCompletedFlights, transpiler: new HarmonyMethod(AccessTools.Method(typeof(FixDailyFlightCleanup), nameof(KeepAssignedFlights))));
        harmony.Patch(moveNext, transpiler: new HarmonyMethod(AccessTools.Method(typeof(FixDailyFlightCleanup), nameof(ClampStaleCursor))));
        harmony.Patch(initializeAirTraffic, postfix: new HarmonyMethod(AccessTools.Method(typeof(FixDailyFlightCleanup), nameof(SubscribeToDayRollover))));
    }

    internal static void SubscribeToDayRollover(AirTrafficController __instance)
    {
        Singleton<TimeController>.Instance.Subscribe((IObserverDaysTime)__instance);
    }

    internal static IEnumerable<CodeInstruction> KeepAssignedFlights(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        int completedIndex = codes.FindIndex((CodeInstruction instruction) => instruction.LoadsField(CompletedField));
        if (completedIndex < 1 || completedIndex + 1 >= codes.Count || !IsLoadLocal(codes[completedIndex - 1]) || !IsBranchIfFalse(codes[completedIndex + 1]))
        {
            throw new InvalidOperationException("ClearCompletedFlights IL changed. Daily cleanup is not enabled and assigned flights are not protected.");
        }
        CodeInstruction loadFlight = codes[completedIndex - 1];
        object skipTarget = codes[completedIndex + 1].operand;
        codes.InsertRange(completedIndex + 2, new[]
        {
            new CodeInstruction(loadFlight.opcode, loadFlight.operand),
            new CodeInstruction(OpCodes.Callvirt, AircraftIsAssignedGetter),
            new CodeInstruction(OpCodes.Brtrue, skipTarget)
        });
        return codes;
    }

    internal static IEnumerable<CodeInstruction> ClampStaleCursor(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        Type stateType = UpdateFlightsStateMachineType();
        FieldInfo cursorField = stateType.GetFields(InstanceMembers).FirstOrDefault((FieldInfo field) => field.Name.Contains("flightIndexToValidate"));
        FieldInfo selfField = stateType.GetFields(InstanceMembers).FirstOrDefault((FieldInfo field) => field.Name.Contains("4__this"));
        if (cursorField == null || selfField == null)
        {
            throw new InvalidOperationException("UpdateFlights iterator fields changed. Daily cleanup is not enabled and the duplicate-check cursor cannot be clamped.");
        }
        FieldInfo flightsField = AccessTools.Field(typeof(AirTrafficController), nameof(AirTrafficController.flights));
        FieldInfo lengthField = AccessTools.Field(flightsField.FieldType, "Length");
        Label resume = generator.DefineLabel();
        List<Label> entryLabels = new List<Label>(codes[0].labels);
        codes[0].labels.Clear();
        codes[0].labels.Add(resume);
        codes.InsertRange(0, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0) { labels = entryLabels },
            new CodeInstruction(OpCodes.Ldfld, cursorField),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, selfField),
            new CodeInstruction(OpCodes.Ldfld, flightsField),
            new CodeInstruction(OpCodes.Ldfld, lengthField),
            new CodeInstruction(OpCodes.Blt, resume),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldc_I4_M1),
            new CodeInstruction(OpCodes.Stfld, cursorField)
        });
        return codes;
    }

    private static Type UpdateFlightsStateMachineType()
    {
        Type stateType = typeof(AirTrafficController).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public).FirstOrDefault((Type type) => type.Name.StartsWith("<UpdateFlights>d__", StringComparison.Ordinal));
        return stateType ?? throw new InvalidOperationException("UpdateFlights iterator type not found. Daily cleanup is not enabled.");
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
