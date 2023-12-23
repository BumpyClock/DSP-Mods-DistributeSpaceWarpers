using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace DistributeSpaceWarper
{
    [BepInPlugin( PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static ManualLogSource _logger;
        private const string PluginGuid = "JClark.DSP.DistributeSpaceWarper";
        private const string PluginName = "Distribute Space Warper";
        private const string PluginVersion = "1.0.11.0";

        public void Awake()
        {
            Harmony harmony = new Harmony(PluginGuid);
            DistributeSpaceWarper.Config.Init(Config);
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