using PLATEAU.CityGML;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.Util;
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
        private IMAConfig materialAdjustConf;
        private IMAMaterialSelector materialSelector;
        
        public MAMaterialChanger(IMAConfig materialAdjustConf, IMAMaterialSelector materialSelector)
        {
            this.materialAdjustConf = materialAdjustConf;
            this.materialSelector = materialSelector;
        }
        
        /// <summary>
        /// 再帰的にマテリアルを変更します
        /// </summary>
        public void ExecRecursive(UniqueParentTransformList targetTrans)
        {
            targetTrans.BfsExec(trans =>
            {
                Exec(trans);
                return NextSearchFlow.Continue;
            });
        }

        /// <summary>
        /// 非再帰的にマテリアルを変更します
        /// </summary>
        public void Exec(Transform trans)
        {
            var cityObjGroups = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroups == null || cityObjGroups.CityObjects.rootCityObjects.Count == 0) return;
            // 最小地物単位にしたので、rootCityObjectsの数は1つのはずです。
            var cityObj = cityObjGroups.CityObjects.rootCityObjects[0];
            var materialResult = materialSelector.Get(cityObj, materialAdjustConf);
            if (!materialResult.IsSucceed) return;
                
            var renderer = trans.GetComponent<Renderer>();
            if (renderer == null) return;
                
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialResult.Get;
            }
            renderer.sharedMaterials = materials;
        }
        
    }
}