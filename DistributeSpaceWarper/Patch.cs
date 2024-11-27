using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
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

        [HarmonyPatch(typeof(PlanetTransport), "GameTick", typeof(long), typeof(bool), typeof(bool))]
        [HarmonyPostfix]
        public static void PlanetTransport_GameTick_Postfix(PlanetTransport __instance)
        {
            try
            {
                if (ModDisabled)
                {
                    Debug.Log("Mod is disabled.");
                    return;
                }

                if (Time.frameCount % WarperTickCount != 0)
                {
                    Debug.Log("Skipping tick due to WarperTickCount.");
                    return;
                }

                int warperId = ItemProto.kWarperId;
                int maxWarperCount = 50;

                // Collect all stations from PlanetTransport and GalacticTransport
                List<StationComponent> stations = CollectStations(__instance, warperId);

                // Identify supplier and receiver stations
                List<StationComponent> supplierStations = new List<StationComponent>();
                List<StationComponent> receiverStations = new List<StationComponent>();
                IdentifyStations(stations, supplierStations, receiverStations, warperId, maxWarperCount);

                // Transfer warpers from suppliers to receivers
                foreach (var receiver in receiverStations)
                {
                    Debug.Log($"Transferring warpers to receiver station {receiver.id}.");
                    TransferWarpersToReceiver(__instance, receiver, supplierStations, warperId, maxWarperCount);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during GameTick Postfix: {ex.Message}");
            }
        }

        private static List<StationComponent> CollectStations(PlanetTransport planetTransport, int warperId)
        {
            List<StationComponent> stations = new List<StationComponent>(planetTransport.stationCursor);

            // Collect stations from PlanetTransport
            StationComponent[] stationPool = planetTransport.stationPool;
            for (int j = 1; j < planetTransport.stationCursor; j++)
            {
                StationComponent station = stationPool[j];
                if (station != null && station.id == j && !station.isCollector &&
                    (station.isStellar || (RemoteTransfer && IsRemoteWarperSupplier(station, warperId)) ||
                     station.storage.Any(s => s.itemId == warperId && (s.localLogic == ELogisticStorage.Supply || s.remoteLogic == ELogisticStorage.Supply))))
                {
                    stations.Add(station);
                }
            }

            // Collect stations from GalacticTransport if RemoteTransfer is enabled
            if (RemoteTransfer)
            {
                GalacticTransport galacticTransport = UIRoot.instance.uiGame.gameData.galacticTransport;

                //GalacticTransport galacticTransport = planetTransport.gameData.galacticTransport;
                foreach (StationComponent station in galacticTransport.stationPool)
                {
                    if (station != null && !station.isCollector && station.isStellar &&
                        station.storage.Any(s => s.itemId == warperId && s.remoteLogic == ELogisticStorage.Supply))
                    {
                        stations.Add(station);
                    }
                }
            }

            return stations;
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

        private static void TransferWarpersToReceiver(PlanetTransport planetTransport, StationComponent receiver, List<StationComponent> suppliers, int warperId, int maxWarperCount)
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

                UpdateTraffic(planetTransport, receiver);

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

        private static void UpdateTraffic(PlanetTransport planetTransport, StationComponent receiver)
        {
            receiver.UpdateNeeds();
            planetTransport.RefreshStationTraffic();
            planetTransport.RefreshDispenserTraffic();
            planetTransport.gameData.galacticTransport.RefreshTraffic(receiver.gid);
        }
    }
}
