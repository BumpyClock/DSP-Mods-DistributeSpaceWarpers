// DistributionEngine.Plan — target computation and ordering
// Sorts receivers by deficit and computes fair-share/ caps.
using System;
using UnityEngine;

namespace DistributeSpaceWarper
{
    /// <summary>
    /// Planning phase: ordering of receivers and target allocation per run.
    /// </summary>
    internal sealed partial class DistributionEngine
    {
        /// <summary>
        /// Orders receivers by descending deficit with a stable ID tiebreaker.
        /// </summary>
        private void SortReceiversByDeficit(int targetPerStation)
        {
            _receivers.Sort((a, b) =>
            {
                int da = Mathf.Max(0, targetPerStation - a.warperCount);
                int db = Mathf.Max(0, targetPerStation - b.warperCount);
                int cmp = db.CompareTo(da);
                return cmp != 0 ? cmp : a.id.CompareTo(b.id);
            });
        }

        /// <summary>
        /// Builds per-receiver targets and per-run caps, returning total demand.
        /// </summary>
        private int BuildTargetsAndCaps(int targetPerStation, int perTickCap)
        {
            _receiverTargets.Clear();
            _receiverCapRemaining.Clear();
            int totalDemand = 0;

            for (int i = 0; i < _receivers.Count; i++)
            {
                int deficit = Mathf.Max(0, targetPerStation - _receivers[i].warperCount);
                _receiverTargets.Add(deficit);
                _receiverCapRemaining.Add(perTickCap == 0 ? int.MaxValue : perTickCap);
                totalDemand += deficit;
            }
            return totalDemand;
        }

        /// <summary>
        /// Computes total available supply across suppliers after reserve.
        /// </summary>
        private int ComputeTotalSupply(int warperId, int reserve, bool remoteTransfer)
        {
            int totalSupply = 0;
            for (int s = 0; s < _suppliers.Count; s++)
            {
                int stock = GetSupplierStock(_suppliers[s], warperId, remoteTransfer);
                int available = stock - reserve;
                if (available > 0) totalSupply += available;
            }
            return totalSupply;
        }

        /// <summary>
        /// Applies proportional fair-share targets given total demand and supply.
        /// </summary>
        private void ApplyFairShareTargets(int totalDemand, int totalSupply)
        {
            for (int i = 0; i < _receiverTargets.Count; i++)
            {
                int deficit = _receiverTargets[i];
                long raw = (long)totalSupply * deficit;
                int share = (int)(raw / totalDemand);
                int capped = Math.Min(deficit, share);
                _receiverTargets[i] = capped;
            }
        }
    }
}
