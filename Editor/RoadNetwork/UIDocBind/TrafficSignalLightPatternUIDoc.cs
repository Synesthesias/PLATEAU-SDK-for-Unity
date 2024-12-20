using PLATEAU.Editor.RoadNetwork.EditingSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using System;
using UnityEditor.UIElements;

namespace PLATEAU.Editor.RoadNetwork.UIDocBind
{
    /// <summary>
    /// 信号制御器のパターンを編集するUIDocumentのバインドや挙動の定義を行うクラス
    /// </summary>
    internal class TrafficSignalLightPatternUIDoc
    {
        /// <summary>
        /// TrafficSignalLightPatternUIをUIDocument向けに拡張したクラス
        /// </summary>
        public class TrafficSignalLightPatternUIEx
        {
            public TrafficSignalLightPatternUIEx(TrafficSignalControllerPattern pattern)
            {
                this.pattern = pattern;
            }

            private TrafficSignalControllerPattern pattern;

            public float CycleTime
            {
                get
                {
                    var sum = 0f;
                    pattern.Phases.ForEach(phase =>
                    {
                        sum += phase.Split;
                    });
                    return sum;
                }
                set
                {
                 
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="system"></param>
        /// <param name="assets"></param>
        /// <param name="root"></param>
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
            trafficSignalLightPatternUIEx = new TrafficSignalLightPatternUIEx(system.SelectedSignalControllerPattern);

            var asset = assets.GetAsset(RoadNetworkEditorAssets.RoadNetworkPatternPanel);
            var patternEditPanelInst = asset.Instantiate();

            var addPatternBtn = patternEditPanelInst.Q<Button>("Add");
            addPatternBtn.clicked += () =>
            {
                if (controller != null)
                {
                    var phase = new TrafficSignalControllerPhase(Guid.NewGuid().ToString());
                    system.SelectedSignalControllerPattern.Phases.Add(phase);
                    SyncTrafficLightPatternPhaseList(assets, patternEditPanelInst, controller);
                }
            };

            var removePatternBtn = patternEditPanelInst.Q<Button>("Remove");
            removePatternBtn.clicked += () =>
            {
                if (controller != null)
                {
                    system.SelectedSignalControllerPattern.Phases.Remove(system.SelectedSignalControllerPattern.Phases.Last());
                    SyncTrafficLightPatternPhaseList(assets, patternEditPanelInst, controller);
                }
            };
            patternPanelRoot.Add(patternEditPanelInst);

            SyncTrafficLightPatternPhaseList(assets, patternEditPanelInst, controller);

            var cotrollerInfo = patternEditPanelInst.Q<VisualElement>("CotrollerInfo");
            var cycleTimeTextField = cotrollerInfo.Q<FloatField>("CycleTime");
            UIDocBindHelper.Helper.Bind(cycleTimeTextField, nameof(trafficSignalLightPatternUIEx.CycleTime), trafficSignalLightPatternUIEx);
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
                SyncTrafficLightPatternPhaseList(system.SelectedSignalControllerPattern.Phases, radioBtnGroup, radioBtnAsset);
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
        private void SyncTrafficLightPatternPhaseList(List<TrafficSignalControllerPhase> phases, RadioButtonGroup radioBtnGroup, VisualTreeAsset radioBtnAsset)
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

                        var userData = UIDocBindHelper.GetUserData(e) as TrafficSignalControllerPhase;
                        var v = e.target as VisualElement;
                        Debug.Assert(userData != null);
                        system.SelectedSignalPhase = userData;
                        Debug.Log(userData.Name);

                        var phasePanelRoot = patternPanelRoot.Q<VisualElement>("PhasePanelRoot");
                        if (phasePanelRoot != null)
                        {
                            phasePanelRoot.Clear();

                            var phasePanel = assets.GetAsset(RoadNetworkEditorAssets.RoadNetworkTrafficLightPatternPhasePanel);
                            var pahseInst = phasePanel.Instantiate();
                            phasePanelRoot.Add(pahseInst);
                            phasePanelRoot.MarkDirtyRepaint();

                            var phase = system.SelectedSignalPhase;
 
                            var spllitField = phasePanelRoot.Q<FloatField>("Split");
                            UIDocBindHelper.Helper.Bind(spllitField, nameof(phase.Split), phase);
                            spllitField.SetValueWithoutNotify(phase.Split);
                            spllitField.RegisterValueChangedCallback((e) =>
                            {

                                var cotrollerInfo = patternPanelRoot.Q<VisualElement>("CotrollerInfo");
                                var cycleTimeTextField = cotrollerInfo.Q<FloatField>("CycleTime");
                                cycleTimeTextField.SetValueWithoutNotify(trafficSignalLightPatternUIEx.CycleTime);
                            });

                            var maskField = phasePanelRoot.Q<MaskField>("EnterableCarTypeMask");
                            UIDocBindHelper.Helper.Bind(maskField, nameof(phase.EnterableVehicleTypeMask), phase);
                            maskField.SetValueWithoutNotify(phase.EnterableVehicleTypeMask);

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
