using UnityEditor;

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

        // 2024年7月のリリース準備のため、開発中の機能を一時的にメニューから非表示にしています。
        // リリースが終わったら下のコメント行を復活させてください。
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
                if (rootVisualElement != null)
                {
                    EditorInterface = new RoadNetworkEditingSystem(new EditorInstance(this), rootVisualElement);
                }
            }
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnGUI()
        {

            //if (GUILayout.Button(Vector3.up * 100, Quaternion.identity, 600, 600, Handles.SphereHandleCap))
            //{
            //    Debug.Log("Button Clicked");
            //}
            //Gizmos.DrawLine(Vector3.zero, Vector3.up * 500);  // OnDrawGizmodsでしか呼ばれない
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

        private class EditorInstance : RoadNetworkEditingSystem.ISystemInstance
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
