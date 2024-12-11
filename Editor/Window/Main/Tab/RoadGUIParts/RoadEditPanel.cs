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

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    internal class RoadEditPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_EditPanel";
        public RoadNetworkEditingSystem EditorInterface { get; private set; }

        private SerializedScriptableRoadMdl lastSelectedRoad;
        private SerializedScriptableRoadMdl selectedRoad;

        // UI要素
        private readonly Button roadSplineStartButton;
        private readonly Button roadSplineStopButton;

        public RoadEditPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {

            // ボタン類の設定
            var editModeToggle = self.Q<Toggle>("EditModeButton");
            if (editModeToggle == null)
            {
                Debug.LogError("EditModeToggle is not found.");
                return;
            }
            editModeToggle.value = RoadNetworkEditingSystem.SingletonInstance?.system != null;
            editModeToggle.RegisterCallback<ChangeEvent<bool>>(OnEditModeToggleClicked());

            // バインドパスの設定
            self.Q<IntegerField>("LeftSide").bindingPath = "numLeftLane";
            self.Q<IntegerField>("RightSide").bindingPath = "numRightLane";
            self.Q<Toggle>("EnableMedianLane").bindingPath = "enableMedianLane";
            self.Q<Toggle>("EnableLeftSideWalk").bindingPath = "enableLeftSideWalk";
            self.Q<Toggle>("EnableRightSideWalk").bindingPath = "enableRightSideWalk";
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
        private EventCallback<ChangeEvent<bool>> OnEditModeToggleClicked()
        {
            return (evt) =>
            {
                if (evt.newValue)
                {
                    OnEditModeActivated();
                }
                else
                {
                    var system = RoadNetworkEditingSystem.SingletonInstance?.system;
                    if (system != null)
                    {
                        system.EnableLimitSceneViewDefaultControl = false;
                        TerminateSystem_();
                    }
                }
            };
        }

        /// <summary> 「編集モード」ボタンが押されて編集モードになったとき </summary>
        private void OnEditModeActivated()
        {


            // 編集システムの初期化
            EditorInterface =
                RoadNetworkEditingSystem.TryInitalize(EditorInterface, rootVisualElement, new EditorInstance(rootVisualElement, this));

            // システムの取得
            var system = EditorInterface.system;
            system.CurrentEditMode = RoadNetworkEditMode.EditRoadStructure;

            system.RoadNetworkSimpleEditModule?.Init();

            system.OnChangedSelectRoadNetworkElement += OnChangedSelectedRoadNetworkElement;
            system.EnableLimitSceneViewDefaultControl = true;
        }


        protected override void OnTabUnselected()
        {

            var sys = RoadNetworkEditingSystem.SingletonInstance?.system;
            if (sys != null)
                TerminateSystem_();
            base.OnTabUnselected();
        }

        void TerminateSystem_()
        {
            rootVisualElement.Unbind();
            EditorInterface.system.OnChangedSelectRoadNetworkElement -= OnChangedSelectedRoadNetworkElement;
            RoadNetworkEditingSystem.TryTerminate(EditorInterface, rootVisualElement);
        }

        private void OnChangedSelectedRoadNetworkElement()
        {
            var roadGroupEditorData = EditorInterface.system.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (roadGroupEditorData != null)
            {
                lastSelectedRoad = selectedRoad;

                // 無ければ生成する あれば流用する
                selectedRoad = this.CreateOrGetRoadGroupData(roadGroupEditorData);

                var roadGroup = roadGroupEditorData.Ref;

                // 既存のモデルオブジェクトを解除
                rootVisualElement.Unbind();

                // 仮 詳細編集モード
                rootVisualElement.TrackSerializedObjectValue(selectedRoad, (se) =>
                {
                    var mod = EditorInterface.system.RoadNetworkSimpleEditModule;
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
                    .RoadNetworkSimpleEditModule);

                // モデルのバインド
                rootVisualElement.Bind(selectedRoad);
            }

            var intersectionData = EditorInterface.system.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            if (intersectionData != null)
            {
                EditorInterface.system.RoadNetworkSimpleEditModule?.Setup(intersectionData);

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
            bool isChanged = selectedRoad.Apply(EditorInterface.system.RoadNetworkSimpleEditModule);
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

            new RoadReproducer().Generate(new RrTargetRoadBases(network, changedRoads));
        }

        private void OnRoadSplineStartButtonClicked()
        {
            UpdateSplineButtonVisual(true);
            selectedRoad.IsSplineEditMode = true;
            selectedRoad.ApplySplineEditMode(RoadNetworkEditingSystem.SingletonInstance?.system.RoadNetworkSimpleEditModule);
        }

        private void OnRoadSplineStopButtonClicked()
        {
            UpdateSplineButtonVisual(false);
            roadSplineStartButton.SetEnabled(true);
            roadSplineStopButton.SetEnabled(false);
            selectedRoad.IsSplineEditMode = false;
            selectedRoad.ApplySplineEditMode(RoadNetworkEditingSystem.SingletonInstance?.system.RoadNetworkSimpleEditModule);
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

        public bool Apply(RoadNetworkSimpleEditSysModule mod)
        {
            throw new NotImplementedException();
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

    }
}
