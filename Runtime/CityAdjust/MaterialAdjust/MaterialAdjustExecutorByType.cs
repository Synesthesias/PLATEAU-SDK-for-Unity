using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// SDKのモデル調整のマテリアル分けに関する機能で、地物型によるマテリアル分けを担当します。
    /// 属性情報によるマテリアル分けは<see cref="MaterialAdjustExecutorByAttr"/>を参照してください。
    /// </summary>
    internal class MaterialAdjustExecutorByType : IMaterialAdjustExecutor
    {
        

        public async Task Exec(AdjustExecutorConf conf)
        {
            
            if (conf.TargetObjs.Any(obj => obj == null))
            {
                Dialogue.Display("対象に削除されたゲームオブジェクトが含まれています。\n選択し直してください。", "OK");
                return;
            }
            
            // 地物タイプに応じてマテリアルを変える下準備として、都市オブジェクトを最小地物単位に分解します。
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
                var matConf = (MaterialAdjustConf<CityObjectTypeHierarchy.Node>)(conf.MaterialAdjustConf);
                var matChangeConf = matConf.GetConfFor(CityObjectTypeHierarchy.GetNodeByType(cityObj.type));
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