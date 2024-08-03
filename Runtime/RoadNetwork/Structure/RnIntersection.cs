using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Structure
{
    /// <summary>
    /// 交差点
    /// </summary>

    [Serializable]
    public class RnIntersection : RnRoadBase
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RnModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 隣接情報
        public List<RnNeighbor> Neighbors { get; set; } = new List<RnNeighbor>();

        // レーンリスト
        private List<RnLane> lanes = new List<RnLane>();

        // 信号制御器
        public TrafficSignalLightController SignalController { get; set; } = null;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 車線
        public IReadOnlyList<RnLane> Lanes => lanes;

        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            foreach (var neighbor in Neighbors)
            {
                if (neighbor.Link != null)
                    yield return neighbor.Link;
            }
        }


        // 所持全レーンを取得
        public override IEnumerable<RnLane> AllLanes
        {
            get
            {
                foreach (var lane in lanes)
                    yield return lane;

                yield break;
            }
        }

        public RnIntersection() { }

        public RnIntersection(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        public override IEnumerable<RnBorder> GetBorders()
        {
            foreach (var neighbor in Neighbors)
            {
                foreach (var lane in neighbor.GetConnectedLanes())
                    yield return new RnBorder(neighbor.Border, lane);
            }
        }

        public void AddLane(RnLane lane)
        {
            if (lanes.Contains(lane))
                return;

            lane.Parent = this;
            lanes.Add(lane);
        }

        public void AddLanes(IEnumerable<RnLane> lanes)
        {
            foreach (var track in lanes)
                AddLane(track);
        }

        public void RemoveLane(RnLane lane)
        {
            if (lanes.Remove(lane))
                lane.Parent = null;
        }

        public Vector3 GetCenterPoint()
        {
            var ret = Neighbors.SelectMany(n => n.Border.Vertices).Aggregate(Vector3.zero, (a, b) => a + b);
            var cnt = Neighbors.Sum(n => n.Border.Count);
            return ret / cnt;
        }

        public void ReplaceBorder(RnRoad link, List<RnWay> borders)
        {
            Neighbors.RemoveAll(n => n.Link == link);
            Neighbors.AddRange(borders.Select(b => new RnNeighbor { Link = link, Border = b }));
        }

        /// <summary>
        /// a,bを繋ぐ経路を計算する
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public RnLane CalcTrackWay(RnRoad from, RnRoad to)
        {
            if (from == to)
                return null;
            var nFrom = Neighbors.FirstOrDefault(n => n.Link == from);
            var nTo = Neighbors.FirstOrDefault(n => n.Link == to);
            if (nFrom == null || nTo == null)
                return null;
            if (nFrom.Border.Count < 2 || nTo.Border.Count < 2)
                return null;

            var fromLane = nFrom.GetConnectedLane();
            if (fromLane == null)
                return null;

            var toLane = nTo.GetConnectedLane();
            if (toLane == null)
                return null;


            fromLane.TryGetBorderNormal(nFrom.Border, out var aLeftPos, out var aLeftNormal, out var aRightPos, out var aRightNormal);
            toLane.TryGetBorderNormal(nTo.Border, out var bLeftPos, out var bLeftNormal, out var bRightPos, out var bRightNormal);

            var rightSp = new Spline
            {
                new BezierKnot(aLeftPos, 10 * aLeftNormal, 10 *aLeftNormal),
                new BezierKnot(bRightPos, 10 *bRightNormal, 10 *bRightNormal)
            };

            var leftSp = new Spline
            {
                new BezierKnot(aRightPos, 10 *aRightNormal, 10 *aRightNormal),
                new BezierKnot(bLeftPos, 10 *bLeftNormal, 10 *bLeftNormal)
            };
#if false
            DebugEx.DrawArrow(aLeftPos, aLeftPos + aLeftNormal * 2, bodyColor: Color.magenta);
            DebugEx.DrawArrow(bLeftPos, bLeftPos + bLeftNormal * 2, bodyColor: Color.magenta);
            DebugEx.DrawArrow(bRightPos, bRightPos + bRightNormal * 2, bodyColor: Color.magenta);
            DebugEx.DrawArrow(aRightPos, aRightPos + aRightNormal * 2, bodyColor: Color.magenta);
#endif
            var rates = Enumerable.Range(0, 10).Select(i => 1f * i / (9)).ToList();
            var leftWay = new RnWay(RnLineString.Create(rates.Select(t =>
            {
                leftSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
            var rightWay = new RnWay(RnLineString.Create(rates.Select(t =>
            {
                rightSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
            return new RnLane(leftWay, rightWay, nFrom.Border, nTo.Border);
        }
    }
}