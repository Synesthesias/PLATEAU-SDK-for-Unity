using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;

namespace PLATEAU.RoadAdjust
{
    /// <summary>
    /// <see cref="RoadReproducer"/>を外部アセンブリ（ランタイムのアプリ側）から呼び出すための公開窓口です。
    /// 道路ネットワークの一部を編集した後に、その部分の道路メッシュと道路標示を作り直す用途を想定しています。
    /// </summary>
    public static class RuntimeRoadReproducer
    {
        /// <summary>
        /// 指定した道路・交差点について、道路メッシュと道路標示を再生成します。
        /// 同じ生成元を持つ既存の生成物があれば置き換えます。
        /// </summary>
        /// <param name="network">対象の道路ネットワーク</param>
        /// <param name="targets">再生成する道路・交差点</param>
        /// <param name="placeCrosswalks">true なら横断歩道を配置し、false なら既存の横断歩道を削除します</param>
        /// <param name="smoothAll">true なら歩道外側も含めて線を滑らかにします。false なら元の形状を尊重します</param>
        public static IReadOnlyList<PLATEAUReproducedRoad> Reproduce(
            RnModel network,
            IEnumerable<RnRoadBase> targets,
            bool placeCrosswalks = true,
            bool smoothAll = false)
        {
            var crosswalk = placeCrosswalks ? CrosswalkFrequency.All : CrosswalkFrequency.Delete;
            ISmoothingStrategy smoothing = smoothAll
                ? new SmoothingStrategySmoothAll()
                : new SmoothingStrategyRespectOriginal();
            // UV4 等のコピーは LOD2 の道路で数秒〜十数秒かかるため、ランタイムでは行いません。
            // 道路の特定は PLATEAUReproducedRoad に記録された GmlID で行う前提です。
            // また、ネットワーク全体ではなく対象と隣接部分だけをコピーして処理します。
            return new RoadReproducer().Generate(
                new RrTargetRoadBases(network, targets, copyNeighborsOnly: true), crosswalk, smoothing, copySourceData: false);
        }
    }
}
