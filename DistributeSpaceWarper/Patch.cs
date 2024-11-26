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

        [HarmonyPatch(typeof(PlanetTransport), "GameTick", typeof(long), typeof(bool), typeof(bool))]
        [HarmonyPostfix]
        public static void PlanetTransport_GameTick_Postfix(PlanetTransport __instance)
        {
            if (ModDisabled)
                return;

            // Run every 60 ticks to reduce performance impact
            if (Time.frameCount % 60 != 0)
                return;

            int warperId = ItemProto.kWarperId;
            int maxWarperCount = 50;

            // Collect all stations on the planet
            List<StationComponent> stations = new List<StationComponent>();
            for (int j = 1; j < __instance.stationCursor; j++)
            {
                StationComponent station = __instance.stationPool[j];
                if (station != null && station.id == j && !station.isCollector && station.isStellar)
                {
                    stations.Add(station);
                }
            }

            // Identify supplier and receiver stations
            var supplierStations = stations.Where(s => IsWarperSupplier(s, warperId)).ToList();
            var receiverStations = stations.Where(s => NeedsWarpers(s, maxWarperCount)).ToList();

            // Transfer warpers from suppliers to receivers
            foreach (var receiver in receiverStations)
            {
                TransferWarpersToReceiver(__instance, receiver, supplierStations, warperId, maxWarperCount);
            }
        }

        private static bool IsWarperSupplier(StationComponent station, int warperId)
        {
            return station.warperCount > 0 && station.storage.Any(s => s.itemId == warperId && s.localLogic == ELogisticStorage.Supply);
        }

        private static bool NeedsWarpers(StationComponent station, int maxWarperCount)
        {
            // Station must be set to demand or none and have less than the max warper count
            return station.warperCount < maxWarperCount;
        }

        private static void TransferWarpersToReceiver(PlanetTransport planetTransport, StationComponent receiver, List<StationComponent> suppliers, int warperId, int maxWarperCount)
        {
            int neededWarperCount = maxWarperCount - receiver.warperCount;

            // Ensure the receiver has space for more warpers
            if (neededWarperCount <= 0)
                return;

            foreach (var supplier in suppliers)
            {
                if (supplier.warperCount <= 0)
                    continue; // Supplier has no warpers

                int transferAmount = Mathf.Min(neededWarperCount, supplier.warperCount);

                // Remove warpers from supplier
                supplier.warperCount -= transferAmount;

                // Add warpers to receiver
                receiver.warperCount += transferAmount;

                // Update station needs and traffic
                receiver.UpdateNeeds();
                planetTransport.RefreshStationTraffic();
                planetTransport.RefreshDispenserTraffic();
                planetTransport.gameData.galacticTransport.RefreshTraffic(receiver.gid);

                neededWarperCount -= transferAmount;

                if (neededWarperCount <= 0)
                    break; // Receiver has received enough warpers
            }
        }
    }
}
