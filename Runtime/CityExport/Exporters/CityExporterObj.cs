using System.IO;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityExport.Exporters
{
    /// <summary>
    /// <see cref="Model"/>をOBJ形式のファイルにします。
    /// </summary>
    public class CityExporterObj : ICityExporter
    {
        public void Export(string destDir, string fileNameWithoutExtension, Model model)
        {
            string filePathWithoutExtension = Path.Combine(destDir, fileNameWithoutExtension).Replace('\\', '/');
            using var objWriter = new ObjWriter();
            objWriter.Write(filePathWithoutExtension + ".obj", model);
        }
    }
}