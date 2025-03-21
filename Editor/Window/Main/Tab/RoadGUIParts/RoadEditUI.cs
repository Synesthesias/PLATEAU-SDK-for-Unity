using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// <see cref="RoadEditPanel"/>のうち、道路選択時にPLATEAUウィンドウ上に表示されるUIです。
    /// </summary>
    internal class RoadEditUI : RoadEditLaneNumUI.IRoadEditLaneNumReceiver, RoadEditShapeUI.IRoadEditShapeReceiver
    {
        private RoadEditLaneNumUI laneNumUI; // 道路レーン数編集のUI
        public RoadEditShapeUI ShapeUI { get; } // 道路形状編集のUI
        private VisualElement roadEditUIRoot;
        
        private SerializedScriptableRoadMdl selectedRoad;

        private RoadNetworkEditingSystem editingSystem;

        public RoadNetworkEditingSystem EditingSystem
        {
            get
            {
                return editingSystem;
            }
            set
            {
                laneNumUI.EditingSystem = value;
                ShapeUI.EditingSystem = value;
                editingSystem = value;
            }
        }

        public RoadEditUI(VisualElement roadEditUIRoot, RoadNetworkEditingSystem roadNetworkEditingSystem)
        {
            this.roadEditUIRoot = roadEditUIRoot;
            
            laneNumUI = new RoadEditLaneNumUI(roadEditUIRoot, this);
            ShapeUI = new RoadEditShapeUI(roadEditUIRoot, this);
            
            EditingSystem = roadNetworkEditingSystem;
        }

        /// <summary>
        /// 道路が選択されたとき
        /// </summary>
        public void OnRoadSelected(EditorData<RnRoadGroup> roadGroupEditorData)
        {
            
            // 無ければ生成する あれば流用する
            selectedRoad = this.CreateOrGetRoadGroupData(roadGroupEditorData);

            laneNumUI.OnRoadSelected(selectedRoad, roadGroupEditorData.Ref.Roads);
            ShapeUI.OnRoadSelected(selectedRoad);
                
            Appear();
        }

        public void Hide()
        {
            roadEditUIRoot.style.display = DisplayStyle.None;
        }

        public void Appear()
        {
            roadEditUIRoot.style.display = DisplayStyle.Flex;
        }

        
        /// <summary>
        /// 道路ネットワークを元に道路メッシュや白線生成を行います
        /// </summary>
        private void ReproduceRoad(RnModel network, IReadOnlyList<RnRoadBase> changedRoads, ISmoothingStrategy smoothingStrategy)
        {
            if (network == null) return;
            new RoadReproducer().Generate(new RrTargetRoadBases(network, changedRoads), laneNumUI.CrosswalkFreq(), smoothingStrategy);
            
        }

        public void OnLaneNumChanged(RnModel network, IReadOnlyList<RnRoad> changedRoads)
        {
            // レーン数の変更により、隣接交差点のトラックが変わるかもしれません。その場合、隣接交差点に隣接する道路の矢印の変更が必要です。
            var intersections = changedRoads
                .Select(r => r.Next as RnIntersection)
                .Concat(changedRoads.Select(r => r.Prev as RnIntersection))
                .Where(i => i != null)
                .Distinct();

            var adjRoads = intersections
                .SelectMany(i => i.GetNeighborRoads())
                .OfType<RnRoad>()
                .Where(r => !changedRoads.Contains(r));

            var targetRoads = changedRoads.Concat(adjRoads).ToList();
            
            // 道路を再生成します。
            ReproduceRoad(network, targetRoads, new SmoothingStrategyRespectOriginal()/*元の形状を尊重*/);
        }

        public void OnRoadShapeChanged(RnModel network, IReadOnlyList<RnRoad> changedRoads)
        {
            ReproduceRoad(network, changedRoads, new SmoothingStrategySmoothAll()/*ユーザーが作った線は忖度して滑らか度向上*/);
        }
        
        private SerializedScriptableRoadMdl CreateOrGetRoadGroupData(EditorData<RnRoadGroup> linkGroupEditorData)
        {
            // モデルオブジェクトを所持してるならそれを利用する
            var mdl = linkGroupEditorData.ReqSubData<ScriptableObjectFolder>();
            return mdl.Item;
        }
    }
}