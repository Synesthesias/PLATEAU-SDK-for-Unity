using System.Collections.Generic;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    public class CityGranularityConverter
    {
        void Convert(IReadOnlyList<GameObject> srcGameObjs, GranularityConvertOption option)
        {
            using var srcModel = UnityMeshToDllModelConverter.Convert(srcGameObjs, true, true, ConvertVertex);
            var converter = new GranularityConverter();
            using var dstModel = converter.Convert(srcModel, option);
            PlateauToUnityModelConverter.CityModelToScene(dstModel, )
        }
        
        private static PlateauVector3d ConvertVertex(Vector3 src)
        {
            return new PlateauVector3d(src.x, src.y, src.z);
        } 
    }
}