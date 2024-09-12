using PLATEAU.RoadNetwork.Structure;
using System;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークから、センターラインを収集します。
    /// センターラインとは、車の進行方向が違う車線を区切る線です。
    /// MCはMarkedWayComposerの略です。
    /// </summary>
    internal class MCCenterLine : IMarkedWayListComposer
    {
        private const float WidthThreshold = 6f; // センターラインのタイプが変わるしきい値、道路の片側の幅
        private const float IntersectionThreshold = 30f; // 交差点との距離が近いかどうかのしきい値

        public MarkedWayList ComposeFrom(RnModel model)
        {
            var ret = new MarkedWayList();
            foreach (var road in model.Roads)
            {
                var carLanes = road.MainLanes;
                var widthType = GetCenterLineTypeOfWidth(road);
                var interDistCalc = new IntersectionDistCalc(road);

                for (int i = 0; i < carLanes.Count; i++)
                {
                    var lane = carLanes[i];
                    // 次のレーンと進行方向が異なる場合、RightLaneはセンターラインです。
                    bool isCenterLane = i < carLanes.Count - 1 && lane.IsReverse != carLanes[i + 1].IsReverse;
                    if (!isCenterLane)
                    {
                        continue;
                    }

                    var srcWay = WayWithMiddlePoint(lane.RightWay);

                    var lineString = new RnLineString();
                    var prevInterType = MarkedWayType.None;
                    for (int j = 0; j < srcWay.Count; j++)
                    {
                        float currentDist = interDistCalc.NearestDistFromIntersection(srcWay, j);
                        var interType = currentDist < IntersectionThreshold
                            ? MarkedWayType.CenterLineNearIntersection
                            : widthType;
                        if (prevInterType != interType && prevInterType != MarkedWayType.None)
                        {
                            // 交差点との距離がしきい値となる点を補間して追加
                            float prevDist = interDistCalc.NearestDistFromIntersection(srcWay, j - 1);
                            float t;
                            if (Mathf.Abs(currentDist - prevDist) < 0.1)
                            {
                                t = 1f;
                            }
                            else
                            {
                                t = (IntersectionThreshold - prevDist) / (currentDist - prevDist);
                            }


                            var lerpedPoint = Vector3.Lerp(srcWay.GetPoint(j - 1), srcWay.GetPoint(j), t);
                            
                            lineString.AddPoint(new RnPoint(lerpedPoint));

                            // 線を追加
                            var dstWay = new RnWay(new RnLineString(lineString.Points), srcWay.IsReversed,
                                srcWay.IsReverseNormal);
                            ret.Add(new MarkedWay(dstWay, prevInterType, lane.IsReverse));
                            lineString = new RnLineString(); // リセット
                            lineString.AddPoint(new RnPoint(lerpedPoint)); // 次の始点
                        }

                        prevInterType = interType;
                        lineString.AddPoint(srcWay.GetPoint(j));
                    }

                    if (lineString.Count > 0)
                        ret.Add(new MarkedWay(new RnWay(new RnLineString(lineString.Points), srcWay.IsReversed,
                            srcWay.IsReverseNormal), prevInterType, lane.IsReverse));
                    break; // センターラインは道路につき1つだけ
                }
            }

            return ret;
        }

        /// <summary> 片側の道路幅からセンターラインのタイプを判定します。 </summary>
        private MarkedWayType GetCenterLineTypeOfWidth(RnRoad road)
        {
            var carLanes = road.MainLanes;
            // 片側の道路幅からタイプを判定します
            bool isOver6M =
                carLanes.Where(l => l.IsReverse).Sum(l => l.CalcWidth()) > WidthThreshold ||
                carLanes.Where(l => !l.IsReverse).Sum(l => l.CalcWidth()) > WidthThreshold;
            var type = isOver6M ? MarkedWayType.CenterLineOver6MWidth : MarkedWayType.CenterLineUnder6MWidth;
            return type;
        }

        /// <summary>
        /// 線の中心に点を挿入した<see cref="RnWay"/>を新たに作って返します。
        /// これにより、隣り合う点で近い交差点が違うケースを考慮せずにすみます。
        /// </summary>
        private RnWay WayWithMiddlePoint(RnWay way)
        {
            float wayLength = way.CalcLength();
            float halfWayLength = wayLength / 2;
            float len = 0;
            var dstLine = new RnLineString();
            dstLine.AddPoint(way.GetPoint(0));
            bool centerAdded = false;
            for (int j = 1; j < way.Count; j++)
            {
                var pCurrent = way.GetPoint(j);
                var pPrev = way.GetPoint(j - 1);
                float lenDiff = (pCurrent.Vertex - pPrev.Vertex).magnitude;
                float prevLen = len;
                len += lenDiff;
                if ((!centerAdded) && len >= halfWayLength)
                {
                    var pos = Vector3.Lerp(pPrev.Vertex, pCurrent.Vertex, (halfWayLength - prevLen) / lenDiff);
                    dstLine.AddPoint(new RnPoint(pos));
                    centerAdded = true;
                }
                dstLine.AddPoint(pCurrent);
            }

            return new RnWay(dstLine, way.IsReversed, way.IsReverseNormal);
        }
    }

    /// <summary>
    /// 接続先の交差点までの距離を計算します。
    /// </summary>
    internal class IntersectionDistCalc
    {
        /// <summary> 道路のPrev方向の交差点までの距離（対象道路を含まない距離） </summary>
        private float PrevLength { get; }
        /// <summary> 道路のNext方向の交差点までの距離（対象道路を含まない距離） </summary>
        private float NextLength { get; }

        private const float Inf = 999999;

        /// <summary>
        /// 交差点を探し、その距離を計算します。
        /// </summary>
        public IntersectionDistCalc(RnRoad road)
        {
            float prevLength = 0;
            var prev = road.Prev;
            while (prev is RnRoad r)
            {
                prev = r.Prev;
                prevLength += RoadLength(r);
            }

            var next = road.Next;
            float nextLength = 0;
            while (next is RnRoad r)
            {
                next = r.Next;
                nextLength += RoadLength(r);
            }

            PrevLength = prev is RnIntersection ? prevLength : Inf;
            NextLength = next is RnIntersection ? nextLength : Inf;
        }

        /// <summary>
        /// 引数で与えられた <see cref="RnWay"/> の <paramref name="wayIndexOrig"/> 番目の頂点について、
        /// 交差点までの距離を返します。
        /// Prev方向とNext方向の距離のうち、近い方を返します。
        /// </summary>
        public float NearestDistFromIntersection(RnWay way, int wayIndexOrig)
        {
            if (way.Count <= 1) return Inf;

            var points = way.IsReversed ? way.Points.Reverse().ToArray() : way.Points.ToArray();
            int wayIndex = way.IsReversed ? way.Count - 1 - wayIndexOrig : wayIndexOrig;

            float prevLen = 0; // 道路の指定地点から、道路のprev方向の端までの距離
            for (int i = 1; i <= wayIndex && i < points.Length; i++)
            {
                prevLen += (points[i].Vertex - points[i - 1].Vertex).magnitude;
            }

            float nextLen = 0; // 道路の指定地点から、道路のnext方向の端までの距離
            for (int i = wayIndex; i < points.Length - 1; i++)
            {
                nextLen += (points[i + 1].Vertex - points[i].Vertex).magnitude;
            }

            float prevSum = prevLen + PrevLength;
            float nextSum = nextLen + NextLength;
            return Math.Min(prevSum, nextSum);
        }
        
        private float RoadLength(RnRoad r)
        {
            if (r.MainLanes.Count > 0 && r.MainLanes[0].RightWay != null)
            {
                return r.MainLanes[0].RightWay.CalcLength();
            }
            return 0;
        }
    }
}