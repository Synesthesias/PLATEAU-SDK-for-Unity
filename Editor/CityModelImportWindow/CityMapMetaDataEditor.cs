using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Runtime.CityMapMeta;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{
    /// <summary>
    /// <see cref="CityMapMetaData"/> のインスペクタでの表示を行います。
    /// </summary>
    [CustomEditor(typeof(CityMapMetaData))]
    public class CityMapMetaDataEditor : UnityEditor.Editor
    {
        private bool foldOutIdGmlTable;
        private bool foldOutReconvert;
        private CityModelImportConfigGUI importConfigGUI = new CityModelImportConfigGUI();
        public override void OnInspectorGUI()
        {
            HeaderDrawer.Reset();
            var metaData = target as CityMapMetaData;
            if (metaData == null)
            {
                EditorGUILayout.HelpBox($"{nameof(metaData)} が null です。", MessageType.Error);
                return;
            }
            
            HeaderDrawer.Draw("IDとGMLファイルの情報");
            this.foldOutIdGmlTable = EditorGUILayout.Foldout(this.foldOutIdGmlTable, "IDとGMLファイルの紐付け");
            if (this.foldOutIdGmlTable)
            {
                foreach (var pair in metaData.idToGmlTable)
                {
                    var str = $"{pair.Key}\n=> {pair.Value}";
                    EditorGUILayout.TextArea(str);
                }
            }

            EditorGUILayout.Space(10);
            
            HeaderDrawer.Draw("再変換画面");
            HeaderDrawer.IncrementDepth();

            this.foldOutReconvert = EditorGUILayout.Foldout(this.foldOutReconvert, "再変換");
            if (this.foldOutReconvert)
            {
                this.importConfigGUI.Draw(metaData.cityModelImportConfig);
                
            }
            
            // base.OnInspectorGUI();
        }
    }
}