using PLATEAU.RoadNetwork.Structure;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.EditingSystem;
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

        private RoadEditUI roadEditUI; // 道路選択時にPLATEAUウィンドウ上に表示されるUI
        private readonly VisualElement roadEditUIRoot;
        

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
            
            EditingSystem.roadEditSceneViewGui?.Init(false);
            
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
            roadEditUI.Hide();

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
            
            // 選択されたものが道路の場合
            var roadGroupEditorData = EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (roadGroupEditorData != null)
            {
                roadEditUI.OnRoadSelected(roadGroupEditorData);
            }

            // 選択されたものが交差点の場合
            var intersectionData = EditingSystem.roadNetworkEditTarget.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            if (intersectionData != null)
            {
                EditingSystem.intersectionEditSceneViewGui?.SetupIntersection(intersectionData.Ref);
                roadEditUI.Hide();
            }
        }
        
    }
}
