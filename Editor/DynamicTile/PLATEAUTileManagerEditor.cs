using UnityEditor;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PLATEAUTileManagerのEditor拡張クラス。
    /// </summary>
    [CustomEditor(typeof(PLATEAUTileManager))]
    public class PLATEAUTileManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // 通常のInspector表示

            PLATEAUTileManager tileManager = (PLATEAUTileManager)target;

            if (PLATEAUTileManager.showDebugTileInfo)
            {
                if (GUILayout.Button("Clear All"))
                {
                    tileManager.ClearAll();
                    SceneViewCameraTracker.Initialize();
                }

                if (GUILayout.Button("Show Debug Extent"))
                {
                    tileManager.ShowBounds();
                }
            }
        }
    }
}
