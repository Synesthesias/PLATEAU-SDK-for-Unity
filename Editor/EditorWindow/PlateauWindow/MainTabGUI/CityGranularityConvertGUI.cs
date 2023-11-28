using System;
using System.Threading.Tasks;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「結合/分離」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityGranularityConvertGUI : IEditorDrawable
    {
        private readonly UnityEditor.EditorWindow parentEditorWindow;
        private GameObject[] selected = Array.Empty<GameObject>();
        private Vector2 scrollSelected;
        private int selectedUnit = 2;
        private static readonly string[] UnitOptions = { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" };
        private readonly DestroyOrPreserveSrcGUI destroyOrPreserveGUI = new();
        
        private readonly bool isExecTaskRunning = false;

        public CityGranularityConvertGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;           
        }

        public void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Array.Clear(selected,0, selected.Length);
        }

        private void OnSelectionChanged()
        {
            //選択アイテムのフィルタリング処理
            selected = Selection.gameObjects;
            parentEditorWindow.Repaint();
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("選択したモデルデータの結合・分離を行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in selected)
                {
                    EditorGUILayout.LabelField(obj.name);
                }
                EditorGUILayout.EndScrollView();
            }

            PlateauEditorStyle.Heading("設定", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
            {
                EditorGUIUtility.labelWidth = 50;
                this.selectedUnit =
                    PlateauEditorStyle.PopupWithLabelWidth(
                    "分割・結合単位", this.selectedUnit, UnitOptions, 90);
                destroyOrPreserveGUI.Draw();
            }

            // if(selectedUnit == 0 )
            // {
            //     this.foldOutOption = PlateauEditorStyle.FoldOut(this.foldOutOption, "Option", () =>
            //     {
            //         using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
            //         {
            //             toggleMaxSize = EditorGUILayout.ToggleLeft("メッシュが最大サイズを超える場合はグリッド分割する", toggleMaxSize);
            //         }
            //     }, 30);
            // }

            PlateauEditorStyle.Separator(0);

            using (new EditorGUI.DisabledScope(isExecTaskRunning))
            {
                if (PlateauEditorStyle.MainButton(isExecTaskRunning ? "処理中..." : "実行"))
                {
                    Exec().ContinueWithErrorCatch();
                }
            }
        }

        private async Task Exec()
        {
            Debug.Log("変換開始");
            var converter = new CityGranularityConverter();
            var convertConf = new GranularityConvertOptionUnity(
                new GranularityConvertOption((MeshGranularity)selectedUnit, 1),
                selected,
                destroyOrPreserveGUI.Current == DestroyOrPreserveSrcGUI.PreserveOrDestroy.Destroy
            );
            await converter.ConvertAsync(convertConf);
            selected = new GameObject[] { };
        }
    }
}
