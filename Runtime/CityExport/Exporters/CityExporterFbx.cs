using System.IO;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityExport.Exporters
{
    public class CityExporterFbx : ICityExporter
    {
        public FbxFileFormat FbxFileFormat { get; set; } = FbxFileFormat.Binary;
        
        public void Export(string destDir, string fileNameWithoutExtension, Model model)
        {
            string destPath = Path.Combine(destDir, fileNameWithoutExtension + ".fbx");
            FbxWriter.Write(destPath, model, new FbxWriteOptions(FbxFileFormat));
        }
    }
}