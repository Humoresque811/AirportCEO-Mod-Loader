using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

internal static class FixRestoredClickRegistration
{
    [HarmonyPatch(typeof(BagController), nameof(BagController.RestoreBagFromSerializer))]
    internal static class Bags
    {
        internal static void Prefix(BagController __instance, out string __state)
        {
            __state = __instance.AssetModel?.reference?.referenceID;
        }

        internal static void Postfix(BagController __instance, string __state)
        {
            MoveRegistration(__instance, __state, __instance.ReferenceID);
        }
    }

    [HarmonyPatch(typeof(AircraftModel), nameof(AircraftModel.Initialize))]
    internal static class Aircraft
    {
        internal static void Prefix(AircraftModel __instance, out string __state)
        {
            __state = __instance.reference?.referenceID;
        }

        internal static void Postfix(AircraftModel __instance, string __state)
        {
            if (__state == null || __instance.reference == null)
            {
                return;
            }
            SelectionController selection = Singleton<SelectionController>.Instance;
            if (selection != null && selection.allClickableObjects.TryGetValue(__state, out IGameWorldClickHandler registered) && registered is AircraftController aircraft && ReferenceEquals(aircraft.am, __instance))
            {
                MoveRegistration(aircraft, __state, __instance.reference.referenceID);
            }
        }
    }

    private static void MoveRegistration(IGameWorldClickHandler clickable, string oldID, string newID)
    {
        SelectionController selection = Singleton<SelectionController>.Instance;
        if (oldID == null || oldID == newID || selection == null)
        {
            return;
        }
        if (selection.allClickableObjects.TryGetValue(oldID, out IGameWorldClickHandler registered) && ReferenceEquals(registered, clickable))
        {
            selection.allClickableObjects.Remove(oldID);
            selection.AddClickableObjectToList(clickable);
        }
    }
}
