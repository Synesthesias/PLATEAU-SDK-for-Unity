using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    internal class ObjectSelectGui : Element
    {
        public UniqueParentTransformList SelectedTransforms { get; private set; } = new UniqueParentTransformList();
        private readonly EditorWindow parentEditorWindow;
        private readonly CityMaterialAdjustGUI materialAdjustGUI;
        private readonly ScrollView scrollView = new (GUILayout.MaxHeight(100));

        public ObjectSelectGui(CityMaterialAdjustGUI materialAdjustGUI, EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;
            this.materialAdjustGUI = materialAdjustGUI;
            OnSelectionChanged();
        }

        public override void DrawContent()
        {
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollView.Draw(() =>
                {
                    foreach (Transform trans in SelectedTransforms.Get)
                    {
                        EditorGUILayout.LabelField(trans == null ? "(削除されたゲームオブジェクト)" : trans.name);
                    }
                });

            }
        }
        
        private void OnSelectionChanged()
        {
            if (materialAdjustGUI?.CurrentSearcher?.IsSearched is true) return; // 「検索」ボタンを押したら対象は変更できないようにします。
            UpdateSelection();
            
        }

        public void UpdateSelection()
        {
            SelectedTransforms = new UniqueParentTransformList(Selection.transforms);
            parentEditorWindow.Repaint();
        }
        
        public override void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            SelectedTransforms.Reset();
        }
        
    }
}