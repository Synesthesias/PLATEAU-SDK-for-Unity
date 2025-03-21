using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.Editor.Window.Main.Tab.RoadGuiParts;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路ネットワーク編集で、シーンビュー上のGUIを表示します。
    /// </summary>
    internal class RoadNetworkEditSceneViewGui
    {

        public RoadNetworkEditSceneViewGui(GameObject root, RnModel rnModel,
            RoadNetworkEditTargetSelectButton editTargetSelectButton, RoadNetworkEditTarget target,
            ISplineEditedReceiver splineEditedReceiver)
        {
            ReConstruct(root, rnModel, editTargetSelectButton, target);
            splineEditor = new RnSplineEditor(splineEditedReceiver);
        }

        private GameObject roadNetworkEditingSystemObjRoot;
        private RnModel roadNetwork;
        private RoadNetworkEditTargetSelectButton editTargetSelectButton;
        private RoadNetworkEditTarget editTarget;
        private RoadLaneDetailEditor roadDetailEditor; // 個別レーン編集
        private RoadNetworkEditorGizmos gizmosDrawer;  // 道路レーンの線を描画
        
        // 道路レーンの編集モード
        private RoadShapeEditState roadShapeEditState = RoadShapeEditState.Normal;

        private Dictionary<RnIntersection, EditorData<RnIntersection>> intersectionEditorData =
            new Dictionary<RnIntersection, EditorData<RnIntersection>>();
        

        private EditorDataList<EditorData<RnRoadGroup>> roadGroupEditorData =
            new EditorDataList<EditorData<RnRoadGroup>>();
        
        public RnSplineEditor SplineEditorMod { get => splineEditor; }
        private RnSplineEditor splineEditor; // 全車道編集

        
        /// <summary> 道路のレーンをドラッグで編集する機能です。 </summary>
        private WaySlider waySlider = new WaySlider();

        /// <summary>
        /// 計算や処理に必要な要素を初期化する
        /// </summary>
        public void ReConstruct(GameObject root, RnModel rnModel,
            RoadNetworkEditTargetSelectButton targetSelectButton, RoadNetworkEditTarget target)
        {
            Assert.IsNotNull(root);
            Assert.IsNotNull(rnModel);
            Assert.IsNotNull(targetSelectButton);
            roadNetworkEditingSystemObjRoot = root;
            roadNetwork = rnModel;
            editTargetSelectButton = targetSelectButton;
            this.editTarget = target;
            
            ClearCache();
        }

        /// <summary>
        /// 計算や処理を行う初期化
        /// それらに必要な要素は初期化済みとする
        /// </summary>
        public void Init(RoadShapeEditState shapeEditState)
        {
            ClearCache();

            SplineEditorMod.Initialize();
            
            // ノードに紐づくオブジェクトを作成 editor用のデータを作成
            var nodePrefabPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/RoadNetwork/Node.prefab";
            // プレハブをResourcesフォルダからロード
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(nodePrefabPath);
            Assert.IsNotNull(prefab);

            foreach (var intersection in roadNetwork.Intersections)
            {
                intersectionEditorData.Add(intersection, new EditorData<RnIntersection>(intersection));
            }
            
            var numNode = roadNetwork.Intersections.Count;
            if (numNode == 0)
                return;

            List<RoadGroupEditorData>
                roadGroups =
                    new List<RoadGroupEditorData>(numNode * (numNode - 1)); // node同士の繋がりを表現するコレクション prev+next名で表現する
            HashSet<RnIntersectionEdge> calcedNeighbor = new HashSet<RnIntersectionEdge>(numNode * (numNode - 1)); // 計算済みのNeighborを保持する
            foreach (var road in roadNetwork.Roads)
            {
                var link = road;
                if (link == null)
                {
                    continue;
                }

                var linkGroup = link.CreateRoadGroup();
                if (linkGroup == null)
                {
                    continue;
                }

                // 同じものを格納済みかチェック
                var isContain = false;
                foreach (var group in roadGroups)
                {
                    isContain = RnRoadGroup.IsSameRoadGroup(group.RoadGroup.Ref, linkGroup);
                    if (isContain == true)
                    {
                        break;
                    }
                }

                if (isContain)
                    continue;

                // 編集用データを追加
                var editorData = new EditorData<RnRoadGroup>(linkGroup);
                var rgEditorData = editorData.Add<RoadGroupEditorData>();
                roadGroups.Add(rgEditorData);
                roadGroupEditorData.Add(editorData);
            }

            // 仮 編集可能なデータに勝手に修正
            foreach (var linkGroupEditorData in roadGroupEditorData)
            {
                var data = linkGroupEditorData.Ref;
                var nl = data.GetLeftLaneCount();
                var nr = data.GetRightLaneCount();
                data.SetLaneCount(nl, nr);
                linkGroupEditorData.IsEditable = true;
            }


            // キャッシュの生成
            roadGroupEditorData.AddCache("linkGroup", (d) =>
            {
                if (d.IsEditable == false)
                {
                    return null;
                }

                return d.GetSubData<RoadGroupEditorData>();
            });
            //linkGroupEditorData.Select((d) => d.GetSubData<LinkGroupEditorData>()).ToList();

            SetRoadShapeEditState(shapeEditState);   
        }
        



        public void SetRoadShapeEditState(RoadShapeEditState editState)
        {
            roadShapeEditState = editState;
            if (editState == RoadShapeEditState.IndividualLane)
            {
                roadDetailEditor = new RoadLaneDetailEditor();
            }
        }

        private void ClearCache()
        {
            roadGroupEditorData.Clear();
            roadGroupEditorData.ClearCache();
            intersectionEditorData.Clear();
        }

        /// <summary>
        /// 描画します
        /// </summary>
        public void Update(SceneView sceneView)
        {
            if (roadNetworkEditingSystemObjRoot == null)
                return;

            RefreshGUI();
            
            if(gizmosDrawer != null) gizmosDrawer.SetDrawingActive(roadShapeEditState == RoadShapeEditState.Normal);

            if (roadShapeEditState == RoadShapeEditState.IndividualLane)
            {
                UpdateRoadOnIndividualLaneEditMode();
            }
            else
            {
                UpdateRoadOnSimpleMode(sceneView);
            }
            

            // 仮で呼び出し　描画の更新がワンテンポ遅れるため　
            EditorUtility.SetDirty(roadNetworkEditingSystemObjRoot);
        }

        private void RefreshGUI()
        {
            // guiの更新

            editTargetSelectButton.connections = this.roadGroupEditorData;
            //if (connections.Count > 0)
            //{
            //    Handles.DrawLines(pts);
            //    //Gizmos.DrawLineList(pts);
            //}
            editTargetSelectButton.intersections = null;
            editTargetSelectButton.intersections = intersectionEditorData.Values;
            //var intersectionsPoss = guisys.intersections;
            //intersectionsPoss.Capacity = nodeEditorData.Count;
            //foreach (var item in nodeEditorData.Values)
            //{
            //    if (item.IsIntersection == false)
            //        continue;
            //    intersectionsPoss.Add(item.RefGameObject.transform.position);
            //}
        }

        private void UpdateRoadOnSimpleMode(SceneView sceneView)
        {

            if (this.roadGroupEditorData.TryGetCache<RoadGroupEditorData>("linkGroup", out var eConn) == false)
            {
                Assert.IsTrue(false);
                return;
            }

            List<RoadGroupEditorData> connections = eConn.ToList();
            connections.Remove(null);

            // 道路レーンをドラッグでスライドする
            if (roadShapeEditState == RoadShapeEditState.Normal)
            {
                var slidingWay = waySlider.Draw(this, editTarget, sceneView, out bool isRoadChanged);
                if (isRoadChanged)
                {
                    OnRoadChanged(editTarget.SelectedRoadNetworkElement as EditorData<RnRoadGroup>);
                }
            

                // gizmos描画の更新
                gizmosDrawer = GetRoadNetworkEditorGizmos();

                // gizmosの更新
                var lines = new LaneLineGizmoGenerator().Generate(
                    editTarget.SelectedRoadNetworkElement,
                    waySlider.WaySlideCalcCache?.ClosestWay,
                    this.roadGroupEditorData,
                    slidingWay);
                gizmosDrawer.SetLine(lines);
            }
            
        }

        private void UpdateRoadOnIndividualLaneEditMode()
        {
            var selectedRoadGroup = editTarget.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (selectedRoadGroup == null) return;
            roadDetailEditor.Draw(selectedRoadGroup.Ref, editTarget);
        }
        
        private RoadNetworkEditorGizmos GetRoadNetworkEditorGizmos()
        {
            if (roadNetworkEditingSystemObjRoot == null) return null;
            return roadNetworkEditingSystemObjRoot.GetComponent<RoadNetworkEditorGizmos>();
        }

        public void Terminate()
        {
            
            GetRoadNetworkEditorGizmos()?.Clear();
            roadDetailEditor = null;
            
            ClearCache();
        }
        
        private void OnRoadChanged(EditorData<RnRoadGroup> roadGroupEditorData)
        {
            // 道路を生成
            var roads = roadGroupEditorData.Ref.Roads;
            new RoadReproducer().Generate(new RrTargetRoadBases(roadNetwork, roads.ToArray()), CrosswalkFrequency.All, new SmoothingStrategySmoothAll());
        }

        
    }
}