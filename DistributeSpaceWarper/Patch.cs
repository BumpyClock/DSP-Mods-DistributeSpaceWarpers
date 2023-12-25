using System;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DistributeSpaceWarper
{
    static class Patch
    {
        private static bool ModDisabled => Config.Utility.DisableMod.Value == true || Config.Utility.UninstallMod.Value == true;
        private static int _ilsId = 2104;

        [HarmonyPatch(typeof(PlanetTransport), "Import", typeof(BinaryReader))]
        [HarmonyPostfix]
        public static void PlanetTransport_Import_Postfix(BinaryReader r, PlanetTransport __instance)
        {
            if (Config.Utility.UninstallMod.Value == true)
            {
                int warperId = ItemProto.kWarperId;
                PrefabDesc prefabDesc = LDB.items.Select(_ilsId).prefabDesc;
                UninstallMod(__instance, prefabDesc, warperId);
            }
        }
        
        
        private static void UninstallMod(PlanetTransport instance, PrefabDesc prefabDesc, int warperId)
        {
            int warperSlotIndex = prefabDesc.stationMaxItemKinds;
            for (int j = 1; j < instance.stationCursor; j++)
            {
                if (instance.stationPool[j] != null && instance.stationPool[j].id == j)
                {
                    StationComponent stationComponent = instance.stationPool[j];
                    if (stationComponent.isCollector == true || stationComponent.isStellar == false)
                    {
                        continue;
                    }
                    if (stationComponent.storage.Length > prefabDesc.stationMaxItemKinds
                        && stationComponent.storage.Last().itemId == warperId)
                    {
                        instance.SetStationStorage(
                            stationComponent.id, 
                            warperSlotIndex, 
                            0, 0, 
                            ELogisticStorage.None, 
                            ELogisticStorage.None, 
                            GameMain.mainPlayer);
                        List<StationStore> storeCopy = new List<StationStore>(stationComponent.storage);
                        storeCopy.RemoveAt(warperSlotIndex);
                        stationComponent.storage = storeCopy.ToArray();
                    }
                    instance.RefreshStationTraffic();
                    instance.RefreshDispenserTraffic();
                    instance.gameData.galacticTransport.RefreshTraffic(stationComponent.gid);
                    stationComponent.UpdateNeeds();
                }
            }
        }


        [HarmonyPatch(typeof(PlanetTransport), "GameTick", typeof(long), typeof(bool), typeof(bool))]
        [HarmonyPostfix]
        public static void PlanetTransport_GameTick_Postfix(PlanetTransport __instance)
        {
            if (ModDisabled == true)
            {
                return;
            }
            ELogisticStorage defaultLocalMode = Config.General.WarperLocalMode.Value;
            ELogisticStorage defaultRemoteMode = Config.General.WarperRemoteMode.Value;
            bool warpersRequiredToggleAutomation = Config.General.WarpersRequiredToggleAutomation.Value;
            int defaultMaxValue = Config.General.WarperMaxValue.Value;
            int warperId = ItemProto.kWarperId;
            PrefabDesc prefabDesc = LDB.items.Select(_ilsId).prefabDesc;
            int warperSlotIndex = prefabDesc.stationMaxItemKinds;
            
            for (int j = 1; j < __instance.stationCursor; j++)
            {
                try
                {
                    if (__instance.stationPool[j] != null && __instance.stationPool[j].id == j)
                    {
                        StationComponent stationComponent = __instance.stationPool[j];
                        if (stationComponent.isCollector == true || stationComponent.isStellar == false)
                        {
                            continue;
                        }
                        bool needRefreshTraffic = false;
                        if (stationComponent.storage.Length < prefabDesc.stationMaxItemKinds + 1)
                        {
                            ModDebug.Error(1);
                            List<StationStore> storeCopy = new List<StationStore>(stationComponent.storage);
                            storeCopy.Add(new StationStore());
                            stationComponent.storage = storeCopy.ToArray();
                            __instance.SetStationStorage(
                                stationComponent.id,
                                warperSlotIndex,
                                warperId,
                                defaultMaxValue,
                                defaultLocalMode,
                                defaultRemoteMode,
                                GameMain.mainPlayer);
                        }
                        if (stationComponent.storage[warperSlotIndex].itemId != warperId)
                        {
                            __instance.SetStationStorage(
                                stationComponent.id,
                                warperSlotIndex,
                                warperId,
                                defaultMaxValue,
                                defaultLocalMode,
                                defaultRemoteMode,
                                GameMain.mainPlayer);
                        }
                        if (warpersRequiredToggleAutomation == true)
                        {
                            if (stationComponent.warperNecessary == true && stationComponent.storage[warperSlotIndex].localLogic != defaultLocalMode)
                            {
                                __instance.SetStationStorage(
                                    stationComponent.id,
                                    warperSlotIndex,
                                    warperId,
                                    defaultMaxValue,
                                    defaultLocalMode,
                                    stationComponent.storage[warperSlotIndex].remoteLogic,
                                    GameMain.mainPlayer);
                            }
                            else if (stationComponent.warperNecessary == false && stationComponent.storage[warperSlotIndex].localLogic != ELogisticStorage.Supply)
                            {
                                __instance.SetStationStorage(
                                    stationComponent.id,
                                    warperSlotIndex,
                                    warperId,
                                    defaultMaxValue,
                                    ELogisticStorage.Supply,
                                    stationComponent.storage[warperSlotIndex].remoteLogic,
                                    GameMain.mainPlayer);
                            }
                        }
                        int manualWarperStoreIndex = Array.FindIndex(stationComponent.storage, store => store.itemId == warperId);
                        if (manualWarperStoreIndex != -1
                            && manualWarperStoreIndex != warperSlotIndex
                            && stationComponent.storage[warperSlotIndex].count < stationComponent.storage[warperSlotIndex].max
                            && stationComponent.storage[manualWarperStoreIndex].count > stationComponent.storage[manualWarperStoreIndex].max)
                        {
                            int warperOverflowInManualSlot = stationComponent.storage[manualWarperStoreIndex].count - stationComponent.storage[manualWarperStoreIndex].max;
                            int warperNeedInAutomaticSlot = stationComponent.storage[warperSlotIndex].max - stationComponent.storage[warperSlotIndex].count;
                            int transferAmount = Mathf.Min(warperNeedInAutomaticSlot, warperOverflowInManualSlot);
                            stationComponent.storage[manualWarperStoreIndex].count -= transferAmount;
                            stationComponent.storage[warperSlotIndex].count += transferAmount;
                            stationComponent.storage[warperSlotIndex].localOrder = 0;
                            stationComponent.storage[warperSlotIndex].remoteOrder = 0;
                            needRefreshTraffic = true;
                        }
                        if (manualWarperStoreIndex != -1 &&
                            (stationComponent.storage[warperSlotIndex].count == stationComponent.storage[warperSlotIndex].max
                             && stationComponent.storage[warperSlotIndex].localLogic == ELogisticStorage.Demand && stationComponent.storage[warperSlotIndex].totalOrdered != 0
                            ||
                            stationComponent.storage[warperSlotIndex].count == 0 &&
                            stationComponent.storage[warperSlotIndex].localLogic == ELogisticStorage.Supply && stationComponent.storage[warperSlotIndex].totalOrdered != 0))
                        {
                            int storageCount = stationComponent.storage[warperSlotIndex].count;
                            stationComponent.storage[warperSlotIndex].count = 0;

                            __instance.SetStationStorage(
                                stationComponent.id,
                                warperSlotIndex,
                                0, defaultMaxValue,
                                defaultLocalMode,
                                defaultRemoteMode,
                                GameMain.mainPlayer);

                            __instance.SetStationStorage(
                                stationComponent.id,
                                warperSlotIndex,
                                warperId, defaultMaxValue,
                                defaultLocalMode, defaultRemoteMode,
                                GameMain.mainPlayer);
                            stationComponent.storage[warperSlotIndex].count = storageCount;
                            needRefreshTraffic = true;
                        }
                        if (needRefreshTraffic)
                        {
                            stationComponent.UpdateNeeds();
                            __instance.RefreshStationTraffic();
                            __instance.RefreshDispenserTraffic();
                            __instance.gameData.galacticTransport.RefreshTraffic(stationComponent.gid);
                        }
                    }

                }
                catch (Exception e)
                {
                    ModDebug.Error($"Error occurred :{e}");
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationStorage), "OnItemPickerReturn", typeof(ItemProto))]
        public static bool UIStationStorage_OnItemPickerReturn_Prefix(UIStationStorage __instance, ItemProto itemProto)
        {
            if (ModDisabled == true)
            {
                return true;
            }
            int warperId = ItemProto.kWarperId;
            int IlsId = 2104;
            PrefabDesc prefabDesc = LDB.items.Select(IlsId).prefabDesc;
            int warperSlotIndex = prefabDesc.stationMaxItemKinds;
     
            if (itemProto == null)
            {
                return false;
            }
            if (__instance.station == null || __instance.index >= __instance.station.storage.Length)
            {
                return false;
            }
            ItemProto itemProto2 = LDB.items.Select((int)__instance.stationWindow.factory.entityPool[__instance.station.entityId].protoId);
            if (itemProto2 == null)
            {
                return false;
            }
            for (int i = 0; i < __instance.station.storage.Length; i++)
            {
                if (__instance.station.storage[i].itemId == itemProto.ID)
                {
                    if (__instance.station.isStellar == true && __instance.station.isCollector == false && i == warperSlotIndex && itemProto.ID == warperId)
                    {
                        continue;
                    }
                    UIRealtimeTip.Popup("不选择重复物品".Translate(), true, 0);
                    return false;
                }
            }
            __instance.stationWindow.transport.SetStationStorage(__instance.station.id, __instance.index, 
                itemProto.ID, itemProto2.prefabDesc.stationMaxItemCount, ELogisticStorage.Supply,
                (!__instance.station.isStellar) ? ELogisticStorage.None : ELogisticStorage.Supply, GameMain.mainPlayer);
            return false;
        }
    }
}

