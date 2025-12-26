using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnRoadBase))]
    public class RnDataRoadBase : IPrimitiveData
    {
        // #NOTE : タイルメッシュ読み込みだと実質意味を持たなくなります.タイル内のすべての道路が同じ物を参照するようになるためです
        //       : 代わりにTargetGroupKeysを使ってください
        // #TODO : 互換性のために残しておく. シミュレーター側で使っているようなので.使われなくなったら消す
        [Obsolete("UseTargetTrans")]
        public PLATEAUCityObjectGroup TargetTran => TargetTrans.FirstOrDefault();

        // #NOTE : タイルメッシュ読み込みだと実質意味を持たなくなります.タイル内のすべての道路が同じ物を参照するようになるためです
        //       : 代わりにTargetGroupKeysを使ってください
        // #TODO : 最終的には消す予定です。現在使っている個所があるのでdeprecatedにもしていませんが今後deprecated -> 削除されます
        [field: SerializeField]
        [RoadNetworkSerializeMember( "targetTrans")]
        public List<PLATEAUCityObjectGroup> TargetTrans { get; set; } = new List<PLATEAUCityObjectGroup>();
        
        [field:SerializeField]
        [RoadNetworkSerializeMember("targetGroupKeys")]
        public List<RnCityObjectGroupKey> TargetGroupKeys { get; set; } = new List<RnCityObjectGroupKey>();

        [field: SerializeField]
        [RoadNetworkSerializeMember("sideWalks")]
        public List<RnID<RnDataSideWalk>> SideWalks { get; set; } = new();
    }
}