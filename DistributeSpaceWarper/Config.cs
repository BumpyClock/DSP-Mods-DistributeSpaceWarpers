namespace DistributeSpaceWarper
{
    using BepInEx.Configuration;

    public static class Config
    {
        private static readonly string GENERAL_SECTION = "General";
        private static readonly string UTILITY_SECTION = "Utility";

        public static class General
        {
            public static ConfigEntry<bool> WarperRemoteMode;
            public static ConfigEntry<int> WarperRemoteTransportCost;
            public static ConfigEntry<int> WarperTickCount;
            public static ConfigEntry<bool> WarperLocalTransportCost;
            public static ConfigEntry<bool> WarpersRequiredToggleAutomation;
        }
        
        public static class Utility
        {
            public static ConfigEntry<bool> DisableMod;
            public static ConfigEntry<bool> UninstallMod;
        }
        internal static void Init(ConfigFile config)
        {
            ////////////////////
            // General Config //
            ////////////////////
            
            General.WarperTickCount = config.Bind(GENERAL_SECTION, "WarperTickCount", 60,
                new ConfigDescription("Default number of ticks before distributing warpers. Note: Maximum of 260, defaults to 60",
                    new AcceptableValueRange<int>(0, 275), new { }));

            General.WarperRemoteMode = config.Bind(GENERAL_SECTION, "WarperRemoteMode", false,
                "By default only search local ILS/PLS for supplies. Enable this to get Warpers from different planets as well"
            );
            
            General.WarpersRequiredToggleAutomation =  config.Bind(GENERAL_SECTION, "WarpersRequiredToggleAutomation", true,
                "If enabled, when `Warpers Required` toggle ticked on, this will setup warper slot to default local mode. " +
                "When toggle is ticked off this will set wraper slot to local supply.");

            General.WarperLocalTransportCost = config.Bind(GENERAL_SECTION, "WarperTransportCost", true,
                "If enabled, transporting Warpers costs 1 warper. Disable for moving Warpers at no costs.");
            
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
        }
    }
}