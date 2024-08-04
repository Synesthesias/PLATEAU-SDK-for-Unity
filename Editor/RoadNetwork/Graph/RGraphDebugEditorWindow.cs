using PLATEAU.RoadNetwork.Graph;
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

        /// <Summary>
        /// ウィンドウのパーツを表示します。
        /// </Summary>
        private void OnGUI()
        {
            if (Graph == null)
                return;
            EditorGUILayout.BeginVertical("Info");
            EditorGUILayout.IntField("Polygon", Graph.Polygons.Count);
            EditorGUILayout.IntField("Edge", Graph.GetAllEdge().Count());
            EditorGUILayout.IntField("Vertex", Graph.GetAllVertex().Count());
            mergeEpsilon = EditorGUILayout.FloatField("MergeEpsilon", mergeEpsilon);
            mergeCellLength = EditorGUILayout.IntField("MergeCellLength", mergeCellLength);
            if (GUILayout.Button("Merge"))
            {
                Graph.MergeVertices(mergeEpsilon, mergeCellLength);
            }

            EditorGUILayout.EndVertical();
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