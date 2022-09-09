using PLATEAU.CityLoader;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Editor.PlateauWindow.Import.AreaSelect;
using UnityEditor;

namespace PLATEAU.Editor.CityLoader
{
    [CustomEditor(typeof(PLATEAUCityModelLoader))]
    public class PLATEAUCityModelLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var loader = (PLATEAUCityModelLoader)target;
            if (PlateauEditorStyle.MainButton("範囲選択"))
            {
                var areaSelector = new AreaSelectorStarter();
                areaSelector.Start();
            }
            EditorGUILayout.LabelField("インポート前パス:");
            PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathBeforeImport);
            EditorGUILayout.LabelField("インポート後パス:");
            PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathAfterImport);
            
        }   
    }
}
