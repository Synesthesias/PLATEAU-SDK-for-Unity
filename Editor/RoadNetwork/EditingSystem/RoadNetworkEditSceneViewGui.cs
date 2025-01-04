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
        

        private EditorDataList<EditorData<RnRoadGroup>> roadGroupEditorData =
            new EditorDataList<EditorData<RnRoadGroup>>();
        
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
            
            UpdateRoad(sceneView);


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
        
        private RoadNetworkEditorGizmos GetRoadNetworkEditorGizmos()
        {
            if (roadNetworkEditingSystemObjRoot == null) return null;
            return roadNetworkEditingSystemObjRoot.GetComponent<RoadNetworkEditorGizmos>();
        }

        public void Terminate()
        {
            
            GetRoadNetworkEditorGizmos()?.Clear();
            
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