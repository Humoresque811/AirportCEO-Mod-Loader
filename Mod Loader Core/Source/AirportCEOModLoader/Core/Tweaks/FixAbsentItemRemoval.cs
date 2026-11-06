using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch]
internal static class FixAbsentItemRemoval
{
    private static readonly MethodInfo RemoveIfPresentMethod = AccessTools.Method(typeof(FixAbsentItemRemoval), nameof(RemoveIfPresent));

    internal static IEnumerable<MethodBase> TargetMethods()
    {
        return new MethodBase[]
        {
            AccessTools.DeclaredMethod(typeof(AirTrafficController), nameof(AirTrafficController.RemoveFromAircraftList)),
            AccessTools.DeclaredMethod(typeof(AirTrafficController), nameof(AirTrafficController.RemoveFromFlightList)),
            AccessTools.DeclaredMethod(typeof(AirportController), nameof(AirportController.RemoveFromAssetList)),
            AccessTools.DeclaredMethod(typeof(AirportController), nameof(AirportController.ValidateExistingConstructionMaterial)),
            AccessTools.DeclaredMethod(typeof(AirportDeliveryController), nameof(AirportDeliveryController.RemoveFromShipmentLists)),
            AccessTools.DeclaredMethod(typeof(BusinessController), nameof(BusinessController.RemoveFromBusinessList)),
            AccessTools.DeclaredMethod(typeof(TaxiwayController), nameof(TaxiwayController.RemoveBuilderNode)),
            AccessTools.DeclaredMethod(typeof(EnvironmentController), nameof(EnvironmentController.AttemptRemoveTerrainObjects)),
            AccessTools.DeclaredMethod(typeof(BuildingController), nameof(BuildingController.RemoveFromObjectList)),
            AccessTools.DeclaredMethod(typeof(BeltAnimatorManager), nameof(BeltAnimatorManager.RemoveFromAnimations)),
            AccessTools.DeclaredMethod(typeof(GridReference), nameof(GridReference.RemoveFromInformation)),
            AccessTools.DeclaredMethod(typeof(JobTaskController), nameof(JobTaskController.RemoveJobTaskFromCollections)),
            AccessTools.DeclaredMethod(typeof(NotificationController), nameof(NotificationController.RemoveNotification), new[] { typeof(Notification) }),
            AccessTools.DeclaredMethod(typeof(NotificationController), nameof(NotificationController.RemoveWorldNotification)),
            AccessTools.DeclaredMethod(typeof(TrafficController), nameof(TrafficController.RemoveVehicleFromList)),
            AccessTools.DeclaredMethod(typeof(TrafficController), nameof(TrafficController.RemoveTrailerFromList)),
            AccessTools.DeclaredMethod(typeof(InteractionItemModel), nameof(InteractionItemModel.UnoccupyInteractionPoint)),
            AccessTools.DeclaredMethod(typeof(InteractionItemModel), nameof(InteractionItemModel.AddRemoveInteractionPointToAvailabilityList)),
            AccessTools.DeclaredMethod(typeof(AircraftBreakingSystem), nameof(AircraftBreakingSystem.UnSubscribeFromSystem)),
            AccessTools.DeclaredMethod(typeof(AircraftDirectMovementSystem), nameof(AircraftDirectMovementSystem.UnSubscribeFromSystem)),
            AccessTools.DeclaredMethod(typeof(AircraftPathMovementSystem), nameof(AircraftPathMovementSystem.UnSubscribeFromSystem))
        }.Where(method => method != null);
    }

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && instruction.operand is MethodInfo called && TryGetItemType(called, out Type itemType))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = RemoveIfPresentMethod.MakeGenericMethod(itemType);
            }
            yield return instruction;
        }
    }

    private static bool TryGetItemType(MethodInfo called, out Type itemType)
    {
        itemType = null;
        Type declaring = called.DeclaringType;
        if (called.Name != nameof(DynamicArray<int>.Remove) || called.IsStatic || declaring == null || !declaring.IsGenericType || declaring.GetGenericTypeDefinition() != typeof(DynamicArray<>))
        {
            return false;
        }
        ParameterInfo[] parameters = called.GetParameters();
        Type candidate = declaring.GetGenericArguments()[0];
        if (parameters.Length != 1 || parameters[0].ParameterType != candidate)
        {
            return false;
        }
        itemType = candidate;
        return true;
    }

    public static void RemoveIfPresent<T>(DynamicArray<T> list, T item) where T : IComparable<T>
    {
        T[] array = list.array;
        int length = list.Length;
        for (int i = 0; i < length; i++)
        {
            if (array[i] != null && array[i].CompareTo(item) == 0)
            {
                array[i] = array[length - 1];
                array[length - 1] = default;
                list.Length = length - 1;
                if (list.autoShrink)
                {
                    list.AttemptShrinkArray();
                }
                return;
            }
        }
    }
}
