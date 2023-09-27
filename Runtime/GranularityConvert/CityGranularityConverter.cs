using System;
using System.Collections.Generic;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    public class CityGranularityConverter
    {
        public async void ConvertAsync(IReadOnlyList<GameObject> srcGameObjs, GranularityConvertOption option)
        {
            using var srcModel = UnityMeshToDllModelConverter.Convert(srcGameObjs, true, true, ConvertVertex);
            var converter = new GranularityConverter();
            using var dstModel = converter.Convert(srcModel, option);
            bool result = await PlateauToUnityModelConverter.PlateauModelToScene(
                null, new DummyProgressDisplay(), "", true, null, null, dstModel, new DummyAttributeDataHelper());
            if (!result)
            {
                throw new Exception("Failed to covert plateau model to scene game objects.");
            }
        }
        
        private static PlateauVector3d ConvertVertex(Vector3 src)
        {
            return new PlateauVector3d(src.x, src.y, src.z);
        } 
    }
}