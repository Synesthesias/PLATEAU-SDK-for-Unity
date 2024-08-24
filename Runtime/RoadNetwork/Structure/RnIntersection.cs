using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnTrack : ARnParts<RnNeighbor>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RnWay FromBorder { get; set; }

        // 対象のtranオブジェクト
        public RnWay ToBorder { get; set; }

        // スプライン
        public Spline Spline { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }

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

        private List<RnTrack> tracks = new();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public override PLATEAUCityObjectGroup CityObjectGroup => TargetTran;

        public IReadOnlyList<RnNeighbor> Neighbors => neighbors;

        // 中央線トラック
        public IReadOnlyList<RnTrack> Tracks => tracks;

        // 車線
        public IReadOnlyList<RnLane> Lanes => lanes;

        public RnIntersection() { }

        public RnIntersection(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }


        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            foreach (var neighbor in Neighbors)
            {
                if (neighbor.Road != null)
                    yield return neighbor.Road;
            }
        }

        public override IEnumerable<RnBorder> GetBorders()
        {
            foreach (var neighbor in Neighbors)
            {
                foreach (var lane in neighbor.GetConnectedLanes())
                    yield return new RnBorder(neighbor.Border, lane);
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

        /// <summary>
        /// 隣接情報追加. borderがnullの場合は追加しない
        /// </summary>
        /// <param name="road"></param>
        /// <param name="border"></param>
        public void AddNeighbor(RnRoadBase road, RnWay border)
        {
            if (border == null)
                return;
            neighbors.Add(new RnNeighbor { Road = road, Border = border });
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
            RemoveNeighbors(n => n.Road == link);
            neighbors.AddRange(borders.Select(b => new RnNeighbor { Road = link, Border = b }));
        }

        public void RemoveNeighbors(Func<RnNeighbor, bool> predicate)
        {
            for (var i = 0; i < neighbors.Count; i++)
            {
                var n = neighbors[i];
                if (predicate(n))
                {
                    neighbors.RemoveAt(i);
                    i--;
                    // トラックからも削除する
                    tracks.RemoveAll(x => x.FromBorder == n.Border || x.ToBorder == n.Border);
                }
            }
        }


        /// <summary>
        /// road/laneに接続している隣接情報を削除する
        /// </summary>
        /// <param name="road"></param>
        /// <param name="lane"></param>
        public void RemoveNeighbor(RnRoad road, RnLane lane)
        {
            RemoveNeighbors(x => x.Road == road && ((lane.PrevBorder?.IsSameLine(x.Border) ?? false) || (lane.NextBorder?.IsSameLine(x.Border) ?? false)));
        }

        /// <summary>
        /// 隣接情報からotherを削除する. other側の接続は消えない
        /// </summary>
        /// <param name="other"></param>
        public override void UnLink(RnRoadBase other)
        {
            // 削除するBorderに接続しているレーンも削除
            var borders = neighbors.Where(n => n.Road == other).Select(n => n.Border).ToList();

            RemoveNeighbors(n => n.Road == other);

            var removeLanes = lanes.Where(l => l.BothWays.Any(b => borders.Contains(b))).ToList();
            foreach (var r in removeLanes)
                RemoveLane(r);
        }

        /// <summary>
        /// 自身の切断する
        /// </summary>
        public override void DisConnect(bool removeFromModel)
        {
            foreach (var n in Neighbors)
                n.Road?.UnLink(this);
            neighbors.Clear();
            if (removeFromModel)
                ParentModel?.RemoveIntersection(this);
        }

        public void BuildTracks(float tangentLength = 10f, float splitLength = 2f)
        {
            tracks.Clear();
            foreach (var from in Neighbors)
            {
                var fromLane = from.GetConnectedLane();
                if (fromLane == null)
                    continue;

                // この交差点に入ってくるレーンのみを対象とする
                if (fromLane.NextBorder.IsSameLine(from.Border) == false)
                    continue;
                foreach (var to in Neighbors)
                {
                    if (from == to)
                        continue;

                    var toLane = to.GetConnectedLane();
                    if (toLane == null)
                        continue;

                    // この交差点から出ていくレーンのみを対象とする
                    if (toLane.PrevBorder.IsSameLine(to.Border) == false)
                        continue;

                    var spline = this.CalcTrackSpline(from.Border, to.Border, tangentLength, splitLength);
                    tracks.Add(new RnTrack
                    {
                        FromBorder = from.Border,
                        ToBorder = to.Border,
                        Spline = spline
                    });
                }
            }
        }

        /// <summary>
        /// a,bを繋ぐ経路を計算する
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tangentLength"></param>
        public RnLane CalcTrackWay(RnRoadBase from, RnRoadBase to, float tangentLength = 10f)
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
                new(aLeftPos, tangentLength * aLeftNormal, tangentLength *aLeftNormal),
                new(bRightPos, tangentLength *bRightNormal, tangentLength *bRightNormal)
            };

            var leftSp = new Spline
            {
                new(aRightPos, tangentLength *aRightNormal, tangentLength *aRightNormal),
                new(bLeftPos, tangentLength *bLeftNormal, tangentLength *bLeftNormal)
            };

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

    public static class RnIntersectionEx
    {
        public static RnNeighbor GetIntersectionNeighbor(this RnIntersection self)
        {
            return self.Neighbors.FirstOrDefault();
        }

        public static Spline CalcTrackSpline(this RnIntersection self, RnWay fromBorder, RnWay toBorder,
            float tangentLength = 10f, float splitLength = 2f)
        {
            if (fromBorder == toBorder)
                return null;
            var nFrom = self.Neighbors.FirstOrDefault(n => n.Border == fromBorder);
            var nTo = self.Neighbors.FirstOrDefault(n => n.Border == toBorder);
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

            fromBorder.GetLerpPoint(0.5f, out var fromPos);
            toBorder.GetLerpPoint(0.5f, out var toPos);

            return new Spline
            {
                new(fromPos, tangentLength * aLeftNormal, tangentLength *aLeftNormal),
                new(toPos, tangentLength *bRightNormal, tangentLength *bRightNormal)
            };
        }

        /// <summary>
        /// a,bを繋ぐ経路を計算する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="fromBorder"></param>
        /// <param name="toBorder"></param>
        /// <param name="tangentLength"></param>
        /// <param name="splitLength"></param>
        public static RnWay CalcTrackWay(this RnIntersection self, RnWay fromBorder, RnWay toBorder, float tangentLength = 10f, float splitLength = 2f)
        {
            if (fromBorder == toBorder)
                return null;
            var nFrom = self.Neighbors.FirstOrDefault(n => n.Border == fromBorder);
            var nTo = self.Neighbors.FirstOrDefault(n => n.Border == toBorder);
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

            fromBorder.GetLerpPoint(0.5f, out var fromPos);
            toBorder.GetLerpPoint(0.5f, out var toPos);

            var rightSp = new Spline
            {
                new(fromPos, tangentLength * aLeftNormal, tangentLength *aLeftNormal),
                new(toPos, tangentLength *bRightNormal, tangentLength *bRightNormal)
            };

            var num = Mathf.Max(2, Mathf.CeilToInt((fromPos - toPos).magnitude / splitLength));
            var rates = Enumerable.Range(0, num).Select(i => 1f * i / (num - 1)).ToList();
            return new RnWay(RnLineString.Create(rates.Select(t =>
            {
                rightSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
        }
    }
}