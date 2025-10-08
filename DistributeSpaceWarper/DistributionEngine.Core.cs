// DistributionEngine.Core — orchestrator entrypoint
// Part of the fair, low‑overhead warper distribution engine.
// Coordinates snapshot → plan → execute → refresh each scheduled tick.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DistributeSpaceWarper
{
    /// <summary>
    /// Central engine for distributing space warpers fairly with low overhead.
    /// Partial class split across Core/Snapshot/Plan/Execute/Refresh files.
    /// </summary>
    internal sealed partial class DistributionEngine
    {
        private static readonly DistributionEngine _instance = new DistributionEngine();
        public static DistributionEngine Instance => _instance;

        private readonly List<StationComponent> _suppliers = new List<StationComponent>(256);
        private readonly List<StationComponent> _receivers = new List<StationComponent>(256);
        private readonly List<int> _receiverTargets = new List<int>(256);
        private readonly List<int> _receiverCapRemaining = new List<int>(256);
        private readonly List<int> _affectedPlanets = new List<int>(64);
        private readonly List<int> _affectedGids = new List<int>(256);

        private int _rotationStartSupplier = 0;

        /// <summary>
        /// Executes a single distribution pass: scans stations, computes targets,
        /// transfers warpers, and refreshes traffic/needs as required.
        /// </summary>
        /// <param name="gt">Galactic transport context (provides station pools).</param>
        public void Run(GalacticTransport gt)
        {
            if (gt == null || gt.gameData == null) return;

            int warperId = ItemProto.kWarperId;
            int targetPerStation = Mathf.Clamp(Config.Advanced.WarperTarget.Value, 1, 50);
            int reserve = Mathf.Clamp(Config.Advanced.SupplierReserve.Value, 0, 50);
            int perTickCap = Mathf.Clamp(Config.Advanced.MaxPerTickPerReceiver.Value, 0, 50);
            bool fairShare = Config.Advanced.FairShareDistribution.Value;
            bool remoteTransfer = Config.General.WarperRemoteMode.Value;
            bool costEnabled = Config.General.WarperTransportCost.Value;
            int localCost = Mathf.Max(0, Config.General.WarperLocalTransportCost.Value);
            int remoteCost = Mathf.Max(0, Config.General.WarperRemoteTransportCost.Value);

            SnapshotStations(gt, warperId, targetPerStation, remoteTransfer);
            if (_receivers.Count == 0 || _suppliers.Count == 0) return;

            SortReceiversByDeficit(targetPerStation);
            int totalDemand = BuildTargetsAndCaps(targetPerStation, perTickCap);
            int totalSupply = ComputeTotalSupply(warperId, reserve, remoteTransfer);
            if (fairShare && totalDemand > 0 && totalSupply > 0)
            {
                ApplyFairShareTargets(totalDemand, totalSupply);
            }

            ServeSuppliers(warperId, targetPerStation, reserve, costEnabled, localCost, remoteCost, remoteTransfer);
            PostRefresh();
        }
    }
}
