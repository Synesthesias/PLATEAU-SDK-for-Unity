using PLATEAU.CityInfo;
using PLATEAU.Util;
using UnityEngine;
using Material = UnityEngine.Material;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    public interface IMAMaterialChanger
    {
        void ExecRecursive(UniqueParentTransformList targetTrans);
        void Exec(Transform trans);
        bool ShouldChange(Transform trans);
    }
    
    /// <summary>
    /// マテリアル分け機能の3手順、分解 → マテリアル変更 → 結合のうち、マテリアル変更です。
    /// MAはMaterialAdjustの略とします。
    /// </summary>
    public class MAMaterialChanger : IMAMaterialChanger
    {
        private IMAConfig materialAdjustConf;
        private IMAMaterialSelector materialSelector;
        
        public MAMaterialChanger(IMAConfig materialAdjustConf, IMAMaterialSelector materialSelector)
        {
            this.materialAdjustConf = materialAdjustConf;
            this.materialSelector = materialSelector;
        }
        
        /// <summary>
        /// 再帰的にマテリアルを変更します。
        /// 引数は最小地物単位であることが前提です。
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
        /// 非再帰的にマテリアルを変更します。
        /// 引数は最小地物単位であることが前提です。
        /// </summary>
        public void Exec(Transform trans)
        {
            var materialResult = GetMaterial(trans);
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

        /// <summary>
        /// <paramref name="trans"/>に対応する、変更用のマテリアルの有無と、ある場合はそのマテリアルを返します。
        /// 引数は最小地物単位であることが前提です。
        /// </summary>
        private Result<Material> GetMaterial(Transform trans)
        {
            var cityObjGroups = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroups == null || cityObjGroups.CityObjects.rootCityObjects.Count == 0) return new Result<Material>(false, null);
            // 最小地物単位にしたので、rootCityObjectsの数は1つのはずです。
            var cityObj = cityObjGroups.CityObjects.rootCityObjects[0];
            return materialSelector.Get(cityObj, materialAdjustConf);
        }

        /// <summary>
        /// <paramref name="trans"/>がマテリアル分けの対象となるかどうかを返します。
        /// 引数の結合単位は問いません。また、子までは確認しません。
        /// </summary>
        public bool ShouldChange(Transform trans)
        {
            var cityObjGroups = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroups == null) return false;
            var cityObjs = cityObjGroups.GetAllCityObjects();
            foreach (var co in cityObjs)
            {
                if (materialSelector.Get(co, materialAdjustConf).IsSucceed) return true;
            }

            return false;
        }
        
    }

    /// <summary>
    /// マテリアル分けを行わないときに使うダミーです。
    /// </summary>
    public class MADummyMaterialChanger : IMAMaterialChanger
    {
        public void ExecRecursive(UniqueParentTransformList targetTrans)
        { // nop
        }

        public void Exec(Transform trans)
        { // nop
        }

        public bool ShouldChange(Transform trans)
        {
            return false;
        }
    }
}