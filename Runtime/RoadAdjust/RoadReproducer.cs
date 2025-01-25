using PLATEAU.Util;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IRrTarget = PLATEAU.RoadAdjust.RoadNetworkToMesh.IRrTarget;
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
        /// 引数<paramref name="doSubdivide"/>の説明は<see cref="LineSmoother"/>を参照してください。
        /// </summary>
        public void Generate(IRrTarget target, CrosswalkFrequency crosswalkFrequency, bool doSubdivide)
        {
            // 道路ネットワークから道路メッシュを生成
            var rnm = new RoadNetworkToMesh.RoadNetworkToMesh(target, RnmLineSeparateType.Combine);
            rnm.Generate(doSubdivide);

            // 道路標示を生成
            var rm = new RoadMarking.RoadMarkingGenerator(target, crosswalkFrequency);
            rm.Generate(doSubdivide);
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