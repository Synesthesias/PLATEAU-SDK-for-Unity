using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.RoadNetwork.Structure
{
    public class PLATEAURnStructureModel
        : MonoBehaviour
            , IRoadNetworkObject
            , ISerializationCallbackReceiver
    {
        // 勝手にシリアライズ/デシリアライズされると開発中だと困ることもあるのでフラグに出しておく
        [SerializeField]
        private bool autoSaving = true;

        // シリアライズ用フィールド
        [SerializeField]
        private RoadNetworkStorage storage;

        /// <summary>
        /// 道路構造
        /// </summary>
        public RnModel RoadNetwork { get; set; }

        /// <summary>
        /// RnModelをRoadNetworkStorageへのシリアライズ
        /// </summary>
        public void Serialize()
        {
            // 一応ランタイムでも呼べるようにUNITY_EDITORで切っていない
            if (RoadNetwork == null)
                return;
            var sw = new Stopwatch();
            sw.Start();
            storage = RoadNetwork.Serialize();
            sw.Stop();
            DebugEx.Log($"S{GetType().Name} Serialize({sw.ElapsedMilliseconds}[ms])");
        }

        /// <summary>
        /// RoadNetworkStorageからRnModelへのデシリアライズ
        /// </summary>
        public void Deserialize()
        {
            // 一応ランタイムでも呼べるようにUNITY_EDITORで切っていない
            var sw = new Stopwatch();
            sw.Start();
            RoadNetwork ??= new RnModel();
            RoadNetwork.Deserialize(storage);
            sw.Stop();
            DebugEx.Log($"S{GetType().Name} Deserialize({sw.ElapsedMilliseconds}[ms])");

            // 信号制御器のテスト処理　マージ前に削除予定
            TestSetter();
            [Obsolete("テスト用　マージ前に削除予定")]
            void TestSetter()
            {
                //// 設定先のストレージ
                //var getter = GetRoadNetworkDataGetter();
                //var storage = getter.;

                // Setterの取得
                var setter = GetRoadNetworkDataSetter();

                // ID生成に必要な機能を作成
                var roadIdGenrator = setter.CreateIdGenerator<Data.RnDataRoadBase>();
                var controllerIdGenrator = setter.CreateIdGenerator<Data.RnDataTrafficLightController>();
                var lightIdGenrator = setter.CreateIdGenerator<Data.RnDataTrafficLight>();
                var patternIdGenrator = setter.CreateIdGenerator<Data.RnDataTrafficSignalPattern>();
                var phaseIdGenrator = setter.CreateIdGenerator<Data.RnDataTrafficSignalPhase>();

                // テストデータの作成
                var controllers = new List<Data.RnDataTrafficLightController>();
                var lights = new List<Data.RnDataTrafficLight>();
                var patterns = new List<Data.RnDataTrafficSignalPattern>();
                var phases = new List<Data.RnDataTrafficSignalPhase>();

                // 親からデータを作成　子への参照はデータ生成直後に行う

                // 信号制御器のデータ作成
                var intersectionId = new RnID<RnDataRoadBase>(9, roadIdGenrator); // 適当な交差点ID
                var controllerA = new Data.RnDataTrafficLightController()
                {
                    Parent = intersectionId,  // 道路のID
                    OffsetTrafficLight = RnID<RnDataTrafficLight>.Undefind
                };
                controllers.Add(controllerA);
                var controllerAId = new RnID<RnDataTrafficLightController>(0, controllerIdGenrator);

                // 適当な道路ID
                var nRoad = 3;
                var roadIds = new List<RnID<RnDataRoadBase>>(nRoad);
                for (int i = 0; i < nRoad; i++)
                {
                    roadIds.Add(new RnID<RnDataRoadBase>(i, roadIdGenrator));
                }

                // 信号機のデータ作成
                var nLight = nRoad;
                var roadIdEnum = roadIds.GetEnumerator();
                for (int i = 0; i < nLight; i++)
                {
                    roadIdEnum.MoveNext();

                    var light = new Data.RnDataTrafficLight()
                    {
                        Parent = controllerAId,
                        RoadId = roadIdEnum.Current,
                        LaneType = "Lane",
                        Distance = 1.0f,
                    };
                    lights.Add(light);
                }

                // 信号制御器に信号機を紐づけ
                controllerA.TrafficLights = new List<RnID<RnDataTrafficLight>>(nLight);
                for (int i = 0; i < nLight; i++)
                {
                    controllerA.TrafficLights.Add(new RnID<RnDataTrafficLight>(i, lightIdGenrator));
                }

                // 信号パターンの作成
                var nPattern = 2;
                for (int i = 0; i < nPattern; i++)
                {
                    var pattern = new Data.RnDataTrafficSignalPattern()
                    {
                        Parent = controllerAId,
                        OffsetSeconds = 10.0f,
                        OffsetType = OffsetRelationType.Absolute,
                        StartOffsets = new DateTime(2024, 1, 1)
                    };
                    patterns.Add(pattern);
                }

                // 信号制御器に信号パターンを紐づけ
                controllerA.SignalPatterns = new List<RnID<RnDataTrafficSignalPattern>>(nPattern);
                for (int i = 0; i < nPattern; i++)
                {
                    controllerA.SignalPatterns.Add(new RnID<RnDataTrafficSignalPattern>(i, patternIdGenrator));
                }

                // パターンごとにフェーズを作成する
                UnityEngine.Debug.Assert(controllerA.SignalPatterns.Count == patterns.Count);
                var patternIdCnt = 0;
                var patternIds = controllerA.SignalPatterns;
                foreach (var pattern in patterns)
                {
                    // 信号フェーズのデータ作成
                    var nPhasePerPattern = 3;   //　1パターンに対していくつフェーズがあるか
                    var phasesPerPattern = new List<Data.RnDataTrafficSignalPhase>(nPhasePerPattern);
                    for (int i = 0; i < nPhasePerPattern; i++)
                    {
                        var phase = new Data.RnDataTrafficSignalPhase()
                        {
                            Parent = patternIds[patternIdCnt],
                            Order = i,
                            Split = 5.0f,
                            EnterableVehicleTypeMask = (int)(VehicleType.Smarll),
                            BlueRoadPairs = roadIds,
                            YellowRoadPairs = new List<RnID<RnDataRoadBase>>(),
                            RedRoadPairs = new List<RnID<RnDataRoadBase>>(),
                        };
                        phasesPerPattern.Add(phase);
                    }
                    phases.AddRange(phasesPerPattern);

                    // 信号パターンに信号フェーズを紐づけ
                    pattern.Phases = new List<RnID<RnDataTrafficSignalPhase>>(nPhasePerPattern);
                    for (int i = 0; i < nPhasePerPattern; i++)
                    {
                        pattern.Phases.Add(
                            new RnID<RnDataTrafficSignalPhase>(i + patternIdCnt * nPhasePerPattern, phaseIdGenrator));
                    }

                    patternIdCnt++;
                }


                // テストデータの設定
                var testData = new RnTrafficLightDataSet(
                    controllers, lights, patterns, phases);

                var isSuc = setter.SetTrafficSignalLightController(in testData);
                UnityEngine.Debug.Assert(isSuc);


            }
        }

        /// <summary>
        /// 道路ネットワークのデータを設定するためのクラスを取得する
        /// </summary>
        /// <returns></returns>
        public RoadNetworkDataSetter GetRoadNetworkDataSetter()
        {
            return new RoadNetworkDataSetter(storage);
        }

        /// <summary>
        /// 道路ネットワークのデータを取得するためのクラスを取得する
        /// </summary>
        /// <returns></returns>
        public RoadNetworkDataGetter GetRoadNetworkDataGetter()
        {
            return new RoadNetworkDataGetter(storage);
        }

        public void OnBeforeSerialize()
        {
            // シリアライズ処理は異常な回数呼ばれるのでここで呼ぶのはまずいので何もしない
            // シーン保存時に行うようにしている.
            // https://discussions.unity.com/t/onbeforeserialize-constantly-called-in-editor-mode/115994
        }

        public void OnAfterDeserialize()
        {
            // Deserialize()は頻繁に呼ばれるわけではない & コンパイル後にでシリアライズされるように
            // OnAfterDeserializeでは呼ぶようにする
            if (!autoSaving)
                return;
            Deserialize();
        }

        [Conditional("UNITY_EDITOR")]
        public static void OnSceneSaving(Scene scene, string path)
        {
            // Scene内のPLATEAURnStructureModelをシリアライズチェックを行う
            foreach (var obj in scene.GetRootGameObjects())
            {
                foreach (var model in obj.GetComponentsInChildren<PLATEAURnStructureModel>())
                {
                    if (!model.autoSaving)
                        continue;
                    model.Serialize();
                }
            }
        }
    }
}