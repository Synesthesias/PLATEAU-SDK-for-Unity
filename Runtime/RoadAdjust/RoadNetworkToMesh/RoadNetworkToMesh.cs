using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュに変換します。
    /// </summary>
    public class RoadNetworkToMesh
    {
        private readonly RnModel model;
        private readonly RnmLineSeparateType lineSeparateType;
        
        public RoadNetworkToMesh(RnModel model, RnmLineSeparateType lineSeparateType)
        {
            if (model == null)
            {
                Debug.LogError("道路ネットワークがありません。");
            }
            this.model = model;
            this.lineSeparateType = lineSeparateType;
        }

        public void Generate()
        {
            // 道路ネットワークから輪郭線を生成します。
            IRnmContourGenerator[] contourGenerators;
            switch (lineSeparateType)
            {
                case RnmLineSeparateType.Combine:
                    contourGenerators = new IRnmContourGenerator[]
                    {
                        new RnmContourGeneratorRoadCombine(), // 道路
                        new RnmContourGeneratorIntersectionCombine() // 交差点(結合)
                    };
                    break;
                case RnmLineSeparateType.Separate:
                    contourGenerators = new IRnmContourGenerator[]
                    {
                        new RnmContourGeneratorCarLane(), // 車道
                        new RnmContourGeneratorSidewalk(), // 歩道
                        new RnmContourGeneratorIntersectionSeparate() // 交差点(分割)
                    };
                    break;
                default:
                    throw new ArgumentException($"Unknown {nameof(RnmLineSeparateType)}");
            }
            var contours = new RnmContourGenerator(contourGenerators).Generate(model);
            
            // 輪郭線からメッシュとゲームオブジェクトを生成します。
            foreach (var contour in contours)
            {
                var obj = new GameObject("RoadNetworkToMeshDebug");
                var comp = obj.AddComponent<PLATEAURoadNetworkToMeshDebug>();
                comp.Init(contour);
                var mesh = new ContourToMesh().Generate(contour);
                var renderer = obj.AddComponent<MeshRenderer>();
                var filter = obj.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                if (contour.SourceObject != null)
                {
                    var srcRenderer = contour.SourceObject.GetComponent<MeshRenderer>();
                    var srcMats = srcRenderer.sharedMaterials;
                    if (srcMats != null)
                    {
                        renderer.materials = srcMats;
                    }
                }
            }
            
        }
        
        
        
    }
}