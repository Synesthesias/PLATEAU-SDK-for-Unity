using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// マテリアル分けの設定値とCityObjectを引数として、
    /// そのCityObjectにふさわしいマテリアルを選択します。
    /// 設定のキーの型による差を吸収する目的のインターフェイスです。
    /// </summary>
    public interface IMAMaterialSelector
    {
        public Result<Material> Get(CityObjectList.CityObject cityObj, IMAConfig materialAdjustConf);
    }
        
    /// <summary>
    /// <see cref="IMAMaterialSelector"/>であり、マテリアル分けの基準が地物型である場合の実装です。
    /// </summary>
    internal class MAMaterialSelectorByType : IMAMaterialSelector
    {
        public Result<Material> Get(CityObjectList.CityObject cityObj, IMAConfig materialAdjustConf)
        {
            var matChangeConf = ((MAMaterialConfig<CityObjectTypeHierarchy.Node>)materialAdjustConf).GetConfFor(CityObjectTypeHierarchy.GetNodeByType(cityObj.type));
            if (matChangeConf == null || !matChangeConf.ShouldChangeMaterial) return new Result<Material>(false, null);
            var material = matChangeConf.Material;
            return new Result<Material>(true, material);
        }
    }

    /// <summary>
    /// <see cref="IMAMaterialSelector"/>であり、マテリアル分けの基準が属性情報である場合の実装です。
    /// </summary>
    internal class MAMaterialSelectorByAttr : IMAMaterialSelector
    {
        private string attrKey;
            
        public MAMaterialSelectorByAttr(string attrKey)
        {
            this.attrKey = attrKey;
        }
            
        public Result<Material> Get(CityObjectList.CityObject cityObj, IMAConfig materialAdjustConf)
        {
            var matConf = (MAMaterialConfig<string>)materialAdjustConf;
            if (!cityObj.AttributesMap.TryGetValueWithSlash(attrKey, out var attrValue))
            {
                return new Result<Material>(false, null);
            }
            string attr = attrValue.StringValue;

            var matChangeConf = matConf.GetConfFor(attr);
            if (matChangeConf == null || !matChangeConf.ShouldChangeMaterial || string.IsNullOrEmpty(attr))
            {
                return new Result<Material>(false, null);
            }

            return new Result<Material>(true, matChangeConf.Material);
        }
    }
}