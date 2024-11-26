using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using System.Linq;
using System.Text.RegularExpressions;

namespace PLATEAU.RoadNetwork.Util
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
            return self.Lod;
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

            if (ret == 0)
                ret |= RRoadTypeMask.Undefined;

            return ret;
        }

    }
}