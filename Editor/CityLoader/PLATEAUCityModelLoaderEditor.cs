using System.Xml.Resolvers;
using PLATEAU.Behaviour;
using PLATEAU.CityLoader;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;

namespace PLATEAU.Editor.CityLoader
{
    [CustomEditor(typeof(PLATEAUCityModelLoader))]
    public class PLATEAUCityModelLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var loader = (PLATEAUCityModelLoader)target;
            EditorGUILayout.LabelField("インポート前パス:");
            PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathBeforeImport);
            EditorGUILayout.LabelField("インポート後パス:");
            PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathAfterImport);
            base.OnInspectorGUI();
        }   
    }
}
