using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace StationRangeLimiter
{
    static class Patch
    {
        private class ModData
        {
            public List<int> EnforcedLocalRangeStations = new List<int>();
            public List<int> EnforcedRemoteRangeStations = new List<int>();
        }
        private static ModData _data = new ModData();


        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "LoadCurrentGame", typeof(string))]
        public static void LoadCurrentGame_Postfix(string saveName)
        {
            string path = Application.persistentDataPath + "/" + "SRL_Mod_" + saveName + ".lst";
            if (File.Exists(path))
            {
                string jsonString = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(jsonString, _data);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "SaveCurrentGame", typeof(string))]

        public static void SaveCurrentGame_Postfix(string saveName)
        {
            string path = Application.persistentDataPath + "/" + "SRL_Mod_" + saveName + ".lst";
            string jsonString = JsonUtility.ToJson(_data);
            File.WriteAllText(path, jsonString);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnUpdate")]
        public static void _OnUpdate_Postfix(UIStationWindow __instance, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
        {
            UIButton enforceRemoteRangeButton =
                ___warperNecessaryButton.transform.parent.Find("EnforceRemoteRangeButton").GetComponent<UIButton>();
            UIButton enforceLocalRangeButton =
                ___warperNecessaryButton.transform.parent.Find("EnforceLocalRangeButton").GetComponent<UIButton>();
            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent.entityId);
            bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent.entityId);
            enforceLocalRangeButton.transform.Find(___warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceLocalRange == true;
            enforceRemoteRangeButton.transform.Find(___warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceRemoteRange == true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnRegEvent")]
        public static void _OnRegEvent_Postfix(UIStationWindow __instance, bool ___event_lock, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
        {
            UIButton enforceRemoteRangeButton =
                ___warperNecessaryButton.transform.parent.Find("EnforceRemoteRangeButton").GetComponent<UIButton>();
            UIButton enforceLocalRangeButton =
                ___warperNecessaryButton.transform.parent.Find("EnforceLocalRangeButton").GetComponent<UIButton>();
            enforceRemoteRangeButton.onClick += i =>
            {
                OnEnforceRemoteRangeButtonClick(enforceRemoteRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
            };
            enforceLocalRangeButton.onClick += i =>
            {
                OnEnforceLocalRangeButtonClick(enforceLocalRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
            };
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnUnregEvent")]
        public static void _OnUnregEvent_Postfix(UIStationWindow __instance, bool ___event_lock, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
        {
            UIButton enforceRemoteRangeButton =
                ___warperNecessaryButton.transform.parent.Find("EnforceRemoteRangeButton").GetComponent<UIButton>();
            UIButton enforceLocalRangeButton =
                ___warperNecessaryButton.transform.parent.Find("EnforceLocalRangeButton").GetComponent<UIButton>();
            enforceRemoteRangeButton.onClick -= i =>
            {
                OnEnforceRemoteRangeButtonClick(enforceRemoteRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
            };
            enforceLocalRangeButton.onClick -= i =>
            {
                OnEnforceLocalRangeButtonClick(enforceLocalRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
            };
        }

        private static void OnEnforceRemoteRangeButtonClick(UIButton enforceRemoteRangeButton, Image warperNecessaryCheck, UIStationWindow instance, bool event_lock)
        {
            if (event_lock || instance.stationId == 0 || instance.factory == null)
                return;
            StationComponent stationComponent = instance.transport.stationPool[instance.stationId];
            if (stationComponent == null || stationComponent.id != instance.stationId)
                return;
            bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent.entityId);
            if (enforceRemoteRange == true)
            {
                _data.EnforcedRemoteRangeStations.Remove(stationComponent.entityId);
            }
            else
            {
                _data.EnforcedRemoteRangeStations.Add(stationComponent.entityId);
            }
            enforceRemoteRangeButton.transform.Find(warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceRemoteRange == false;
        }

        private static void OnEnforceLocalRangeButtonClick(UIButton enforceLocalRangeButton, Image warperNecessaryCheck, UIStationWindow instance, bool event_lock)
        {
            if (event_lock || instance.stationId == 0 || instance.factory == null)
                return;
            StationComponent stationComponent = instance.transport.stationPool[instance.stationId];
            if (stationComponent == null || stationComponent.id != instance.stationId)
                return;
            bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent.entityId);
            if (enforceLocalRange == true)
            {
                _data.EnforcedLocalRangeStations.Remove(stationComponent.entityId);
            }
            else
            {
                _data.EnforcedLocalRangeStations.Add(stationComponent.entityId);
            }
            enforceLocalRangeButton.transform.Find(warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceLocalRange == false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnCreate")]
        public static void _OnCreatePostfix(UIStationWindow __instance, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
        {
            UIButton enforceRemoteRangeButton = Object.Instantiate(___warperNecessaryButton, ___warperNecessaryButton.transform.parent);
            RectTransform enforceRemoteRangeButtonRectTransform = (RectTransform)enforceRemoteRangeButton.transform;
            enforceRemoteRangeButtonRectTransform.anchoredPosition -= new Vector2(0, 40);
            enforceRemoteRangeButton.name = "EnforceRemoteRangeButton";
            Object.Destroy(enforceRemoteRangeButton.GetComponentInChildren<Localizer>());
            enforceRemoteRangeButton.GetComponentInChildren<Text>().text = "Enforce remote range";
            UIButton enforceLocalRangeButton = Object.Instantiate(___warperNecessaryButton, ___warperNecessaryButton.transform.parent);
            RectTransform enforceLocalRangeButtonRectTransform = (RectTransform)enforceLocalRangeButton.transform;
            enforceLocalRangeButton.name = "EnforceLocalRangeButton";
            enforceLocalRangeButtonRectTransform.anchoredPosition -= new Vector2(0, 20);
            Object.Destroy(enforceLocalRangeButton.GetComponentInChildren<Localizer>());
            enforceLocalRangeButton.GetComponentInChildren<Text>().text = "Enforce local range";
        }

        private static int split_inc(ref int n, ref int m, int p)
        {
            if (n == 0)
            {
                return 0;
            }
            int num1 = m / n;
            int num2 = m - num1 * n;
            n -= p;
            int num3 = num2 - n;
            int num4 = num3 > 0 ? num1 * p + num3 : num1 * p;
            m -= num4;
            return num4;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "InternalTickRemote", typeof(PlanetFactory),
            typeof(int), typeof(double), typeof(float), typeof(float), typeof(int),
            typeof(StationComponent[]), typeof(AstroData[]), typeof(VectorLF3), typeof(Quaternion), typeof(bool),
            typeof(int[]))]
        public static bool InternalTickRemote_Prefix(StationComponent __instance, bool ___warperFree,
            int ____tmp_iter_remote, PlanetFactory factory, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries,
            StationComponent[] gStationPool, AstroData[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot,
            bool starmap, int[] consumeRegister)
        {
            bool flag = shipWarpSpeed > shipSailSpeed + 1f;
            ___warperFree = DSPGame.IsMenuDemo;
            if (__instance.warperCount < __instance.warperMaxCount)
            {
                StationStore[] array = __instance.storage;
                lock (array)
                {
                    for (int i = 0; i < __instance.storage.Length; i++)
                    {
                        if (__instance.storage[i].itemId == 1210 && __instance.storage[i].count > 0)
                        {
                            __instance.warperCount++;
                            int num = __instance.storage[i].inc / __instance.storage[i].count;
                            StationStore[] array2 = __instance.storage;
                            int num2 = i;
                            array2[num2].count -= 1;
                            StationStore[] array3 = __instance.storage;
                            int num3 = i;
                            array3[num3].inc -= num;
                            break;
                        }
                    }
                }
            }
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int itemId = 0;
            int num9 = 0;
            int num10 = 0;
            int itemId2 = 0;
            int num11 = 0;
            int num12 = 0;
            int num13 = 0;
            int num14 = 0;
            int num15 = 0;
            int num16 = 0;
            int num17 = 0;
            if (timeGene == __instance.gene)
            {
                ____tmp_iter_remote++;
                if (__instance.remotePairCount > 0 && __instance.idleShipCount > 0)
                {
                    __instance.remotePairProcess %= __instance.remotePairCount;
                    int num18 = __instance.remotePairProcess;
                    SupplyDemandPair supplyDemandPair;
                    StationComponent stationComponent;
                    double num20;
                    bool flag4;
                    StationComponent stationComponent2;
                    double num21;
                    bool flag6;
                    for (; ; )
                    {
                        int num19 = (shipCarries - 1) * __instance.deliveryShips / 100;
                        supplyDemandPair = __instance.remotePairs[__instance.remotePairProcess];
                        if (supplyDemandPair.supplyId == __instance.gid)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                num4 = __instance.storage[supplyDemandPair.supplyIndex].max;
                                num5 = __instance.storage[supplyDemandPair.supplyIndex].count;
                                num6 = __instance.storage[supplyDemandPair.supplyIndex].inc;
                                num7 = __instance.storage[supplyDemandPair.supplyIndex].remoteSupplyCount;
                                num8 = __instance.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
                                itemId = __instance.storage[supplyDemandPair.supplyIndex].itemId;
                            }
                        }
                        if (supplyDemandPair.supplyId == __instance.gid && num4 <= num19)
                        {
                            num19 = num4 - 1;
                        }
                        if (num19 < 0)
                        {
                            num19 = 0;
                        }
                        if (supplyDemandPair.supplyId == __instance.gid && num5 > num19 && num7 > num19 && num8 > num19)
                        {
                            stationComponent = gStationPool[supplyDemandPair.demandId];
                            if (stationComponent != null)
                            {
                                num20 = (astroPoses[__instance.planetId].uPos - astroPoses[stationComponent.planetId].uPos).magnitude + astroPoses[__instance.planetId].uRadius + astroPoses[stationComponent.planetId].uRadius;
                                bool flag3 = num20 < __instance.tripRangeShips;
                                bool demandRemoteRange = num20 < stationComponent.tripRangeShips;
                                bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent.entityId);
                                flag4 = (num20 >= __instance.warpEnableDist);
                                if (__instance.warperNecessary && flag4 && (__instance.warperCount < 2 || !flag))
                                {
                                    flag3 = false;
                                }
                                if (flag3)
                                {
                                    StationStore[] array = stationComponent.storage;
                                    lock (array)
                                    {
                                        num11 = stationComponent.storage[supplyDemandPair.demandIndex].remoteDemandCount;
                                        num12 = stationComponent.storage[supplyDemandPair.demandIndex].totalDemandCount;
                                    }
                                }
                                if (flag3 && (enforceRemoteRange == true && demandRemoteRange == true || enforceRemoteRange == false) && num11 > 0 && num12 > 0)
                                {
                                    break;
                                }
                            }
                        }
                        if (supplyDemandPair.demandId == __instance.gid)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                num9 = __instance.storage[supplyDemandPair.demandIndex].remoteDemandCount;
                                num10 = __instance.storage[supplyDemandPair.demandIndex].totalDemandCount;
                            }
                        }
                        if (supplyDemandPair.demandId == __instance.gid && num9 > 0 && num10 > 0)
                        {
                            stationComponent2 = gStationPool[supplyDemandPair.supplyId];
                            if (stationComponent2 != null)
                            {
                                num21 = (astroPoses[__instance.planetId].uPos - astroPoses[stationComponent2.planetId].uPos).magnitude + astroPoses[__instance.planetId].uRadius + astroPoses[stationComponent2.planetId].uRadius;
                                bool flag5 = num21 < __instance.tripRangeShips;
                                bool supplyRemoteRange = num21 < stationComponent2.tripRangeShips;
                                bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent2.entityId);
                                if (flag5 && !__instance.includeOrbitCollector && stationComponent2.isCollector)
                                {
                                    flag5 = false;
                                }
                                flag6 = (num21 >= __instance.warpEnableDist);
                                if (__instance.warperNecessary && flag6 && (__instance.warperCount < 2 || !flag))
                                {
                                    flag5 = false;
                                }
                                StationStore[] array = stationComponent2.storage;
                                lock (array)
                                {
                                    num13 = stationComponent2.storage[supplyDemandPair.supplyIndex].max;
                                    num14 = stationComponent2.storage[supplyDemandPair.supplyIndex].count;
                                    num15 = stationComponent2.storage[supplyDemandPair.supplyIndex].inc;
                                    num16 = stationComponent2.storage[supplyDemandPair.supplyIndex].remoteSupplyCount;
                                    num17 = stationComponent2.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
                                }
                                if (num13 <= num19)
                                {
                                    num19 = num13 - 1;
                                }
                                if (num19 < 0)
                                {
                                    num19 = 0;
                                }
                                if (flag5 && (enforceRemoteRange && supplyRemoteRange || enforceRemoteRange == false) && num14 > num19 && num16 > num19 && num17 > num19)
                                {
                                    goto Block_47;
                                }
                            }
                        }
                        __instance.remotePairProcess++;
                        __instance.remotePairProcess %= __instance.remotePairCount;
                        if (num18 == __instance.remotePairProcess)
                        {
                            goto IL_1356;
                        }
                    }
                    long num22 = __instance.CalcTripEnergyCost(num20, shipSailSpeed, flag);
                    if (__instance.energy < num22)
                    {
                        goto IL_1356;
                    }
                    int num23 = (shipCarries < num5) ? shipCarries : num5;
                    int num24 = num5;
                    int num25 = num6;
                    int num26 = split_inc(ref num24, ref num25, num23);
                    int num27 = __instance.QueryIdleShip(__instance.nextShipIndex);
                    if (num27 >= 0)
                    {
                        __instance.nextShipIndex = (num27 + 1) % __instance.workShipDatas.Length;
                        __instance.workShipDatas[__instance.workShipCount].stage = -2;
                        __instance.workShipDatas[__instance.workShipCount].planetA = __instance.planetId;
                        __instance.workShipDatas[__instance.workShipCount].planetB = stationComponent.planetId;
                        __instance.workShipDatas[__instance.workShipCount].otherGId = stationComponent.gid;
                        __instance.workShipDatas[__instance.workShipCount].direction = 1;
                        __instance.workShipDatas[__instance.workShipCount].t = 0f;
                        __instance.workShipDatas[__instance.workShipCount].itemId = (__instance.workShipOrders[__instance.workShipCount].itemId = itemId);
                        __instance.workShipDatas[__instance.workShipCount].itemCount = num23;
                        __instance.workShipDatas[__instance.workShipCount].inc = num26;
                        __instance.workShipDatas[__instance.workShipCount].gene = ____tmp_iter_remote;
                        __instance.workShipDatas[__instance.workShipCount].shipIndex = num27;
                        __instance.workShipOrders[__instance.workShipCount].otherStationGId = stationComponent.gid;
                        __instance.workShipOrders[__instance.workShipCount].thisIndex = supplyDemandPair.supplyIndex;
                        __instance.workShipOrders[__instance.workShipCount].otherIndex = supplyDemandPair.demandIndex;
                        __instance.workShipOrders[__instance.workShipCount].thisOrdered = 0;
                        __instance.workShipOrders[__instance.workShipCount].otherOrdered = num23;
                        if (flag4)
                        {
                            int[] array4 = consumeRegister;
                            lock (array4)
                            {
                                if (__instance.warperCount >= 2)
                                {
                                    ShipData[] array5 = __instance.workShipDatas;
                                    int num28 = __instance.workShipCount;
                                    array5[num28].warperCnt += 2;
                                    __instance.warperCount -= 2;
                                    consumeRegister[1210] += 2;
                                }
                                else if (__instance.warperCount >= 1)
                                {
                                    ShipData[] array6 = __instance.workShipDatas;
                                    int num29 = __instance.workShipCount;
                                    array6[num29].warperCnt += 1;
                                    __instance.warperCount--;
                                    consumeRegister[1210]++;
                                }
                                else if (___warperFree)
                                {
                                    ShipData[] array7 = __instance.workShipDatas;
                                    int num30 = __instance.workShipCount;
                                    array7[num30].warperCnt += 2;
                                }
                            }
                        }
                        StationStore[] array = stationComponent.storage;
                        lock (array)
                        {
                            StationStore[] array8 = stationComponent.storage;
                            int demandIndex = supplyDemandPair.demandIndex;
                            array8[demandIndex].remoteOrder += num23;
                        }
                        __instance.workShipCount++;
                        __instance.idleShipCount--;
                        __instance.IdleShipGetToWork(num27);
                        array = __instance.storage;
                        lock (array)
                        {
                            StationStore[] array9 = __instance.storage;
                            int supplyIndex = supplyDemandPair.supplyIndex;
                            array9[supplyIndex].count -= num23;
                            StationStore[] array10 = __instance.storage;
                            int supplyIndex2 = supplyDemandPair.supplyIndex;
                            array10[supplyIndex2].inc -= num26;
                        }
                        __instance.energy -= num22;
                    }
                    goto IL_1356;
                Block_47:
                    long num31 = __instance.CalcTripEnergyCost(num21, shipSailSpeed, flag);
                    if (!stationComponent2.isCollector && !stationComponent2.isVeinCollector)
                    {
                        bool flag7 = false;
                        __instance.remotePairProcess %= __instance.remotePairCount;
                        int num32 = __instance.remotePairProcess + 1;
                        int num33 = __instance.remotePairProcess;
                        num32 %= __instance.remotePairCount;
                        SupplyDemandPair supplyDemandPair2;
                        for (; ; )
                        {
                            supplyDemandPair2 = __instance.remotePairs[num32];
                            if (supplyDemandPair2.supplyId == __instance.gid && supplyDemandPair2.demandId == stationComponent2.gid)
                            {
                                StationStore[] array = __instance.storage;
                                lock (array)
                                {
                                    num5 = __instance.storage[supplyDemandPair2.supplyIndex].count;
                                    num6 = __instance.storage[supplyDemandPair2.supplyIndex].inc;
                                    num7 = __instance.storage[supplyDemandPair2.supplyIndex].remoteSupplyCount;
                                    num8 = __instance.storage[supplyDemandPair2.supplyIndex].totalSupplyCount;
                                    itemId = __instance.storage[supplyDemandPair2.supplyIndex].itemId;
                                }
                            }
                            if (supplyDemandPair2.supplyId == __instance.gid && supplyDemandPair2.demandId == stationComponent2.gid)
                            {
                                StationStore[] array = stationComponent2.storage;
                                lock (array)
                                {
                                    num11 = stationComponent2.storage[supplyDemandPair2.demandIndex].remoteDemandCount;
                                    num12 = stationComponent2.storage[supplyDemandPair2.demandIndex].totalDemandCount;
                                }
                            }
                            int num19 = 0;
                            if (supplyDemandPair2.supplyId == __instance.gid && supplyDemandPair2.demandId == stationComponent2.gid && num5 >= num19 && num7 >= num19 && num8 >= num19 && num11 > 0 && num12 > 0)
                            {
                                break;
                            }
                            num32++;
                            num32 %= __instance.remotePairCount;
                            if (num33 == num32)
                            {
                                goto IL_F6C;
                            }
                        }
                        if (__instance.energy >= num31)
                        {
                            int num34 = (shipCarries < num5) ? shipCarries : num5;
                            int num35 = num5;
                            int num36 = num6;
                            int num37 = split_inc(ref num35, ref num36, num34);
                            int num38 = __instance.QueryIdleShip(__instance.nextShipIndex);
                            if (num38 >= 0)
                            {
                                __instance.nextShipIndex = (num38 + 1) % __instance.workShipDatas.Length;
                                __instance.workShipDatas[__instance.workShipCount].stage = -2;
                                __instance.workShipDatas[__instance.workShipCount].planetA = __instance.planetId;
                                __instance.workShipDatas[__instance.workShipCount].planetB = stationComponent2.planetId;
                                __instance.workShipDatas[__instance.workShipCount].otherGId = stationComponent2.gid;
                                __instance.workShipDatas[__instance.workShipCount].direction = 1;
                                __instance.workShipDatas[__instance.workShipCount].t = 0f;
                                __instance.workShipDatas[__instance.workShipCount].itemId = (__instance.workShipOrders[__instance.workShipCount].itemId = itemId);
                                __instance.workShipDatas[__instance.workShipCount].itemCount = num34;
                                __instance.workShipDatas[__instance.workShipCount].inc = num37;
                                __instance.workShipDatas[__instance.workShipCount].gene = ____tmp_iter_remote;
                                __instance.workShipDatas[__instance.workShipCount].shipIndex = num38;
                                __instance.workShipOrders[__instance.workShipCount].otherStationGId = stationComponent2.gid;
                                __instance.workShipOrders[__instance.workShipCount].thisIndex = supplyDemandPair2.supplyIndex;
                                __instance.workShipOrders[__instance.workShipCount].otherIndex = supplyDemandPair2.demandIndex;
                                __instance.workShipOrders[__instance.workShipCount].thisOrdered = 0;
                                __instance.workShipOrders[__instance.workShipCount].otherOrdered = num34;
                                if (flag6)
                                {
                                    int[] array4 = consumeRegister;
                                    lock (array4)
                                    {
                                        if (__instance.warperCount >= 2)
                                        {
                                            ShipData[] array11 = __instance.workShipDatas;
                                            int num39 = __instance.workShipCount;
                                            array11[num39].warperCnt += 2;
                                            __instance.warperCount -= 2;
                                            consumeRegister[1210] += 2;
                                        }
                                        else if (__instance.warperCount >= 1)
                                        {
                                            ShipData[] array12 = __instance.workShipDatas;
                                            int num40 = __instance.workShipCount;
                                            array12[num40].warperCnt += 1;
                                            __instance.warperCount--;
                                            consumeRegister[1210]++;
                                        }
                                        else if (___warperFree)
                                        {
                                            ShipData[] array13 = __instance.workShipDatas;
                                            int num41 = __instance.workShipCount;
                                            array13[num41].warperCnt += 2;
                                        }
                                    }
                                }
                                StationStore[] array = stationComponent2.storage;
                                lock (array)
                                {
                                    StationStore[] array14 = stationComponent2.storage;
                                    int demandIndex2 = supplyDemandPair2.demandIndex;
                                    array14[demandIndex2].remoteOrder += num34;
                                }
                                __instance.workShipCount++;
                                __instance.idleShipCount--;
                                __instance.IdleShipGetToWork(num38);
                                array = __instance.storage;
                                lock (array)
                                {
                                    StationStore[] array15 = __instance.storage;
                                    int supplyIndex3 = supplyDemandPair2.supplyIndex;
                                    array15[supplyIndex3].count -= num34;
                                    StationStore[] array16 = __instance.storage;
                                    int supplyIndex4 = supplyDemandPair2.supplyIndex;
                                    array16[supplyIndex4].inc -= num37;
                                }
                                __instance.energy -= num31;
                                flag7 = true;
                            }
                        }
                    IL_F6C:
                        if (flag7)
                        {
                            goto IL_1356;
                        }
                    }
                    if (__instance.energy >= num31)
                    {
                        int num42 = __instance.QueryIdleShip(__instance.nextShipIndex);
                        if (num42 >= 0)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                itemId2 = __instance.storage[supplyDemandPair.demandIndex].itemId;
                            }
                            __instance.nextShipIndex = (num42 + 1) % __instance.workShipDatas.Length;
                            __instance.workShipDatas[__instance.workShipCount].stage = -2;
                            __instance.workShipDatas[__instance.workShipCount].planetA = __instance.planetId;
                            __instance.workShipDatas[__instance.workShipCount].planetB = stationComponent2.planetId;
                            __instance.workShipDatas[__instance.workShipCount].otherGId = stationComponent2.gid;
                            __instance.workShipDatas[__instance.workShipCount].direction = 1;
                            __instance.workShipDatas[__instance.workShipCount].t = 0f;
                            __instance.workShipDatas[__instance.workShipCount].itemId = (__instance.workShipOrders[__instance.workShipCount].itemId = itemId2);
                            __instance.workShipDatas[__instance.workShipCount].itemCount = 0;
                            __instance.workShipDatas[__instance.workShipCount].inc = 0;
                            __instance.workShipDatas[__instance.workShipCount].gene = ____tmp_iter_remote;
                            __instance.workShipDatas[__instance.workShipCount].shipIndex = num42;
                            __instance.workShipOrders[__instance.workShipCount].otherStationGId = stationComponent2.gid;
                            __instance.workShipOrders[__instance.workShipCount].thisIndex = supplyDemandPair.demandIndex;
                            __instance.workShipOrders[__instance.workShipCount].otherIndex = supplyDemandPair.supplyIndex;
                            __instance.workShipOrders[__instance.workShipCount].thisOrdered = shipCarries;
                            __instance.workShipOrders[__instance.workShipCount].otherOrdered = -shipCarries;
                            if (flag6)
                            {
                                int[] array4 = consumeRegister;
                                lock (array4)
                                {
                                    if (__instance.warperCount >= 2)
                                    {
                                        ShipData[] array17 = __instance.workShipDatas;
                                        int num43 = __instance.workShipCount;
                                        array17[num43].warperCnt += 2;
                                        __instance.warperCount -= 2;
                                        consumeRegister[1210] += 2;
                                    }
                                    else if (__instance.warperCount >= 1)
                                    {
                                        ShipData[] array18 = __instance.workShipDatas;
                                        int num44 = __instance.workShipCount;
                                        array18[num44].warperCnt += 1;
                                        __instance.warperCount--;
                                        consumeRegister[1210]++;
                                    }
                                    else if (___warperFree)
                                    {
                                        ShipData[] array19 = __instance.workShipDatas;
                                        int num45 = __instance.workShipCount;
                                        array19[num45].warperCnt += 2;
                                    }
                                }
                            }
                            array = __instance.storage;
                            lock (array)
                            {
                                StationStore[] array20 = __instance.storage;
                                int demandIndex3 = supplyDemandPair.demandIndex;
                                array20[demandIndex3].remoteOrder += shipCarries;
                            }
                            array = stationComponent2.storage;
                            lock (array)
                            {
                                StationStore[] array21 = stationComponent2.storage;
                                int supplyIndex5 = supplyDemandPair.supplyIndex;
                                array21[supplyIndex5].remoteOrder -= shipCarries;
                            }
                            __instance.workShipCount++;
                            __instance.idleShipCount--;
                            __instance.IdleShipGetToWork(num42);
                            __instance.energy -= num31;
                        }
                    }
                IL_1356:
                    __instance.remotePairProcess++;
                    __instance.remotePairProcess %= __instance.remotePairCount;
                }
            }
            float num46 = Mathf.Sqrt(shipSailSpeed / 600f);
            float num47 = num46;
            if (num47 > 1f)
            {
                num47 = Mathf.Log(num47) + 1f;
            }
            AstroData astroData = astroPoses[__instance.planetId];
            float num48 = shipSailSpeed * 0.03f * num47;
            float num49 = shipSailSpeed * 0.12f * num47;
            float num50 = shipSailSpeed * 0.4f * num46;
            float num51 = num46 * 0.006f + 1E-05f;
            int j = 0;
            while (j < __instance.workShipCount)
            {
                ShipData shipData = __instance.workShipDatas[j];
                bool flag8 = false;
                Quaternion quaternion = Quaternion.identity;
                if (shipData.otherGId <= 0)
                {
                    shipData.direction = -1;
                    if (shipData.stage > 0)
                    {
                        shipData.stage = 0;
                    }
                }
                if (shipData.stage < -1)
                {
                    if (shipData.direction > 0)
                    {
                        shipData.t += 0.03335f;
                        if (shipData.t > 1f)
                        {
                            shipData.t = 0f;
                            shipData.stage = -1;
                        }
                    }
                    else
                    {
                        shipData.t -= 0.03335f;
                        if (shipData.t < 0f)
                        {
                            shipData.t = 0f;
                            __instance.AddItem(shipData.itemId, shipData.itemCount, shipData.inc);
                            factory.NotifyShipDelivery(shipData.planetB, gStationPool[shipData.otherGId], shipData.planetA, __instance, shipData.itemId, shipData.itemCount);
                            if (__instance.workShipOrders[j].itemId > 0)
                            {
                                StationStore[] array = __instance.storage;
                                lock (array)
                                {
                                    if (__instance.storage[__instance.workShipOrders[j].thisIndex].itemId == __instance.workShipOrders[j].itemId)
                                    {
                                        StationStore[] array22 = __instance.storage;
                                        int thisIndex = __instance.workShipOrders[j].thisIndex;
                                        array22[thisIndex].remoteOrder -= __instance.workShipOrders[j].thisOrdered;
                                    }
                                }
                                __instance.workShipOrders[j].ClearThis();
                            }
                            Array.Copy(__instance.workShipDatas, j + 1, __instance.workShipDatas, j, __instance.workShipDatas.Length - j - 1);
                            Array.Copy(__instance.workShipOrders, j + 1, __instance.workShipOrders, j, __instance.workShipOrders.Length - j - 1);
                            __instance.workShipCount--;
                            __instance.idleShipCount++;
                            __instance.WorkShipBackToIdle(shipData.shipIndex);
                            Array.Clear(__instance.workShipDatas, __instance.workShipCount, __instance.workShipDatas.Length - __instance.workShipCount);
                            Array.Clear(__instance.workShipOrders, __instance.workShipCount, __instance.workShipOrders.Length - __instance.workShipCount);
                            j--;
                            goto IL_38B1;
                        }
                    }
                    shipData.uPos = astroData.uPos + Maths.QRotateLF(astroData.uRot, __instance.shipDiskPos[shipData.shipIndex]);
                    shipData.uVel.x = 0f;
                    shipData.uVel.y = 0f;
                    shipData.uVel.z = 0f;
                    shipData.uSpeed = 0f;
                    shipData.uRot = astroData.uRot * __instance.shipDiskRot[shipData.shipIndex];
                    shipData.uAngularVel.x = 0f;
                    shipData.uAngularVel.y = 0f;
                    shipData.uAngularVel.z = 0f;
                    shipData.uAngularSpeed = 0f;
                    shipData.pPosTemp = Vector3.zero;
                    shipData.pRotTemp = Quaternion.identity;
                    __instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
                    goto IL_36C7;
                }
                if (shipData.stage == -1)
                {
                    if (shipData.direction > 0)
                    {
                        shipData.t += num51;
                        float num52 = shipData.t;
                        if (shipData.t > 1f)
                        {
                            shipData.t = 1f;
                            num52 = 1f;
                            shipData.stage = 0;
                        }
                        __instance.shipRenderers[shipData.shipIndex].anim.z = num52;
                        num52 = (3f - num52 - num52) * num52 * num52;
                        shipData.uPos = astroData.uPos + Maths.QRotateLF(astroData.uRot, __instance.shipDiskPos[shipData.shipIndex] + __instance.shipDiskPos[shipData.shipIndex].normalized * (25f * num52));
                        shipData.uRot = astroData.uRot * __instance.shipDiskRot[shipData.shipIndex];
                    }
                    else
                    {
                        shipData.t -= num51 * 0.6666667f;
                        float num52 = shipData.t;
                        if (shipData.t < 0f)
                        {
                            shipData.t = 1f;
                            num52 = 0f;
                            shipData.stage = -2;
                        }
                        __instance.shipRenderers[shipData.shipIndex].anim.z = num52;
                        num52 = (3f - num52 - num52) * num52 * num52;
                        VectorLF3 lhs = astroData.uPos + Maths.QRotateLF(astroData.uRot, __instance.shipDiskPos[shipData.shipIndex]);
                        VectorLF3 lhs2 = astroData.uPos + Maths.QRotateLF(astroData.uRot, shipData.pPosTemp);
                        shipData.uPos = lhs * (1f - num52) + lhs2 * num52;
                        shipData.uRot = astroData.uRot * Quaternion.Slerp(__instance.shipDiskRot[shipData.shipIndex], shipData.pRotTemp, num52 * 2f - 1f);
                    }
                    shipData.uVel.x = 0f;
                    shipData.uVel.y = 0f;
                    shipData.uVel.z = 0f;
                    shipData.uSpeed = 0f;
                    shipData.uAngularVel.x = 0f;
                    shipData.uAngularVel.y = 0f;
                    shipData.uAngularVel.z = 0f;
                    shipData.uAngularSpeed = 0f;
                    goto IL_36C7;
                }
                if (shipData.stage == 0)
                {
                    AstroData astroData2 = astroPoses[shipData.planetB];
                    VectorLF3 lhs3;
                    if (shipData.direction > 0)
                    {
                        lhs3 = astroData2.uPos + Maths.QRotateLF(astroData2.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * 25f);
                    }
                    else
                    {
                        lhs3 = astroData.uPos + Maths.QRotateLF(astroData.uRot, __instance.shipDiskPos[shipData.shipIndex] + __instance.shipDiskPos[shipData.shipIndex].normalized * 25f);
                    }
                    VectorLF3 vectorLF = lhs3 - shipData.uPos;
                    double num53 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
                    VectorLF3 vectorLF2 = (shipData.direction > 0) ? (astroData.uPos - shipData.uPos) : (astroData2.uPos - shipData.uPos);
                    double num54 = vectorLF2.x * vectorLF2.x + vectorLF2.y * vectorLF2.y + vectorLF2.z * vectorLF2.z;
                    bool flag9 = num54 <= astroData.uRadius * astroData.uRadius * 2.25;
                    bool flag10 = false;
                    if (num53 < 6.0)
                    {
                        shipData.t = 1f;
                        shipData.stage = shipData.direction;
                        flag10 = true;
                    }
                    float num55 = 0f;
                    if (flag)
                    {
                        double num56 = (astroData.uPos - astroData2.uPos).magnitude * 2.0;
                        double num57 = (shipWarpSpeed < num56) ? shipWarpSpeed : num56;
                        double num58 = __instance.warpEnableDist * 0.5;
                        if (shipData.warpState <= 0f)
                        {
                            shipData.warpState = 0f;
                            if (num54 > 25000000.0 && num53 > num58 && shipData.uSpeed >= shipSailSpeed && (shipData.warperCnt > 0 || ___warperFree))
                            {
                                shipData.warperCnt--;
                                shipData.warpState += (float)dt;
                            }
                        }
                        else
                        {
                            num55 = (float)(num57 * ((Math.Pow(1001.0, shipData.warpState) - 1.0) / 1000.0));
                            double num59 = num55 * 0.0449 + 5000.0 + shipSailSpeed * 0.25;
                            double num60 = num53 - num59;
                            if (num60 < 0.0)
                            {
                                num60 = 0.0;
                            }
                            if (num53 < num59)
                            {
                                shipData.warpState -= (float)(dt * 4.0);
                            }
                            else
                            {
                                shipData.warpState += (float)dt;
                            }
                            if (shipData.warpState < 0f)
                            {
                                shipData.warpState = 0f;
                            }
                            else if (shipData.warpState > 1f)
                            {
                                shipData.warpState = 1f;
                            }
                            if (shipData.warpState > 0f)
                            {
                                num55 = (float)(num57 * ((Math.Pow(1001.0, shipData.warpState) - 1.0) / 1000.0));
                                if (num55 * dt > num60)
                                {
                                    num55 = (float)(num60 / dt * 1.01);
                                }
                            }
                        }
                    }
                    double num61 = num53 / (shipData.uSpeed + 0.1) * 0.382 * num47;
                    float num62;
                    if (shipData.warpState > 0f)
                    {
                        num62 = (shipData.uSpeed = shipSailSpeed + num55);
                        if (num62 > shipSailSpeed)
                        {
                            num62 = shipSailSpeed;
                        }
                    }
                    else
                    {
                        float num63 = (float)(shipData.uSpeed * num61) + 6f;
                        if (num63 > shipSailSpeed)
                        {
                            num63 = shipSailSpeed;
                        }
                        float num64 = (float)dt * (flag9 ? num48 : num49);
                        if (shipData.uSpeed < num63 - num64)
                        {
                            shipData.uSpeed += num64;
                        }
                        else if (shipData.uSpeed > num63 + num50)
                        {
                            shipData.uSpeed -= num50;
                        }
                        else
                        {
                            shipData.uSpeed = num63;
                        }
                        num62 = shipData.uSpeed;
                    }
                    int num65 = -1;
                    double rhs = 0.0;
                    double num66 = 1E+40;
                    int num67 = shipData.planetA / 100 * 100;
                    int num68 = shipData.planetB / 100 * 100;
                    for (int k = num67; k < num67 + 10; k++)
                    {
                        float uRadius = astroPoses[k].uRadius;
                        if (uRadius >= 1f)
                        {
                            VectorLF3 vectorLF3 = shipData.uPos - astroPoses[k].uPos;
                            double num69 = vectorLF3.x * vectorLF3.x + vectorLF3.y * vectorLF3.y + vectorLF3.z * vectorLF3.z;
                            double num70 = -(shipData.uVel.x * vectorLF3.x + shipData.uVel.y * vectorLF3.y + shipData.uVel.z * vectorLF3.z);
                            if ((num70 > 0.0 || num69 < uRadius * uRadius * 7f) && num69 < num66)
                            {
                                rhs = ((num70 < 0.0) ? 0.0 : num70);
                                num65 = k;
                                num66 = num69;
                            }
                        }
                    }
                    if (num68 != num67)
                    {
                        for (int l = num68; l < num68 + 10; l++)
                        {
                            float uRadius2 = astroPoses[l].uRadius;
                            if (uRadius2 >= 1f)
                            {
                                VectorLF3 vectorLF4 = shipData.uPos - astroPoses[l].uPos;
                                double num71 = vectorLF4.x * vectorLF4.x + vectorLF4.y * vectorLF4.y + vectorLF4.z * vectorLF4.z;
                                double num72 = -(shipData.uVel.x * vectorLF4.x + shipData.uVel.y * vectorLF4.y + shipData.uVel.z * vectorLF4.z);
                                if ((num72 > 0.0 || num71 < uRadius2 * uRadius2 * 7f) && num71 < num66)
                                {
                                    rhs = ((num72 < 0.0) ? 0.0 : num72);
                                    num65 = l;
                                    num66 = num71;
                                }
                            }
                        }
                    }
                    VectorLF3 vectorLF5 = VectorLF3.zero;
                    VectorLF3 rhs2 = VectorLF3.zero;
                    float num73 = 0f;
                    VectorLF3 vectorLF6 = Vector3.zero;
                    if (num65 > 0)
                    {
                        float num74 = astroPoses[num65].uRadius;
                        if (num65 % 100 == 0)
                        {
                            num74 *= 2.5f;
                        }
                        double num75 = Math.Max(1.0, ((astroPoses[num65].uPosNext - astroPoses[num65].uPos).magnitude - 0.5) * 0.6);
                        double num76 = 1.0 + 1600.0 / num74;
                        double num77 = 1.0 + 250.0 / num74;
                        num76 *= num75 * num75;
                        double num78 = (num65 == shipData.planetA || num65 == shipData.planetB) ? 1.25f : 1.5f;
                        double num79 = Math.Sqrt(num66);
                        double num80 = num74 / num79 * 1.6 - 0.1;
                        if (num80 > 1.0)
                        {
                            num80 = 1.0;
                        }
                        else if (num80 < 0.0)
                        {
                            num80 = 0.0;
                        }
                        double num81 = num79 - num74 * 0.82;
                        if (num81 < 1.0)
                        {
                            num81 = 1.0;
                        }
                        double num82 = (num62 - 6f) / (num81 * num47) * 0.6 - 0.01;
                        if (num82 > 1.5)
                        {
                            num82 = 1.5;
                        }
                        else if (num82 < 0.0)
                        {
                            num82 = 0.0;
                        }
                        VectorLF3 vectorLF7 = shipData.uPos + (VectorLF3)shipData.uVel * rhs - astroPoses[num65].uPos;
                        double num83 = vectorLF7.magnitude / num74;
                        if (num83 < num78)
                        {
                            double num84 = (num83 - 1.0) / (num78 - 1.0);
                            if (num84 < 0.0)
                            {
                                num84 = 0.0;
                            }
                            num84 = 1.0 - num84 * num84;
                            rhs2 = vectorLF7.normalized * (num82 * num82 * num84 * 2.0 * (1f - shipData.warpState));
                        }
                        VectorLF3 vectorLF8 = shipData.uPos - astroPoses[num65].uPos;
                        VectorLF3 lhs4 = new VectorLF3(vectorLF8.x / num79, vectorLF8.y / num79, vectorLF8.z / num79);
                        vectorLF5 += lhs4 * num80;
                        num73 = (float)num80;
                        double num85 = num79 / num74;
                        num85 *= num85;
                        num85 = (num76 - num85) / (num76 - num77);
                        if (num85 > 1.0)
                        {
                            num85 = 1.0;
                        }
                        else if (num85 < 0.0)
                        {
                            num85 = 0.0;
                        }
                        if (num85 > 0.0)
                        {
                            VectorLF3 v = Maths.QInvRotateLF(astroPoses[num65].uRot, vectorLF8);
                            VectorLF3 lhs5 = Maths.QRotateLF(astroPoses[num65].uRotNext, v) + astroPoses[num65].uPosNext;
                            num85 = (3.0 - num85 - num85) * num85 * num85;
                            vectorLF6 = (lhs5 - shipData.uPos) * num85;
                        }
                    }
                    Vector3 vector;
                    shipData.uRot.ForwardUp(out shipData.uVel, out vector);
                    Vector3 vector2 = vector * (1f - num73) + (Vector3)vectorLF5 * num73;
                    vector2 -= Vector3.Dot(vector2, shipData.uVel) * shipData.uVel;
                    vector2.Normalize();
                    Vector3 vector3 = vectorLF.normalized + rhs2;
                    Vector3 a = Vector3.Cross(shipData.uVel, vector3);
                    float num86 = shipData.uVel.x * vector3.x + shipData.uVel.y * vector3.y + shipData.uVel.z * vector3.z;
                    Vector3 a2 = Vector3.Cross(vector, vector2);
                    float num87 = vector.x * vector2.x + vector.y * vector2.y + vector.z * vector2.z;
                    if (num86 < 0f)
                    {
                        a = a.normalized;
                    }
                    if (num87 < 0f)
                    {
                        a2 = a2.normalized;
                    }
                    float d = (num61 < 3.0) ? ((3.25f - (float)num61) * 4f) : (num62 / shipSailSpeed * (flag9 ? 0.2f : 1f));
                    a = a * d + a2 * 2f;
                    Vector3 a3 = a - shipData.uAngularVel;
                    float d2 = (a3.sqrMagnitude < 0.1f) ? 1f : 0.05f;
                    shipData.uAngularVel += a3 * d2;
                    double num88 = shipData.uSpeed * dt;
                    shipData.uPos.x = shipData.uPos.x + shipData.uVel.x * num88 + vectorLF6.x;
                    shipData.uPos.y = shipData.uPos.y + shipData.uVel.y * num88 + vectorLF6.y;
                    shipData.uPos.z = shipData.uPos.z + shipData.uVel.z * num88 + vectorLF6.z;
                    Vector3 normalized = shipData.uAngularVel.normalized;
                    double num89 = shipData.uAngularVel.magnitude * dt * 0.5;
                    float w = (float)Math.Cos(num89);
                    float num90 = (float)Math.Sin(num89);
                    Quaternion lhs6 = new Quaternion(normalized.x * num90, normalized.y * num90, normalized.z * num90, w);
                    shipData.uRot = lhs6 * shipData.uRot;
                    if (shipData.warpState > 0f)
                    {
                        float num91 = shipData.warpState * shipData.warpState * shipData.warpState;
                        shipData.uRot = Quaternion.Slerp(shipData.uRot, Quaternion.LookRotation(vector3, vector2), num91);
                        shipData.uAngularVel *= 1f - num91;
                    }
                    if (num53 < 100.0)
                    {
                        float num92 = 1f - (float)num53 / 100f;
                        num92 = (3f - num92 - num92) * num92 * num92;
                        num92 *= num92;
                        if (shipData.direction > 0)
                        {
                            quaternion = Quaternion.Slerp(shipData.uRot, astroData2.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f)), num92);
                        }
                        else
                        {
                            Vector3 vector4 = (shipData.uPos - astroData.uPos).normalized;
                            Vector3 normalized2 = (shipData.uVel - Vector3.Dot(shipData.uVel, vector4) * vector4).normalized;
                            quaternion = Quaternion.Slerp(shipData.uRot, Quaternion.LookRotation(normalized2, vector4), num92);
                        }
                        flag8 = true;
                    }
                    if (flag10)
                    {
                        shipData.uRot = quaternion;
                        if (shipData.direction > 0)
                        {
                            shipData.pPosTemp = Maths.QInvRotateLF(astroData2.uRot, shipData.uPos - astroData2.uPos);
                            shipData.pRotTemp = Quaternion.Inverse(astroData2.uRot) * shipData.uRot;
                        }
                        else
                        {
                            shipData.pPosTemp = Maths.QInvRotateLF(astroData.uRot, shipData.uPos - astroData.uPos);
                            shipData.pRotTemp = Quaternion.Inverse(astroData.uRot) * shipData.uRot;
                        }
                        quaternion = Quaternion.identity;
                        flag8 = false;
                    }
                    if (__instance.shipRenderers[shipData.shipIndex].anim.z > 1f)
                    {
                        ShipRenderingData[] array23 = __instance.shipRenderers;
                        int shipIndex = shipData.shipIndex;
                        array23[shipIndex].anim.z -= (float)dt * 0.3f;
                    }
                    else
                    {
                        __instance.shipRenderers[shipData.shipIndex].anim.z = 1f;
                    }
                    __instance.shipRenderers[shipData.shipIndex].anim.w = shipData.warpState;
                    goto IL_36C7;
                }
                if (shipData.stage == 1)
                {
                    AstroData astroData3 = astroPoses[shipData.planetB];
                    float num93;
                    if (shipData.direction > 0)
                    {
                        shipData.t -= num51 * 0.6666667f;
                        num93 = shipData.t;
                        if (shipData.t < 0f)
                        {
                            shipData.t = 1f;
                            num93 = 0f;
                            shipData.stage = 2;
                        }
                        num93 = (3f - num93 - num93) * num93 * num93;
                        float num94 = num93 * 2f;
                        float num95 = num93 * 2f - 1f;
                        VectorLF3 lhs7 = astroData3.uPos + Maths.QRotateLF(astroData3.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * 7.2700005f);
                        if (num93 > 0.5f)
                        {
                            VectorLF3 lhs8 = astroData3.uPos + Maths.QRotateLF(astroData3.uRot, shipData.pPosTemp);
                            shipData.uPos = lhs7 * (1f - num95) + lhs8 * num95;
                            shipData.uRot = astroData3.uRot * Quaternion.Slerp(gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f), shipData.pRotTemp, num95 * 1.5f - 0.5f);
                        }
                        else
                        {
                            VectorLF3 lhs9 = astroData3.uPos + Maths.QRotateLF(astroData3.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * -14.4f);
                            shipData.uPos = lhs9 * (1f - num94) + lhs7 * num94;
                            shipData.uRot = astroData3.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
                        }
                    }
                    else
                    {
                        shipData.t += num51;
                        num93 = shipData.t;
                        if (shipData.t > 1f)
                        {
                            shipData.t = 1f;
                            num93 = 1f;
                            shipData.stage = 0;
                        }
                        num93 = (3f - num93 - num93) * num93 * num93;
                        shipData.uPos = astroData3.uPos + Maths.QRotateLF(astroData3.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * (-14.4f + 39.4f * num93));
                        shipData.uRot = astroData3.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
                    }
                    shipData.uVel.x = 0f;
                    shipData.uVel.y = 0f;
                    shipData.uVel.z = 0f;
                    shipData.uSpeed = 0f;
                    shipData.uAngularVel.x = 0f;
                    shipData.uAngularVel.y = 0f;
                    shipData.uAngularVel.z = 0f;
                    shipData.uAngularSpeed = 0f;
                    __instance.shipRenderers[shipData.shipIndex].anim.z = num93 * 1.7f - 0.7f;
                    goto IL_36C7;
                }
                if (shipData.direction > 0)
                {
                    shipData.t -= 0.0334f;
                    if (shipData.t < 0f)
                    {
                        shipData.t = 0f;
                        StationComponent stationComponent3 = gStationPool[shipData.otherGId];
                        StationStore[] array24 = stationComponent3.storage;
                        if ((astroPoses[shipData.planetA].uPos - astroPoses[shipData.planetB].uPos).sqrMagnitude > __instance.warpEnableDist * __instance.warpEnableDist && shipData.warperCnt == 0 && stationComponent3.warperCount > 0)
                        {
                            int[] array4 = consumeRegister;
                            lock (array4)
                            {
                                shipData.warperCnt++;
                                stationComponent3.warperCount--;
                                consumeRegister[1210]++;
                            }
                        }
                        if (shipData.itemCount > 0)
                        {
                            stationComponent3.AddItem(shipData.itemId, shipData.itemCount, shipData.inc);
                            factory.NotifyShipDelivery(shipData.planetA, __instance, shipData.planetB, stationComponent3, shipData.itemId, shipData.itemCount);
                            shipData.itemCount = 0;
                            shipData.inc = 0;
                            if (__instance.workShipOrders[j].otherStationGId > 0)
                            {
                                StationStore[] array = array24;
                                lock (array)
                                {
                                    if (array24[__instance.workShipOrders[j].otherIndex].itemId == __instance.workShipOrders[j].itemId)
                                    {
                                        StationStore[] array25 = array24;
                                        int otherIndex = __instance.workShipOrders[j].otherIndex;
                                        array25[otherIndex].remoteOrder -= __instance.workShipOrders[j].otherOrdered;
                                    }
                                }
                                __instance.workShipOrders[j].ClearOther();
                            }
                            if (__instance.remotePairCount > 0)
                            {
                                __instance.remotePairProcess %= __instance.remotePairCount;
                                int num96 = __instance.remotePairProcess;
                                int num97 = __instance.remotePairProcess;
                                do
                                {
                                    SupplyDemandPair supplyDemandPair3 = __instance.remotePairs[num97];
                                    if (supplyDemandPair3.demandId == __instance.gid && supplyDemandPair3.supplyId == stationComponent3.gid)
                                    {
                                        StationStore[] array = __instance.storage;
                                        lock (array)
                                        {
                                            num9 = __instance.storage[supplyDemandPair3.demandIndex].remoteDemandCount;
                                            num10 = __instance.storage[supplyDemandPair3.demandIndex].totalDemandCount;
                                            itemId2 = __instance.storage[supplyDemandPair3.demandIndex].itemId;
                                        }
                                    }
                                    if (supplyDemandPair3.demandId == __instance.gid && supplyDemandPair3.supplyId == stationComponent3.gid)
                                    {
                                        StationStore[] array = array24;
                                        lock (array)
                                        {
                                            num14 = array24[supplyDemandPair3.supplyIndex].count;
                                            num15 = array24[supplyDemandPair3.supplyIndex].inc;
                                            num16 = array24[supplyDemandPair3.supplyIndex].remoteSupplyCount;
                                            num17 = array24[supplyDemandPair3.supplyIndex].totalSupplyCount;
                                        }
                                    }
                                    if (supplyDemandPair3.demandId == __instance.gid && supplyDemandPair3.supplyId == stationComponent3.gid && num9 > 0 && num10 > 0 && num14 >= shipCarries && num16 >= shipCarries && num17 >= shipCarries)
                                    {
                                        int num98 = (shipCarries < num14) ? shipCarries : num14;
                                        int num99 = num14;
                                        int num100 = num15;
                                        int num101 = split_inc(ref num99, ref num100, num98);
                                        shipData.itemId = (__instance.workShipOrders[j].itemId = itemId2);
                                        shipData.itemCount = num98;
                                        shipData.inc = num101;
                                        StationStore[] array = array24;
                                        lock (array)
                                        {
                                            StationStore[] array26 = array24;
                                            int supplyIndex6 = supplyDemandPair3.supplyIndex;
                                            array26[supplyIndex6].count -= num98;
                                            StationStore[] array27 = array24;
                                            int supplyIndex7 = supplyDemandPair3.supplyIndex;
                                            array27[supplyIndex7].inc -= num101;
                                        }
                                        __instance.workShipOrders[j].otherStationGId = stationComponent3.gid;
                                        __instance.workShipOrders[j].thisIndex = supplyDemandPair3.demandIndex;
                                        __instance.workShipOrders[j].otherIndex = supplyDemandPair3.supplyIndex;
                                        __instance.workShipOrders[j].thisOrdered = num98;
                                        __instance.workShipOrders[j].otherOrdered = 0;
                                        array = __instance.storage;
                                        lock (array)
                                        {
                                            StationStore[] array28 = __instance.storage;
                                            int demandIndex4 = supplyDemandPair3.demandIndex;
                                            array28[demandIndex4].remoteOrder += num98;
                                            break;
                                        }
                                    }
                                    num97++;
                                    num97 %= __instance.remotePairCount;
                                }
                                while (num96 != num97);
                            }
                        }
                        else
                        {
                            int itemId3 = shipData.itemId;
                            int num102 = shipCarries;
                            int inc;
                            stationComponent3.TakeItem(ref itemId3, ref num102, out inc);
                            shipData.itemCount = num102;
                            shipData.inc = inc;
                            StationStore[] array;
                            if (__instance.workShipOrders[j].otherStationGId > 0)
                            {
                                array = array24;
                                lock (array)
                                {
                                    if (array24[__instance.workShipOrders[j].otherIndex].itemId == __instance.workShipOrders[j].itemId)
                                    {
                                        StationStore[] array29 = array24;
                                        int otherIndex2 = __instance.workShipOrders[j].otherIndex;
                                        array29[otherIndex2].remoteOrder -= __instance.workShipOrders[j].otherOrdered;
                                    }
                                }
                                __instance.workShipOrders[j].ClearOther();
                            }
                            array = __instance.storage;
                            lock (array)
                            {
                                if (__instance.storage[__instance.workShipOrders[j].thisIndex].itemId == __instance.workShipOrders[j].itemId && __instance.workShipOrders[j].thisOrdered != num102)
                                {
                                    int num103 = num102 - __instance.workShipOrders[j].thisOrdered;
                                    StationStore[] array30 = __instance.storage;
                                    int thisIndex2 = __instance.workShipOrders[j].thisIndex;
                                    array30[thisIndex2].remoteOrder += num103;
                                    RemoteLogisticOrder[] array31 = __instance.workShipOrders;
                                    int num104 = j;
                                    array31[num104].thisOrdered += num103;
                                }
                            }
                        }
                        shipData.direction = -1;
                    }
                }
                else
                {
                    shipData.t += 0.0334f;
                    if (shipData.t > 1f)
                    {
                        shipData.t = 0f;
                        shipData.stage = 1;
                    }
                }
                AstroData astroData4 = astroPoses[shipData.planetB];
                shipData.uPos = astroData4.uPos + Maths.QRotateLF(astroData4.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * -14.4f);
                shipData.uVel.x = 0f;
                shipData.uVel.y = 0f;
                shipData.uVel.z = 0f;
                shipData.uSpeed = 0f;
                shipData.uRot = astroData4.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
                shipData.uAngularVel.x = 0f;
                shipData.uAngularVel.y = 0f;
                shipData.uAngularVel.z = 0f;
                shipData.uAngularSpeed = 0f;
                shipData.pPosTemp = Vector3.zero;
                shipData.pRotTemp = Quaternion.identity;
                __instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
                goto IL_36C7;
            IL_38B1:
                j++;
                continue;
            IL_36C7:
                __instance.workShipDatas[j] = shipData;
                if (flag8)
                {
                    var temp1 = shipData.uVel * shipData.uSpeed;
                    __instance.shipRenderers[shipData.shipIndex].SetPose(ref shipData.uPos, ref quaternion, ref relativePos, ref relativeRot, ref temp1, (shipData.itemCount > 0) ? shipData.itemId : 0);
                    if (starmap)
                    {
                        __instance.shipUIRenderers[shipData.shipIndex].SetPose(ref shipData.uPos, ref quaternion, (float)(astroPoses[shipData.planetA].uPos - astroPoses[shipData.planetB].uPos).magnitude, shipData.uSpeed, (shipData.itemCount > 0) ? shipData.itemId : 0);
                    }
                }
                else
                {
                    var temp1 = shipData.uVel * shipData.uSpeed;
                    __instance.shipRenderers[shipData.shipIndex].SetPose(ref shipData.uPos, ref shipData.uRot, ref relativePos, ref relativeRot, ref temp1, (shipData.itemCount > 0) ? shipData.itemId : 0);
                    if (starmap)
                    {
                        __instance.shipUIRenderers[shipData.shipIndex].SetPose(ref shipData.uPos, ref shipData.uRot, (float)(astroPoses[shipData.planetA].uPos - astroPoses[shipData.planetB].uPos).magnitude, shipData.uSpeed, (shipData.itemCount > 0) ? shipData.itemId : 0);
                    }
                }
                if (__instance.shipRenderers[shipData.shipIndex].anim.z < 0f)
                {
                    __instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
                }
                goto IL_38B1;
            }
            __instance.ShipRenderersOnTick(astroPoses, ref relativePos, ref relativeRot);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "InternalTickLocal",
            typeof(PlanetFactory), typeof(int), typeof(float), typeof(float), typeof(float), typeof(int), typeof(StationComponent[]))]
        public static bool InternalTickLocal(StationComponent __instance, int ____tmp_iter_local, PlanetFactory factory, int timeGene, float dt, float power, float droneSpeed, int droneCarries, StationComponent[] stationPool)
        {
            __instance.energy += (int)(__instance.energyPerTick * power);
            __instance.energy -= 1000L;
            if (__instance.energy > __instance.energyMax)
            {
                __instance.energy = __instance.energyMax;
            }
            else if (__instance.energy < 0L)
            {
                __instance.energy = 0L;
            }
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int itemId = 0;
            int num6 = 0;
            int num7 = 0;
            int itemId2 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            int num13 = 0;
            int num14 = 0;
            int num15 = __instance.workDroneCount + __instance.idleDroneCount;
            if (timeGene == __instance.gene % 20 || (num15 >= 75 && timeGene % 10 == __instance.gene % 10))
            {
                ____tmp_iter_local++;
                if (__instance.localPairCount > 0 && __instance.idleDroneCount > 0)
                {
                    __instance.localPairProcess %= __instance.localPairCount;
                    int num16 = __instance.localPairProcess;
                    SupplyDemandPair supplyDemandPair;
                    StationComponent stationComponent;
                    float x;
                    float y;
                    float z;
                    float x2;
                    float y2;
                    float z2;
                    double num20;
                    double num21;
                    StationComponent stationComponent2;
                    float x3;
                    float y3;
                    float z3;
                    float x4;
                    float y4;
                    float z4;
                    double num24;
                    double num25;
                    for (; ; )
                    {
                        int num17 = (droneCarries - 1) * __instance.deliveryDrones / 100;
                        supplyDemandPair = __instance.localPairs[__instance.localPairProcess];
                        if (supplyDemandPair.supplyId == __instance.id)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                num = __instance.storage[supplyDemandPair.supplyIndex].max;
                                num2 = __instance.storage[supplyDemandPair.supplyIndex].count;
                                num3 = __instance.storage[supplyDemandPair.supplyIndex].inc;
                                num4 = __instance.storage[supplyDemandPair.supplyIndex].localSupplyCount;
                                num5 = __instance.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
                            }
                        }
                        if (supplyDemandPair.supplyId == __instance.id && num <= num17)
                        {
                            num17 = num - 1;
                        }
                        if (num17 < 0)
                        {
                            num17 = 0;
                        }
                        if (supplyDemandPair.supplyId == __instance.id && num2 > num17 && num4 > num17 && num5 > num17)
                        {
                            stationComponent = stationPool[supplyDemandPair.demandId];
                            if (stationComponent != null)
                            {
                                x = __instance.droneDock.x;
                                y = __instance.droneDock.y;
                                z = __instance.droneDock.z;
                                x2 = stationComponent.droneDock.x;
                                y2 = stationComponent.droneDock.y;
                                z2 = stationComponent.droneDock.z;
                                double num18 = Math.Sqrt(x * x + y * y + z * z);
                                double num19 = Math.Sqrt(x2 * x2 + y2 * y2 + z2 * z2);
                                num20 = (num18 + num19) * 0.5;
                                num21 = (x * x2 + y * y2 + z * z2) / (num18 * num19);
                                if (num21 < -1.0)
                                {
                                    num21 = -1.0;
                                }
                                else if (num21 > 1.0)
                                {
                                    num21 = 1.0;
                                }
                                if (num21 >= __instance.tripRangeDrones - 1E-06)
                                {
                                    StationStore[] array = stationComponent.storage;
                                    lock (array)
                                    {
                                        num13 = stationComponent.storage[supplyDemandPair.demandIndex].localDemandCount;
                                        num14 = stationComponent.storage[supplyDemandPair.demandIndex].totalDemandCount;
                                    }
                                }
                                bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent.entityId);
                                if ((enforceLocalRange == false && num21 >= __instance.tripRangeDrones - 1E-06 ||
                                     enforceLocalRange == true && num21 >= __instance.tripRangeDrones - 1E-06 && num21 >= stationComponent.tripRangeDrones - 1E-06)
                                    && num13 > 0 && num14 > 0)
                                {
                                    break;
                                }
                            }
                        }
                        if (supplyDemandPair.demandId == __instance.id)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                num6 = __instance.storage[supplyDemandPair.demandIndex].localDemandCount;
                                num7 = __instance.storage[supplyDemandPair.demandIndex].totalDemandCount;
                            }
                        }
                        if (supplyDemandPair.demandId == __instance.id && num6 > 0 && num7 > 0)
                        {
                            stationComponent2 = stationPool[supplyDemandPair.supplyId];
                            if (stationComponent2 != null)
                            {
                                x3 = __instance.droneDock.x;
                                y3 = __instance.droneDock.y;
                                z3 = __instance.droneDock.z;
                                x4 = stationComponent2.droneDock.x;
                                y4 = stationComponent2.droneDock.y;
                                z4 = stationComponent2.droneDock.z;
                                double num22 = Math.Sqrt(x3 * x3 + y3 * y3 + z3 * z3);
                                double num23 = Math.Sqrt(x4 * x4 + y4 * y4 + z4 * z4);
                                num24 = (num22 + num23) * 0.5;
                                num25 = (x3 * x4 + y3 * y4 + z3 * z4) / (num22 * num23);
                                if (num25 < -1.0)
                                {
                                    num25 = -1.0;
                                }
                                else if (num25 > 1.0)
                                {
                                    num25 = 1.0;
                                }
                                StationStore[] array = stationComponent2.storage;
                                lock (array)
                                {
                                    num8 = stationComponent2.storage[supplyDemandPair.supplyIndex].max;
                                    num9 = stationComponent2.storage[supplyDemandPair.supplyIndex].count;
                                    num10 = stationComponent2.storage[supplyDemandPair.supplyIndex].inc;
                                    num11 = stationComponent2.storage[supplyDemandPair.supplyIndex].localSupplyCount;
                                    num12 = stationComponent2.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
                                }
                                if (num8 <= num17)
                                {
                                    num17 = num8 - 1;
                                }
                                if (num17 < 0)
                                {
                                    num17 = 0;
                                }
                                bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent2.entityId);
                                if ((enforceLocalRange == false && num25 >= __instance.tripRangeDrones - 1E-06 ||
                                    enforceLocalRange == true && num25 >= __instance.tripRangeDrones - 1E-06 && num25 >= stationComponent2.tripRangeDrones - 1E-06) &&
                                     num9 > num17 && num11 > num17 && num12 > num17)
                                {
                                    goto Block_43;
                                }
                            }
                        }
                        __instance.localPairProcess++;
                        __instance.localPairProcess %= __instance.localPairCount;
                        if (num16 == __instance.localPairProcess)
                        {
                            goto IL_10D7;
                        }
                    }
                    double num26 = (float)Math.Acos(num21);
                    double num27 = num20 * num26;
                    long num28 = (long)(num27 * 20000.0 * 2.0 + 800000.0);
                    if (__instance.energy >= num28)
                    {
                        StationStore[] array = __instance.storage;
                        lock (array)
                        {
                            num2 = __instance.storage[supplyDemandPair.supplyIndex].count;
                            num3 = __instance.storage[supplyDemandPair.supplyIndex].inc;
                            itemId = __instance.storage[supplyDemandPair.supplyIndex].itemId;
                        }
                        int num29 = (droneCarries < num2) ? droneCarries : num2;
                        int num30 = num2;
                        int num31 = num3;
                        int num32 = split_inc(ref num30, ref num31, num29);
                        __instance.workDroneDatas[__instance.workDroneCount].begin = new Vector3(x, y, z);
                        __instance.workDroneDatas[__instance.workDroneCount].end = new Vector3(x2, y2, z2);
                        __instance.workDroneDatas[__instance.workDroneCount].endId = stationComponent.id;
                        __instance.workDroneDatas[__instance.workDroneCount].direction = 1f;
                        __instance.workDroneDatas[__instance.workDroneCount].maxt = (float)num27;
                        __instance.workDroneDatas[__instance.workDroneCount].t = -1.5f;
                        __instance.workDroneDatas[__instance.workDroneCount].itemId = (__instance.workDroneOrders[__instance.workDroneCount].itemId = itemId);
                        __instance.workDroneDatas[__instance.workDroneCount].itemCount = num29;
                        __instance.workDroneDatas[__instance.workDroneCount].inc = num32;
                        __instance.workDroneDatas[__instance.workDroneCount].gene = ____tmp_iter_local;
                        __instance.workDroneOrders[__instance.workDroneCount].otherStationId = stationComponent.id;
                        __instance.workDroneOrders[__instance.workDroneCount].thisIndex = supplyDemandPair.supplyIndex;
                        __instance.workDroneOrders[__instance.workDroneCount].otherIndex = supplyDemandPair.demandIndex;
                        __instance.workDroneOrders[__instance.workDroneCount].thisOrdered = 0;
                        __instance.workDroneOrders[__instance.workDroneCount].otherOrdered = num29;
                        array = stationComponent.storage;
                        lock (array)
                        {
                            StationStore[] array2 = stationComponent.storage;
                            int demandIndex = supplyDemandPair.demandIndex;
                            array2[demandIndex].localOrder += num29;
                        }
                        __instance.workDroneCount++;
                        __instance.idleDroneCount--;
                        array = __instance.storage;
                        lock (array)
                        {
                            StationStore[] array3 = __instance.storage;
                            int supplyIndex = supplyDemandPair.supplyIndex;
                            array3[supplyIndex].count -= num29;
                            StationStore[] array4 = __instance.storage;
                            int supplyIndex2 = supplyDemandPair.supplyIndex;
                            array4[supplyIndex2].inc -= num32;
                        }
                        __instance.energy -= num28;
                    }
                    goto IL_10D7;
                Block_43:
                    double num33 = (float)Math.Acos(num25);
                    double num34 = num24 * num33;
                    long num35 = (long)(num34 * 20000.0 * 2.0 + 800000.0);
                    bool flag2 = false;
                    __instance.localPairProcess %= __instance.localPairCount;
                    int num36 = __instance.localPairProcess + 1;
                    int num37 = __instance.localPairProcess;
                    num36 %= __instance.localPairCount;
                    SupplyDemandPair supplyDemandPair2;
                    for (; ; )
                    {
                        supplyDemandPair2 = __instance.localPairs[num36];
                        if (supplyDemandPair2.supplyId == __instance.id && supplyDemandPair2.demandId == stationComponent2.id)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                num2 = __instance.storage[supplyDemandPair2.supplyIndex].count;
                                num3 = __instance.storage[supplyDemandPair2.supplyIndex].inc;
                                num4 = __instance.storage[supplyDemandPair2.supplyIndex].localSupplyCount;
                                num5 = __instance.storage[supplyDemandPair2.supplyIndex].totalSupplyCount;
                                itemId = __instance.storage[supplyDemandPair2.supplyIndex].itemId;
                            }
                        }
                        if (supplyDemandPair2.supplyId == __instance.id && supplyDemandPair2.demandId == stationComponent2.id)
                        {
                            StationStore[] array = stationComponent2.storage;
                            lock (array)
                            {
                                num13 = stationComponent2.storage[supplyDemandPair2.demandIndex].localDemandCount;
                                num14 = stationComponent2.storage[supplyDemandPair2.demandIndex].totalDemandCount;
                            }
                        }
                        int num17 = 0;
                        if (supplyDemandPair2.supplyId == __instance.id && supplyDemandPair2.demandId == stationComponent2.id && num2 > num17 && num4 > num17 && num5 > num17 && num13 > 0 && num14 > 0)
                        {
                            break;
                        }
                        num36++;
                        num36 %= __instance.localPairCount;
                        if (num37 == num36)
                        {
                            goto IL_E11;
                        }
                    }
                    if (__instance.energy >= num35)
                    {
                        int num38 = (droneCarries < num2) ? droneCarries : num2;
                        int num39 = num2;
                        int num40 = num3;
                        int num41 = split_inc(ref num39, ref num40, num38);
                        __instance.workDroneDatas[__instance.workDroneCount].begin = new Vector3(x3, y3, z3);
                        __instance.workDroneDatas[__instance.workDroneCount].end = new Vector3(x4, y4, z4);
                        __instance.workDroneDatas[__instance.workDroneCount].endId = stationComponent2.id;
                        __instance.workDroneDatas[__instance.workDroneCount].direction = 1f;
                        __instance.workDroneDatas[__instance.workDroneCount].maxt = (float)num34;
                        __instance.workDroneDatas[__instance.workDroneCount].t = -1.5f;
                        __instance.workDroneDatas[__instance.workDroneCount].itemId = (__instance.workDroneOrders[__instance.workDroneCount].itemId = itemId);
                        __instance.workDroneDatas[__instance.workDroneCount].itemCount = num38;
                        __instance.workDroneDatas[__instance.workDroneCount].inc = num41;
                        __instance.workDroneDatas[__instance.workDroneCount].gene = ____tmp_iter_local;
                        __instance.workDroneOrders[__instance.workDroneCount].otherStationId = stationComponent2.id;
                        __instance.workDroneOrders[__instance.workDroneCount].thisIndex = supplyDemandPair2.supplyIndex;
                        __instance.workDroneOrders[__instance.workDroneCount].otherIndex = supplyDemandPair2.demandIndex;
                        __instance.workDroneOrders[__instance.workDroneCount].thisOrdered = 0;
                        __instance.workDroneOrders[__instance.workDroneCount].otherOrdered = num38;
                        StationStore[] array = stationComponent2.storage;
                        lock (array)
                        {
                            StationStore[] array5 = stationComponent2.storage;
                            int demandIndex2 = supplyDemandPair2.demandIndex;
                            array5[demandIndex2].localOrder += num38;
                        }
                        __instance.workDroneCount++;
                        __instance.idleDroneCount--;
                        array = __instance.storage;
                        lock (array)
                        {
                            StationStore[] array6 = __instance.storage;
                            int supplyIndex3 = supplyDemandPair2.supplyIndex;
                            array6[supplyIndex3].count -= num38;
                            StationStore[] array7 = __instance.storage;
                            int supplyIndex4 = supplyDemandPair2.supplyIndex;
                            array7[supplyIndex4].inc -= num41;
                        }
                        __instance.energy -= num35;
                        flag2 = true;
                    }
                IL_E11:
                    if (!flag2 && __instance.energy >= num35)
                    {
                        StationStore[] array = __instance.storage;
                        lock (array)
                        {
                            itemId2 = __instance.storage[supplyDemandPair.demandIndex].itemId;
                        }
                        __instance.workDroneDatas[__instance.workDroneCount].begin = new Vector3(x3, y3, z3);
                        __instance.workDroneDatas[__instance.workDroneCount].end = new Vector3(x4, y4, z4);
                        __instance.workDroneDatas[__instance.workDroneCount].endId = stationComponent2.id;
                        __instance.workDroneDatas[__instance.workDroneCount].direction = 1f;
                        __instance.workDroneDatas[__instance.workDroneCount].maxt = (float)num34;
                        __instance.workDroneDatas[__instance.workDroneCount].t = -1.5f;
                        __instance.workDroneDatas[__instance.workDroneCount].itemId = (__instance.workDroneOrders[__instance.workDroneCount].itemId = itemId2);
                        __instance.workDroneDatas[__instance.workDroneCount].itemCount = 0;
                        __instance.workDroneDatas[__instance.workDroneCount].gene = ____tmp_iter_local;
                        __instance.workDroneOrders[__instance.workDroneCount].otherStationId = stationComponent2.id;
                        __instance.workDroneOrders[__instance.workDroneCount].thisIndex = supplyDemandPair.demandIndex;
                        __instance.workDroneOrders[__instance.workDroneCount].otherIndex = supplyDemandPair.supplyIndex;
                        __instance.workDroneOrders[__instance.workDroneCount].thisOrdered = droneCarries;
                        __instance.workDroneOrders[__instance.workDroneCount].otherOrdered = -droneCarries;
                        array = __instance.storage;
                        lock (array)
                        {
                            StationStore[] array8 = __instance.storage;
                            int demandIndex3 = supplyDemandPair.demandIndex;
                            array8[demandIndex3].localOrder += droneCarries;
                        }
                        array = stationComponent2.storage;
                        lock (array)
                        {
                            StationStore[] array9 = stationComponent2.storage;
                            int supplyIndex5 = supplyDemandPair.supplyIndex;
                            array9[supplyIndex5].localOrder -= droneCarries;
                        }
                        __instance.workDroneCount++;
                        __instance.idleDroneCount--;
                        __instance.energy -= num35;
                    }
                IL_10D7:
                    __instance.localPairProcess++;
                    __instance.localPairProcess %= __instance.localPairCount;
                }
            }
            float num42 = dt * droneSpeed;
            for (int i = 0; i < __instance.workDroneCount; i++)
            {
                if (__instance.workDroneDatas[i].t > 0f && __instance.workDroneDatas[i].t < __instance.workDroneDatas[i].maxt)
                {
                    DroneData[] array10 = __instance.workDroneDatas;
                    int num43 = i;
                    array10[num43].t += num42 * __instance.workDroneDatas[i].direction;
                    if (__instance.workDroneDatas[i].t <= 0f)
                    {
                        __instance.workDroneDatas[i].t = -0.0001f;
                    }
                    else if (__instance.workDroneDatas[i].t >= __instance.workDroneDatas[i].maxt)
                    {
                        __instance.workDroneDatas[i].t = __instance.workDroneDatas[i].maxt + 0.0001f;
                    }
                }
                else
                {
                    DroneData[] array11 = __instance.workDroneDatas;
                    int num44 = i;
                    array11[num44].t += dt * __instance.workDroneDatas[i].direction;
                    if (__instance.workDroneDatas[i].t >= __instance.workDroneDatas[i].maxt + 1.5f)
                    {
                        __instance.workDroneDatas[i].direction = -1f;
                        __instance.workDroneDatas[i].t = __instance.workDroneDatas[i].maxt + 1.5f;
                        StationComponent stationComponent3 = stationPool[__instance.workDroneDatas[i].endId];
                        StationStore[] array12 = stationComponent3.storage;
                        if (__instance.workDroneDatas[i].itemCount > 0)
                        {
                            stationComponent3.AddItem(__instance.workDroneDatas[i].itemId, __instance.workDroneDatas[i].itemCount, __instance.workDroneDatas[i].inc);
                            __instance.workDroneDatas[i].itemCount = 0;
                            __instance.workDroneDatas[i].inc = 0;
                            factory.NotifyDroneDelivery(factory, __instance, stationComponent3, __instance.workDroneDatas[i].itemId, __instance.workDroneDatas[i].itemCount);
                            if (__instance.workDroneOrders[i].otherStationId > 0)
                            {
                                StationStore[] array = array12;
                                lock (array)
                                {
                                    if (array12[__instance.workDroneOrders[i].otherIndex].itemId == __instance.workDroneOrders[i].itemId)
                                    {
                                        StationStore[] array13 = array12;
                                        int otherIndex = __instance.workDroneOrders[i].otherIndex;
                                        array13[otherIndex].localOrder -= __instance.workDroneOrders[i].otherOrdered;
                                    }
                                }
                                __instance.workDroneOrders[i].ClearOther();
                            }
                            if (__instance.localPairCount > 0)
                            {
                                __instance.localPairProcess %= __instance.localPairCount;
                                int num45 = __instance.localPairProcess;
                                int num46 = __instance.localPairProcess;
                                do
                                {
                                    SupplyDemandPair supplyDemandPair3 = __instance.localPairs[num46];
                                    if (supplyDemandPair3.demandId == __instance.id && supplyDemandPair3.supplyId == stationComponent3.id)
                                    {
                                        StationStore[] array = __instance.storage;
                                        lock (array)
                                        {
                                            num6 = __instance.storage[supplyDemandPair3.demandIndex].localDemandCount;
                                            num7 = __instance.storage[supplyDemandPair3.demandIndex].totalDemandCount;
                                            itemId2 = __instance.storage[supplyDemandPair3.demandIndex].itemId;
                                        }
                                    }
                                    if (supplyDemandPair3.demandId == __instance.id && supplyDemandPair3.supplyId == stationComponent3.id)
                                    {
                                        StationStore[] array = array12;
                                        lock (array)
                                        {
                                            num9 = array12[supplyDemandPair3.supplyIndex].count;
                                            num10 = array12[supplyDemandPair3.supplyIndex].inc;
                                            num11 = array12[supplyDemandPair3.supplyIndex].localSupplyCount;
                                            num12 = array12[supplyDemandPair3.supplyIndex].totalSupplyCount;
                                        }
                                    }
                                    if (supplyDemandPair3.demandId == __instance.id && supplyDemandPair3.supplyId == stationComponent3.id && num6 > 0 && num7 > 0 && num9 > 0 && num11 > 0 && num12 > 0)
                                    {
                                        int num47 = (droneCarries < num9) ? droneCarries : num9;
                                        int num48 = num9;
                                        int num49 = num10;
                                        int num50 = split_inc(ref num48, ref num49, num47);
                                        __instance.workDroneDatas[i].itemId = (__instance.workDroneOrders[i].itemId = itemId2);
                                        __instance.workDroneDatas[i].itemCount = num47;
                                        __instance.workDroneDatas[i].inc = num50;
                                        StationStore[] array = array12;
                                        lock (array)
                                        {
                                            StationStore[] array14 = array12;
                                            int supplyIndex6 = supplyDemandPair3.supplyIndex;
                                            array14[supplyIndex6].count -= num47;
                                            StationStore[] array15 = array12;
                                            int supplyIndex7 = supplyDemandPair3.supplyIndex;
                                            array15[supplyIndex7].inc -= num50;
                                        }
                                        __instance.workDroneOrders[i].otherStationId = stationComponent3.id;
                                        __instance.workDroneOrders[i].thisIndex = supplyDemandPair3.demandIndex;
                                        __instance.workDroneOrders[i].otherIndex = supplyDemandPair3.supplyIndex;
                                        __instance.workDroneOrders[i].thisOrdered = num47;
                                        __instance.workDroneOrders[i].otherOrdered = 0;
                                        array = __instance.storage;
                                        lock (array)
                                        {
                                            StationStore[] array16 = __instance.storage;
                                            int demandIndex4 = supplyDemandPair3.demandIndex;
                                            array16[demandIndex4].localOrder += num47;
                                            break;
                                        }
                                    }
                                    num46++;
                                    num46 %= __instance.localPairCount;
                                }
                                while (num45 != num46);
                            }
                        }
                        else
                        {
                            int itemId3 = __instance.workDroneDatas[i].itemId;
                            int num51 = droneCarries;
                            int inc;
                            stationComponent3.TakeItem(ref itemId3, ref num51, out inc);
                            __instance.workDroneDatas[i].itemCount = num51;
                            __instance.workDroneDatas[i].inc = inc;
                            StationStore[] array;
                            if (__instance.workDroneOrders[i].otherStationId > 0)
                            {
                                array = array12;
                                lock (array)
                                {
                                    if (array12[__instance.workDroneOrders[i].otherIndex].itemId == __instance.workDroneOrders[i].itemId)
                                    {
                                        StationStore[] array17 = array12;
                                        int otherIndex2 = __instance.workDroneOrders[i].otherIndex;
                                        array17[otherIndex2].localOrder -= __instance.workDroneOrders[i].otherOrdered;
                                    }
                                }
                                __instance.workDroneOrders[i].ClearOther();
                            }
                            array = __instance.storage;
                            lock (array)
                            {
                                if (__instance.storage[__instance.workDroneOrders[i].thisIndex].itemId == __instance.workDroneOrders[i].itemId && __instance.workDroneOrders[i].thisOrdered != num51)
                                {
                                    int num52 = num51 - __instance.workDroneOrders[i].thisOrdered;
                                    StationStore[] array18 = __instance.storage;
                                    int thisIndex = __instance.workDroneOrders[i].thisIndex;
                                    array18[thisIndex].localOrder += num52;
                                    LocalLogisticOrder[] array19 = __instance.workDroneOrders;
                                    int num53 = i;
                                    array19[num53].thisOrdered += num52;
                                }
                            }
                        }
                    }
                    if (__instance.workDroneDatas[i].t < -1.5f)
                    {
                        __instance.AddItem(__instance.workDroneDatas[i].itemId, __instance.workDroneDatas[i].itemCount, __instance.workDroneDatas[i].inc);
                        StationComponent srcStation = stationPool[__instance.workDroneDatas[i].endId];
                        factory.NotifyDroneDelivery(factory, srcStation, __instance, __instance.workDroneDatas[i].itemId, __instance.workDroneDatas[i].itemCount);
                        if (__instance.workDroneOrders[i].itemId > 0)
                        {
                            StationStore[] array = __instance.storage;
                            lock (array)
                            {
                                if (__instance.storage[__instance.workDroneOrders[i].thisIndex].itemId == __instance.workDroneOrders[i].itemId)
                                {
                                    StationStore[] array20 = __instance.storage;
                                    int thisIndex2 = __instance.workDroneOrders[i].thisIndex;
                                    array20[thisIndex2].localOrder -= __instance.workDroneOrders[i].thisOrdered;
                                }
                            }
                            __instance.workDroneOrders[i].ClearThis();
                        }
                        Array.Copy(__instance.workDroneDatas, i + 1, __instance.workDroneDatas, i, __instance.workDroneDatas.Length - i - 1);
                        Array.Copy(__instance.workDroneOrders, i + 1, __instance.workDroneOrders, i, __instance.workDroneOrders.Length - i - 1);
                        __instance.workDroneCount--;
                        __instance.idleDroneCount++;
                        Array.Clear(__instance.workDroneDatas, __instance.workDroneCount, __instance.workDroneDatas.Length - __instance.workDroneCount);
                        Array.Clear(__instance.workDroneOrders, __instance.workDroneCount, __instance.workDroneOrders.Length - __instance.workDroneCount);
                        i--;
                    }
                }
            }
            return false;
        }
    }
}

