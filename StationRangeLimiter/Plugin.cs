using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace StationRangeLimiter
{
    [BepInPlugin( PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static ManualLogSource _logger;
        private const string PluginGuid = "JClark.DSP.StationRangeLimiter";
        private const string PluginName = "Station Range Limiter";
        private const string PluginVersion = "1.0.4.0";

        public void Awake()
        {
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(Patch));
        }
        public void Start()
        {
            _logger = base.Logger;
            _logger.LogInfo("Loaded!");
        }
    }
}