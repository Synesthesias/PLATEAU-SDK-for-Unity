#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos
{
    /// <summary>
    /// シーンビューの設定でギズモ表示がオフになっているとき、
    /// オンにしてくださいというメッセージを表示するためのクラスです。
    /// </summary>
    public class GizmoActiveChecker
    {
        public void CheckAndShow(SceneView sceneView)
        {
            if (!IsGizmoActive())
            {
                ShowMessage(sceneView);
            }
        }

        private bool IsGizmoActive()
        {
            SceneView sv =
                EditorWindow.GetWindow<SceneView>(null, false);
            return sv.drawGizmos;
        }

        private void ShowMessage(SceneView sceneView)
        {
            Handles.BeginGUI();
            // Sceneビューの中心にメッセージボックスを配置
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.red, background = Texture2D.whiteTexture },
                fontSize = 19
            };
            var centeredRect = new Rect((sceneView.position.width - 600) / 2, (sceneView.position.height - 120) / 2, 600,
                120);
            GUI.Box(centeredRect,
                "注意：\nギズモ表示がオフです。\n範囲を表示するために、シーンビューのギズモ表示切替ボタンを\nクリックしてオンにしてください。",
                style);
            Handles.EndGUI();
        }
    }
}
#endif