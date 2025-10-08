using System;
using HarmonyLib;
// Patch — schedules the distribution engine on true game ticks
// Keeps the Harmony patch surface minimal; all logic lives in DistributionEngine.
using UnityEngine;

namespace DistributeSpaceWarper
{
/// <summary>
/// Harmony patch host for scheduling DistributionEngine on GalacticTransport.GameTick.
/// </summary>
static class Patch
    {
        /// <summary>Whether the mod is disabled or uninstall flow is active.</summary>
        private static bool ModDisabled => Config.Utility.DisableMod.Value == true || Config.Utility.UninstallMod.Value == true;
        /// <summary>Cadence in game ticks between engine runs.</summary>
        public static int WarperTickCount => Config.General.WarperTickCount.Value;
        private static int _tickCounter = 0;

        // Patch GalacticTransport.GameTick instead of PlanetTransport.GameTick
        // This runs once per game tick and has access to all stations
        [HarmonyPatch(typeof(GalacticTransport), "GameTick")]
        [HarmonyPostfix]
        /// <summary>
        /// Postfix on GalacticTransport.GameTick; runs DistributionEngine on a fixed cadence.
        /// </summary>
        /// <param name="__instance">GalacticTransport providing access to game data/stations.</param>
        public static void GalacticTransport_GameTick_Postfix(GalacticTransport __instance)
        {
            try
            {
                if (ModDisabled) return;

                int cadence = Mathf.Max(1, WarperTickCount);
                _tickCounter++;
                if ((_tickCounter % cadence) != 0) return;

                if (__instance?.gameData == null) return;

                DistributionEngine.Instance.Run(__instance);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DistributeSpaceWarper] Error during GalacticTransport GameTick: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
