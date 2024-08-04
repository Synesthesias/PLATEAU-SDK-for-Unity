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
        private List<RnNeighbor> neighbors = new List<RnNeighbor>();

        // 交差点内のレーン情報
        private List<RnLane> lanes = new List<RnLane>();

        // 信号制御器
        public TrafficSignalLightController SignalController { get; set; } = null;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnNeighbor> Neighbors => neighbors;

        // 車線
        public IReadOnlyList<RnLane> Lanes => lanes;

        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            foreach (var neighbor in Neighbors)
            {
                if (neighbor.Road != null)
                    yield return neighbor.Road;
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

        /// <summary>
        /// 隣接情報追加
        /// </summary>
        /// <param name="link"></param>
        /// <param name="border"></param>
        public void AddNeighbor(RnRoad link, RnWay border)
        {
            neighbors.Add(new RnNeighbor { Road = link, Border = border });
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
            neighbors.RemoveAll(n => n.Road == link);
            neighbors.AddRange(borders.Select(b => new RnNeighbor { Road = link, Border = b }));
        }

        /// <summary>
        /// 隣接情報からotherを削除する
        /// </summary>
        /// <param name="other"></param>
        public override void UnLink(RnRoadBase other)
        {
            var borders = neighbors.Where(n => n.Road == other).Select(n => n.Border).ToList();
            neighbors.RemoveAll(n => n.Road == other);

            // 削除するBorderに接続しているレーンも削除
            var removeLanes = lanes.Where(l => l.BothWays.Any(b => borders.Contains(b))).ToList();
            foreach (var r in removeLanes)
                RemoveLane(r);
        }

        /// <summary>
        /// 自身の切断する
        /// </summary>
        public override void DisConnect()
        {
            foreach (var n in Neighbors)
                n.Road?.UnLink(this);
            neighbors.Clear();
            ParentModel?.RemoveIntersection(this);
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
            var nFrom = Neighbors.FirstOrDefault(n => n.Road == from);
            var nTo = Neighbors.FirstOrDefault(n => n.Road == to);
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