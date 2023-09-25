using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityExport;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「結合/分離」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityCombineSeparateGUI : IEditorDrawable
    {
        private Vector2 scrollSelected;
        private int selectedUnit;
        private string[] unitOptions = { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" };
        private bool foldOutOption = true;
        private bool toggleMaxSize;
        private bool isExecTaskRunning = false;

        public CityCombineSeparateGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            Selection.selectionChanged += () => { 
                parentEditorWindow.Repaint();
            };           
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("選択したモデルデータの結合・分離を行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in Selection.gameObjects)
                {
                    if (obj.GetComponent<PLATEAUCityObjectGroup>() != null)
                    {
                        EditorGUILayout.LabelField(obj.name);
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.Heading("結合・分離単位", null);
                this.selectedUnit = EditorGUILayout.Popup("単位",this.selectedUnit, unitOptions);

                this.foldOutOption = PlateauEditorStyle.FoldOut(this.foldOutOption, "Option", () =>
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        toggleMaxSize = EditorGUILayout.ToggleLeft("メッシュが最大サイズを超える場合はグリッド分割する", toggleMaxSize);
                    }
                }, 30);

                PlateauEditorStyle.Separator(0);

                using (new EditorGUI.DisabledScope(isExecTaskRunning))
                {
                    if (PlateauEditorStyle.MainButton(isExecTaskRunning ? "処理中..." : "実行"))
                    {
                        //isExecTaskRunning = true;
                        //実行処理
                    }
                }
            }

        }
    }
}
