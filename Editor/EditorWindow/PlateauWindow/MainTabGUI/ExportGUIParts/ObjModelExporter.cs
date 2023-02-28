using System.IO;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// Model(中間形式)をOBJファイルにエクスポートします。
    /// </summary>
    internal class ObjModelExporter : IPlateauModelExporter
    {
        public void DrawConfigGUI()
        {
            // OBJファイルに固有の設定項目はありません。
        }

        public void Export(string destDir, string fileNameWithoutExtension, Model model)
        {
            string filePathWithoutExtension = Path.Combine(destDir, fileNameWithoutExtension).Replace('\\', '/');
            using var objWriter = new ObjWriter();
            objWriter.Write(filePathWithoutExtension + ".obj", model);
        }
    }
}
