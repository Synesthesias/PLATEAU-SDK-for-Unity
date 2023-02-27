using PLATEAU.MeshWriter;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// 都市エクスポート設定のうち、ファイル形式にFBXを選択したときに固有のインポート設定です。
    /// </summary>
    internal class FbxExportConfig : PlateauModelExporter
    {
        public FbxFileFormat FbxFileFormat { get; set; } = FbxFileFormat.Binary;
        
        public void DrawConfigGUI()
        {
            FbxFileFormat = (FbxFileFormat)EditorGUILayout.EnumPopup("FBXフォーマット", FbxFileFormat);
        }

        
    }
}
