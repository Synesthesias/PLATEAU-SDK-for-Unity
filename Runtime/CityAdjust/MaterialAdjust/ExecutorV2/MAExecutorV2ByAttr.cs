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
                if (cog == null)
                {
                    return NextSearchFlow.Continue;
                }

                foreach (var co in cog.GetAllCityObjects())
                {
                    string attrKey = conf.AttrKey;
                    if (co.AttributesMap.TryGetValueWithSlash(attrKey, out var attrVal))
                    {
                        string attrValStr = attrVal.StringValue;
                        // 親自身に属性値を適用します。
                        adjuster.RegisterAttribute(co.GmlID, attrValStr);
                        foreach (var childCO in co.Children)
                        {
                            RegisterAttributeConditional(childCO, conf, adjuster, attrValStr);
                        }
                        
                        // 子にも属性値を適用します。
                        foreach (var childCog in trans.GetComponentsInChildren<PLATEAUCityObjectGroup>())
                        {
                            if (childCog.transform == trans) continue;
                            foreach(var childCO in childCog.GetAllCityObjects())
                            { 
                                RegisterAttributeConditional(childCO, conf, adjuster, attrValStr);
                            }
                            
                        }
                    }
                }

                return NextSearchFlow.Continue;
            });
        }
        

        /// <summary>
        /// 条件付きで属性情報をC++に渡します。
        /// </summary>
        private bool RegisterAttributeConditional(SerializableCityObjectList.SerializableCityObject co, MAExecutorConfByAttr conf, MaterialAdjusterByAttr adjuster, string parentAttrVal)
        {
            // 特別ケースに対処:
            // 親と自身に同名のキーをとる属性情報があり、かつどちらもマテリアル変更対象である場合、子を優先して登録します。
            var attrMap = co.AttributesMap;
            if (attrMap.TryGetValueWithSlash(conf.AttrKey, out var selfAttr) && IsMatChangeTarget(conf, selfAttr.StringValue) && IsMatChangeTarget(conf, parentAttrVal))
            {
                return adjuster.RegisterAttribute(co.GmlID, selfAttr.StringValue);
            }
            // マテリアル変更対象である場合、登録します。
            if (IsMatChangeTarget(conf, parentAttrVal))
            {
                return adjuster.RegisterAttribute(co.GmlID, parentAttrVal);
            }

            return false;
        }

        private bool IsMatChangeTarget(MAExecutorConfByAttr conf, string attrValueStr)
        {
            var matConf = ((MAMaterialConfig<string>)conf.MaterialAdjustConf).GetConfFor(attrValueStr);
            if (matConf == null || !matConf.ChangeMaterial || matConf.Material == null) return false;
            return true;
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