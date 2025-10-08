// DistributionEngine.Execute — transfer loops and supplier rotation
// Executes local-first, then remote transfers with per-receiver caps.
using System;
using UnityEngine;

namespace DistributeSpaceWarper
{
    /// <summary>
    /// Execution phase: rotates suppliers to avoid bias and transfers warpers.
    /// </summary>
    internal sealed partial class DistributionEngine
    {
        /// <summary>
        /// Iterates suppliers starting from a rotating index and serves receivers.
        /// </summary>
        private void ServeSuppliers(int warperId, int targetPerStation, int reserve,
            bool costEnabled, int localCost, int remoteCost, bool remoteTransfer)
        {
            if (_suppliers.Count == 0) return;
            if (_rotationStartSupplier >= _suppliers.Count) _rotationStartSupplier = 0;
            for (int n = 0; n < _suppliers.Count; n++)
            {
                int idx = _rotationStartSupplier + n;
                if (idx >= _suppliers.Count) idx -= _suppliers.Count;
                ServeFromSupplier(_suppliers[idx], warperId, targetPerStation, reserve,
                    costEnabled, localCost, remoteCost, remoteTransfer);
            }
            _rotationStartSupplier++;
            if (_rotationStartSupplier >= _suppliers.Count) _rotationStartSupplier = 0;
        }

        /// <summary>
        /// Serves receivers from a single supplier (local first, then remote if enabled).
        /// </summary>
        private void ServeFromSupplier(StationComponent supplier, int warperId, int targetPerStation, int reserve,
            bool costEnabled, int localCost, int remoteCost, bool remoteTransfer)
        {
            int stock = GetSupplierStock(supplier, warperId, remoteTransfer);
            int available = stock - reserve;
            if (available <= 0) return;

            // First pass: same-planet receivers
            available = ServeSupplierToReceivers(supplier, warperId, targetPerStation, available, costEnabled, localCost, true);
            if (available <= 0) return;

            // Second pass: remote receivers if enabled
            if (remoteTransfer)
            {
                ServeSupplierToReceivers(supplier, warperId, targetPerStation, available, costEnabled, remoteCost, false);
            }
        }

        /// <summary>
        /// Moves warpers from a supplier to matching receivers until resources or targets are exhausted.
        /// </summary>
        private int ServeSupplierToReceivers(StationComponent supplier, int warperId, int targetPerStation, int available,
            bool costEnabled, int costPerMove, bool localOnly)
        {
            int planetId = supplier.planetId;
            for (int i = 0; i < _receivers.Count && available > 0; i++)
            {
                StationComponent r = _receivers[i];
                if (localOnly && r.planetId != planetId) continue;
                if (!localOnly && r.planetId == planetId) continue;

                int targetLeft = _receiverTargets[i];
                if (targetLeft <= 0)
                {
                    // If fair-share allocated zero, allow overflow to real deficit
                    targetLeft = Mathf.Max(0, targetPerStation - r.warperCount);
                    if (targetLeft <= 0) continue;
                }

                int capLeft = _receiverCapRemaining[i];
                if (capLeft <= 0) continue;

                int cost = costEnabled ? costPerMove : 0;
                int room = available - cost;
                if (room <= 0) break;

                int give = targetLeft;
                if (give > capLeft) give = capLeft;
                if (give > room) give = room;
                if (give <= 0) continue;

                int remove = give + cost;
                int itemId = warperId;
                supplier.TakeItem(ref itemId, ref remove, out _);

                r.warperCount += give;

                // Track targets and caps
                int newTarget = _receiverTargets[i] - give;
                _receiverTargets[i] = newTarget < 0 ? 0 : newTarget;
                int newCap = _receiverCapRemaining[i] - give;
                _receiverCapRemaining[i] = newCap < 0 ? 0 : newCap;

                available -= (give + cost);

                AddAffected(planetId: r.planetId, gid: r.gid);
            }

            return available;
        }
    }
}
