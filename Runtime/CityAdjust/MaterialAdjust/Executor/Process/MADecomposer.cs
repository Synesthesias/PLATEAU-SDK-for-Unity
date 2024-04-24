using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// マテリアル分け機能の3手順、分解 → マテリアル変更 → 結合のうちの分解です。
    /// 地物タイプに応じてマテリアルを変える下準備として、都市オブジェクトを最小地物単位に分解します。
    /// MAはMaterialAdjustの略とします。
    /// </summary>
    internal class MADecomposer
    {
        public async Task<Result<GranularityConvertResult>> ExecAsync(MAExecutorConf conf)
        {
            // 事前チェック
            if (!conf.Validate()) return new Result<GranularityConvertResult>(false, null);
            
            // 分解
            var granularityConverter = new CityGranularityConverter();
            var granularityConvertConf = new GranularityConvertOptionUnity(
                new GranularityConvertOption(MeshGranularity.PerAtomicFeatureObject, 1),
                conf.TargetTransforms, conf.DoDestroySrcObjs
            );
            var decomposeConf = conf.Copy();
            decomposeConf.MeshGranularity = MAGranularity.PerAtomicFeatureObject;
            var result = await granularityConverter.ConvertProgressiveAsync(decomposeConf);
            if (!result.IsSucceed)
            {
                Debug.LogError("ゲームオブジェクトの分解に失敗しました。");
            }

            return new Result<GranularityConvertResult>(result.IsSucceed, result);
        }
    }
}