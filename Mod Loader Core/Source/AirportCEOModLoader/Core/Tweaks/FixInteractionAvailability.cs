using System;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixInteractionAvailability
{
    private static readonly Action<InteractionItemModel, Enums.OperatorType, InteractionOccupancyData> UpdateAvailability = AccessTools.MethodDelegate<Action<InteractionItemModel, Enums.OperatorType, InteractionOccupancyData>>(AccessTools.Method(typeof(InteractionItemModel), "AddRemoveInteractionPointToAvailabilityList"));

    [HarmonyPatch(typeof(InteractionItemModel), nameof(InteractionItemModel.VerifyCurrentOccupation))]
    [HarmonyPrefix]
    private static bool ReleaseStaleOccupants(InteractionItemModel __instance)
    {
        InteractionOccupancyData[] points = __instance.interactionPoints;
        for (int i = 0; i < points.Length; i++)
        {
            InteractionOccupancyData point = points[i];
            if (point.ignoreVerification)
            {
                continue;
            }

            string occupant = point.OccupyingObjectReferenceID;
            if (occupant.Equals(""))
            {
                continue;
            }

            PersonController person = Singleton<AirportController>.Instance.GetPersonByReferenceID<PersonController>(occupant);
            if (person != null && Utils.GetDistance2D(person.Position, point.transform.position) <= 5f)
            {
                continue;
            }

            point.OccupyingObjectReferenceID = "";
            if (!point.isNotOccupiable && !IsHeadingToPoint(person, __instance, i))
            {
                point.hasBeenOccupied = false;
                UpdateAvailability(__instance, Enums.OperatorType.Add, point);
            }
        }

        return false;
    }

    private static bool IsHeadingToPoint(PersonController person, InteractionItemModel item, int pointIndex)
    {
        return person != null && person.PersonModel.currentInteractionPointIndex == pointIndex && item.ReferenceID.Equals(person.PersonModel.currentInteractionItemReferenceID);
    }
}
