using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// SDKのモデル調整のマテリアル分けに関する機能を担います。
    /// </summary>
    internal class CityMaterialAdjuster
    {
        private readonly IReadOnlyCollection<GameObject> targetObjs;
        public MaterialAdjustConf MaterialAdjustConf { get; }

        public CityMaterialAdjuster(IReadOnlyCollection<GameObject> targetObjs)
        {
            this.targetObjs = targetObjs;
            var targetTransforms = targetObjs.Select(obj => obj.transform).ToArray();
            var foundTypes = new CityTypeSearcher().Search(targetTransforms);
            MaterialAdjustConf = new MaterialAdjustConf(foundTypes);
        }

        public async Task Exec()
        {
            // 地物タイプに応じてマテリアルを変える下準備として、都市オブジェクトを最小地物単位に分解します。
            var granularityConverter = new CityGranularityConverter();
            var granularityConf = new GranularityConvertOption(MeshGranularity.PerAtomicFeatureObject, 1);
            var result = await granularityConverter.ConvertAsync(targetObjs.ToList(), granularityConf);
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
                var typeConf = MaterialAdjustConf.GetConfFor(cityObj.type);
                if (typeConf == null || !typeConf.ShouldChangeMaterial) continue;
                var renderer = obj.GetComponent<Renderer>();
                if (renderer == null) continue;
                var materialConf = typeConf.Material;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = materialConf;
                }
                renderer.sharedMaterials = materials;
            }
        }
    }

    
}