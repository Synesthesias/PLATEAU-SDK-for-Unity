using PLATEAU.RoadNetwork.Structure;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.EditingSystem;
using UnityEditor;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    internal class RoadEditPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_EditPanel";
        private RoadNetworkEditingSystem EditingSystem { get; set; }

        private RoadEditUI roadEditUI; // 道路選択時にPLATEAUウィンドウ上に表示されるUI
        private readonly VisualElement roadEditUIRoot;
        private readonly VisualElement intersectionEditInstruction;

        private readonly Button removeIntersectionButton;

        // UI要素
        private readonly Toggle editModeToggle;


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

            // 「道路編集」は道路が選択されるまでオフにします
            roadEditUIRoot = rootVisualElement.Q<VisualElement>("RoadEditPanel");
            roadEditUI = new RoadEditUI(roadEditUIRoot, EditingSystem);
            roadEditUI.Hide();

            intersectionEditInstruction = rootVisualElement.Q<VisualElement>("IntersectionEditInstruction");
            intersectionEditInstruction.style.display = DisplayStyle.None;

            removeIntersectionButton = rootVisualElement.Q<Button>("RemoveIntersectionButton");
            removeIntersectionButton.clicked += OnRemoveIntersectionButtonClicked;
        }

        private void OnRemoveIntersectionButtonClicked()
        {
            if (EditingSystem == null) return;

            var selectedIntersection = EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            if (selectedIntersection == null) return;

            // モデル削除
            var road = new RoadReproduceSource(selectedIntersection.Ref);
            var meshObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.RoadMesh, road, ReproducedRoadDirection.None);
            var crosswalkObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, road, ReproducedRoadDirection.None);
            var lineObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.LaneLineAndArrow, road, ReproducedRoadDirection.None);
            if (meshObj != null)
            {
                Object.DestroyImmediate(meshObj);
            }
            if (crosswalkObj != null)
            {
                Object.DestroyImmediate(crosswalkObj);
            }
            if (lineObj != null)
            {
                Object.DestroyImmediate(lineObj);
            }

            selectedIntersection.Ref.DisConnect(true);

            // システムのキャッシュ削除
            EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement = null;
            EditingSystem.roadEditSceneViewGui?.Init(RoadShapeEditState.Normal);

            OnChangedSelectedRoadBase();
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
            EditingSystem = new RoadNetworkEditingSystem(roadEditUI.ShapeUI);
            roadEditUI.EditingSystem = EditingSystem;

            EditingSystem.roadEditSceneViewGui?.Init(RoadShapeEditState.Normal);

            EditingSystem.roadNetworkEditTarget.OnChangedSelectRoadNetworkElement += OnChangedSelectedRoadBase;
            EditingSystem.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl = true;
        }

        /// <summary> 「編集モード」ボタンが押されて編集モードが解除されたとき </summary>
        private void OnEditModeDeactivated()
        {
            TerminateSystem();
        }

        protected override void OnTabUnselected()
        {
            TerminateSystem();
            base.OnTabUnselected();
        }

        private void TerminateSystem()
        {

            rootVisualElement.Unbind();
            roadEditUI.Hide();

            if (EditingSystem != null)
            {
                EditingSystem.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl = false;
                EditingSystem.roadNetworkEditTarget.OnChangedSelectRoadNetworkElement -= OnChangedSelectedRoadBase;
            }

            RoadNetworkEditingSystem.TryTerminate(EditingSystem, rootVisualElement);
            if (editModeToggle != null) editModeToggle.value = false;
            intersectionEditInstruction.style.display = DisplayStyle.None;
            SceneView.RepaintAll();
        }

        /// <summary>
        /// 道路または交差点が選択されたとき
        /// </summary>
        private void OnChangedSelectedRoadBase()
        {
            EditingSystem.intersectionEditSceneViewGui.Terminate();

            switch (EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement)
            {
                // 選択されたものが道路の場合
                case EditorData<RnRoadGroup> roadGroupEditorData:
                    roadEditUI.OnRoadSelected(roadGroupEditorData);
                    intersectionEditInstruction.style.display = DisplayStyle.None;
                    break;
                // 選択されたものが交差点の場合
                case EditorData<RnIntersection> intersectionData:
                    EditingSystem.intersectionEditSceneViewGui?.SetupIntersection(intersectionData.Ref);
                    roadEditUI.Hide();
                    intersectionEditInstruction.style.display = DisplayStyle.Flex;
                    break;
                // 選択されたものがない場合
                default:
                    roadEditUI.Hide();
                    intersectionEditInstruction.style.display = DisplayStyle.None;
                    break;
            }
        }

    }
}
