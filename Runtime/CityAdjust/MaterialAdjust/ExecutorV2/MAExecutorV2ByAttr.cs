using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.MaterialAdjust;
using PLATEAU.Util;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2
{
    /// <summary>
    /// 属性情報によるマテリアル分けを実行します。
    /// </summary>
    public class MAExecutorV2ByAttr : IMAExecutorV2
    {
        public async Task<UniqueParentTransformList> ExecAsync(MAExecutorConf confBase)
        {
            var conf = (MAExecutorConfByAttr)confBase;
            using var adjuster = MaterialAdjusterByAttr.Create();
            var common = new MAExecutorV2Common();
            common.Prepare(conf.TargetTransforms, out var nonLibData, out var materialRegistry, out var sceneResult);
            SendAttributesToCpp(conf, adjuster);
            SendConfToCpp(conf, materialRegistry, adjuster);
            foreach (var target in conf.TargetTransforms.Get)
            {
                var model = common.ConvertToCppModel(target, materialRegistry);
                adjuster.Exec(model); // ここで実行します。
                await common.PlaceModelToSceneAsync(model, target, materialRegistry, nonLibData, sceneResult);
            }
            common.Finishing(sceneResult, nonLibData, conf);
            return sceneResult.GeneratedRootTransforms;
        }

        /// <summary>
        /// 属性情報をC++に送ります。
        /// </summary>
        private void SendAttributesToCpp(MAExecutorConfByAttr conf, MaterialAdjusterByAttr adjuster)
        {
            conf.TargetTransforms.BfsExec(trans =>
            {
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog == null) return NextSearchFlow.Continue;
                // Transformの属性情報を送ります
                SendAttributesOfCityObjGroup(cog, conf, adjuster);
                
                // 親の属性情報は子にも適用します
                // foreach (var childCog in trans.GetComponentsInChildren<PLATEAUCityObjectGroup>())
                // {
                //     SendAttributesOfCityObjGroup(childCog, conf, adjuster);
                // }
                return NextSearchFlow.Continue;
            });
        }

        private void SendAttributesOfCityObjGroup(PLATEAUCityObjectGroup cog, MAExecutorConfByAttr conf, MaterialAdjusterByAttr adjuster)
        {
            foreach (var co in cog.GetAllCityObjects())
            {
                if (co.AttributesMap.TryGetValueWithSlash(conf.AttrKey, out var attrValue))
                {
                    adjuster.RegisterAttribute(co.GmlID, attrValue.StringValue);
                    foreach (var child in co.Children)
                    {
                        adjuster.RegisterAttribute(child.GmlID, attrValue.StringValue);
                    }
                }
            }
        }

        /// <summary>
        /// マテリアル分け設定をC++に送ります。
        /// </summary>
        private void SendConfToCpp(MAExecutorConfByAttr conf, GameMaterialIDRegistry materialRegistry, MaterialAdjusterByAttr adjuster)
        {
            var materialsConf = (MAMaterialConfig<string>)conf.MaterialAdjustConf;
            for (int i = 0; i < materialsConf.Length; i++)
            {
                string attrKey = materialsConf.GetKeyNameAt(i);
                var matConf = materialsConf.GetMaterialChangeConfAt(i);
                if (!matConf.ChangeMaterial) continue;
                materialRegistry.TryAddMaterial(matConf.Material, out var matID);
                bool registered = adjuster.RegisterMaterialPattern(attrKey, matID);
                if (!registered)
                {
                    Debug.LogWarning($"duplicate attribute key: {attrKey}");
                }
            }
        }
        
    }
}