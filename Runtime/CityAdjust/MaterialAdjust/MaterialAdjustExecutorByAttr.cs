using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    internal class MaterialAdjustExecutorByAttr : IMaterialAdjustExecutor
    {

        public async Task Exec(AdjustExecutorConf confBase)
        {
            var conf = (AdjustExecutorConfByAttr)confBase;
            if (!conf.Validate()) return;
            
            // TODO 以下は、試験的にMaterialAdjustExecutorByType.csからコピーしたものです。あとでもっと良くする。
            
            var granularityConverter = new CityGranularityConverter();
            var granularityConvertConf = new GranularityConvertOptionUnity(
                new GranularityConvertOption(MeshGranularity.PerAtomicFeatureObject, 1),
                conf.TargetObjs.ToArray(), conf.DoDestroySrcObjs
            );
            var result = await granularityConverter.ConvertAsync(granularityConvertConf);
            if (!result.IsSucceed)
            {
                Debug.LogError("ゲームオブジェクトの分解に失敗しました。");
                return;
            }
            
            // マテリアルを変更します。
            foreach (var obj in result.GeneratedObjs)
            {
                var cityObjGroups = obj.GetComponent<PLATEAUCityObjectGroup>();
                if (cityObjGroups == null || cityObjGroups.CityObjects.rootCityObjects.Count == 0) continue;
                // 最小地物単位にしたので、rootCityObjectsの数は1つのはずです。
                var cityObj = cityObjGroups.CityObjects.rootCityObjects[0];
                var matConf = (MaterialAdjustConf<string>)(conf.MaterialAdjustConf);
                
                // ここから違う
                if (!cityObj.AttributesMap.TryGetValueWithSlash(conf.AttrKey, out var attrValue)) continue;
                string attr = attrValue.StringValue;
                if (string.IsNullOrEmpty(attr)) return;
                var matChangeConf = matConf.GetConfFor(attr);
                
                // ここから同じ
                if (matChangeConf == null || !matChangeConf.ShouldChangeMaterial) continue;
                var renderer = obj.GetComponent<Renderer>();
                if (renderer == null) continue;
                var materialConf = matChangeConf.Material;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = materialConf;
                }
                renderer.sharedMaterials = materials;
            }
            
            // 結合し直します。
            var granularityConverterAfter = new CityGranularityConverter();
            var granularityConvertConfAfter = new GranularityConvertOptionUnity(
                new GranularityConvertOption(conf.MeshGranularity, 1),
                result.GeneratedRootObjs.ToArray(), true
            );
            var resultAfter = await granularityConverterAfter.ConvertAsync(granularityConvertConfAfter);
            if (!resultAfter.IsSucceed)
            {
                Debug.LogError("ゲームオブジェクトの結合に失敗しました。");
                return;
            }
            
        }
    }
}