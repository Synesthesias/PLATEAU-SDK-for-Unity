using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using CityObjectList = PLATEAU.PolygonMesh.CityObjectList;

namespace PLATEAU.GranularityConvert
{
    public class CityGranularityConverter
    {
        public async Task ConvertAsync(IReadOnlyList<GameObject> srcGameObjs, GranularityConvertOption option)
        {
            // 属性情報を覚えておきます。
            var cityObjectGroups = srcGameObjs
                .Select(go => go.GetComponent<PLATEAUCityObjectGroup>())
                .Where(cityObj => cityObj != null);
            var attributes = GmlIdToAttributes.ComposeFrom(cityObjectGroups);
            
            // ゲームオブジェクトを共通ライブラリのModelに変換します。
            using var srcModel = UnityMeshToDllModelConverter.Convert(srcGameObjs, true, false, ConvertVertex);
            
            // 共通ライブラリの機能でモデルを分割・結合します。
            var converter = new GranularityConverter();
            using var dstModel = converter.Convert(srcModel, option);
            
            // Modelをゲームオブジェクトに変換して配置します。
            var generatedObjs = new List<GameObject>();
            bool result = await PlateauToUnityModelConverter.PlateauModelToScene(generatedObjs,
                null, new DummyProgressDisplay(), "", true,
                null, null, dstModel, new DummyAttributeDataHelper(), true);
            if (!result)
            {
                throw new Exception("Failed to covert plateau model to scene game objects.");
            }
            
            // 覚えておいた属性情報を再構成します。
            // foreach (var gameObj in generatedObjs)
            // {
            //     var component = gameObj.AddComponent<PLATEAUCityObjectGroup>();
            //     var rootCityObjs = component.CityObjects.rootCityObjects;
            //     
            //     
            //     component.SetSerializableCityObject();
            // }
        }
        
        private static PlateauVector3d ConvertVertex(Vector3 src)
        {
            return new PlateauVector3d(src.x, src.y, src.z);
        } 
    }
}