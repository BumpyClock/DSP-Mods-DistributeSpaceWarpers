namespace DistributeSpaceWarper
{
    using BepInEx.Configuration;

    public static class Config
    {
        private static readonly string GENERAL_SECTION = "General";
        private static readonly string UTILITY_SECTION = "Utility";

        public static class General
        {
            public static ConfigEntry<ELogisticStorage> WarperLocalMode;
            public static ConfigEntry<ELogisticStorage> WarperRemoteMode;
            public static ConfigEntry<int> WarperMaxValue;
            public static ConfigEntry<bool> ShowWarperSlot;
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
            
            General.WarperMaxValue = config.Bind(GENERAL_SECTION, "WarperMaxValue", 100,
                new ConfigDescription("Default number of items set for warper slot. Note: Should be in increments of 100. Otherwise may cause issues.",
                    new AcceptableValueRange<int>(0, 10000), new { }));
                
            General.WarperLocalMode = config.Bind(GENERAL_SECTION, "WarperLocalMode", ELogisticStorage.Demand,
                "Default local logistics mode of the Warpers"
            );

            General.WarperRemoteMode = config.Bind(GENERAL_SECTION, "WarperRemoteMode", ELogisticStorage.Demand,
                "Default remote logistics mode of the Warpers"
            );
            
            General.WarpersRequiredToggleAutomation =  config.Bind(GENERAL_SECTION, "WarpersRequiredToggleAutomation", true,
                "If enabled, when `Warpers Required` toggle ticked on, this will setup warper slot to default local mode. " +
                "When toggle is ticked off this will set wraper slot to local supply.");
            
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