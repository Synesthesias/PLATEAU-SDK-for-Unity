using PLATEAU.RoadNetwork.Structure;
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

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    internal class RoadEditPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_EditPanel";
        public RoadNetworkEditingSystem EditingSystem { get; private set; }

        private SerializedScriptableRoadMdl lastSelectedRoad;
        private SerializedScriptableRoadMdl selectedRoad;

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
            editModeToggle.value = RoadNetworkEditingSystem.SingletonInstance?.system != null;
            editModeToggle.RegisterCallback<ChangeEvent<bool>>(OnEditModeToggleClicked);

            // バインドパスの設定
            self.Q<IntegerField>("LeftSide").bindingPath = "numLeftLane";
            self.Q<IntegerField>("RightSide").bindingPath = "numRightLane";
            self.Q<Toggle>("EnableMedianLane").bindingPath = "enableMedianLane";
            self.Q<Toggle>("EnableLeftSideWalk").bindingPath = "enableLeftSideWalk";
            self.Q<Toggle>("EnableRightSideWalk").bindingPath = "enableRightSideWalk";
            placeCrosswalkToggle = GetT("PlaceCrosswalk");
            
            var d = self.Q<Toggle>("DetailEditMode");
            if (d != null)
                d.bindingPath = "isEditingDetailMode";

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
            EditingSystem =
                RoadNetworkEditingSystem.TryInitalize(EditingSystem, rootVisualElement, new EditorInstance(rootVisualElement, this));
            
            // システムの取得
            var system = EditingSystem.system;
            system.CurrentEditMode = RoadNetworkEditMode.EditRoadStructure;
            
            system.EditSceneViewGui?.Init();
            
            system.OnChangedSelectRoadNetworkElement += OnChangedSelectedRoadBase;
            system.EnableLimitSceneViewDefaultControl = true;
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
            var sys = RoadNetworkEditingSystem.SingletonInstance?.system;
            if (sys != null)
            {
                sys.EnableLimitSceneViewDefaultControl = false;
                sys.OnChangedSelectRoadNetworkElement -= OnChangedSelectedRoadBase;
            }
            
            RoadNetworkEditingSystem.TryTerminate(EditingSystem, rootVisualElement);
            if(editModeToggle != null) editModeToggle.value = false;
        }

        /// <summary>
        /// 道路または交差点が選択されたとき
        /// </summary>
        private void OnChangedSelectedRoadBase()
        {
            var roadGroupEditorData = EditingSystem.system.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (roadGroupEditorData != null)
            {
                lastSelectedRoad = selectedRoad;

                // 無ければ生成する あれば流用する
                selectedRoad = this.CreateOrGetRoadGroupData(roadGroupEditorData);


                // 既存のモデルオブジェクトを解除
                rootVisualElement.Unbind();

                // 仮 詳細編集モード → 未完成のためGUI上ではオフにしています
                rootVisualElement.TrackSerializedObjectValue(selectedRoad, (se) =>
                {
                    var mod = EditingSystem.system.EditSceneViewGui;
                    var obj = se as IScriptableRoadMdl;
                    if (mod.CanSetDtailMode())
                    {
                        if (mod.IsDetailMode() != obj.IsEditingDetailMode)
                        {
                            mod.SetDetailMode(obj.IsEditingDetailMode);
                        }
                    }
                });

                UpdateSplineButtonVisual(false);
                lastSelectedRoad?.DisableSplineEditMode(RoadNetworkEditingSystem.SingletonInstance?.system
                    .EditSceneViewGui);

                // モデルのバインド
                rootVisualElement.Bind(selectedRoad);
                
                // 現在の交差点の有無をチェックボックスに反映させます
                var roads = roadGroupEditorData.Ref.Roads.FirstOrDefault()?.TargetTrans;
                var roadTrans = roads == null || roads.Count == 0 || roads[0] == null ? null : roads[0].transform;
                bool doCrosswalkExist = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, roadTrans, ReproducedRoadDirection.Next);
                placeCrosswalkToggle.value = doCrosswalkExist;
                prevPlaceCrosswalkToggle = doCrosswalkExist;
            }

            var intersectionData = EditingSystem.system.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            if (intersectionData != null)
            {
                EditingSystem.system.EditSceneViewGui?.SetupIntersection(intersectionData);

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
            bool isChanged = selectedRoad.Apply(EditingSystem.system.EditSceneViewGui);
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

        private void ReproduceRoad(RnModel network, IReadOnlyList<RnRoad> changedRoads)
        {
            new RoadReproducer().Generate(new RrTargetRoadBases(network, changedRoads), CrosswalkFreq());
            prevPlaceCrosswalkToggle = placeCrosswalkToggle.value;
        }
        
        private void OnRoadSplineStartButtonClicked()
        {
            UpdateSplineButtonVisual(true);
            selectedRoad.IsSplineEditMode = true;
            selectedRoad.ApplySplineEditMode(RoadNetworkEditingSystem.SingletonInstance?.system.EditSceneViewGui);
        }

        private void OnRoadSplineStopButtonClicked()
        {
            UpdateSplineButtonVisual(false);
            roadSplineStartButton.SetEnabled(true);
            roadSplineStopButton.SetEnabled(false);
            selectedRoad.IsSplineEditMode = false;
            selectedRoad.ApplySplineEditMode(RoadNetworkEditingSystem.SingletonInstance?.system.EditSceneViewGui);
            
            // 道路に適用します
            var network = EditingSystem.system.RoadNetwork;
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
        

        private class EditorInstance : RoadNetworkEditingSystem.ISystemInstance
        {
            public EditorInstance(VisualElement root, RoadEditPanel editor)
            {
                this.root = root;
                this.editor = editor;
            }
            VisualElement root;
            RoadEditPanel editor;

            public void RequestReinitialize()
            {
                editor.OnTabSelected(root);
                ReInitialize();
            }

            public void ReInitialize()
            {
            }
        }

        private CrosswalkFrequency CrosswalkFreq()
        {
            return placeCrosswalkToggle.value ? CrosswalkFrequency.All : CrosswalkFrequency.Delete;
        }
    }
}
