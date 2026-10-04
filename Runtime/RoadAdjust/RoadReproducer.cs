using PLATEAU.RoadAdjust.RoadMarking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IRrTarget = PLATEAU.RoadAdjust.RoadNetworkToMesh.IRrTarget;
using PLATEAUReproducedRoad = PLATEAU.RoadAdjust.RoadNetworkToMesh.PLATEAUReproducedRoad;
using RnmLineSeparateType = PLATEAU.RoadAdjust.RoadNetworkToMesh.RnmLineSeparateType;

namespace PLATEAU.RoadAdjust
{
    /// <summary>
    /// 道路ネットワークからの道路メッシュ生成(<see cref="RoadNetworkToMesh"/>)と
    /// 道路標示生成(<see cref="RoadMarking"/>)を合わせて
    /// よりよい見た目の道路を生成します。
    /// </summary>
    internal class RoadReproducer
    {
        /// <summary>
        /// 生成します。
        /// 生成された道路を返します。
        /// </summary>
        /// <param name="copySourceData">生成元オブジェクトの UV4・属性・コンポーネントを道路メッシュへコピーするか。詳細は <see cref="RoadNetworkToMesh.RoadNetworkToMesh"/> を参照。</param>
        public List<PLATEAUReproducedRoad> Generate(IRrTarget target, CrosswalkFrequency crosswalkFrequency, ISmoothingStrategy smoothingStrategy, bool copySourceData = true)
        {
            // 生成物の検索で毎回シーン全体を走査しないよう、一覧をまとめて取得しておきます。
            using var findCache = PLATEAUReproducedRoad.BeginFindCache();

            // 道路ネットワークから道路メッシュを生成
            var rnm = new RoadNetworkToMesh.RoadNetworkToMesh(target, RnmLineSeparateType.Combine, copySourceData);
            var generatedRoads = rnm.Generate(smoothingStrategy);

            // 道路標示を生成
            var rm = new RoadMarking.RoadMarkingGenerator(target, crosswalkFrequency);
            var generatedIntersections = rm.Generate(smoothingStrategy);

            // 輪郭線が無い場合 RoadNetworkToMesh.Generate は null を返すため、空として扱います。
            return (generatedRoads ?? new List<PLATEAUReproducedRoad>())
                .Concat(generatedIntersections ?? new List<PLATEAUReproducedRoad>())
                .ToList();
        }

        public static Transform GenerateDstParent()
        {
            var dstParent = SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .FirstOrDefault(obj => obj.name == "ReproducedRoad");
            if (dstParent == null)
            {
                dstParent = new GameObject("ReproducedRoad");
            }

            return dstParent.transform;
        }
    }
    

}