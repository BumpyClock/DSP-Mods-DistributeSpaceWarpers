// Configuration bindings for Distribute Space Warper
// Groups settings into General, Utility, and Advanced sections.
﻿namespace DistributeSpaceWarper
{
    using BepInEx.Configuration;

    /// <summary>
    /// Central configuration holder. Bind once in Plugin.Awake and access via static fields.
    /// </summary>
    public static class Config
    {
        private static readonly string GENERAL_SECTION = "General";
        private static readonly string UTILITY_SECTION = "Utility";

        /// <summary>
        /// General gameplay and cadence settings.
        /// </summary>
        public static class General
        {
            /// <summary>
            /// Enables sourcing warpers from remote (cross‑planet) suppliers.
            /// </summary>
            /// <value>Default: false.</value>
            public static ConfigEntry<bool> WarperRemoteMode;

            /// <summary>
            /// If enabled, transporting warpers consumes warpers according to the cost settings.
            /// </summary>
            /// <value>Default: true.</value>
            public static ConfigEntry<bool> WarperTransportCost;

            /// <summary>
            /// Cost in warpers per remote (cross‑planet) transfer when transport cost is enabled.
            /// </summary>
            /// <value>Default: 2. Range: 0–10.</value>
            public static ConfigEntry<int> WarperRemoteTransportCost;

            /// <summary>
            /// Number of game ticks between distribution runs.
            /// </summary>
            /// <value>Default: 60. Range: 1–275.</value>
            public static ConfigEntry<int> WarperTickCount;

            /// <summary>
            /// Cost in warpers per local (same‑planet) transfer when transport cost is enabled.
            /// </summary>
            /// <value>Default: 1. Range: 0–10.</value>
            public static ConfigEntry<int> WarperLocalTransportCost;

            /// <summary>
            /// When the in‑game "Warpers Required" toggle is on, auto‑fills the warper slot; stops when off.
            /// </summary>
            /// <value>Default: true.</value>
            public static ConfigEntry<bool> WarpersRequiredToggleAutomation;
        }

        /// <summary>
        /// Utility toggles for disabling/uninstalling the mod safely.
        /// </summary>
        public static class Utility
        {
            /// <summary>
            /// Disables all mod effects without removing extra slots; useful for troubleshooting.
            /// </summary>
            /// <value>Default: false.</value>
            public static ConfigEntry<bool> DisableMod;

            /// <summary>
            /// Runs the uninstall flow to remove extra slots from all ILS. BACK UP YOUR SAVE FIRST.
            /// </summary>
            /// <value>Default: false.</value>
            public static ConfigEntry<bool> UninstallMod;
        }
        /// <summary>
        /// Advanced behavior controls for fairness, throughput, and reserves.
        /// </summary>
        public static class Advanced
        {
            /// <summary>
            /// Target number of warpers to hold at each receiver station.
            /// </summary>
            /// <value>Default: 50. Range: 1–50.</value>
            public static ConfigEntry<int> WarperTarget;

            /// <summary>
            /// Maximum number of warpers delivered to a single receiver per run (0 = unlimited).
            /// </summary>
            /// <value>Default: 10. Range: 0–50.</value>
            public static ConfigEntry<int> MaxPerTickPerReceiver;

            /// <summary>
            /// Minimum number of warpers to keep in each supplier (prevents total drain).
            /// </summary>
            /// <value>Default: 0. Range: 0–50.</value>
            public static ConfigEntry<int> SupplierReserve;

            /// <summary>
            /// Allocate supply proportionally to receiver deficits for strict fairness each run.
            /// </summary>
            /// <value>Default: true.</value>
            public static ConfigEntry<bool> FairShareDistribution;

            /// <summary>
            /// If enabled, uses a faster cadence when deficits exist and backs off when stable.
            /// </summary>
            /// <value>Default: false.</value>
            public static ConfigEntry<bool> AdaptiveCadence;
        }

        /// <summary>
        /// Binds all configuration entries. Call once on plugin startup.
        /// </summary>
        internal static void Init(ConfigFile config)
        {
            ////////////////////
            // General Config //
            ////////////////////
            
            General.WarperTickCount = config.Bind(GENERAL_SECTION, "WarperTickCount", 60,
                new ConfigDescription("Number of game ticks between distribution passes. Min 1, Max 275 (defaults to 60)",
                    new AcceptableValueRange<int>(1, 275), new { }));

            General.WarperRemoteMode = config.Bind(GENERAL_SECTION, "WarperRemoteMode", false,
                "By default only search local ILS/PLS for supplies. Enable this to get Warpers from different planets as well"
            );

            General.WarperTransportCost = config.Bind(GENERAL_SECTION, "WarperTransportCost", true,
                "If enabled, transporting Warpers costs 1 warper. Disable for moving Warpers at no costs.");

            General.WarpersRequiredToggleAutomation =  config.Bind(GENERAL_SECTION, "WarpersRequiredToggleAutomation", true,
                "If enabled, when `Warpers Required` toggle ticked on, this will auto fill the warper slot. " +
                "When toggle is ticked off this will stop filling the wraper slot from suppliers");

            General.WarperLocalTransportCost = config.Bind(GENERAL_SECTION, "WarperLocalTransportCost", 1,
               new ConfigDescription( "If enabled, transporting Warpers costs 1 warper. Disable for moving Warpers at no costs.",
               new AcceptableValueRange<int>(0,10), new { }));
            
            General.WarperRemoteTransportCost = config.Bind(GENERAL_SECTION, "WarperRemoteTransportCost", 2,
                new ConfigDescription("Default cost of transporting Warpers from different planets. Note: Maximum of 10, defaults to 2",
                    new AcceptableValueRange<int>(0, 10), new { }));

            ////////////////////
            // Utility Config //
            ////////////////////

            Utility.DisableMod = config.Bind(UTILITY_SECTION, "DisableMod", false,
                "While true this will disable all mod effects but will not remove additional slot from ILS. " +
                "Useful if uninstalling mod failed for some reason.");
            
            Utility.UninstallMod = config.Bind(UTILITY_SECTION, "UninstallMod", false,
                "WARNING!!! BACKUP YOUR SAVE BEFORE DOING THIS!!! This will not work if mod cannot load properly! " +
                "If this is true, mod will remove additional slot from all current ILS. " +
                "This will destroy any items in additional slot " +
                "To correctly uninstall mod and get vanilla save please follow this steps. " +
                "Step #1: Set UninstallMod to true. " + 
                "Step #2: Load your save. " +
                "Step #3: Save your game. " +
                "Step #4: Exit the game and remove this mod."
                );

            ////////////////////////
            // Advanced Behavior  //
            ////////////////////////

            Advanced.WarperTarget = config.Bind(GENERAL_SECTION, "WarperTarget", 50,
                new ConfigDescription("Target warpers per station (typically 50)", new AcceptableValueRange<int>(1, 50), new { }));

            Advanced.MaxPerTickPerReceiver = config.Bind(GENERAL_SECTION, "MaxPerTickPerReceiver", 10,
                new ConfigDescription("Cap warpers delivered to a single receiver per run (0 = unlimited)", new AcceptableValueRange<int>(0, 50), new { }));

            Advanced.SupplierReserve = config.Bind(GENERAL_SECTION, "SupplierReserve", 0,
                new ConfigDescription("Minimum warpers to keep in each supplier (0-50)", new AcceptableValueRange<int>(0, 50), new { }));

            Advanced.FairShareDistribution = config.Bind(GENERAL_SECTION, "FairShareDistribution", true,
                new ConfigDescription("If true, allocate supply proportionally to receiver deficits", null, new { }));

            Advanced.AdaptiveCadence = config.Bind(GENERAL_SECTION, "AdaptiveCadence", false,
                new ConfigDescription("If true, speed up when deficits exist and slow down when stable", null, new { }));
        }
    }
}
