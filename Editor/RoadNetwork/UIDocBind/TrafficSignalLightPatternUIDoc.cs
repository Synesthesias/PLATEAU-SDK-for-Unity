using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;
using UnityEngine.UIElements;
using PLATEAU.RoadNetwork;
using System.Linq;
using System;
using UnityEditor.UIElements;
using static PLATEAU.Editor.RoadNetwork.TrafficSignalLightPatternUIDoc;

namespace PLATEAU.Editor.RoadNetwork
{
    public class TrafficSignalLightPatternUIDoc
    {
        public class TrafficSignalLightPatternUIEx
        {
            public TrafficSignalLightPatternUIEx(TrafficSignalControlPattern pattern)
            {
                this.pattern = pattern;
            }

            private TrafficSignalControlPattern pattern;

            public float CycleTime
            {
                get
                {
                    var sum = 0f;
                    pattern.Phases.ForEach(phase =>
                    {
                        sum += phase.SplitSeconds;
                    });
                    return sum;
                }
                set
                {
                 
                }
            }
        }

        public TrafficSignalLightPatternUIDoc(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            this.system = system;
            this.assets = assets;
            this.patternPanelRoot = root;
            Init();
        }

        private IRoadNetworkEditingSystem system;
        private RoadNetworkEditorAssets assets;
        private VisualElement patternPanelRoot;
        private VisualElement phasePanelRoot;

        private TrafficSignalLightPatternUIEx trafficSignalLightPatternUIEx;

        void Init()
        {
            patternPanelRoot.Clear();
            var controller = system.SelectedRoadNetworkElement as TrafficSignalLightController;

            // UI表示用に拡張したクラスを作成
            trafficSignalLightPatternUIEx = new TrafficSignalLightPatternUIEx(system.SelectedTrafficPattern);

            var asset = assets.GetAsset(RoadNetworkEditorAssets.RoadNetworkPatternPanel);
            var patternEditPanelInst = asset.Instantiate();

            var addPatternBtn = patternEditPanelInst.Q<Button>("Add");
            addPatternBtn.clicked += () =>
            {
                if (controller != null)
                {
                    var phase = new TrafficSignalPhase(Guid.NewGuid().ToString());
                    system.SelectedTrafficPattern.Phases.Add(phase);
                    SyncTrafficLightPatternPhaseList(assets, patternEditPanelInst, controller);
                }
            };

            var removePatternBtn = patternEditPanelInst.Q<Button>("Remove");
            removePatternBtn.clicked += () =>
            {
                if (controller != null)
                {
                    system.SelectedTrafficPattern.Phases.Remove(system.SelectedTrafficPattern.Phases.Last());
                    SyncTrafficLightPatternPhaseList(assets, patternEditPanelInst, controller);
                }
            };
            patternPanelRoot.Add(patternEditPanelInst);

            SyncTrafficLightPatternPhaseList(assets, patternEditPanelInst, controller);

            var cotrollerInfo = patternEditPanelInst.Q<VisualElement>("CotrollerInfo");
            var cycleTimeTextField = cotrollerInfo.Q<FloatField>("CycleTime");
            UIDocBind.Helper.Bind(cycleTimeTextField, nameof(trafficSignalLightPatternUIEx.CycleTime), trafficSignalLightPatternUIEx);
            cycleTimeTextField.SetValueWithoutNotify(trafficSignalLightPatternUIEx.CycleTime);
        }



        /// <summary>
        /// 信号制御パターンリストを同期します。
        /// </summary>
        /// <param name="assets">RoadNetworkEditorAssets</param>
        /// <param name="panelInst">パネルのインスタンス</param>
        /// <param name="trafficLightController">交通信号灯制御器</param>
        private void SyncTrafficLightPatternPhaseList(RoadNetworkEditorAssets assets, TemplateContainer panelInst, TrafficSignalLightController trafficLightController)
        {
            // 信号制御パターンリストの取得
            var radioBtnGroup = panelInst.Q<RadioButtonGroup>("PhaseSelecter");
            if (trafficLightController != null)
            {
                var radioBtnAsset = assets.GetAsset(RoadNetworkEditorAssets.RadioButton);
                // 信号制御パターンリストの同期
                SyncTrafficLightPatternPhaseList(system.SelectedTrafficPattern.Phases, radioBtnGroup, radioBtnAsset);
            }
            else
            {
                radioBtnGroup.Clear();
            }
        }

        /// <summary>
        /// 信号制御パターンリストを同期します。
        /// </summary>
        /// <param name="phases">信号制御パターンリスト</param>
        /// <param name="radioBtnGroup">ラジオボタングループ</param>
        /// <param name="radioBtnAsset">ラジオボタンのアセット</param>
        private void SyncTrafficLightPatternPhaseList(List<TrafficSignalPhase> phases, RadioButtonGroup radioBtnGroup, VisualTreeAsset radioBtnAsset)
        {
            // ラジオボタングループ内のラジオボタンをループし、パターンにリンクされていないものを削除する
            var children = radioBtnGroup.Children().ToArray();
            foreach (var radioButton in children)
            {
                if (!phases.Contains(radioButton.userData))
                {
                    radioBtnGroup.Remove(radioButton);
                }
            }

            // パターンをループする
            foreach (var item in phases)
            {
                bool isContains = false;

                // ラジオボタングループ内のラジオボタンをループする
                foreach (var radioButton in radioBtnGroup.Children())
                {
                    // ラジオボタンが現在のパターンに既にリンクされているかをチェックする
                    if (radioButton.userData == item)
                    {
                        isContains = true;
                        break;
                    }
                }

                // パターンがどのラジオボタンにもリンクされていない場合、新しいラジオボタンを作成し、パターンにリンクする
                if (!isContains)
                {
                    var inst = radioBtnAsset.Instantiate();
                    var radioBtn = inst.Q<RadioButton>("RadioButton");
                    var index = phases.FindIndex(phases => phases == item);
                    radioBtn.label = item.Name;
                        //String.Format("P{0}", index);

                    radioBtn.RegisterValueChangedCallback((e) =>
                    {
                        if (e.newValue == false)
                            return;

                        var userData = UIDocBind.GetUserData(e) as TrafficSignalPhase;
                        var v = e.target as VisualElement;
                        Debug.Assert(userData != null);
                        system.SelectedTrafficPhase = userData;
                        Debug.Log(userData.Name);

                        var phasePanelRoot = patternPanelRoot.Q<VisualElement>("PhasePanelRoot");
                        if (phasePanelRoot != null)
                        {
                            phasePanelRoot.Clear();

                            var phasePanel = assets.GetAsset(RoadNetworkEditorAssets.RoadNetworkTrafficLightPatternPhasePanel);
                            var pahseInst = phasePanel.Instantiate();
                            phasePanelRoot.Add(pahseInst);
                            phasePanelRoot.MarkDirtyRepaint();

                            var phase = system.SelectedTrafficPhase;
 
                            var spllitField = phasePanelRoot.Q<FloatField>("Split");
                            UIDocBind.Helper.Bind(spllitField, nameof(phase.SplitSeconds), phase);
                            spllitField.SetValueWithoutNotify(phase.SplitSeconds);
                            spllitField.RegisterValueChangedCallback((e) =>
                            {

                                var cotrollerInfo = patternPanelRoot.Q<VisualElement>("CotrollerInfo");
                                var cycleTimeTextField = cotrollerInfo.Q<FloatField>("CycleTime");
                                cycleTimeTextField.SetValueWithoutNotify(trafficSignalLightPatternUIEx.CycleTime);
                            });

                            var maskField = phasePanelRoot.Q<MaskField>("EnterableCarTypeMask");
                            UIDocBind.Helper.Bind(maskField, nameof(phase.EnterableVehicleType), phase);
                            maskField.SetValueWithoutNotify(phase.EnterableVehicleType);

                        }
                        else
                        {
                            Debug.LogError("<VisualElement>PhasePanelRoot　が見つからない");
                        }
                    });
                    radioBtn.userData = item;
                    radioBtnGroup.Add(inst);
                }
            }
        }

    }
}
