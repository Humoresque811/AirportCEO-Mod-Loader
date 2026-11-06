using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Serializers;
using UnityEngine;

namespace AirportCEOModLoader.Core.Tweaks;

internal static class FixBrokenSaveRecords
{
    private const ulong PersonsSignature = 0x39D2C084C265C099UL;
    private const ulong AssetsSignature = 0xD5107A684BD14015UL;
    private const ulong AircraftSignature = 0xED3B9C0BDE63B08DUL;
    private const ulong VehiclesSignature = 0xCE23BF7C41485415UL;

    [HarmonyPatch(typeof(PersonSerializer), nameof(PersonSerializer.DeserializePersons))]
    internal static class Persons
    {
        internal static bool Prepare() => MatchesVanilla(AccessTools.DeclaredMethod(typeof(PersonSerializer), nameof(PersonSerializer.DeserializePersons)), PersonsSignature);

        internal static bool Prefix()
        {
            if (!TryRead(PersonSerializer.savePath + "/PersonData.json", PersonDataWrapper.CreateFromJSON, out PersonDataWrapper data))
            {
                return false;
            }
            SaveLoadGameDataController load = Singleton<SaveLoadGameDataController>.Instance;
            AirportController airport = Singleton<AirportController>.Instance;
            ObjectPoolController pool = Singleton<ObjectPoolController>.Instance;
            for (int i = 0; i < data.passengers.Count; i++)
            {
                PassengerModelSerializable saved = data.passengers[i];
                if (load.deserializeOnlyOnePerson && !load.personToDeserialize.Equals(saved.referenceID))
                {
                    continue;
                }
                PassengerController passenger = null;
                try
                {
                    passenger = pool.GetPassenger();
                    passenger.InitializePerson();
                    passenger.PassengerModel.SetPassengerModelFromSerializer(saved);
                    if (saved.isAtAirport && !saved.isPaused)
                    {
                        passenger.Launch();
                    }
                    airport.AddPersonToList(passenger);
                    if (passenger.PassengerModel.isVisitor)
                    {
                        airport.IncreaseVisitorCount();
                    }
                }
                catch (Exception ex)
                {
                    Skip("passenger", saved.referenceID, ex, true);
                    Discard(passenger);
                }
            }
            bool ceoRestored = false;
            JobTaskController jobs = Singleton<JobTaskController>.Instance;
            for (int j = 0; j < data.employees.Count; j++)
            {
                EmployeeModelSerializable saved = data.employees[j];
                if (load.deserializeOnlyOnePerson && !load.personToDeserialize.Equals(saved.referenceID))
                {
                    continue;
                }
                try
                {
                    if (saved.employeeType == Enums.EmployeeType.Contractor)
                    {
                        ContractorController contractor = pool.GetContractor();
                        contractor.Initialize(Enums.EmployeeType.Contractor);
                        contractor.EmployeeModel.SetEmployeeModelFromSerializer(saved);
                        airport.AddPersonToList(contractor);
                        if (saved.isAtAirport && !saved.isPaused)
                        {
                            contractor.Launch();
                        }
                        continue;
                    }
                    if (saved.employeeType == Enums.EmployeeType.CEO)
                    {
                        if (ceoRestored)
                        {
                            continue;
                        }
                        ceoRestored = true;
                    }
                    EmployeeController employee = pool.GetEmployee();
                    employee.Initialize(saved.employeeType);
                    employee.EmployeeModel.SetEmployeeModelFromSerializer(saved);
                    airport.AddPersonToList(employee);
                    if (employee.EmployeeModel.isHired)
                    {
                        Enums.WhiteCollarEmployeeType whiteCollarType = jobs.GetWhiteCollarEmployeeType(employee.EmployeeModel.employeeType);
                        if (whiteCollarType != Enums.WhiteCollarEmployeeType.None)
                        {
                            jobs.AddWhiteCollarWorkerToList(employee.EmployeeModel);
                            employee.EmployeeModel.isWorking = jobs.IsEmployeeWorkingHours(whiteCollarType);
                        }
                    }
                    if (saved.isAtAirport && !saved.isPaused)
                    {
                        employee.Launch();
                    }
                }
                catch (Exception ex)
                {
                    Skip("employee", saved.referenceID, ex, false);
                }
            }
            try
            {
                Singleton<ProgressionManager>.Instance.AddDeserializedAdmins();
            }
            catch (Exception ex)
            {
                Skip("executive list", "", ex, true);
            }
            if (PersonSerializer.debugPrints)
            {
                Debug.Log(data.TotalObjectsCount + " Persons were deseraliazed");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(AssetSerializer), nameof(AssetSerializer.DeserializeAssets))]
    internal static class Assets
    {
        internal static bool Prepare() => MatchesVanilla(AccessTools.DeclaredMethod(typeof(AssetSerializer), nameof(AssetSerializer.DeserializeAssets)), AssetsSignature);

        internal static bool Prefix(bool debugPrints, string savePath)
        {
            if (!TryRead(savePath + "/AssetData.json", AssetWrapper.CreateFromJSON, out AssetWrapper data))
            {
                return false;
            }
            AirportController airport = Singleton<AirportController>.Instance;
            AirTrafficController traffic = Singleton<AirTrafficController>.Instance;
            ObjectPoolController pool = Singleton<ObjectPoolController>.Instance;
            for (int i = 0; i < data.bags.Count; i++)
            {
                BagModel saved = data.bags[i];
                BagController bag = null;
                string temporaryID = null;
                try
                {
                    if (airport.GetAssetByReferenceID(saved.referenceID) != null || !BagStillNeeded(saved, airport, traffic))
                    {
                        continue;
                    }
                    bag = pool.GetBaggage();
                    bag.Initialize();
                    temporaryID = bag.ReferenceID;
                    bag.RestoreBagFromSerializer(saved);
                    bag.Launch();
                }
                catch (Exception ex)
                {
                    Skip("bag", saved.referenceID, ex, true);
                    Discard(bag, temporaryID);
                }
            }
            for (int j = 0; j < data.tools.Count; j++)
            {
                ToolModel saved = data.tools[j];
                ToolController tool = null;
                try
                {
                    if (airport.GetPersonByReferenceID<PersonController>(saved.owningParentReferenceID) == null || airport.GetAssetByReferenceID(saved.referenceID) != null)
                    {
                        continue;
                    }
                    tool = pool.GetTool();
                    tool.Initialize();
                    tool.RestoreToolFromSerializer(saved);
                    tool.Launch();
                    tool.AssetModel.assetColor = Color.white;
                }
                catch (Exception ex)
                {
                    Skip("tool", saved.referenceID, ex, true);
                    Discard(tool, null);
                }
            }
            for (int k = 0; k < data.commodities.Count; k++)
            {
                GoodsModel saved = data.commodities[k];
                GoodsController goods = null;
                try
                {
                    goods = pool.GetGoods();
                    goods.Initialize();
                    goods.RestoreGoodsFromSerializer(saved);
                    goods.Launch();
                }
                catch (Exception ex)
                {
                    Skip("goods", saved.referenceID, ex, true);
                    Discard(goods, null);
                }
            }
            if (debugPrints)
            {
                Debug.Log(data.TotalObjectsCount + " assets were deseraliazed!");
            }
            return false;
        }

        private static bool BagStillNeeded(BagModel saved, AirportController airport, AirTrafficController traffic)
        {
            switch (saved.travelDirection)
            {
                case Enums.TravelDirection.Departure:
                    CommercialFlightModel flight = traffic.GetFlightByReferenceID<CommercialFlightModel>(saved.destinationObjectReferenceID);
                    if (flight == null)
                    {
                        return false;
                    }
                    if (saved.containerType == Enums.DeliveryContainerType.CheckedInBaggage && (flight.cargoLoadingToAircraftCompleted || (flight.pushbackRequested && flight.pushbackCompleted) || flight.isCompleted))
                    {
                        return false;
                    }
                    return saved.hasBeenCheckedIn || airport.GetPersonByReferenceID<PersonController>(saved.owningParentReferenceID) != null;
                case Enums.TravelDirection.Arrival:
                    return airport.GetPersonByReferenceID<PersonController>(saved.owningParentReferenceID) != null;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(AircraftSerializer), nameof(AircraftSerializer.DeserializeAircrafts))]
    internal static class Aircraft
    {
        internal static bool Prepare() => MatchesVanilla(AccessTools.DeclaredMethod(typeof(AircraftSerializer), nameof(AircraftSerializer.DeserializeAircrafts)), AircraftSignature);

        internal static bool Prefix(bool debugPrints, string savePath)
        {
            if (!TryRead(savePath + "/AircraftData.json", Wrapper<AircraftModelSerialized>.CreateFromJSON, out Wrapper<AircraftModelSerialized> data))
            {
                return false;
            }
            AirTrafficController traffic = Singleton<AirTrafficController>.Instance;
            Transform root = FolderController.Instance.GetSceneRootTransform();
            for (int i = 0; i < data.Length; i++)
            {
                AircraftModelSerialized saved = data[i];
                AircraftController aircraft = null;
                try
                {
                    GameObject prefab = traffic.GetAircraftGameObject(saved.aircraftType);
                    FlightModel flight = traffic.GetFlightByReferenceID(saved.flightRefID);
                    if (!CustomEnums.TryGetAircraftType(saved.aircraftType, out AircraftType aircraftType))
                    {
                        Debug.LogError("Could not get aircraft with type: " + saved.aircraftType);
                        continue;
                    }
                    if (flight == null || flight.flightType == Enums.FlightType.Unspeficied)
                    {
                        continue;
                    }
                    if (prefab == null || (flight.flightType == Enums.FlightType.GeneralAviation && aircraftType.Size != Enums.ThreeStepScale.Small))
                    {
                        prefab = traffic.GetAircraftGameObject("DHC6");
                    }
                    aircraft = UnityEngine.Object.Instantiate(prefab, saved.position, saved.rotation, root).GetComponent<AircraftController>();
                    aircraft.RestoreAircraftFromSerializer(saved);
                }
                catch (Exception ex)
                {
                    Skip("aircraft", saved.referenceID, ex, true);
                    Discard(aircraft);
                }
            }
            if (debugPrints)
            {
                Debug.Log(data.Length + " Aircraft were deseraliazed");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(VehicleSerializer), nameof(VehicleSerializer.DeserializeVehicles))]
    internal static class Vehicles
    {
        internal static bool Prepare() => MatchesVanilla(AccessTools.DeclaredMethod(typeof(VehicleSerializer), nameof(VehicleSerializer.DeserializeVehicles)), VehiclesSignature);

        internal static bool Prefix(bool debugPrints, string savePath)
        {
            if (!TryRead(savePath + "/VehicleData.json", VehicleWrapper.CreateFromJSON, out VehicleWrapper data))
            {
                return false;
            }
            Transform root = FolderController.Instance.GetSceneRootTransform();
            Restore<PersonCarController, PersonCarModel>(data.personCars, root, false);
            Restore<BusController, BusModel>(data.busses, root, false);
            Restore<DeliveryTruckController, DeliveryTruckModel>(data.deliveryTrucks, root, false);
            Restore<FuelTruckController, FuelTruckModel>(data.fuelTrucks, root, false);
            Restore<ServiceTruckController, ServiceTruckModel>(data.serviceTrucks, root, false);
            Restore<ServiceCarController, ServiceCarModel>(data.serviceCars, root, false);
            Restore<PushbackTruckController, PushbackTruckModel>(data.pushbackTrucks, root, false);
            Restore<CateringTruckController, CateringTruckModel>(data.cateringTrucks, root, false);
            Restore<BeltLoaderTruckController, BeltLoaderTruckModel>(data.beltLoaderTrucks, root, true);
            Restore<StairTruckController, StairTruckModel>(data.stairTrucks, root, false);
            Restore<AirsideShuttleBusController, AirsideShuttleBusModel>(data.airsideShuttleBuses, root, false);
            Restore<DeicingTruckController, DeicingTruckModel>(data.deicingTrucks, root, false);
            Restore<AircraftCabinCleaningTruckController, AircraftCabinCleaningTruckModel>(data.aircraftCabinCleaningTrucks, root, false);
            Restore<MiniBusController, MiniBusModel>(data.miniBuses, root, false);
            Restore<AirportPoliceCarController, AirportPoliceCarModel>(data.airportPoliceCars, root, false);
            Restore<AmbulanceController, AmbulanceModel>(data.ambulances, root, false);
            Restore<FireTruckController, FireTruckModel>(data.fireTrucks, root, false);
            TrafficController traffic = Singleton<TrafficController>.Instance;
            List<TrailerController> trailers = new();
            for (int i = 0; i < data.trailers.Count; i++)
            {
                TrailerModel saved = data.trailers[i];
                TrailerController trailer = null;
                try
                {
                    if (saved.trailerType == Enums.TrailerType.Unspecified || saved.referenceID.Equals(""))
                    {
                        continue;
                    }
                    trailer = UnityEngine.Object.Instantiate(traffic.GetTrailerPrefab(saved.trailerType), root).GetComponent<TrailerController>();
                    trailer.RestoreTrailerFromSerialization(saved);
                    trailer.Initialize();
                    traffic.AddTrailerToList(trailer);
                    trailers.Add(trailer);
                }
                catch (Exception ex)
                {
                    Skip("trailer", saved.referenceID, ex, true);
                    if (trailer != null && traffic.GetTrailerByReferenceID<TrailerController>(saved.referenceID) != trailer)
                    {
                        Attempt(() => UnityEngine.Object.Destroy(trailer.gameObject));
                    }
                }
            }
            for (int j = 0; j < trailers.Count; j++)
            {
                try
                {
                    trailers[j].ReattachTrailer();
                }
                catch (Exception ex)
                {
                    Skip("trailer attachment for", trailers[j].ReferenceID, ex, true);
                }
            }
            for (int k = 0; k < trailers.Count; k++)
            {
                try
                {
                    trailers[k].Launch();
                }
                catch (Exception ex)
                {
                    Skip("trailer launch for", trailers[k].ReferenceID, ex, true);
                }
            }
            if (debugPrints)
            {
                Debug.Log(data.TotalObjectsCount + " vehicles were deseraliazed");
            }
            return false;
        }

        private static void Restore<TController, TModel>(List<TModel> saved, Transform root, bool useVariation) where TController : VehicleController where TModel : VehicleModel
        {
            TrafficController traffic = Singleton<TrafficController>.Instance;
            for (int i = 0; i < saved.Count; i++)
            {
                TModel model = saved[i];
                TController vehicle = null;
                bool registered = false;
                try
                {
                    GameObject prefab = traffic.GetVehiclePrefab(model.vehicleType, model.vehicleSubType, useVariation ? model.variationIndex : 0);
                    vehicle = UnityEngine.Object.Instantiate(prefab, root).GetComponent<TController>();
                    vehicle.Initialize();
                    vehicle.RestoreVehicleFromSerialization(model);
                    traffic.AddVehicleToList(vehicle);
                    registered = true;
                    vehicle.Launch();
                }
                catch (Exception ex)
                {
                    Skip("vehicle", model.referenceID, ex, true);
                    Discard(vehicle, registered);
                }
            }
        }
    }

    private static bool MatchesVanilla(MethodBase method, ulong expected)
    {
        if (method == null)
        {
            return false;
        }
        ulong hash = 14695981039346656037UL;
        foreach (KeyValuePair<OpCode, object> instruction in PatchProcessor.ReadMethodBody(method))
        {
            if (instruction.Value is not MemberInfo member)
            {
                continue;
            }
            foreach (char c in member.DeclaringType?.Name + "::" + member.Name + "\n")
            {
                hash = (hash ^ c) * 1099511628211UL;
            }
        }
        if (hash == expected)
        {
            return true;
        }
        AirportCEOModLoader.ModLoaderLogger.LogWarning($"{method.DeclaringType.Name}.{method.Name} is not the version this loader was written for ({hash:X16}), so broken records in that part of a save are not skipped.");
        return false;
    }

    private static bool TryRead<T>(string path, Func<string, T> parse, out T data) where T : class
    {
        try
        {
            data = parse(Utils.ReadFile(path));
            if (data != null)
            {
                return true;
            }
            Skip("save file", path, new InvalidOperationException("The file is empty or not valid save data."), true);
        }
        catch (Exception ex)
        {
            Skip("save file", path, ex, true);
        }
        data = null;
        return false;
    }

    private static void Skip(string record, string referenceID, Exception ex, bool reportLoadIssue)
    {
        AirportCEOModLoader.ModLoaderLogger.LogError($"Skipped broken {record} {referenceID} while loading the save. {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
        if (reportLoadIssue)
        {
            Singleton<SaveLoadGameDataController>.Instance.exceptionOccurredDuringLoading = true;
        }
    }

    private static void Discard(PersonController person)
    {
        if (person == null)
        {
            return;
        }
        string referenceID = null;
        Attempt(() => referenceID = person.ReferenceID);
        Attempt(person.Reset);
        Attempt(() => person.PersonModel.moveData.UnSubscribeFromMovement());
        Attempt(() =>
        {
            if (person.hasBeenLaunched)
            {
                FloorManager.UnSubscribe(person.floorManagerIndex);
            }
        });
        Attempt(() => ForgetClickable(person, referenceID));
        Attempt(() =>
        {
            AirportController airport = Singleton<AirportController>.Instance;
            if (referenceID != null && airport.GetPersonByReferenceID<PersonController>(referenceID) == person)
            {
                airport.RemoveFromPersonList(person);
            }
        });
        Attempt(() => UnityEngine.Object.Destroy(person.gameObject));
    }

    private static void Discard(AircraftController aircraft)
    {
        if (aircraft == null)
        {
            return;
        }
        AirTrafficController traffic = Singleton<AirTrafficController>.Instance;
        Attempt(aircraft.StopAllCoroutines);
        Attempt(() => FloorManager.UnSubscribe(aircraft.floorManagerIndex));
        Attempt(() => ForgetClickable(aircraft, aircraft.am?.reference?.referenceID));
        Attempt(() => ForgetClickable(aircraft, aircraft.am?.modelSerialized?.referenceID));
        string referenceID = aircraft.am?.reference?.referenceID;
        if (referenceID != null && traffic.referenceToAircraft.TryGetValue(referenceID, out AircraftController listed) && ReferenceEquals(listed, aircraft))
        {
            traffic.referenceToAircraft.Remove(referenceID);
            RemoveExact(traffic.allAircraft, aircraft);
        }
        Attempt(() => UnityEngine.Object.Destroy(aircraft.gameObject));
    }

    private static void RemoveExact<T>(DynamicArray<T> array, T item) where T : class, IComparable<T>
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (ReferenceEquals(array.array[i], item))
            {
                array.array[i] = array.array[array.Length - 1];
                array.array[array.Length - 1] = null;
                array.Length--;
                return;
            }
        }
    }

    private static void Discard(VehicleController vehicle, bool registered)
    {
        if (vehicle == null)
        {
            return;
        }
        TrafficController traffic = Singleton<TrafficController>.Instance;
        Attempt(vehicle.StopAllCoroutines);
        Attempt(() => FloorManager.UnSubscribe(vehicle.floorManagerIndex));
        Attempt(() => ForgetClickable(vehicle, vehicle.vehicleModel?.referenceID));
        if (registered && vehicle.vehicleModel != null && traffic.allVehicles.TryGetValue(vehicle.vehicleModel.referenceID, out VehicleController listed) && ReferenceEquals(listed, vehicle))
        {
            traffic.allVehicles.Remove(vehicle.vehicleModel.referenceID);
            RemoveExact(traffic.allVehiclesArray, vehicle);
        }
        Attempt(() => UnityEngine.Object.Destroy(vehicle.gameObject));
    }

    private static void Discard(AssetController asset, string temporaryID)
    {
        if (asset == null)
        {
            return;
        }
        string referenceID = null;
        Attempt(() => referenceID = asset.ReferenceID);
        Attempt(asset.StopAllCoroutines);
        Attempt(asset.ResetAsset);
        Attempt(() => ForgetClickable(asset, temporaryID));
        Attempt(() => ForgetClickable(asset, referenceID));
        Attempt(() =>
        {
            AirportController airport = Singleton<AirportController>.Instance;
            if (referenceID != null && airport.GetAssetByReferenceID(referenceID) == asset)
            {
                airport.RemoveFromAssetList(asset);
            }
        });
        Attempt(() => UnityEngine.Object.Destroy(asset.gameObject));
    }

    private static void ForgetClickable(object owner, string referenceID)
    {
        SelectionController selection = Singleton<SelectionController>.Instance;
        if (selection != null && !string.IsNullOrEmpty(referenceID) && selection.allClickableObjects.TryGetValue(referenceID, out IGameWorldClickHandler registered) && ReferenceEquals(registered, owner))
        {
            selection.allClickableObjects.Remove(referenceID);
        }
    }

    private static void Attempt(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
        }
    }
}
