using PLATEAU.CityGML;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using UnityEngine;
using Material = UnityEngine.Material;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// マテリアル分け機能の3手順、分解 → マテリアル変更 → 結合のうち、マテリアル変更です。
    /// MAはMaterialAdjustの略とします。
    /// </summary>
    internal class MAMaterialChanger
    {
        private IMaterialAdjustConf materialAdjustConf;
        private IMAMaterialSelector materialSelector;
        
        public MAMaterialChanger(IMaterialAdjustConf materialAdjustConf, IMAMaterialSelector materialSelector)
        {
            this.materialAdjustConf = materialAdjustConf;
            this.materialSelector = materialSelector;
        }
        
        public void Exec(GranularityConvertResult decomposeReturned)
        {
            foreach (var obj in decomposeReturned.GeneratedObjs)
            {
                var cityObjGroups = obj.GetComponent<PLATEAUCityObjectGroup>();
                if (cityObjGroups == null || cityObjGroups.CityObjects.rootCityObjects.Count == 0) continue;
                // 最小地物単位にしたので、rootCityObjectsの数は1つのはずです。
                var cityObj = cityObjGroups.CityObjects.rootCityObjects[0];
                var materialResult = materialSelector.Get(cityObj, materialAdjustConf);
                if (!materialResult.IsSucceed) continue;
                
                var renderer = obj.GetComponent<Renderer>();
                if (renderer == null) continue;
                
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = materialResult.Get;
                }
                renderer.sharedMaterials = materials;
            }
        }
        
    }
}