using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<EditorData<RnRoadGroup>> Connections { get => roadGroupEditorData; }

        public RoadNetworkEditSceneViewGui(GameObject root, RnModel rnModel,
            RoadNetworkEditTargetSelectButton editTargetSelectButton, RoadNetworkEditTarget target)
        {
            ReConstruct(root, rnModel, editTargetSelectButton, target);
        }

        private GameObject roadNetworkEditingSystemObjRoot;
        private RnModel roadNetwork;
        private RoadNetworkEditTargetSelectButton editTargetSelectButton;
        private RoadNetworkEditTarget editTarget;
        
        // 詳細編集モードかどうか
        public bool isEditingDetailMode = false;

        private Dictionary<RnIntersection, EditorData<RnIntersection>> intersectionEditorData =
            new Dictionary<RnIntersection, EditorData<RnIntersection>>();

        private Dictionary<RnRoadBase, NodeEditorData> nodeEditorData = new Dictionary<RnRoadBase, NodeEditorData>();

        private EditorDataList<EditorData<RnRoadGroup>> roadGroupEditorData =
            new EditorDataList<EditorData<RnRoadGroup>>();

        private Dictionary<RnPoint, EditorData<RnPoint>> ptEditorData = new Dictionary<RnPoint, EditorData<RnPoint>>();
        

        public EditingIntersection EditingIntersectionMod { get => editingIntersection; }
        private EditingIntersection editingIntersection = new();
        public RnSplineEditor SplineEditorMod { get => splineEditor; }
        private RnSplineEditor splineEditor = new();
        
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
        public void Init()
        {
            ClearCache();

            SplineEditorMod.Initialize();

            nodeEditorData = new Dictionary<RnRoadBase, NodeEditorData>(roadNetwork.Intersections.Count);
            // ノードに紐づくオブジェクトを作成 editor用のデータを作成
            var nodePrefabPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/RoadNetwork/Node.prefab";
            // プレハブをResourcesフォルダからロード
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(nodePrefabPath);
            Assert.IsNotNull(prefab);
            var id = 0;
            StringBuilder sb = new StringBuilder("Node".Length + "XXX".Length);
            foreach (var node in roadNetwork.Intersections)
            {
                var subData = new NodeEditorData();
                nodeEditorData.Add(node, subData);

                id++;
                sb.Clear();
            }

            foreach (var intersection in roadNetwork.Intersections)
            {
                intersectionEditorData.Add(intersection, new EditorData<RnIntersection>(intersection));
            }


            var lineE = roadNetwork.CollectAllLineStrings();
            foreach (var line in lineE)
            {
                //Ray ray;
                foreach (var point in line.Points)
                {
                    var ptData = new EditorData<RnPoint>(point);
                    var isSuc = ptEditorData.TryAdd(point, ptData);
                    if (isSuc)
                    {
                        var d = ptData.Add<PointEditorData>();
                        Assert.IsNotNull(d);
                    }
                }
            }

            // Nodeとその近辺のPointを紐づける
            HashSet<RnPoint> points = new HashSet<RnPoint>(500); // capacityは適当
            var numNode = roadNetwork.Intersections.Count;
            if (numNode == 0)
                return;

            List<RoadGroupEditorData>
                roadGroups =
                    new List<RoadGroupEditorData>(numNode * (numNode - 1)); // node同士の繋がりを表現するコレクション prev+next名で表現する
            HashSet<RnNeighbor> calcedNeighbor = new HashSet<RnNeighbor>(numNode * (numNode - 1)); // 計算済みのNeighborを保持する
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
                var prevIntersection = linkGroup.PrevIntersection;
                var nextIntersection = linkGroup.NextIntersection;
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
                if (prevIntersection != null)
                    nodeEditorData[prevIntersection].Connections.Add(rgEditorData);
                if (nextIntersection != null)
                    nodeEditorData[nextIntersection].Connections.Add(rgEditorData);
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

            return;
        }

        /// <summary>
        /// 詳細編集モードに移行できるか
        /// 何かの編集機能を利用途中であったりすると移行できない（例　２つのノードをクリックする必要がある機能で1つ目をクリックした後）
        /// </summary>
        /// <returns></returns>
        public bool CanSetDtailMode()
        {
            return true;
        }

        /// <summary>
        /// 詳細編集モードか？
        /// </summary>
        /// <returns></returns>
        public bool IsDetailMode()
        {
            return isEditingDetailMode;
        }

        /// <summary>
        /// 詳細編集モードに移行する
        /// </summary>
        /// <param name="isDetailMode"></param>
        public void SetDetailMode(bool isDetailMode)
        {
            isEditingDetailMode = isDetailMode;
        }

        private void ClearCache()
        {
            nodeEditorData.Clear();
            roadGroupEditorData.Clear();
            roadGroupEditorData.ClearCache();
            ptEditorData.Clear();
            intersectionEditorData.Clear();
        }

        /// <summary>
        /// 描画します
        /// </summary>
        public void Update(SceneView sceneView)
        {
            if (roadNetworkEditingSystemObjRoot == null)
                return;
            
            UpdateRoad(sceneView);
            UpdateIntersection();


            // 仮で呼び出し　描画の更新がワンテンポ遅れるため　
            EditorUtility.SetDirty(roadNetworkEditingSystemObjRoot);
        }

        private void UpdateRoad(SceneView sceneView)
        {
            // マウス位置に近いwayを算出

            if (this.roadGroupEditorData.TryGetCache<RoadGroupEditorData>("linkGroup", out var eConn) == false)
            {
                Assert.IsTrue(false);
                return;
            }

            List<RoadGroupEditorData> connections = eConn.ToList();
            connections.Remove(null);

            // 道路レーンをドラッグでスライドする
            var slidingWay = waySlider.Draw(this, editTarget, sceneView, out bool isRoadChanged);
            if (isRoadChanged)
            {
                OnRoadChanged(editTarget.SelectedRoadNetworkElement as EditorData<RnRoadGroup>);
            }
            

            // gizmos描画の更新
            var gizmosdrawer = GetRoadNetworkEditorGizmos();

            // gizmosの更新
            var lines = new LaneLineGizmoGenerator().Generate(
                editTarget.SelectedRoadNetworkElement,
                waySlider.WaySlideCalcCache?.ClosestWay,
                this.roadGroupEditorData,
                slidingWay);
            gizmosdrawer.SetLine(lines);

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

        private void UpdateIntersection()
        {
            var editingIntersectionMod = EditingIntersectionMod;
            if (editingIntersectionMod.Intersection == null) return;
            
            RoadEditSceneGUIState state = new RoadEditSceneGUIState();

            var currentCamera = SceneView.currentDrawingSceneView.camera;
            state.currentCamera = currentCamera;


            

            var buttonSize = 2.0f;

            bool isSelectdEntablePoint = editingIntersectionMod.IsSelectdEntablePoint;
            if (isSelectdEntablePoint == false)
            {
                foreach (var item in editingIntersectionMod.EnterablePoints)
                {
                    // 流入点の位置にボタンを表示する
                    if (Handles.Button(item.CalcCenter(), Quaternion.identity, buttonSize, buttonSize,
                            RoadNetworkEntarablePointButtonHandleCap))
                    {
                        editingIntersectionMod.SetEntablePoint(item);
                        // 流入点が選択された
                        break;
                    }
                }
            }
            else
            {
                foreach (var item in editingIntersectionMod.ExitablePoints)
                {
                    // 流出点の位置にボタンを表示する
                    if (Handles.Button(item.CalcCenter(), Quaternion.identity, buttonSize, buttonSize,
                            RoadNetworkExitablePointButtonHandleCap))
                    {
                        // 流出点が選択された
                        editingIntersectionMod.SetExitablePoint(item);
                        break;
                    }
                }
            }

            // Trackの生成、削除に必要な設定が済んで更新できるか？
            if (editingIntersectionMod.CanTryUpdateTrack)
            {
                editingIntersectionMod.UpdateTrack();
            }


            // 遅延実行 コレクションの要素数などを変化させる
            if (state.delayCommand != null)
                state.delayCommand.Invoke();

            // 変更を通知する
            if (state.isDirtyTarget)
            {
                editTarget.SetDirty();
            }
        }
        
        private static void RoadNetworkExitablePointButtonHandleCap(int controlID, Vector3 position,
            Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Repaint)
                Handles.color = Color.blue;
            Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
        }
        
        private static void RoadNetworkEntarablePointButtonHandleCap(int controlID, Vector3 position,
            Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Repaint)
                Handles.color = Color.red;
            Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
        }

        private RoadNetworkEditorGizmos GetRoadNetworkEditorGizmos()
        {
            if (roadNetworkEditingSystemObjRoot == null) return null;
            return roadNetworkEditingSystemObjRoot.GetComponent<RoadNetworkEditorGizmos>();
        }

        public void SetupIntersection(EditorData<RnIntersection> data)
        {
            editingIntersection.SetTarget(data);
            editingIntersection.Activate(true);
        }

        public void Terminate()
        {
            editingIntersection.SetTarget(null);
            editingIntersection.Activate(false);
            
            GetRoadNetworkEditorGizmos().Clear();
            
            ClearCache();
        }
        
        private void OnRoadChanged(EditorData<RnRoadGroup> roadGroupEditorData)
        {
            // 道路を生成
            var roads = roadGroupEditorData.Ref.Roads;
            new RoadReproducer().Generate(new RrTargetRoadBases(roadNetwork, roads.ToArray()), CrosswalkFrequency.All);
        }

        
    }
}