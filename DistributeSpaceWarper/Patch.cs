using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DistributeSpaceWarper
{
    static class Patch
    {
        private static bool ModDisabled => Config.Utility.DisableMod.Value == true || Config.Utility.UninstallMod.Value == true;
        private static bool RemoteTransfer => Config.General.WarperRemoteMode.Value == true;
        public static int WarperTickCount => Config.General.WarperTickCount.Value;

        public static bool WarperTransportCost => Config.General.WarperTransportCost.Value == true;
        public static int WarperLocalTransportCost => Config.General.WarperLocalTransportCost.Value;
        public static int WarperRemoteTransportCost => Config.General.WarperRemoteTransportCost.Value;

        // Patch GalacticTransport.GameTick instead of PlanetTransport.GameTick
        // This runs once per game tick and has access to all stations
        [HarmonyPatch(typeof(GalacticTransport), "GameTick")]
        [HarmonyPostfix]
        public static void GalacticTransport_GameTick_Postfix(GalacticTransport __instance)
        {
            try
            {
                if (ModDisabled)
                {
                    return;
                }

                if (Time.frameCount % WarperTickCount != 0)
                {
                    return;
                }

                // Access GameData through GalacticTransport
                if (__instance.gameData == null)
                {
                    return;
                }

                int warperId = ItemProto.kWarperId;
                int maxWarperCount = 50;

                // Collect all stations from all planets and galactic transport
                List<StationComponent> allStations = new List<StationComponent>();

                // Iterate through all factory planets
                foreach (var planetData in __instance.gameData.factories)
                {
                    if (planetData == null || planetData.transport == null)
                        continue;

                    PlanetTransport planetTransport = planetData.transport;
                    for (int j = 1; j < planetTransport.stationCursor; j++)
                    {
                        StationComponent station = planetTransport.stationPool[j];
                        if (station != null && station.id == j && !station.isCollector &&
                            (station.isStellar || (RemoteTransfer && IsRemoteWarperSupplier(station, warperId)) ||
                             station.storage.Any(s => s.itemId == warperId && (s.localLogic == ELogisticStorage.Supply || s.remoteLogic == ELogisticStorage.Supply))))
                        {
                            allStations.Add(station);
                        }
                    }
                }

                // Also add galactic transport stations if RemoteTransfer is enabled
                if (RemoteTransfer)
                {
                    foreach (StationComponent station in __instance.stationPool)
                    {
                        if (station != null && !station.isCollector && station.isStellar &&
                            station.storage.Any(s => s.itemId == warperId && s.remoteLogic == ELogisticStorage.Supply))
                        {
                            if (!allStations.Contains(station))
                            {
                                allStations.Add(station);
                            }
                        }
                    }
                }

                // Identify supplier and receiver stations
                List<StationComponent> supplierStations = new List<StationComponent>();
                List<StationComponent> receiverStations = new List<StationComponent>();
                IdentifyStations(allStations, supplierStations, receiverStations, warperId, maxWarperCount);

                // Transfer warpers from suppliers to receivers
                foreach (var receiver in receiverStations)
                {
                    TransferWarpersToReceiver(receiver, supplierStations, warperId, maxWarperCount);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DistributeSpaceWarper] Error during GalacticTransport GameTick: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private static void IdentifyStations(List<StationComponent> stations, List<StationComponent> supplierStations, List<StationComponent> receiverStations, int warperId, int maxWarperCount)
        {
            foreach (var station in stations)
            {
                if (IsWarperSupplier(station, warperId))
                {
                    Debug.Log($"Station {station.id} is added to supplier stations.");
                    supplierStations.Add(station);
                }
                else if (NeedsWarpers(station, maxWarperCount))
                {
                    Debug.Log($"Station {station.id} needs warpers.");
                    receiverStations.Add(station);
                }
            }
        }

        private static bool IsWarperSupplier(StationComponent station, int warperId)
        {
            bool isSupplier = station.warperCount > 0 && station.storage.Any(s => s.itemId == warperId && s.localLogic == ELogisticStorage.Supply);
            if (isSupplier)
            {
                Debug.Log($"Station {station.id} is identified as a warper supplier.");
            }
            return isSupplier;
        }

        private static bool IsRemoteWarperSupplier(StationComponent station, int warperId)
        {
            bool isRemoteSupplier = station.storage.Any(s => s.itemId == warperId && s.remoteLogic == ELogisticStorage.Supply);
            if (isRemoteSupplier)
            {
                Debug.Log($"Station {station.id} is identified as a remote warper supplier.");
            }
            return isRemoteSupplier;
        }

        private static bool NeedsWarpers(StationComponent station, int maxWarperCount)
        {
            bool needsWarpers = (station.warperCount < maxWarperCount && station.warperNecessary);
            if (needsWarpers)
            {
                Debug.Log($"Station {station.id} needs warpers (current: {station.warperCount}, max: {maxWarperCount}).");
            }
            return needsWarpers;
        }

        private static void TransferWarpersToReceiver(StationComponent receiver, List<StationComponent> suppliers, int warperId, int maxWarperCount)
        {
            int neededWarperCount = maxWarperCount - receiver.warperCount;
            Debug.Log($"Receiver station {receiver.id} needs {neededWarperCount} warpers.");

            if (neededWarperCount <= 0)
            {
                Debug.Log($"Receiver station {receiver.id} does not need more warpers.");
                return;
            }

            foreach (var supplier in suppliers)
            {
                if (supplier.warperCount <= 0)
                {
                    Debug.Log($"Supplier station {supplier.id} has no warpers left to transfer.");
                    continue;
                }

                int transportCost = CalculateTransportCost(supplier, receiver);
                Debug.Log($"Transport cost from supplier {supplier.id} to receiver {receiver.id} is {transportCost}.");

                int supplierWarperCount = supplier.storage
                    .Where(s => s.itemId == warperId && (s.localLogic == ELogisticStorage.Supply || s.remoteLogic == ELogisticStorage.Supply))
                    .Sum(s => s.count);

                int transferableWarperCount = supplierWarperCount - transportCost;
                int transferAmount = Mathf.Min(neededWarperCount, transferableWarperCount);
                transferAmount = Mathf.Min(transferAmount, supplierWarperCount - transportCost);

                if (transferAmount <= 0)
                {
                    Debug.Log($"No transferable warpers from supplier {supplier.id} to receiver {receiver.id}.");
                    continue;
                }

                int itemCountToRemove = transferAmount + transportCost;
                Debug.Log($"Removing {itemCountToRemove} warpers from supplier station {supplier.id}.");
                supplier.TakeItem(ref warperId, ref itemCountToRemove, out _);

                receiver.warperCount += transferAmount;
                Debug.Log($"Added {transferAmount} warpers to receiver station {receiver.id} (new count: {receiver.warperCount}).");

                UpdateTraffic(receiver);

                neededWarperCount -= transferAmount;

                if (neededWarperCount <= 0)
                {
                    Debug.Log($"Receiver station {receiver.id} has received enough warpers.");
                    break;
                }
            }
        }

        private static int CalculateTransportCost(StationComponent supplier, StationComponent receiver)
        {
            return WarperTransportCost ? (supplier.planetId == receiver.planetId ? WarperLocalTransportCost : WarperRemoteTransportCost) : 0;
        }

        private static void UpdateTraffic(StationComponent receiver)
        {
            receiver.UpdateNeeds();

            // Get the planet transport from GameMain
            if (GameMain.data == null)
                return;

            PlanetData planet = GameMain.data.galaxy?.PlanetById(receiver.planetId);
            if (planet?.factory?.transport != null)
            {
                // These methods might have been removed in 0.10.33, wrap in try-catch
                try
                {
                    planet.factory.transport.RefreshStationTraffic();
                    planet.factory.transport.RefreshDispenserTraffic();
                }
                catch (Exception)
                {
                    // Methods don't exist in this version, skip
                }
            }

            // Refresh galactic traffic
            if (GameMain.data.galacticTransport != null)
            {
                GameMain.data.galacticTransport.RefreshTraffic(receiver.gid);
            }
        }
    }
}
