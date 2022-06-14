using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{
    [CustomEditor(typeof(CityMapMetaData))]
    public class CityMapMetaDataEditor : UnityEditor.Editor
    {
        private bool foldOut = false;
        private CityModelImportConfigGUI importConfigGUI = new CityModelImportConfigGUI();
        public override void OnInspectorGUI()
        {
            var metaData = target as CityMapMetaData;
            if (metaData == null)
            {
                EditorGUILayout.HelpBox($"{nameof(metaData)} が null です。", MessageType.Error);
                return;
            }
            
            this.foldOut = EditorGUILayout.Foldout(this.foldOut, "IDとGMLファイルの紐付け");
            if (this.foldOut)
            {
                foreach (var pair in metaData.idToGmlTable)
                {
                    var str = $"{pair.Key}\n=> {pair.Value}";
                    EditorGUILayout.TextArea(str);
                }
            }

            EditorGUILayout.Space(10);
            var importConfig = this.importConfigGUI.Draw();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("変換元");
            EditorGUILayout.TextArea(metaData.importSourcePath);
            if (PlateauEditorStyle.MainButton("再変換"))
            {
                OnConvertButtonPushed(importConfig, metaData.importSourcePath);
            }
        }

        private void OnConvertButtonPushed(CityModelImportConfig importConfig, string sourceUdxFolderPath)
        {
            var window = CityModelImportWindow.OpenWithConfig(importConfig, sourceUdxFolderPath);
        }
    }
}