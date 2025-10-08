using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributeSpaceWarper
{
/// <summary>
/// Small helper for logging and a few debug utilities.
/// Wraps BepInEx logger and keeps TRACE logs behind a symbol.
/// </summary>
public static class ModDebug
    {
        /// <summary>Logger instance provided by the plugin at startup.</summary>
        private static ManualLogSource Logger { get; set; }

        /// <summary>Sets the logger instance used by this utility.</summary>
        public static void SetLogger(ManualLogSource logger)
        {
            Logger = logger;
        }

        /// <summary>Asserts a condition using Unity's assertion utility.</summary>
        public static void Assert(bool condition)
        {
            UnityEngine.Assertions.Assert.IsTrue(condition);
        }

        /// <summary>Logs an info-level message.</summary>
        public static void Log(object message)
        {
            Logger.Log(LogLevel.Info, message);
        }
        /// <summary>Logs an error-level message.</summary>
        public static void Error(object message)
        {
            Logger.Log(LogLevel.Error, message);
        }

        /// <summary>Logs a trace-only message when compiled with TRACE defined.</summary>
        public static void Trace(object message)
        {
#if TRACE
            Logger.Log(LogLevel.Info, "DISTR_SPACE_WARP-" + message);
#endif
        }

        /// <summary>Logs human-readable planet type for a given planet.</summary>
        public static void LogPlanetType(PlanetData planet)
        {
            //Gas planet range
            switch (planet.type)
            {
                case EPlanetType.Gas:
                    ModDebug.Log("Gas");
                    break;
                case EPlanetType.Desert:
                    ModDebug.Log("Desert");
                    break;
                case EPlanetType.Ice:
                    ModDebug.Log("Ice");
                    break;
                case EPlanetType.Ocean:
                    ModDebug.Log("Ocean");
                    break;
                case EPlanetType.Vocano:
                    ModDebug.Log("Vocano");
                    break;
                case EPlanetType.None:
                    ModDebug.Log("None");
                    break;
            };
        }

        /// <summary>Logs current build command mode from an integer code.</summary>
        public static void LogCmdMode(int mode)
        {
            //Gas planet range
            switch (mode)
            {
                case -1:
                    ModDebug.Log("CmdMode: Destruct Mode");
                    break;
                case -2:
                    ModDebug.Log("CmdMode: Upgrade Mode");
                    break;
                case 1:
                    ModDebug.Log("CmdMode: Normal Build Mode");
                    break;
                case 2:
                    ModDebug.Log("CmdMode: Build Mode - Belt");
                    break;
                case 3:
                    ModDebug.Log("CmdMode: Build Mode - Inserter");
                    break;
                case 4:
                    ModDebug.Log("CmdMode: Build Mode - Ground");
                    break;
            };
        }
    }
}
