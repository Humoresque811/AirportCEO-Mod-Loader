using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch]
internal static class FixLoadStepExceptions
{
    internal static MethodBase TargetMethod()
    {
        return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(SaveLoadGameDataController), nameof(SaveLoadGameDataController.LoadGameDataCoroutine)));
    }

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Dictionary<MethodInfo, MethodInfo> guards = new()
        {
            { AccessTools.DeclaredMethod(typeof(ConstructionController), nameof(ConstructionController.DeserializeConstructionData)), Guard(nameof(DeserializeConstructionData)) },
            { AccessTools.DeclaredMethod(typeof(BuildingController), nameof(BuildingController.ValidateStandBoardingPosition)), Guard(nameof(ValidateStandBoardingPosition)) },
            { AccessTools.DeclaredMethod(typeof(GridManager), nameof(GridManager.UpdateFullGrid)), Guard(nameof(UpdateFullGrid)) },
            { AccessTools.DeclaredMethod(typeof(TaxiwayController), nameof(TaxiwayController.ValidateTaxiwayOccupiees)), Guard(nameof(ValidateTaxiwayOccupiees)) },
            { AccessTools.DeclaredMethod(typeof(DevelopmentActionController), nameof(DevelopmentActionController.ClaimAllServiceVehicles)), Guard(nameof(ClaimAllServiceVehicles)) },
            { AccessTools.DeclaredMethod(typeof(BuildingController), nameof(BuildingController.UpdateAllRunwayLetters)), Guard(nameof(UpdateAllRunwayLetters)) },
            { AccessTools.DeclaredMethod(typeof(BuildingController), nameof(BuildingController.UpdateAllAgentChangeable)), Guard(nameof(UpdateAllAgentChangeable)) },
            { AccessTools.DeclaredMethod(typeof(JobTaskController), nameof(JobTaskController.RegenerateFloorCleaningJobTasksFilthTiles)), Guard(nameof(RegenerateFloorCleaningJobTasksFilthTiles)) },
            { AccessTools.DeclaredMethod(typeof(EnvironmentController), nameof(EnvironmentController.AdjustEnvironment)), Guard(nameof(AdjustEnvironment)) },
            { AccessTools.DeclaredMethod(typeof(TrafficController), nameof(TrafficController.VerifyTraffic)), Guard(nameof(VerifyTraffic)) },
            { AccessTools.DeclaredMethod(typeof(FloorManager), nameof(FloorManager.UpdateSubscribedObjects)), Guard(nameof(UpdateSubscribedObjects)) },
            { AccessTools.DeclaredMethod(typeof(BuildingController), nameof(BuildingController.ValidateAircraftReferenceObjects)), Guard(nameof(ValidateAircraftReferenceObjects)) },
            { AccessTools.DeclaredMethod(typeof(UIController), nameof(UIController.SetupOverlays)), Guard(nameof(SetupOverlays)) },
            { AccessTools.DeclaredMethod(typeof(AirTrafficController), nameof(AirTrafficController.RunAircraftStatusSanityCheck)), Guard(nameof(RunAircraftStatusSanityCheck)) },
            { AccessTools.DeclaredMethod(typeof(IncidentController), nameof(IncidentController.ValidateIncidents)), Guard(nameof(ValidateIncidents)) },
            { AccessTools.DeclaredMethod(typeof(BusinessController), nameof(BusinessController.SetBusinessOpenHoursState)), Guard(nameof(SetBusinessOpenHoursState)) },
            { AccessTools.DeclaredMethod(typeof(GameDataController), nameof(GameDataController.AttemptSpawnInGamePlayerAvatar)), Guard(nameof(AttemptSpawnInGamePlayerAvatar)) }
        };
        HashSet<MethodInfo> wrapped = new();
        foreach (CodeInstruction instruction in instructions)
        {
            if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && instruction.operand is MethodInfo called && guards.TryGetValue(called, out MethodInfo guard))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = guard;
                wrapped.Add(called);
            }
            yield return instruction;
        }
        if (wrapped.Count != guards.Count)
        {
            AirportCEOModLoader.ModLoaderLogger.LogWarning($"Only {wrapped.Count} of {guards.Count} load steps were found in the save loader, the others are not protected against exceptions.");
        }
    }

    private static MethodInfo Guard(string name)
    {
        return AccessTools.DeclaredMethod(typeof(FixLoadStepExceptions), name);
    }

    private static void Run(string step, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError($"{step} failed while loading the save and was skipped so the load can finish. {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
            Singleton<SaveLoadGameDataController>.Instance.exceptionOccurredDuringLoading = true;
        }
    }

    public static void DeserializeConstructionData(ConstructionController controller) => Run("ConstructionController.DeserializeConstructionData", () => controller.DeserializeConstructionData());

    public static void ValidateStandBoardingPosition(BuildingController controller) => Run("BuildingController.ValidateStandBoardingPosition", () => controller.ValidateStandBoardingPosition());

    public static void UpdateFullGrid() => Run("GridManager.UpdateFullGrid", GridManager.UpdateFullGrid);

    public static void ValidateTaxiwayOccupiees(TaxiwayController controller) => Run("TaxiwayController.ValidateTaxiwayOccupiees", () => controller.ValidateTaxiwayOccupiees());

    public static void ClaimAllServiceVehicles(DevelopmentActionController controller) => Run("DevelopmentActionController.ClaimAllServiceVehicles", () => controller.ClaimAllServiceVehicles());

    public static void UpdateAllRunwayLetters(BuildingController controller) => Run("BuildingController.UpdateAllRunwayLetters", () => controller.UpdateAllRunwayLetters());

    public static void UpdateAllAgentChangeable(BuildingController controller) => Run("BuildingController.UpdateAllAgentChangeable", () => controller.UpdateAllAgentChangeable());

    public static void RegenerateFloorCleaningJobTasksFilthTiles(JobTaskController controller) => Run("JobTaskController.RegenerateFloorCleaningJobTasksFilthTiles", () => controller.RegenerateFloorCleaningJobTasksFilthTiles());

    public static void AdjustEnvironment(EnvironmentController controller) => Run("EnvironmentController.AdjustEnvironment", () => controller.AdjustEnvironment());

    public static void VerifyTraffic(TrafficController controller) => Run("TrafficController.VerifyTraffic", () => controller.VerifyTraffic());

    public static void UpdateSubscribedObjects() => Run("FloorManager.UpdateSubscribedObjects", FloorManager.UpdateSubscribedObjects);

    public static void ValidateAircraftReferenceObjects(BuildingController controller) => Run("BuildingController.ValidateAircraftReferenceObjects", () => controller.ValidateAircraftReferenceObjects());

    public static void SetupOverlays(UIController controller, bool zones, bool queues, bool rooms, bool conveyors, bool taxiways) => Run("UIController.SetupOverlays", () => controller.SetupOverlays(zones, queues, rooms, conveyors, taxiways));

    public static void RunAircraftStatusSanityCheck(AirTrafficController controller) => Run("AirTrafficController.RunAircraftStatusSanityCheck", () => controller.RunAircraftStatusSanityCheck());

    public static void ValidateIncidents(IncidentController controller) => Run("IncidentController.ValidateIncidents", () => controller.ValidateIncidents());

    public static void SetBusinessOpenHoursState(BusinessController controller) => Run("BusinessController.SetBusinessOpenHoursState", () => controller.SetBusinessOpenHoursState());

    public static void AttemptSpawnInGamePlayerAvatar() => Run("GameDataController.AttemptSpawnInGamePlayerAvatar", GameDataController.AttemptSpawnInGamePlayerAvatar);
}
