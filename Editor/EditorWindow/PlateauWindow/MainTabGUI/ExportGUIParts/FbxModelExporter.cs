using System.IO;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// Model(中間形式)をFBXファイルに出力します。
    /// </summary>
    internal class FbxModelExporter : IPlateauModelExporter
    {
        public FbxFileFormat FbxFileFormat { get; set; } = FbxFileFormat.Binary;
        
        public void DrawConfigGUI()
        {
            FbxFileFormat = (FbxFileFormat)EditorGUILayout.EnumPopup("FBXフォーマット", FbxFileFormat);
        }

        public void Export(string destDir, string fileNameWithoutExtension, Model model)
        {
            string destPath = Path.Combine(destDir, fileNameWithoutExtension + ".fbx");
            FbxWriter.Write(destPath, model, new FbxWriteOptions(FbxFileFormat));
        }
    }
}
