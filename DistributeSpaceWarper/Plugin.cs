using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Diagnostics;

namespace DistributeSpaceWarper
{
/// <summary>
/// BepInEx plugin entrypoint. Binds configuration and applies Harmony patches.
/// </summary>
[BepInPlugin( PluginGuid, PluginName, PluginVersion)]
[BepInProcess("DSPGAME.exe")]
public class Plugin : BaseUnityPlugin
    {
        private static ManualLogSource _logger;
        private const string PluginGuid = "BumpyClock.DSP.DistributeSpaceWarper";
        private const string PluginName = "Distribute Space Warper";
        private const string PluginVersion = "1.0.7";

    /// <summary>
    /// Initializes configuration and applies Harmony patches for this plugin.
    /// </summary>
    public void Awake()
    {
        Harmony harmony = new Harmony(PluginGuid);
        DistributeSpaceWarper.Config.Init(Config);
        harmony.PatchAll(typeof(Plugin));
        harmony.PatchAll(typeof(Patch));
    }
    /// <summary>
    /// Sets up logging source and optional debug breakpoint in DEBUG builds.
    /// </summary>
    public void Start()
    {
        _logger = base.Logger;
        _logger.LogInfo("Loaded!");

            ModDebug.SetLogger(_logger);

#if DEBUG
            Debugger.Break();
#endif
        }
    }
}
