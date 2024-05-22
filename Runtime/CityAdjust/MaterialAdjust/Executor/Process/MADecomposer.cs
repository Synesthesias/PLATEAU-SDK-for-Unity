using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
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
        public async Task<Result<GranularityConvertResult>> ExecAsync(MAExecutorConf conf, Transform currentTarget, IMACondition maCondition)
        {
            var decomposer = new CityGranularityConverter();
            var decomposeConf = conf.Copy();
            decomposeConf.MeshGranularity = MAGranularity.PerAtomicFeatureObject;
            decomposeConf.TargetTransforms = new UniqueParentTransformList(currentTarget);
            var decomposeResult = await decomposer.ConvertProgressiveAsync(decomposeConf, maCondition);
            if (!decomposeResult.IsSucceed)
            {
                Debug.LogError("ゲームオブジェクトの分解に失敗しました");
                return new Result<GranularityConvertResult>(false, null);
            }
            return new Result<GranularityConvertResult>(true, decomposeResult);
        }
    }
}