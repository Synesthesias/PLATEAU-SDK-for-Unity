﻿using PLATEAU.CityInfo;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnTrack : ARnParts<RnTrack>
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

    [Serializable]
    public class RnNeighbor : ARnParts<RnNeighbor>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        // Roadとの境界線
        public RnWay Border { get; set; }

        // 隣接道路(交差点)基本的にRoadだが、初期のPLATEAUモデルによってはIntersectionもあり得るため基底クラスで持っている
        // 隣接していない場合はnull
        public RnRoadBase Road { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // この境界が道路と接続しているか
        public bool IsBorder => Road != null;

        // 有効値判定(Border != null)
        public bool IsValid => Border != null;

        /// <summary>
        /// この境界とつながっているレーン
        /// </summary>
        /// <returns></returns>
        public RnLane GetConnectedLane()
        {
            return GetConnectedLanes().FirstOrDefault();
        }

        /// <summary>
        /// この境界とつながっているレーンリスト
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetConnectedLanes()
        {
            if (Border == null)
                yield break;
            if (Road == null)
                yield break;

            if (Road is RnRoad road)
            {
                foreach (var lane in road.MainLanes)
                {
                    // Borderと同じ線上にあるレーンを返す
                    if (lane.AllBorders.Any(b => b.IsSameLine(Border)))
                        yield return lane;
                }
            }
        }
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

        // 交差点の外形情報. 時計回り/反時計回りかは保証されていないが, 連結はしている
        private List<RnNeighbor> edges = new List<RnNeighbor>();

        // 信号制御器
        public TrafficSignalLightController SignalController { get; set; } = null;

        // 交差点内のトラック
        private List<RnTrack> tracks = new();


        //----------------------------------
        // end: フィールド
        //----------------------------------

        public override PLATEAUCityObjectGroup CityObjectGroup => TargetTran;

        public IEnumerable<RnNeighbor> Neighbors => edges.Where(e => e.IsBorder);

        public IReadOnlyList<RnNeighbor> Edges => edges;

        // 交差点内のトラック
        public IReadOnlyList<RnTrack> Tracks => tracks;

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
            foreach (var neighbor in Neighbors.Where(n => n.Road != null))
            {
                yield return new RnBorder(neighbor.Border);
            }
        }

        /// <summary>
        /// 隣接情報追加. borderがnullの場合は追加しない
        /// </summary>
        /// <param name="road"></param>
        /// <param name="border"></param>
        public void AddEdge(RnRoadBase road, RnWay border)
        {
            if (border == null)
                return;
            edges.Add(new RnNeighbor { Road = road, Border = border });
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
            edges.AddRange(borders.Select(b => new RnNeighbor { Road = link, Border = b }));
        }

        public void RemoveNeighbors(Func<RnNeighbor, bool> predicate)
        {
            for (var i = 0; i < edges.Count; i++)
            {
                var n = edges[i];
                if (predicate(n))
                {
                    edges.RemoveAt(i);
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
            var borders = edges.Where(n => n.Road == other).Select(n => n.Border).ToList();
            RemoveNeighbors(n => n.Road == other);
        }

        /// <summary>
        /// 自身の切断する
        /// </summary>
        public override void DisConnect(bool removeFromModel)
        {
            foreach (var n in Neighbors)
                n.Road?.UnLink(this);
            edges.Clear();
            if (removeFromModel)
                ParentModel?.RemoveIntersection(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tangentLength"></param>
        /// <param name="splitLength"></param>
        /// <param name="allowUTurn">Uターンを許可する</param>
        public void BuildTracks(float tangentLength = 10f, float splitLength = 2f, bool allowUTurn = false)
        {
            Align();
            tracks.Clear();

            // targetEntranceLane : trueの場合は入ってくるレーンを対象とする. falseは出ていくレーンを対象とする
            static bool IsTarget(RnNeighbor neighbor, bool targetEntranceLane)
            {
                if (neighbor.Border == null || neighbor.Road == null)
                    return false;
                if (neighbor.Border.IsValid == false)
                    return false;
                if (neighbor.Road is RnRoad road)
                {
                    // #NOTE : 車線一つしかない場合は、出ていくレーンも対象とする
                    if (road.MainLanes.Count == 1)
                        return true;
                    if (targetEntranceLane)
                        return road.GetConnectedLanes(neighbor.Border).Any(l => l.NextBorder.IsSameLine(neighbor.Border));
                    else
                        return road.GetConnectedLanes(neighbor.Border).Any(l => l.PrevBorder.IsSameLine(neighbor.Border));
                }
                // 交差点同士の接続の場合はレーン全部対象
                else if (neighbor.Road is RnIntersection)
                {
                    return true;
                }

                return false;
            }

            foreach (var from in Edges)
            {
                if (IsTarget(from, true) == false)
                    continue;

                foreach (var to in Edges)
                {
                    if (from == to)
                        continue;
                    if (IsTarget(to, false) == false)
                        continue;
                    if (allowUTurn == false && from.Road == to.Road)
                        continue;

                    var fromNormal = from.Border.GetEdgeNormal((from.Border.Count - 1) / 2).normalized;
                    var toNormal = -to.Border.GetEdgeNormal((to.Border.Count - 1) / 2).normalized;

                    from.Border.GetLerpPoint(0.5f, out var fromPos);
                    to.Border.GetLerpPoint(0.5f, out var toPos);

                    var spline = new Spline
                    {
                        new(fromPos, tangentLength * fromNormal, -tangentLength *fromNormal),
                        new(toPos, tangentLength *toNormal, -tangentLength *toNormal)
                    }; ;
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
        /// edgesの順番を整列する
        /// </summary>
        public void Align()
        {
            for (var i = 0; i < edges.Count; ++i)
            {
                var e0 = edges[i].Border;
                for (var j = i + 1; j < edges.Count; ++j)
                {
                    var e1 = edges[j].Border;
                    if (RnPoint.Equals(e0.GetPoint(-1), e1.GetPoint(0)))
                    {
                        (edges[i + 1], edges[j]) = (edges[j], edges[i + 1]);
                        break;
                    }
                    if (RnPoint.Equals(e0.GetPoint(-1), e1.GetPoint(-1)))
                    {
                        e1.Reverse(false);
                        (edges[i + 1], edges[j]) = (edges[j], edges[i + 1]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// selfの全頂点の重心を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public override Vector3 GetCenter()
        {
            var a = Neighbors
                .Where(n => n.Border != null)
                .SelectMany(n => n.Border.Vertices)
                .Aggregate(new { sum = Vector3.zero, i = 0 }, (a, p) => new { sum = a.sum + p, i = a.i + 1 });
            if (a.i == 0)
                return Vector3.zero;
            return a.sum / a.i;
        }
#if false
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
#endif
    }

    public static class RnIntersectionEx
    {


#if false
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
#endif
    }
}