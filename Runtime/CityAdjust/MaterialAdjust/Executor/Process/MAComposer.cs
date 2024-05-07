using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// マテリアル分け機能の3手順、分解 → マテリアル変更 → 結合のうちの結合です。
    /// MAはMaterialAdjustの略とします。
    /// </summary>
    internal class MAComposer
    {
        private MAExecutorConf conf;
        
        public MAComposer(MAExecutorConf conf)
        {
            this.conf = conf;
        }
        
        public async Task<Result<GranularityConvertResult>> ExecAsync(UniqueParentTransformList targetTrans, MAGranularity dstGranularity, IMACondition maCondition)
        {
            
                
            var granularityConverterAfter = new CityGranularityConverter();
            
            var composeConf = conf.Copy();
            composeConf.DoDestroySrcObjs = true;
            composeConf.TargetTransforms = new UniqueParentTransformList(targetTrans.Get);
            composeConf.MeshGranularity = dstGranularity;
            
            var composeResult = await granularityConverterAfter.ConvertProgressiveAsync(composeConf, maCondition);
            if (!composeResult.IsSucceed)
            {
                Debug.LogError("ゲームオブジェクトの結合に失敗しました。");
            }
            if(!composeResult.GeneratedRootTransforms.Get.Any())
            {
                Debug.LogWarning("結合後のゲームオブジェクトが存在しません。");
                return new Result<GranularityConvertResult>(false, composeResult);
            }

            // 親を付け替え
            // var parent = new GameObject(conf.DstObjName).transform;
            // parent.parent = composeResult.GeneratedRootTransforms.Get.First().parent;
            // foreach (var trans in composeResult.GeneratedRootTransforms.Get)
            // {
            //     trans.parent = parent;
            // }
            // #if UNITY_EDITOR
            // Selection.activeTransform = parent;
            // #endif

            return new Result<GranularityConvertResult>(composeResult.IsSucceed, composeResult);
        }
    }
}