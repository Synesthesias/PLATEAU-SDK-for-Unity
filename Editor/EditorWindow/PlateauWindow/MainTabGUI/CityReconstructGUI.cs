using System;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「結合/分離」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityReconstructGUI : IEditorDrawable
    {
        private UnityEditor.EditorWindow parentEditorWindow;
        private GameObject[] Selected = new GameObject[0];
        private Vector2 scrollSelected;
        private int selectedUnit;
        private string[] unitOptions = { "地域単位", "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)" };
        private bool foldOutOption = true;
        private bool toggleMaxSize = true;
        private bool isExecTaskRunning = false;

        public CityReconstructGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;           
        }

        public void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Array.Clear(Selected,0, Selected.Length);
        }

        private void OnSelectionChanged()
        {
            //選択アイテムのフィルタリング処理
            Selected = Selection.gameObjects.Where(x => x.GetComponent<PLATEAUCityObjectGroup>() != null).ToArray<GameObject>();
            parentEditorWindow.Repaint();
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("選択したモデルデータの結合・分離を行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in Selected)
                {
                    EditorGUILayout.LabelField(obj.name);
                }
                EditorGUILayout.EndScrollView();
            }

            PlateauEditorStyle.Heading("結合・分離単位", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
            {
                EditorGUIUtility.labelWidth = 50;
                this.selectedUnit = EditorGUILayout.Popup("単位", this.selectedUnit, unitOptions);
            };

            if(selectedUnit == 0 )
            {
                this.foldOutOption = PlateauEditorStyle.FoldOut(this.foldOutOption, "Option", () =>
                {
                    using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
                    {
                        toggleMaxSize = EditorGUILayout.ToggleLeft("メッシュが最大サイズを超える場合はグリッド分割する", toggleMaxSize);
                    }
                }, 30);
            }

            PlateauEditorStyle.Separator(0);

            using (new EditorGUI.DisabledScope(isExecTaskRunning))
            {
                if (PlateauEditorStyle.MainButton(isExecTaskRunning ? "処理中..." : "実行"))
                {
                    //isExecTaskRunning = true;
                    //TODO: 実行処理

                }
            }
        }
    }
}
