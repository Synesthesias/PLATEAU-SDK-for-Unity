using PLATEAU.CityGML;
using PLATEAU.CityMeta;

namespace PLATEAU.Editor.CityImport
{
    internal static class SceneMeshPlacerToSceneV2
    {
        public static void Place(CityMetaData metaData)
        {
            string[] gmlRelativePaths = metaData.gmlRelativePaths;
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                string gmlFullPath = metaData.cityImportConfig.sourcePath.UdxRelativeToFullPath(gmlRelativePath);
                // TODO ここに続きを書く
            }
        }
    }
}