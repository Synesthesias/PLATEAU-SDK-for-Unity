﻿using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.Graph
{
    public class RGraphDebugEditorWindow : EditorWindow
    {
        public interface IInstanceHelper
        {
            // グラフ取得
            PLATEAURGraph GetGraph();

            // グラフ作成
            PLATEAURGraph CreateGraph();

            // 非表示対象オブジェクト
            HashSet<object> InVisibleObjects { get; }

            // 選択済みオブジェクト
            HashSet<object> SelectedObjects { get; }

            bool IsTarget(RFace face);
        }

        private const string WindowName = "PLATEAU RGraph Editor";

        public IInstanceHelper InstanceHelper { get; set; }

        // 作成時にマージも同時に行うかどうか
        public bool mergeOnCreate = true;
        private float mergeCellSize = 0.2f;
        private int mergeCellLength = 2;
        private float heightTolerance = 0.1f;
        private float removeMidPointTolerance = 0.3f;
        private float lod1PointHeightTolerance = 1.5f;

        private int showVertexId = -1;

        public HashSet<RFace> TargetFaces { get; } = new();
        EdgeEdit edgeEdit = new EdgeEdit();
        FaceEdit faceEdit = new FaceEdit();
        // FoldOutの状態を保持する
        private HashSet<object> FoldOuts { get; } = new();
        // Trackのスクロール位置
        public Vector2 trackScrollPosition;

        private class FaceEdit
        {
            public void Update(RGraphDebugEditorWindow work, RFace f)
            {
                if (f == null)
                    return;

                RnEditorUtil.TargetToggle($"ID[{f.DebugMyId}]", work.InstanceHelper.SelectedObjects, f);
                EditorGUILayout.EnumFlagsField("RoadType", f.RoadTypes);
                if (GUILayout.Button("Remove Inner Vertex"))
                {
                    f.RemoveInnerVertex();
                }

                if (GUILayout.Button("RemoveIsolatedEdge"))
                {
                    f.RemoveIsolatedEdge();
                }

                if (f.RoadTypes.IsSideWalk() && GUILayout.Button("Modify SideWalk Shape"))
                {
                    RGraphEx.ModifySideWalkShape(f);
                }
            }
        }

        private class EdgeEdit
        {
            public void Update(RGraphDebugEditorWindow work, REdge e)
            {
                if (e == null)
                    return;
                RnEditorUtil.TargetToggle($"ID[{e.DebugMyId}]", work.InstanceHelper.SelectedObjects, e);
                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField("Edge ID", e.DebugMyId.ToString());
                    EditorGUILayout.LongField("V0", e.V0.GetDebugMyIdOrDefault());
                    EditorGUILayout.LongField("V1", e.V1.GetDebugMyIdOrDefault());
                }
            }
        }


        private class VertexEdit
        {

            public void Update(RGraphDebugEditorWindow work, RVertex v)
            {
                if (v == null)
                    return;

                RnEditorUtil.TargetToggle($"ID[{v.DebugMyId}]", work.InstanceHelper.SelectedObjects, v);
                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.Vector3Field("Pos", v.Position);
                    foreach (var e in v.Edges)
                    {
                        EditorGUILayout.LongField("Edge", (long)e.DebugMyId);
                    }
                }

            }
        }
        VertexEdit vertexEdit = new VertexEdit();


        /// <summary>
        /// 選択/非表示オブジェクトなどの情報を削除する
        /// </summary>
        public void ClearPickedObjects()
        {
            if (InstanceHelper == null)
                return;
            FoldOuts.Clear();
            InstanceHelper.SelectedObjects?.Clear();
            InstanceHelper.InVisibleObjects?.Clear();
        }

        public void Reinitialize() { }

        private void Initialize() { }

        private void OnEnable()
        {
            Initialize();
        }

        private void VertexGUI(RVertex v)
        {
            if ((int)v.DebugMyId != showVertexId)
                return;
            RnEditorUtil.Separator();
            EditorGUILayout.IntField("ID", (int)v.DebugMyId);
            foreach (var e in v.GetNeighborVertices())
            {
                EditorGUILayout.IntField("Neighbor", (int)e.DebugMyId);
            }
            RnEditorUtil.Separator();
        }

        /// <Summary>
        /// ウィンドウのパーツを表示します。
        /// </Summary>
        private void OnGUI()
        {
            if (InstanceHelper == null)
                return;

            var target = InstanceHelper.GetGraph();
            if (!target)
            {

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create Graph"))
                    {
                        target = InstanceHelper.CreateGraph();
                        if (mergeOnCreate)
                        {
                            target.Graph.Optimize(mergeCellSize, mergeCellLength, removeMidPointTolerance, lod1PointHeightTolerance);
                        }
                    }
                    mergeOnCreate = EditorGUILayout.Toggle("MergeOnCreate", mergeOnCreate);
                }
                return;
            }

            var graph = target.Graph;
            if (graph == null)
                return;


            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Optimize"))
                {
                    graph.Optimize(mergeCellSize, mergeCellLength, removeMidPointTolerance, lod1PointHeightTolerance);
                }

                mergeCellSize = EditorGUILayout.FloatField("mergeCellSize", mergeCellSize);
                mergeCellLength = EditorGUILayout.IntField("MergeCellLength", mergeCellLength);
                removeMidPointTolerance = EditorGUILayout.FloatField("RemoveMidPointTolerance", removeMidPointTolerance);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("InsertVertexInNearEdge"))
                {
                    graph.InsertVertexInNearEdge(mergeCellSize);
                }

                if (GUILayout.Button("Lod1 Adjust Height"))
                {
                    var removed = graph.AdjustSmallLodHeight(mergeCellSize, mergeCellLength, lod1PointHeightTolerance);
                    foreach (var r in removed)
                    {
                        DebugEx.DrawSphere(r.Position, 1f, Color.red, duration: 10);
                    }
                }

                if (GUILayout.Button("Vertex Reduction"))
                {
                    graph.VertexReduction(mergeCellSize, mergeCellLength, removeMidPointTolerance);
                }

                if (GUILayout.Button("MergeIsolatedVertices"))
                {
                    graph.RemoveIsolatedEdgeFromFace();
                }

                if (GUILayout.Button("Edge Reduction"))
                    graph.EdgeReduction();

                if (GUILayout.Button("Separate Face"))
                    graph.SeparateFaces();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Check Intersection"))
                {
                    graph.InsertVerticesInEdgeIntersection(heightTolerance);
                }
                heightTolerance = EditorGUILayout.FloatField("HeightEpsilon", heightTolerance);
            }

            //if (GUILayout.Button("Create RnModel"))
            //    InstanceHelper.CreateRnModel();

            //if (GUILayout.Button("Create TranMEsh"))
            //    InstanceHelper.CreateTranMesh();

            RnEditorUtil.Separator();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Face", graph.Faces.Count);
                EditorGUILayout.IntField("Edge", graph.GetAllEdges().Count());
                EditorGUILayout.IntField("Vertex", graph.GetAllVertices().Count());
            }

            showVertexId = EditorGUILayout.IntField("ShowVertexId", showVertexId);


            RnEditorUtil.Separator();
            EditorGUILayout.LabelField("=============== Face ==============");
            InstanceHelper.SelectedObjects.RemoveWhere(o =>
            {
                if (o is RFace face && graph.Faces.Contains(face) == false)
                    return true;
                return false;
            });


            if (RnEditorUtil.Foldout($"Face[{graph.Faces.Count}]", FoldOuts, "Face"))
            {
                using var scope = new EditorGUILayout.ScrollViewScope(trackScrollPosition);
                trackScrollPosition = scope.scrollPosition;
                foreach (var face in graph.Faces)
                {
                    if (IsSceneSelected(face) == false && InstanceHelper.SelectedObjects.Contains(face) == false)
                        continue;
                    if (RnEditorUtil.Foldout($"{face.GetDebugLabelOrDefault()}[E{face.Edges.Count}]", FoldOuts, face))
                    {
                        using var indent = new EditorGUI.IndentLevelScope();
                        faceEdit.Update(this, face);
                    }
                }
            }

            if (GUILayout.Button("Copy Check"))
            {
                var copy = graph.DeepCopy();
                Assert.IsTrue(RGraphEx.IsEqual(graph, copy));
            }

            if (GUILayout.Button("Convert To RoadNetwork Graph"))
            {
                target.Graph = graph.ConvertRnModelGraph(RoadNetworkFactory.RoadPackTypes, true);
            }
        }

        /// <summary>
        /// ウィンドウを取得する、存在しない場合に生成する
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static RGraphDebugEditorWindow OpenWindow(IInstanceHelper instance, bool focus)
        {
            var ret = GetWindow<RGraphDebugEditorWindow>(WindowName, focus);
            ret.InstanceHelper = instance;
            return ret;
        }

        /// <summary>
        /// ウィンドウのインスタンスを確認する
        /// ラップ関数
        /// </summary>
        /// <returns></returns>
        public static bool HasOpenInstances()
        {
            return HasOpenInstances<RGraphDebugEditorWindow>();
        }

        /// <summary>
        /// Scene上で選択されているかどうか
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static bool IsSceneSelected(RFace face)
        {
            return RnEx.IsEditorSceneSelected(face?.CityObjectGroup);
        }
    }
}