using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkLane
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnID<RoadNetworkDataLane> MyId { get; set; }

        // 親リンク
        public RoadNetworkLink ParentLink { get; set; }

        // 連結しているレーン(上流)
        public List<RoadNetworkLane> NextLanes { get; private set; } = new List<RoadNetworkLane>();

        // 連結しているレーン(下流)
        public List<RoadNetworkLane> PrevLanes { get; private set; } = new List<RoadNetworkLane>();

        // 境界線(下流)
        public RoadNetworkWay PrevBorder { get; set; }

        // 境界線(上流)
        public RoadNetworkWay NextBorder { get; set; }

        // 車線(左)
        public RoadNetworkWay LeftWay { get; private set; }

        // 車線(右)
        public RoadNetworkWay RightWay { get; private set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 左右両方のWayを返す
        public IEnumerable<RoadNetworkWay> BothWays
        {
            get
            {
                return Enumerable.Repeat(LeftWay, 1).Concat(Enumerable.Repeat(RightWay, 1)).Where(w => w != null);
            }
        }

        public IEnumerable<RoadNetworkWay> AllBorders
        {
            get
            {
                return Enumerable.Repeat(PrevBorder, 1).Concat(Enumerable.Repeat(NextBorder, 1)).Where(w => w != null);
            }
        }

        public bool IsValidWay => LeftWay != null && RightWay != null;

        /// <summary>
        /// 道の両方に接続先があるかどうか
        /// </summary>
        public bool IsBothConnectedLane => IsValidWay && PrevBorder != null && NextBorder != null;

        public IEnumerable<Vector3> Vertices
        {
            get
            {
                foreach (var v in PrevBorder?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;

                foreach (var v in LeftWay?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;

                foreach (var v in NextBorder?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;

                foreach (var v in RightWay?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;
            }
        }

        public RoadNetworkLane(RoadNetworkWay leftWay, RoadNetworkWay rightWay, RoadNetworkWay startBorder, RoadNetworkWay endBorder)
        {
            LeftWay = leftWay;
            RightWay = rightWay;
            PrevBorder = startBorder;
            NextBorder = endBorder;
        }

        //デシリアライズの為に必要
        public RoadNetworkLane() { }

        // 向き反転させる
        public void Reverse()
        {
            (PrevBorder, NextBorder) = (NextBorder?.ReversedWay(), PrevBorder?.ReversedWay());
            (LeftWay, RightWay) = (RightWay?.ReversedWay(), LeftWay?.ReversedWay());

            (NextLanes, PrevLanes) = (PrevLanes, NextLanes);

        }

        /// <summary>
        /// LaneをsplitNumで分割する
        /// </summary>
        /// <param name="splitNum"></param>
        /// <returns></returns>
        public List<RoadNetworkLane> SplitLane(int splitNum)
        {
            if (IsValidWay == false)
                return null;

            if (splitNum <= 1)
                return new List<RoadNetworkLane> { this };

            // #TODO : とりあえず２分割のみ対応
            // Borderの中心を開始点とする
            var startSubWays = PrevBorder.Split(2);
            if (startSubWays.Any() == false)
                return null;

            var endSubWays = NextBorder.Split(2);
            if (endSubWays.Any() == false)
                return null;
            // #TODO : とりあえず２分割のみ対応
            var startPoint = startSubWays[0].Last();
            var endPoint = endSubWays[0].Last();

            // このBorderから出ていくWayすべてを使って中心線を書く
            var targetWays = new List<RoadNetworkWay> { LeftWay, RightWay };

            var candidates = new List<Vector3>();
            foreach (var way in targetWays)
            {
                for (var i = 1; i < way.Count - 1; ++i)
                {
                    var v = way[i];
                    var n = -way.GetVertexNormal(i).normalized;
                    var ray = new Ray(v + n * 0.01f, n);
                    if (GeoGraph2d.PolygonHalfLineIntersectionXZ(Vertices, ray, out var inter, out var t))
                        candidates.Add(Vector3.Lerp(v, inter, 0.5f));
                }
            }

            var centerLineVertices = new List<Vector3> { startPoint };
            while (candidates.Count > 0)
            {
                var before = centerLineVertices.Last();
                var found = candidates
                    // LaneがものすごいUターンしていたりする時の対応
                    // beforeから直接いけないものは無視
                    .Where(c => targetWays.All(w => w.SegmentIntersectionXz(before, c, out var _) == false))
                    .TryFindMin(x => (x - before).sqrMagnitude, out var nearPoint);

                if (found == false)
                {
                    //Assert.IsTrue(found, "center point not found");
                    DebugUtil.DrawArrow(before, before + Vector3.up * 2, arrowSize: 1f, duration: 30f, bodyColor: Color.blue);
                    foreach (var c in candidates)
                        DebugUtil.DrawArrow(c, c + Vector3.up * 100, arrowSize: 1f, duration: 30f, bodyColor: Color.red);
                    break;
                }

                centerLineVertices.Add(nearPoint);
                candidates.RemoveAll(x => (x - nearPoint).sqrMagnitude <= RoadNetworkModel.Epsilon);
            }

            centerLineVertices.Add(endPoint);

            // 自己交差があれば削除する
            GeoGraph2d.RemoveSelfCrossing(centerLineVertices, t => t.Xz(), (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));

            var centerLine = RoadNetworkLineString.Create(centerLineVertices);

            var leftLane = new RoadNetworkLane(LeftWay, new RoadNetworkWay(centerLine, false, true), startSubWays[0], endSubWays[0]);
            var rightLane = new RoadNetworkLane(new RoadNetworkWay(centerLine, false, false), RightWay, startSubWays[1], endSubWays[1]);

            return new List<RoadNetworkLane> { leftLane, rightLane };
        }

        /// <summary>
        /// Xz平面だけで見たときの, 線分(st, en)との最も近い交点を返す
        /// </summary>
        /// <param name="st"></param>
        /// <param name="en"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool SegmentIntersectionXz(Vector3 st, Vector3 en, out Vector3 intersection)
        {
            return GeoGraph2d.PolygonSegmentIntersectionXZ(Vertices, st, en, out intersection, out var t);
        }

        public static RoadNetworkLane CreateOneWayLane(RoadNetworkWay way)
        {
            return new RoadNetworkLane(way, null, null, null);
        }

    }

}