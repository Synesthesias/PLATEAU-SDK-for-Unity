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
        RoadCarLane, // 車道（交差点でない道路）
        IntersectionCarLane, // 車道（交差点）
        SideWalk, // 歩道
        MedianLane, // 中央分離帯
    }

    public static class RnmMaterialTypeExtension
    {
        public static Material ToMaterial(this RnmMaterialType type)
        {
            var mat = type switch
            {
                RnmMaterialType.RoadCarLane => FallbackMaterial.LoadByMaterialFileName("PlateauGenericRoadWithUV"),
                RnmMaterialType.IntersectionCarLane => FallbackMaterial.LoadByMaterialFileName("PlateauGenericRoad"),
                RnmMaterialType.SideWalk => FallbackMaterial.LoadByMaterialFileName("PlateauGenericRoadWithUVTile"),
                RnmMaterialType.MedianLane => FallbackMaterial.LoadByMaterialFileName("PlateauRoadStone"),
                _ => throw new ArgumentOutOfRangeException()
            };
            return mat;
        }
    }
}