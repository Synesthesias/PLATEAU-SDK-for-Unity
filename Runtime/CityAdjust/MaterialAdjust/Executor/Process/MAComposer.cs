using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
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
        
        public async Task<Result<GranularityConvertResult>> ExecAsync(GranularityConvertResult decomposeResult)
        {
            var granularityConverterAfter = new CityGranularityConverter();
            var granularityConvertConfAfter = new GranularityConvertOptionUnity(
                new GranularityConvertOption(conf.MeshGranularity, 1),
                decomposeResult.GeneratedRootTransforms, true
            );
            var composeResult = await granularityConverterAfter.ConvertProgressiveAsync(granularityConvertConfAfter);
            if (!composeResult.IsSucceed)
            {
                Debug.LogError("ゲームオブジェクトの結合に失敗しました。");
            }

            return new Result<GranularityConvertResult>(composeResult.IsSucceed, composeResult);
        }
    }
}