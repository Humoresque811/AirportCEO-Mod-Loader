using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixLoadedInteractionAvailability
{
    private static readonly AccessTools.FieldRef<InteractionItemModel, DynamicArray<InteractionOccupancyData>> AnyPerson = AccessTools.FieldRefAccess<InteractionItemModel, DynamicArray<InteractionOccupancyData>>("availableOccupiableAnyPersonInteractionPoints");

    private static readonly AccessTools.FieldRef<InteractionItemModel, DynamicArray<InteractionOccupancyData>> Passenger = AccessTools.FieldRefAccess<InteractionItemModel, DynamicArray<InteractionOccupancyData>>("availableOccupiablePassengerInteractionPoints");

    private static readonly AccessTools.FieldRef<InteractionItemModel, DynamicArray<InteractionOccupancyData>> Employee = AccessTools.FieldRefAccess<InteractionItemModel, DynamicArray<InteractionOccupancyData>>("availableOccupiableEmployeeInteractionPoints");

    [HarmonyPatch(typeof(InteractionItemModel), nameof(InteractionItemModel.SetObjectFromSerializer), new[] { typeof(InteractionItemSerializable) })]
    [HarmonyPostfix]
    private static void RebuildAvailablePoints(InteractionItemModel __instance)
    {
        DynamicArray<InteractionOccupancyData> anyPerson = AnyPerson(__instance);
        DynamicArray<InteractionOccupancyData> passenger = Passenger(__instance);
        DynamicArray<InteractionOccupancyData> employee = Employee(__instance);
        anyPerson.Clear();
        passenger.Clear();
        employee.Clear();
        InteractionOccupancyData[] points = __instance.interactionPoints;
        for (int i = 0; i < points.Length; i++)
        {
            InteractionOccupancyData point = points[i];
            if (point.isNotOccupiable || !point.OccupyingObjectReferenceID.Equals(""))
            {
                continue;
            }

            switch (point.allowedPersonType)
            {
                case Enums.PersonType.Any:
                    anyPerson.Add(point);
                    break;
                case Enums.PersonType.Passenger:
                    passenger.Add(point);
                    break;
                case Enums.PersonType.Employee:
                    employee.Add(point);
                    break;
            }
        }
    }
}
