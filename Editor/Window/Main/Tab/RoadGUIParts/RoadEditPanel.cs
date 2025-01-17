﻿using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.EditingSystem;
using static PLATEAU.Editor.RoadNetwork.EditingSystem.RoadNetworkEditingSystem;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    internal class RoadEditPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_EditPanel";
        private RoadNetworkEditingSystem EditingSystem { get; set; }

        private SerializedScriptableRoadMdl lastSelectedRoad;
        private SerializedScriptableRoadMdl selectedRoad;
        private Toggle detailEditModeToggle;

        // UI要素
        private readonly Button roadSplineStartButton;
        private readonly Button roadSplineStopButton;
        private readonly Toggle editModeToggle;
        private readonly Toggle placeCrosswalkToggle;

        private bool prevPlaceCrosswalkToggle = false;
        public RoadEditPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
            
            // ボタン類の設定
            editModeToggle = self.Q<Toggle>("EditModeButton");
            if (editModeToggle == null)
            {
                Debug.LogError("EditModeToggle is not found.");
                return;
            }

            editModeToggle.value = false;
            editModeToggle.RegisterCallback<ChangeEvent<bool>>(OnEditModeToggleClicked);

            // バインドパスの設定
            self.Q<IntegerField>("LeftSide").bindingPath = "numLeftLane";
            self.Q<IntegerField>("RightSide").bindingPath = "numRightLane";
            self.Q<Toggle>("EnableMedianLane").bindingPath = "enableMedianLane";
            self.Q<Toggle>("EnableLeftSideWalk").bindingPath = "enableLeftSideWalk";
            self.Q<Toggle>("EnableRightSideWalk").bindingPath = "enableRightSideWalk";
            placeCrosswalkToggle = GetT("PlaceCrosswalk");
            
            detailEditModeToggle = self.Q<Toggle>("DetailEditMode");
            detailEditModeToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                OnChangeDetailEditMode(evt.newValue);
            });

            // 道路の適用ボタン
            var applyRoadButton = rootVisualElement.Q<Button>("ApplyRoadButton");
            if (applyRoadButton == null)
            {
                Debug.LogError("ApplyRoadButton is not found.");
                return;
            }
            applyRoadButton.clicked += OnApplyRoadButtonClicked;

            roadSplineStartButton = rootVisualElement.Q<Button>("RoadSplineStartButton");
            roadSplineStopButton = rootVisualElement.Q<Button>("RoadSplineStopButton");

            // 終了ボタンがuxml上で非可視状態になっているため、ここで有効化する
            {
                var displayStyle = roadSplineStopButton.style.display;
                displayStyle.value = DisplayStyle.Flex;
                roadSplineStopButton.style.display = displayStyle;
                roadSplineStopButton.SetEnabled(false);
            }

            roadSplineStartButton.clicked += OnRoadSplineStartButtonClicked;
            roadSplineStopButton.clicked += OnRoadSplineStopButtonClicked;

        }

        protected override void OnTabSelected(VisualElement root)
        {
            base.OnTabSelected(root);
        }

        /// <summary>
        /// 「編集モード」ボタンが押された時
        /// </summary>
        private void OnEditModeToggleClicked(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                OnEditModeActivated();
            }
            else
            {
                OnEditModeDeactivated();
            }
        }

        /// <summary> 「編集モード」ボタンが押されて編集モードになったとき </summary>
        private void OnEditModeActivated()
        {


            // 編集システムの初期化
            if (EditingSystem != null) EditingSystem.Terminate();
            EditingSystem = new RoadNetworkEditingSystem();
            
            EditingSystem.roadEditSceneViewGui?.Init(detailEditModeToggle.value);
            
            EditingSystem.roadNetworkEditTarget.OnChangedSelectRoadNetworkElement += OnChangedSelectedRoadBase;
            EditingSystem.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl = true;
        }

        /// <summary> 「編集モード」ボタンが押されて編集モードが解除されたとき </summary>
        private void OnEditModeDeactivated()
        {
            TerminateSystem_();
        }

        protected override void OnTabUnselected()
        {
            TerminateSystem_();
            base.OnTabUnselected();
        }

        void TerminateSystem_()
        {
            
            rootVisualElement.Unbind();

            if (EditingSystem != null)
            {
                EditingSystem.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl = false;
                EditingSystem.roadNetworkEditTarget.OnChangedSelectRoadNetworkElement -= OnChangedSelectedRoadBase;
            }
            
            
            
            RoadNetworkEditingSystem.TryTerminate(EditingSystem, rootVisualElement);
            if(editModeToggle != null) editModeToggle.value = false;
            SceneView.RepaintAll();
        }

        /// <summary>
        /// 道路または交差点が選択されたとき
        /// </summary>
        private void OnChangedSelectedRoadBase()
        {
            EditingSystem.intersectionEditSceneViewGui.Terminate();
            
            var roadGroupEditorData = EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (roadGroupEditorData != null)
            {
                lastSelectedRoad = selectedRoad;

                // 無ければ生成する あれば流用する
                selectedRoad = this.CreateOrGetRoadGroupData(roadGroupEditorData);


                // 既存のモデルオブジェクトを解除
                rootVisualElement.Unbind();

                UpdateSplineButtonVisual(false);
                lastSelectedRoad?.DisableSplineEditMode(EditingSystem?.roadEditSceneViewGui);

                // モデルのバインド
                rootVisualElement.Bind(selectedRoad);
                
                // 現在の交差点の有無をチェックボックスに反映させます
                var roads = new RoadReproduceSource(roadGroupEditorData.Ref.Roads.FirstOrDefault());
                bool doCrosswalkExist = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, roads, ReproducedRoadDirection.Next);
                placeCrosswalkToggle.value = doCrosswalkExist;
                prevPlaceCrosswalkToggle = doCrosswalkExist;
            }

            var intersectionData = EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            if (intersectionData != null)
            {
                EditingSystem.intersectionEditSceneViewGui?.SetupIntersection(intersectionData.Ref);

                //var applyIntersectionButton = element.Q<Button>("ApplyIntersectionButton");
                //if (applyIntersectionButton == null)
                //{
                //    Debug.LogError("ApplyIntersectionButton is not found.");
                //    return;
                //}
                //applyIntersectionButton.clicked += () =>
                //{
                //    Debug.Log("clicked apply12");

                //};

            }
        }

        /// <summary>
        /// 道路編集で、EditorWindowのボタン「編集内容を確定」が押された時の処理です。
        /// </summary>
        private void OnApplyRoadButtonClicked()
        {
            bool isChanged = selectedRoad.Apply(EditingSystem.roadEditSceneViewGui);
            isChanged |= prevPlaceCrosswalkToggle != placeCrosswalkToggle.value;
            if (!isChanged)
            {
                return;
            }

            var changedRoads = selectedRoad.TargetScriptableRoadMdl.road.Roads;

            var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnMdl == null)
            {
                Debug.LogError("RoadNetworkStructureModel is not found.");
                return;
            }
            var network = rnMdl.RoadNetwork;

            ReproduceRoad(network, changedRoads);
            
        }

        /// <summary> 「詳細編集モード」のチェックボックスが変わった時 </summary>
        private void OnChangeDetailEditMode(bool newValue)
        {
            EditingSystem?.ChangeDetailEditMode(newValue);
        }
        
        /// <summary>
        /// 道路ネットワークを元に道路メッシュや白線生成を行います
        /// </summary>
        private void ReproduceRoad(RnModel network, IReadOnlyList<RnRoad> changedRoads)
        {
            if (network == null) return;
            new RoadReproducer().Generate(new RrTargetRoadBases(network, changedRoads), CrosswalkFreq());
            prevPlaceCrosswalkToggle = placeCrosswalkToggle.value;
        }
        
        private void OnRoadSplineStartButtonClicked()
        {
            UpdateSplineButtonVisual(true);
            selectedRoad.IsSplineEditMode = true;
            selectedRoad.ApplySplineEditMode(EditingSystem?.roadEditSceneViewGui);
        }

        private void OnRoadSplineStopButtonClicked()
        {
            UpdateSplineButtonVisual(false);
            roadSplineStartButton.SetEnabled(true);
            roadSplineStopButton.SetEnabled(false);
            selectedRoad.IsSplineEditMode = false;
            selectedRoad.ApplySplineEditMode(EditingSystem?.roadEditSceneViewGui);
            
            // 道路に適用します
            var network = EditingSystem?.roadNetworkEditTarget.RoadNetwork;
            ReproduceRoad(network, selectedRoad.TargetScriptableRoadMdl.road.Roads);
        }

        private void UpdateSplineButtonVisual(bool isSplineEditMode)
        {
            roadSplineStartButton.SetEnabled(!isSplineEditMode);
            roadSplineStopButton.SetEnabled(isSplineEditMode);
        }

        private SerializedScriptableRoadMdl CreateOrGetRoadGroupData(EditorData<RnRoadGroup> linkGroupEditorData)
        {
            // モデルオブジェクトを所持してるならそれを利用する
            var mdl = linkGroupEditorData.ReqSubData<ScriptableObjectFolder>();
            return mdl.Item;
        }
        

        private CrosswalkFrequency CrosswalkFreq()
        {
            return placeCrosswalkToggle.value ? CrosswalkFrequency.All : CrosswalkFrequency.Delete;
        }
        
    }
}
