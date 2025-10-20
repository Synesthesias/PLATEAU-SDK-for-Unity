using System;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// シーンに配置されたオブジェクトと動的タイルのどちらかを選択するGUIを提供します。
    /// </summary>
    public class SceneTileChooserGui
    {
        public enum ChooserType
        {
            SceneObject,
            DynamicTile
        }

        private string[] objectSelectOptions = new string[] { "シーンに配置されたオブジェクト", "動的タイル" };
        private int objectSelectedIndex = 0;

        private Action sceneHandler;
        private Action tileHandler;

        public ChooserType SelectedType => (ChooserType)objectSelectedIndex;

        public void DrawAndInvoke(Action scene, Action tile)
        {
            AddHandlers(scene, tile);
            Draw();
            InvokeHandler();
        }

        public void Draw()
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("調整対象の種類", GUILayout.Width(100));
                objectSelectedIndex = EditorGUILayout.Popup(objectSelectedIndex, objectSelectOptions);
            }
        }

        public void AddHandlers(Action scene, Action tile)
        {
            sceneHandler = scene;
            tileHandler = tile;
        }

        public void InvokeHandler()
        {
            if (objectSelectedIndex == 0)
            {
                sceneHandler?.Invoke();
            }
            else if (objectSelectedIndex == 1)
            {
                tileHandler?.Invoke();
            }
        }

    }
}
