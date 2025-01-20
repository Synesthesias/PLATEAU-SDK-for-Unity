using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// <see cref="RoadEditUI"/>のうち、道路レーン数編集のUIと機能です。
    /// </summary>
    internal class RoadEditLaneNumUI
    {
        private bool prevPlaceCrosswalkToggle = false;
        private readonly Toggle placeCrosswalkToggle;
        
        private VisualElement roadEditUIRoot;
        private SerializedScriptableRoadMdl selectedRoadScriptable;
        private IRoadEditLaneNumReceiver roadEditLaneNumReceiver;
        public RoadNetworkEditingSystem EditingSystem { get; set; }
        
        public RoadEditLaneNumUI(VisualElement roadEditUIRoot, IRoadEditLaneNumReceiver roadEditLaneNumReceiver)
        {
            this.roadEditUIRoot = roadEditUIRoot;
            this.roadEditLaneNumReceiver = roadEditLaneNumReceiver;
            
            // バインドパスの設定
            roadEditUIRoot.Q<IntegerField>("LeftSide").bindingPath = "numLeftLane";
            roadEditUIRoot.Q<IntegerField>("RightSide").bindingPath = "numRightLane";
            roadEditUIRoot.Q<Toggle>("EnableMedianLane").bindingPath = "enableMedianLane";
            roadEditUIRoot.Q<Toggle>("EnableLeftSideWalk").bindingPath = "enableLeftSideWalk";
            roadEditUIRoot.Q<Toggle>("EnableRightSideWalk").bindingPath = "enableRightSideWalk";
            placeCrosswalkToggle = roadEditUIRoot.Q<Toggle>("PlaceCrosswalk");
            
            // 道路のレーン数の適用ボタン
            var applyRoadButton = roadEditUIRoot.Q<Button>("ApplyRoadButton");
            applyRoadButton.clicked += OnApplyLaneNumEditButtonClicked;
        }

        public void OnRoadSelected(SerializedScriptableRoadMdl selectedRoadScriptableArg, IEnumerable<RnRoad> roadsArg)
        {
            this.selectedRoadScriptable = selectedRoadScriptableArg;
            // 既存のモデルオブジェクトを解除
            roadEditUIRoot.Unbind();
            
            // モデルのバインド
            roadEditUIRoot.Bind(selectedRoadScriptableArg);
                
            // 現在の横断歩道の有無をチェックボックスに反映させます
            var roads = new RoadReproduceSource(roadsArg.FirstOrDefault());
            bool doCrosswalkExist = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, roads, ReproducedRoadDirection.Next);
            placeCrosswalkToggle.value = doCrosswalkExist;
            prevPlaceCrosswalkToggle = doCrosswalkExist;
        }
        
        /// <summary>
        /// 道路編集で、レーン数編集の確定が押された時の処理です。
        /// </summary>
        private void OnApplyLaneNumEditButtonClicked()
        {
            bool isChanged = selectedRoadScriptable.Apply(EditingSystem.roadEditSceneViewGui);
            isChanged |= prevPlaceCrosswalkToggle != placeCrosswalkToggle.value;
            if (!isChanged)
            {
                return;
            }

            var changedRoads = selectedRoadScriptable.TargetScriptableRoadMdl.road.Roads;

            var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnMdl == null)
            {
                Debug.LogError("RoadNetworkStructureModel is not found.");
                return;
            }
            var network = rnMdl.RoadNetwork;

            roadEditLaneNumReceiver.OnLaneNumChanged(network, changedRoads);
            prevPlaceCrosswalkToggle = placeCrosswalkToggle.value;
        }
        
        public CrosswalkFrequency CrosswalkFreq()
        {
            return placeCrosswalkToggle.value ? CrosswalkFrequency.All : CrosswalkFrequency.Delete;
        }

        internal interface IRoadEditLaneNumReceiver
        {
            public void OnLaneNumChanged(RnModel network, IReadOnlyList<RnRoad> changedRoads);
        }
    }
}