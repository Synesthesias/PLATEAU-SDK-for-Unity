using System;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common.Tile
{
    /// <summary>
    /// シーンに配置されたオブジェクトと動的タイルのどちらかを選択するGUIを提供します。
    /// IMGUIベースで使用する場合に使用　（ElementGroupでは、<see cref="SceneTileChooserElement"/>を使用）
    /// </summary> 
    public enum SceneTileChooserType
    {
        SceneObject,
        DynamicTile
    }

    internal class SceneTileChooserImgui
    {
        private string[] objectSelectOptions = new string[] { "シーンに配置されたオブジェクト", "動的タイル" };
        private int objectSelectedIndex = 0;

        private Action sceneHandler;
        private Action tileHandler;

        private Action<SceneTileChooserType> onSelectionChanged;

        public SceneTileChooserType SelectedType => (SceneTileChooserType)objectSelectedIndex;

        public SceneTileChooserImgui(Action<SceneTileChooserType> onSelectionChanged = null)
        {
            this.onSelectionChanged = onSelectionChanged;
        }

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
                EditorGUI.BeginChangeCheck();
                objectSelectedIndex = EditorGUILayout.Popup(objectSelectedIndex, objectSelectOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    onSelectionChanged?.Invoke(SelectedType);
                }
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
