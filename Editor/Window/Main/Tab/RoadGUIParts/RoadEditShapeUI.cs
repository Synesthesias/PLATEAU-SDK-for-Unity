using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// <see cref="RoadEditUI"/>のうち、道路レーン形状変更のUIと機能です。
    /// </summary>
    internal class RoadEditShapeUI : ISplineEditedReceiver
    {
        private readonly Button individualLaneEditModeButtonStart;
        private readonly Button individualLaneEditModeButtonEnd;
        private readonly Button allLaneEditButtonStart;
        private readonly Button allLaneEditButtonEnd;
        public RoadNetworkEditingSystem EditingSystem { get; set; }
        private SerializedScriptableRoadMdl selectedRoad;
        private SerializedScriptableRoadMdl lastSelectedRoad;
        private readonly IRoadEditShapeReceiver roadEditShapeReceiver;
        private ShapeEditState shapeEditState = ShapeEditState.Normal;
        private VisualElement instructionAllLane;
        private VisualElement instructionIndividualLane;

        public RoadEditShapeUI(VisualElement roadEditUIRoot, IRoadEditShapeReceiver roadEditShapeReceiver)
        {
            this.roadEditShapeReceiver = roadEditShapeReceiver;
            individualLaneEditModeButtonStart = roadEditUIRoot.Q<Button>("DetailEditModeStartButton");
            individualLaneEditModeButtonEnd = roadEditUIRoot.Q<Button>("DetailEditModeEndButton");
            individualLaneEditModeButtonStart.clicked += OnIndividualLaneEditModeStartButtonPushed;
            individualLaneEditModeButtonEnd.clicked += OnIndividualLaneEditModeEndButtonPushed;
            
            allLaneEditButtonStart = roadEditUIRoot.Q<Button>("RoadSplineStartButton");
            allLaneEditButtonEnd = roadEditUIRoot.Q<Button>("RoadSplineEndButton");
            allLaneEditButtonStart.clicked += OnAllLaneEditStartButtonClicked;
            allLaneEditButtonEnd.clicked += OnAllLaneEditEndButtonClicked;

            instructionAllLane = roadEditUIRoot.Q<VisualElement>("EditAllLanesInstruction");
            instructionIndividualLane = roadEditUIRoot.Q<VisualElement>("EditIndividualLaneInstruction");
        }
        
        public void OnRoadSelected(SerializedScriptableRoadMdl nextSelectedRoad)
        {
            shapeEditState = ShapeEditState.Normal;
            UpdateSplineButtonVisual();
            lastSelectedRoad = this.selectedRoad;
            this.selectedRoad = nextSelectedRoad;
            lastSelectedRoad?.DisableSplineEditMode(EditingSystem?.roadEditSceneViewGui);
        }
        
        /// <summary> 「各レーンを個別編集」の開始ボタンが押された時 </summary>
        private void OnIndividualLaneEditModeStartButtonPushed()
        {
            shapeEditState = ShapeEditState.IndividualLane;
            UpdateSplineButtonVisual();
            EditingSystem?.ChangeDetailEditMode(true);
        }
        
        /// <summary> 「各レーンを個別編集」の終了ボタンが押された時 </summary>
        private void OnIndividualLaneEditModeEndButtonPushed()
        {
            shapeEditState = ShapeEditState.Normal;
            UpdateSplineButtonVisual();
            EditingSystem?.ChangeDetailEditMode(false);
        }
        
        /// <summary> 「全車道を一括編集」の開始ボタンが押された時 </summary>
        private void OnAllLaneEditStartButtonClicked()
        {
            shapeEditState = ShapeEditState.AllLanes;
            UpdateSplineButtonVisual();
            selectedRoad.IsSplineEditMode = true;
            selectedRoad.ApplySplineEditMode(EditingSystem?.roadEditSceneViewGui);
        }

        /// <summary> 「全車道を一括編集」の終了ボタンが押された時 </summary>
        private void OnAllLaneEditEndButtonClicked()
        {
            // Enterキーで決定した時の動きを模倣
            EditingSystem?.roadEditSceneViewGui?.SplineEditorMod?.FinishSplineEdit();
            
            // 終了処理
            OnSplineEdited();
        }
        
        // 全車道を一括編集が終了したとき
        public void OnSplineEdited()
        {
            shapeEditState = ShapeEditState.Normal;
            UpdateSplineButtonVisual();
            allLaneEditButtonStart.SetEnabled(true);
            selectedRoad.IsSplineEditMode = false;
            selectedRoad.ApplySplineEditMode(EditingSystem?.roadEditSceneViewGui);
            
            // 道路に適用します
            var network = EditingSystem?.roadNetworkEditTarget.RoadNetwork;
            roadEditShapeReceiver.OnRoadShapeChanged(network, selectedRoad.TargetScriptableRoadMdl.road.Roads);
        }
        
        /// <summary>
        /// 何を編集しているかによってボタンや説明書きのON/OFFを切り替えます。
        /// </summary>
        private void UpdateSplineButtonVisual()
        {
            switch (shapeEditState)
            {
                case ShapeEditState.Normal:
                    allLaneEditButtonStart.style.display = DisplayStyle.Flex;
                    allLaneEditButtonEnd.style.display = DisplayStyle.None;
                    individualLaneEditModeButtonStart.style.display = DisplayStyle.Flex;
                    individualLaneEditModeButtonEnd.style.display = DisplayStyle.None;
                    instructionAllLane.style.display = DisplayStyle.None;
                    instructionIndividualLane.style.display = DisplayStyle.None;
                    break;
                case ShapeEditState.AllLanes:
                    allLaneEditButtonStart.style.display = DisplayStyle.None;
                    allLaneEditButtonEnd.style.display = DisplayStyle.Flex;
                    individualLaneEditModeButtonStart.style.display = DisplayStyle.None;
                    individualLaneEditModeButtonEnd.style.display = DisplayStyle.None;
                    instructionAllLane.style.display = DisplayStyle.Flex;
                    instructionIndividualLane.style.display = DisplayStyle.None;
                    break;
                case ShapeEditState.IndividualLane:
                    allLaneEditButtonStart.style.display = DisplayStyle.None;
                    allLaneEditButtonEnd.style.display = DisplayStyle.None;
                    individualLaneEditModeButtonStart.style.display = DisplayStyle.None;
                    individualLaneEditModeButtonEnd.style.display = DisplayStyle.Flex;
                    instructionAllLane.style.display = DisplayStyle.None;
                    instructionIndividualLane.style.display = DisplayStyle.Flex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        private enum ShapeEditState
        {
            Normal, AllLanes, IndividualLane
        }
        
        public interface IRoadEditShapeReceiver
        {
            void OnRoadShapeChanged(RnModel network, IReadOnlyList<RnRoad> changedRoads);
        }
    }
}