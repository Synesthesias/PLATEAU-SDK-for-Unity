using PLATEAU.Editor.RoadNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
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

        public void Reinitialize()
        {
            EditorInterface = null;
            // 初期化
            Initialize();
        }

        private void Initialize()
        {
            // 初期化
            if (EditorInterface == null)
            {
                EditorInterface = new RoadNetworkEditingSystem(new EditorInstance(this), rootVisualElement);
            }
        }

        private void OnEnable()
        {
            Initialize();
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

        private class EditorInstance : RoadNetworkEditingSystem.IEditorInstance
        {
            public EditorInstance(RoadNetworkEditorWindow window)
            {
                this.window = window;
            }
            RoadNetworkEditorWindow window;

            public void RequestReinitialize()
            {
                window.Reinitialize();
            }
        }

    }
}
