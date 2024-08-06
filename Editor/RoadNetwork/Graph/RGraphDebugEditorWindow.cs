using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Graph
{
    public class RGraphDebugEditorWindow : EditorWindow
    {
        public interface IInstanceHelper
        {
            // グラフ取得
            RGraph GetGraph();

            // グラフ作成
            RGraph CreateGraph();

            long TargetFaceId { get; set; }

            long TargetEdgeId { get; set; }

            long TargetVertexId { get; set; }

            // モデル作成する
            void CreateRnModel();
        }

        private const string WindowName = "PLATEAU RGraph Editor";

        private IInstanceHelper InstanceHelper { get; set; }

        // 作成時にマージも同時に行うかどうか
        public bool mergeOnCreate = true;
        private float mergeEpsilon = 0.2f;
        private int mergeCellLength = 2;
        private bool showOutline = false;

        private int showVertexId = -1;
        private float heightTolerance = 0.1f;

        private class FaceEdit
        {
            public void Update(RFace f)
            {
                if (f == null)
                    return;
            }
        }
        FaceEdit faceEdit = new FaceEdit();

        private class EdgeEdit
        {
            public void Update(REdge e)
            {
                if (e == null)
                    return;

                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField("Edge ID", e.DebugMyId.ToString());
                    EditorGUILayout.LongField("V0", e.V0.GetDebugMyIdOrDefault());
                    EditorGUILayout.LongField("V1", e.V1.GetDebugMyIdOrDefault());
                }
            }
        }
        EdgeEdit edgeEdit = new EdgeEdit();

        private class VertexEdit
        {

            public void Update(RVertex v)
            {
                if (v == null)
                    return;

                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField("Road ID", v.DebugMyId.ToString());
                    EditorGUILayout.Vector3Field("Pos", v.Position);
                    foreach (var e in v.Edges)
                    {
                        EditorGUILayout.LongField("Edge", (long)e.DebugMyId);
                    }
                }

            }
        }
        VertexEdit vertexEdit = new VertexEdit();


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
            EditorGUILayout.Separator();
            EditorGUILayout.IntField("ID", (int)v.DebugMyId);
            foreach (var e in v.GetNeighborVertices())
            {
                EditorGUILayout.IntField("Neighbor", (int)e.DebugMyId);
            }
            EditorGUILayout.Separator();
        }

        /// <Summary>
        /// ウィンドウのパーツを表示します。
        /// </Summary>
        private void OnGUI()
        {
            if (InstanceHelper == null)
                return;

            var graph = InstanceHelper.GetGraph();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Graph"))
                {
                    graph = InstanceHelper.CreateGraph();
                    if (mergeOnCreate)
                    {
                        graph.MergeVertices(mergeEpsilon, mergeCellLength);
                        graph.MergeEdges();
                        graph.SeparateFaces();
                    }
                }
                mergeOnCreate = EditorGUILayout.Toggle("MergeOnCreate", mergeOnCreate);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                mergeEpsilon = EditorGUILayout.FloatField("MergeEpsilon", mergeEpsilon);
                mergeCellLength = EditorGUILayout.IntField("MergeCellLength", mergeCellLength);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Merge"))
                {
                    graph.MergeVertices(mergeEpsilon, mergeCellLength);
                }

                if (GUILayout.Button("Merge Edge"))
                    graph.MergeEdges();

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

            if (GUILayout.Button("Create RnModel"))
                InstanceHelper.CreateRnModel();

            EditorGUILayout.Separator();

            InstanceHelper.TargetFaceId = EditorGUILayout.LongField("Target Face ID", InstanceHelper.TargetFaceId);
            InstanceHelper.TargetEdgeId = EditorGUILayout.LongField("Target Edge ID", InstanceHelper.TargetEdgeId);
            InstanceHelper.TargetVertexId = EditorGUILayout.LongField("Target Vertex ID", InstanceHelper.TargetVertexId);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Face", graph.Faces.Count);
                EditorGUILayout.IntField("Edge", graph.GetAllEdges().Count());
                EditorGUILayout.IntField("Vertex", graph.GetAllVertices().Count());
            }

            showVertexId = EditorGUILayout.IntField("ShowVertexId", showVertexId);


            if (InstanceHelper.TargetFaceId >= 0)
            {
                var face = graph.Faces.FirstOrDefault(f => (long)f.DebugMyId == InstanceHelper.TargetFaceId);
                faceEdit.Update(face);
            }

            if (InstanceHelper.TargetEdgeId >= 0)
            {
                var edge = graph.GetAllEdges().FirstOrDefault(e => (long)e.DebugMyId == InstanceHelper.TargetEdgeId);
                edgeEdit.Update(edge);
            }

            if (InstanceHelper.TargetVertexId >= 0)
            {
                var vertex = graph.GetAllVertices().FirstOrDefault(v => (long)v.DebugMyId == InstanceHelper.TargetVertexId);
                vertexEdit.Update(vertex);
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

    }
}