using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    internal static class SceneMeshPlacerToSceneV2
    {
        public static void Place(ScenePlacementConfig placeConfig, CityMetaData metaData)
        {
            string[] gmlRelativePaths = metaData.gmlRelativePaths;
            // gmlファイルごとのループ
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                string gmlFullPath = metaData.cityImportConfig.sourcePath.UdxRelativeToFullPath(gmlRelativePath);
                // tessellate を false にすることで、3Dモデルができない代わりにパースが高速になります。
                var gmlParserParams = new CitygmlParserParams(true, false);
                var cityModel = CityGml.Load(gmlFullPath, gmlParserParams, DllLogCallback.UnityLogCallbacks, DllLogLevel.Error);
                var primaryCityObjs = cityModel.GetCityObjectsByType(PrimaryCityObjectTypes.PrimaryTypeMask);
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
                int targetLod = metaData.cityImportConfig.scenePlacementConfig.GetPerTypeConfig(gmlType).selectedLod;
                // 主要地物ごとのループ
                foreach (var primaryCityObj in primaryCityObjs)
                {
                    // 対応する3Dモデルを探します。
                    // var foundObj = SearchObj(objin)
                    
                }
            }
        }

        public static ObjInfo SearchObj(List<ObjInfo> objInfos, int lod, string gmlName)
        {
            var foundObj = objInfos
                .Where(info => info.lod == lod)
                .First(info => Path.GetFileName(info.assetsPath).Contains(gmlName));
            return foundObj;
        }
        // public static Mesh SearchMesh(List<ObjInfo> objInfos, int lod, string gmlName, string cityObjId)
        // {
        //     
        // }
    }
}