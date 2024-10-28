using PLATEAU.Dataset;
using PLATEAU.Util;
using System;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュ化したときに、どのマテリアルを割り当てるかです。
    /// </summary>
    public enum RnmMaterialType
    {
        CarLane, // 車道
        SideWalk, // 歩道
        MedianLane, // 中央分離帯
    }

    public static class RnmMaterialTypeExtension
    {
        public static Material ToMaterial(this RnmMaterialType type)
        {
            var mat = type switch
            {
                RnmMaterialType.CarLane => FallbackMaterial.LoadByMaterialFileName("PlateauGenericRoadWithUV"),
                RnmMaterialType.SideWalk => FallbackMaterial.LoadByMaterialFileName("PlateauGenericRoadWithUVTile"),
                RnmMaterialType.MedianLane => FallbackMaterial.LoadByMaterialFileName("PlateauGenericRoadStoneAsphalt"),
                _ => throw new ArgumentOutOfRangeException()
            };
            return mat;
        }
    }
}