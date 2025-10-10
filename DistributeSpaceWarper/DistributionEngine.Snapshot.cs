// DistributionEngine.Snapshot — station scanning helpers
// Single-pass discovery of suppliers and receivers, no LINQ in hot paths.
using System;
using UnityEngine;

namespace DistributeSpaceWarper
{
    /// <summary>
    /// Snapshot phase: gathers suppliers and receivers from factory and galactic pools.
    /// </summary>
    internal sealed partial class DistributionEngine
    {
        /// <summary>
        /// Scans all factories and galactic pools to populate supplier and receiver lists.
        /// </summary>
        /// <param name="gt">Galactic transport (provides station pools).</param>
        /// <param name="warperId">Item ID for warpers.</param>
        /// <param name="targetPerStation">Desired warpers per receiver (cap).</param>
        /// <param name="remoteTransfer">Whether remote suppliers are eligible.</param>
        private void SnapshotStations(GalacticTransport gt, int warperId, int targetPerStation, bool remoteTransfer)
        {
            _suppliers.Clear();
            _receivers.Clear();
            _affectedPlanets.Clear();
            _affectedGids.Clear();

            var factories = gt.gameData.factories;
            if (factories == null) return;

            bool respectWarperToggle = Config.General.WarpersRequiredToggleAutomation.Value;

            for (int f = 0; f < factories.Length; f++)
            {
                var factory = factories[f];
                if (factory == null || factory.transport == null) continue;
                PlanetTransport pt = factory.transport;
                int cursor = pt.stationCursor;
                var pool = pt.stationPool;

                for (int j = 1; j < cursor; j++)
                {
                    StationComponent st = pool[j];
                    if (st == null || st.id != j || st.isCollector) continue;

                    if (IsSupplier(st, warperId, remoteTransfer))
                        _suppliers.Add(st);

                    bool requiresWarper = !respectWarperToggle || st.warperNecessary;
                    if (requiresWarper && st.warperCount < targetPerStation)
                        _receivers.Add(st);
                }
            }

            if (remoteTransfer && gt.stationPool != null)
            {
                var gpool = gt.stationPool;
                for (int i = 0; i < gpool.Length; i++)
                {
                    StationComponent st = gpool[i];
                    if (st == null || st.isCollector || !st.isStellar) continue;
                    if (HasSupplySlot(st, warperId, false, true))
                    {
                        bool found = false;
                        for (int s = 0; s < _suppliers.Count; s++)
                        {
                            if (ReferenceEquals(_suppliers[s], st)) { found = true; break; }
                        }
                        if (!found) _suppliers.Add(st);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a station can supply warpers (local or remote) and has stock.
        /// </summary>
        private bool IsSupplier(StationComponent st, int warperId, bool remoteTransfer)
        {
            bool hasLocal = HasSupplySlot(st, warperId, true, false);
            bool hasRemote = remoteTransfer && HasSupplySlot(st, warperId, false, true);
            if (!hasLocal && !hasRemote) return false;

            int stock = 0;
            var stor = st.storage;
            if (stor != null)
            {
                for (int i = 0; i < stor.Length; i++)
                {
                    var s = stor[i];
                    if (s.itemId == warperId && s.count > 0 &&
                        ((s.localLogic == ELogisticStorage.Supply) || (s.remoteLogic == ELogisticStorage.Supply)))
                    {
                        stock += s.count;
                    }
                }
            }
            return stock > 0;
        }

        /// <summary>
        /// Checks if the station has a storage slot configured for supply for the warper item.
        /// </summary>
        private static bool HasSupplySlot(StationComponent st, int warperId, bool checkLocal, bool checkRemote)
        {
            var stor = st.storage;
            if (stor == null) return false;
            for (int i = 0; i < stor.Length; i++)
            {
                var s = stor[i];
                if (s.itemId != warperId) continue;
                if (checkLocal && s.localLogic == ELogisticStorage.Supply) return true;
                if (checkRemote && s.remoteLogic == ELogisticStorage.Supply) return true;
            }
            return false;
        }

        /// <summary>
        /// Sums up all warpers available for supply (local and optionally remote) at a station.
        /// </summary>
        private int GetSupplierStock(StationComponent st, int warperId, bool remoteTransfer)
        {
            int stock = 0;
            var stor = st.storage;
            if (stor != null)
            {
                for (int i = 0; i < stor.Length; i++)
                {
                    var s = stor[i];
                    if (s.itemId == warperId && s.count > 0 &&
                        ((s.localLogic == ELogisticStorage.Supply) || (remoteTransfer && s.remoteLogic == ELogisticStorage.Supply)))
                    {
                        stock += s.count;
                    }
                }
            }
            return stock;
        }
    }
}
