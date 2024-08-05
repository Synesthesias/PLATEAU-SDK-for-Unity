using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using System.Linq;
using System.Text.RegularExpressions;

namespace PLATEAU.RoadNetwork
{
    public static class CityObjectEx
    {
        private static readonly Regex lodRegex = new Regex(@"^LOD(\d+)$", RegexOptions.Compiled);

        /// <summary>
        /// Lodレベルを取得する
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetLodLevel(this PLATEAUCityObjectGroup self)
        {
            if (!self)
                return 0;
            // #TODO : 親のオブジェクト名で見ているが専用の属性があるか確認すべき
            foreach (var p in self.transform.GetParents())
            {
                var m = lodRegex.Match(p.name);
                if (m.Success)
                {
                    return int.Parse(m.Groups[1].Value);
                }
            }

            return 0;
        }

        public static RRoadTypeMask GetRoadType(this CityInfo.CityObjectList.CityObject self)
        {
            // 緊急輸送道路もはじく必要がある
            // LOD3からチェックする
            var ret = RRoadTypeMask.Empty;
            if (self.AttributesMap.TryGetValue("tran:function", out var tranFunction))
            {
                var str = tranFunction.StringValue;
                if (new[] { "車道部", "車道交差部" }.Contains(str))
                {
                    ret |= RRoadTypeMask.Road;
                }

                if (str == "歩道部")
                {
                    ret |= RRoadTypeMask.SideWalk;
                }

                if (tranFunction.StringValue == "島")
                {
                    ret |= RRoadTypeMask.Median;
                }

                if (str.Contains("高速"))
                {
                    ret |= RRoadTypeMask.HighWay;
                }
            }

            // LOD1
            if (self.AttributesMap.TryGetValue("tran:class", out var tranClass))
            {
                var str = tranClass.StringValue;
                if (str == "道路")
                {
                    ret |= RRoadTypeMask.Road;
                }
            }

            return ret;
        }

    }
}