using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Graph
{
    public class RGraphDebugEditorWindow : UnityEditor.EditorWindow
    {
        private static readonly string WindowName = "PLATEAU RGraph Editor";

        private RGraph Graph { get; set; }

        private float mergeEpsilon = 0.2f;
        private int mergeCellLength = 2;
        private bool showOutline = false;

        private int showVertexId = -1;

        public void Reinitialize()
        {
        }

        private void Initialize()
        {
        }

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
            if (Graph == null)
                return;
            EditorGUILayout.IntField("Face", Graph.Faces.Count);
            //EditorGUILayout.IntField("Edge", Graph.GetAllEdge().Count());
            //EditorGUILayout.IntField("Vertex", Graph.GetAllVertex().Count());
            mergeEpsilon = EditorGUILayout.FloatField("MergeEpsilon", mergeEpsilon);
            mergeCellLength = EditorGUILayout.IntField("MergeCellLength", mergeCellLength);
            showVertexId = EditorGUILayout.IntField("ShowVertexId", showVertexId);
            if (Selection.activeGameObject)
            {
                var selected = Selection.activeGameObject.GetComponent<PLATEAUCityObjectGroup>();
                if (selected)
                {
                }
            }
            if (GUILayout.Button("Merge"))
            {
                Graph.MergeVertices(mergeEpsilon, mergeCellLength);
            }

        }
        /// <summary>
        /// ウィンドウを取得する、存在しない場合に生成する
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static RGraphDebugEditorWindow OpenWindow(RGraph rGraph, bool focus)
        {
            var ret = GetWindow<RGraphDebugEditorWindow>(WindowName, focus);
            ret.Graph = rGraph;
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