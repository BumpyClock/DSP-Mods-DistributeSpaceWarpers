// DistributionEngine.Refresh — UpdateNeeds + traffic refresh batching
// Minimizes refresh calls by deduping affected planets and gids per run.
using System;

namespace DistributeSpaceWarper
{
    /// <summary>
    /// Refresh phase: updates station needs and triggers minimal traffic refreshes.
    /// </summary>
    internal sealed partial class DistributionEngine
    {
        /// <summary>
        /// Tracks affected planet and gid for post-run refresh, deduped.
        /// </summary>
        private void AddAffected(int planetId, int gid)
        {
            bool seenPlanet = false;
            for (int i = 0; i < _affectedPlanets.Count; i++)
            {
                if (_affectedPlanets[i] == planetId) { seenPlanet = true; break; }
            }
            if (!seenPlanet) _affectedPlanets.Add(planetId);

            bool seenGid = false;
            for (int i = 0; i < _affectedGids.Count; i++)
            {
                if (_affectedGids[i] == gid) { seenGid = true; break; }
            }
            if (!seenGid) _affectedGids.Add(gid);
        }

        /// <summary>
        /// Applies UpdateNeeds and refreshes local/galactic traffic once per affected entity.
        /// </summary>
        private void PostRefresh()
        {
            for (int i = 0; i < _receivers.Count; i++)
            {
                var r = _receivers[i];
                r.UpdateNeeds();
            }

            if (GameMain.data == null) return;

            for (int i = 0; i < _affectedPlanets.Count; i++)
            {
                int pid = _affectedPlanets[i];
                PlanetData planet = GameMain.data.galaxy?.PlanetById(pid);
                var transport = planet?.factory?.transport;
                if (transport != null)
                {
                    try
                    {
                        transport.RefreshStationTraffic();
                        transport.RefreshDispenserTraffic();
                    }
                    catch (Exception) { }
                }
            }

            var gt = GameMain.data.galacticTransport;
            if (gt != null)
            {
                for (int i = 0; i < _affectedGids.Count; i++)
                {
                    gt.RefreshTraffic(_affectedGids[i]);
                }
            }
        }
    }
}
