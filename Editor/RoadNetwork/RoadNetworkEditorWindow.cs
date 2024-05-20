using PLATEAU.Editor.RoadNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU
{
    /// <summary>
    /// 道路ネットワーク手動編集機能を提供するエディタウィンドウ
    /// 実際の処理を行うRoadNetworkEditorクラスのインスタンス化、インスタンスの管理に専念する
    /// </summary>
    public class RoadNetworkEditorWindow : UnityEditor.EditorWindow
    {
        public IRoadNetworkEditingSystemInterface EditorInterface { get; private set; }

        private static readonly string WindowName = "PLATEAU RoadNetwork Editor";

        /// <summary>
        /// ウィンドウのインスタンスを確認する
        /// ラップ関数
        /// </summary>
        /// <returns></returns>
        public static bool HasOpenInstances()
        {
            return HasOpenInstances<RoadNetworkEditorWindow>();
        }

        /// <summary>
        /// エディタを取得する
        /// </summary>
        /// <returns></returns>
        public static IRoadNetworkEditingSystemInterface GetEditorInterface()
        {
            return GetWindow(false).EditorInterface;
        }

        [MenuItem("PLATEAU_Dev/PLATEAU RoadNetwork Editor")]
        public static void ShowWindow()
        {
            GetWindow(true);
        }
        private void OnEnable()
        {
            // 初期化
            if (EditorInterface == null)
            {
                EditorInterface = new RoadNetworkEditingSystem(rootVisualElement);
            }
        }

        /// <summary>
        /// ウィンドウを取得する、存在しない場合に生成する
        /// ラップ関数
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        private static RoadNetworkEditorWindow GetWindow(bool focus)
        {
            return GetWindow<RoadNetworkEditorWindow>(WindowName, focus);
        }

    }
}
