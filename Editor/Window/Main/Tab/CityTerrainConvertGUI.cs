using System;
using System.Threading.Tasks;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.TerrainConvert;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;
using System.Linq;
using PLATEAU.CityInfo;
using System.Collections.Generic;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「地形変換」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityTerrainConvertGUI : ITabContent
    {
        private readonly UnityEditor.EditorWindow parentEditorWindow;
        private GameObject[] selected = Array.Empty<GameObject>();
        private Vector2 scrollSelected;
        private int selectedSize = 4;
        private bool fillEdges = true;
        private static TerrainConvertOption.ImageOutput heightmapImageOutput = TerrainConvertOption.ImageOutput.None;
        private static readonly string[] SizeOptions = { "33 x 33", "65 x 65", "129 x 129", "257 x 257", "513 x 513", "1025 x 1025", "2049 x 2049", "4097 x 4097" };
        private static readonly int[] SizeValues = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };
        private readonly DestroyOrPreserveSrcGui preserveOrDestroyGui;
        private PreserveOrDestroy preserveOrDestroy;
        
        private bool isExecTaskRunning = false;

        public CityTerrainConvertGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            preserveOrDestroyGui = new(OnPreserveOrDestroyChanged);
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
            selected = FilterItemsContainDemChildren(Selection.gameObjects);
            parentEditorWindow.Repaint();
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("選択した地形モデルデータをTerrainに変換します。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in selected)
                {
                    if (obj != null)
                        EditorGUILayout.LabelField(obj.name);
                }
                EditorGUILayout.EndScrollView();
            }

            PlateauEditorStyle.Heading("設定", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
            {
                EditorGUIUtility.labelWidth = 50;
                this.selectedSize =
                    PlateauEditorStyle.PopupWithLabelWidth(
                    "解像度", this.selectedSize, SizeOptions, 90);
                
                preserveOrDestroyGui.Draw();
                fillEdges = EditorGUILayout.ToggleLeft("余白を端の高さに合わせる", fillEdges);
            }

            PlateauEditorStyle.Separator(0);

            using (new EditorGUI.DisabledScope(isExecTaskRunning))
            {
                if (PlateauEditorStyle.MainButton(isExecTaskRunning ? "生成中..." : "実行"))
                {
                    Exec().ContinueWithErrorCatch();
                }
            }
        }

        //選択アイテムのフィルタリング処理 (子にTINReliefを含む）
        private GameObject[] FilterItemsContainDemChildren(GameObject[] selection)
        {
            var selectionList = new HashSet<GameObject>();
            foreach (var gameObj in selection)
            {
                var components = gameObj.transform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var grp in components)
                {
                    if (grp.CityObjects.rootCityObjects.Exists(x => x.type == CityGML.CityObjectType.COT_TINRelief))
                    {
                        selectionList.Add(gameObj); break;
                    }
                }
            }
            return selectionList.ToArray();
        }

        //選択アイテムのフィルタリング処理 (TINReliefのアイテム）
        private GameObject[] FilterDemItems(GameObject[] selection)
        {
            var selectionList = new HashSet<GameObject>();
            foreach (var gameObj in selection)
            {
                var components = gameObj.transform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var grp in components)
                {
                    if (grp.CityObjects.rootCityObjects.Exists(x => x.type == CityGML.CityObjectType.COT_TINRelief))
                        selectionList.Add(grp.gameObject);
                }
            }
            return selectionList.ToArray();
        }

        private async Task Exec()
        {
            isExecTaskRunning = true;
            var converter = new CityTerrainConverter();
            var convertOption = new TerrainConvertOption(
                FilterDemItems(selected),
                (int)SizeValues.GetValue(selectedSize),
                preserveOrDestroy == PreserveOrDestroy.Destroy,
                fillEdges,
                heightmapImageOutput
            );

            await converter.ConvertAsync(convertOption);
            selected = new GameObject[] { };
            isExecTaskRunning = false;
        }

        public void OnTabUnselect()
        {
        }

        private void OnPreserveOrDestroyChanged(PreserveOrDestroy pod)
        {
            preserveOrDestroy = pod;
        }
    }
}
