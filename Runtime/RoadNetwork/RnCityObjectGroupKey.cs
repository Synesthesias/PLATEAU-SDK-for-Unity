using PLATEAU.CityInfo;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 主要地物単位のPLATEAUCityObjectGroupのまとまりを表すためのキー
    /// 最小地物の歩道/道路がもともと所属していた主要地物のGmlキーを持つ
    /// </summary>
    [Serializable]
    public struct RnCityObjectGroupKey
    {
        [field:SerializeField]
        public string GmlId { get; set; }
        
        public bool IsValid => !string.IsNullOrEmpty(GmlId);

        public RnCityObjectGroupKey(string gmlId)
        {
            GmlId = gmlId;
        }
    }
}