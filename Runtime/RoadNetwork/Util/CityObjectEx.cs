using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using System.Collections.Generic;
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


        private static readonly string[] sideWalkValues = new string[] { "自転車歩行者道", "植樹帯", "植樹ます", "植樹帯", "路肩", "側帯" };

        private static readonly string[] medianValues = new string[] { "島", "中央帯" };

        public static RRoadTypeMask GetRoadType(this CityInfo.CityObjectList.CityObject self)
        {
            // 緊急輸送道路もはじく必要がある
            // LOD3からチェックする
            var ret = RRoadTypeMask.Empty;

            // COT_Roadは強制的に対象にする
            if ((self.CityObjectType & CityObjectType.COT_Road) != 0)
                ret |= RRoadTypeMask.Road;

            if (self.AttributesMap.TryGetValue("tran:function", out var tranFunction))
            {
                var str = tranFunction.StringValue;
                if (new[] { "車道部", "車道交差部" }.Contains(str))
                {
                    ret |= RRoadTypeMask.Road;
                }

                if (str.Contains("歩道") || sideWalkValues.Contains(str))
                {
                    ret |= RRoadTypeMask.SideWalk;
                }

                if (medianValues.Contains(str))
                {
                    ret |= RRoadTypeMask.Median;
                }

                if (str.Contains("高速"))
                {
                    ret |= RRoadTypeMask.HighWay;
                }

                if (str.Contains("車線"))
                {
                    ret |= RRoadTypeMask.Lane;
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