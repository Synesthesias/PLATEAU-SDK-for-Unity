using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using System.Linq;
using UnityEngine;
using CityObjectList = PLATEAU.CityInfo.CityObjectList;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// マテリアル分けの設定値とCityObjectを引数として、
    /// そのCityObjectにふさわしいマテリアルを選択します。
    /// マテリアル分けの基準が地物型か属性情報かによる差を吸収する目的のインターフェイスです。
    /// </summary>
    public interface IMAMaterialSelector
    {
        public Result<Material> Get(PLATEAUCityObjectGroup cog, CityObjectList.CityObject cityObj, IMAConfig materialAdjustConf);
    }
        
    /// <summary>
    /// <see cref="IMAMaterialSelector"/>であり、マテリアル分けの基準が地物型である場合の実装です。
    /// </summary>
    internal class MAMaterialSelectorByType : IMAMaterialSelector
    {
        public Result<Material> Get(PLATEAUCityObjectGroup cog, CityObjectList.CityObject cityObj, IMAConfig materialAdjustConf)
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
            
        public Result<Material> Get(PLATEAUCityObjectGroup cog, CityObjectList.CityObject cityObj, IMAConfig materialAdjustConf)
        {
            var matConf = (MAMaterialConfig<string>)materialAdjustConf;
            
            // cityObj自身で、属性情報に対応するマテリアルがあればそれを返します。
            var selfResult = GetMaterialFor(cityObj, matConf);
            if (selfResult.IsSucceed)
            {
                return selfResult;
            }

            
            // cityObjが最小地物の場合、自身に対応するマテリアルがなくとも、親の主要地物にあるかもしれないので探します。
            if (cog.Granularity != MeshGranularity.PerAtomicFeatureObject) return Result<Material>.Fail;
            var parentTran = cog.transform.parent;
            if (parentTran == null) return Result<Material>.Fail;
            var outsideParentId = cog.CityObjects.outsideParent;
            var parentCog = parentTran.GetComponent<PLATEAUCityObjectGroup>();
            if (parentCog == null) return Result<Material>.Fail;
            var parentCo = parentCog.PrimaryCityObjects.FirstOrDefault(co => co.GmlID == outsideParentId);
            if (parentCo == null) return Result<Material>.Fail;
            return GetMaterialFor(parentCo, matConf);
        }
        

        /// <summary>
        /// マテリアル分けで、<paramref name="cityObj"/>に対応するマテリアルを返します。
        /// </summary>
        private Result<Material> GetMaterialFor(CityObjectList.CityObject cityObj, MAMaterialConfig<string> matConf)
        {
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