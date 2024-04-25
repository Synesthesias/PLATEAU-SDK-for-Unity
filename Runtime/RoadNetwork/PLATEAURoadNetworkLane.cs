using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork
{

    [Serializable]
    public class PLATEAURoadNetworkLane
    {
        // 自分自身を表すインデックス
        [SerializeField] private int laneIndex = -1;
        public int LaneIndex => laneIndex;

        [SerializeField] public PLATEAUCityObjectGroup cityObjectGroup;

        // 構成する頂点
        // ポリゴン的に時計回りの順に格納されている
        [SerializeField]
        public List<Vector3> vertices = new List<Vector3>();

        // 連結しているレーン
        [SerializeField]
        public PLATEAURoadNetworkIndexMap connectedLaneIndices = new PLATEAURoadNetworkIndexMap();

        // レーンを構成する道
        // 左側が若いインデックスになる
        [SerializeField]
        public List<PLATEAURoadNetworkWay> ways = new List<PLATEAURoadNetworkWay>();

        // 他レーンとの境界線
        [SerializeField]
        public List<PLATEAURoadNetworkBorder> borders = new List<PLATEAURoadNetworkBorder>();

        // 中央線
        [SerializeField]
        public List<Vector3> centerLine = new List<Vector3>();

        // 交差点かどうか
        public bool IsIntersection => borders.Count > 2;

        // 行き止まり
        public bool IsDeadEnd => borders.Count == 1;

        // 孤立状態(どこともつながっていない)
        public bool IsIsolated => borders.Count == 0;

        // 不完全な状態かどうか
        public bool isPartial = false;

        // 左側Way
        public PLATEAURoadNetworkWay LeftWay
        {
            get
            {
                var ret = ways.FirstOrDefault();
                // 最初が右側のWayだったら左側のWayは存在しない
                if (ret?.isRightSide ?? false)
                    ret = null;
                return ret;
            }
        }

        // 右側Way
        public PLATEAURoadNetworkWay RightWay
        {
            get
            {
                var ret = ways.LastOrDefault();
                // 最後が左側だったら右側のWayは存在しない
                if ((ret?.isRightSide ?? true) == false)
                    ret = null;
                return ret;
            }

        }

        // #TODO : 連結先/連結元の判断ができるのか？

        // 連結先レーン
        public IEnumerable<int> NextLaneIndices
        {
            get
            {
                return ways.Select(w => w.nextLaneIndex).Where(index => index >= 0).Distinct();
            }
        }

        // 連結元レーン
        public IEnumerable<int> PrevLanes
        {
            get
            {
                return ways.Select(w => w.prevLaneIndex).Where(index => index >= 0).Distinct();

            }
        }

        public PLATEAURoadNetworkLane(int index, PLATEAUCityObjectGroup src)
        {
            laneIndex = index;
            cityObjectGroup = src;
        }

        public Vector3 GetDrawCenterPoint()
        {
            return vertices[0];
        }

        public void AddConnectedLane(int laneIndex)
        {
            connectedLaneIndices.Add(laneIndex);
        }

        // 頂点 startVertexIndex, startVertexIndex + 1で構成される辺の法線ベクトルを返す
        // 道の外側を向いている. 正規化はされていない
        public Vector3 GetEdgeNormal(int startVertexIndex)
        {
            var p0 = vertices[startVertexIndex];
            var p1 = vertices[(startVertexIndex + 1) % vertices.Count];
            // Vector3.Crossは左手系なので逆
            return -Vector3.Cross(Vector3.up, p1 - p0);
        }

        /// <summary>
        /// Laneを構成する頂点の法線ベクトル(外側向き)を返す.
        /// vertices[i-1] -> vertices[i]の辺とvertices[i] -> vertices[i+1]の辺の平均が返る
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int index)
        {
            var n1 = GetEdgeNormal((index - 1 + vertices.Count) % vertices.Count).normalized;
            var n2 = GetEdgeNormal(index).normalized;
            return (n1 + n2) * 0.5f;
        }

        /// <summary>
        /// 道路の左右で方向をそろえる
        /// </summary>
        public void AlignWayDirection()
        {
            // borderを基準にそこを開始点にする
            // ただし、1度変更したWayは無視する(next/prevで必ず2回反転してしまうので)
            // #TODO : 通常の道だと2つのwayはprevLaneIndex/nextLaneIndexは同じ組だが交差点などwayが3つ以上ある場合に正しく同じ方向を向くかは検証できていない
            HashSet<int> visitedBorder = new HashSet<int>();
            foreach (var border in borders)
            {
                // borderを開始地点にする
                Assert.IsTrue(border.neighborLaneIndex >= 0, $"border.neighborLaneIndex {border.neighborLaneIndex} >= 0");
                if (border.neighborLaneIndex < 0)
                    continue;

                // borderが次になっているWayを反転させる
                foreach (var way in ways
                             .Where(w => w.nextLaneIndex == border.neighborLaneIndex && !visitedBorder
                                 .Contains(w.prevLaneIndex))
                        )
                {
                    way.vertices.Reverse();
                    (way.nextLaneIndex, way.prevLaneIndex) = (way.prevLaneIndex, way.nextLaneIndex);
                    way.isRightSide = !way.isRightSide;
                }
                visitedBorder.Add(border.neighborLaneIndex);
            }

            ways.Sort((a, b) =>
            {
                if (a.isRightSide == b.isRightSide)
                    return 0;
                return a.isRightSide ? 1 : -1;
            });
        }
        /// <summary>
        /// Xz平面だけで見たときの, 半直線rayの最も近い交点を返す
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool HalfLineIntersectionXz(Ray ray, out Vector3 intersection)
        {
            return PolygonUtil.PolygonHalfLineIntersectionXZ(vertices, ray, out intersection, out var t);
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
            return PolygonUtil.PolygonSegmentIntersectionXZ(vertices, st, en, out intersection, out var t);
        }

        /// <summary>
        /// 中央線の構築
        /// </summary>
        public void BuildCenterLine()
        {
            centerLine.Clear();
            HashSet<int> visitedBorder = new HashSet<int>();
            for (var borderIndex = 0; borderIndex < borders.Count; borderIndex++)
            {
                if (visitedBorder.Contains(borderIndex))
                    continue;
                visitedBorder.Add(borderIndex);
                var border = borders[borderIndex];
                // 不正なBorderかどうかの判定を入れておく
                Assert.IsTrue(border.neighborLaneIndex >= 0, $"border.neighborLaneIndex {border.neighborLaneIndex} >= 0");
                if (border.neighborLaneIndex < 0)
                    continue;
                // Borderの中心を開始点とする
                if (border.TryGetCenterVertex(out Vector3 startPoint) == false)
                    continue;
                centerLine.Add(startPoint);

                // このBorderから出ていくWayすべてを使って中心線を書く
                var targetWays = ways.Where(w => w.prevLaneIndex == border.neighborLaneIndex).ToList();

                // #TODO : 一旦prev/nextが一致しているものだけが対象. (枝分かれしているような交差点のレーンは対象外)
                if (targetWays.Count != 2 || targetWays[0].nextLaneIndex != targetWays[1].nextLaneIndex)
                    continue;

                // Borderに対してWayが2つないと中心線が取れないので無視する
                // #TODO : 3本以上ある場合が想像できないが要対応
                if (targetWays.Count != 2)
                    continue;

                visitedBorder.Add(targetWays[0].nextLaneIndex);

                var candidates = new List<Vector3>();
                foreach (var way in targetWays)
                {
                    for (var i = 1; i < way.vertices.Count - 1; ++i)
                    {
                        var v = way.vertices[i];
                        var n = -way.GetVertexNormal(i).normalized;
                        var ray = new Ray(v + n * 0.01f, n);
                        if (HalfLineIntersectionXz(ray, out var inter))
                            candidates.Add(Vector3.Lerp(v, inter, 0.5f));
                    }
                }

                while (candidates.Count > 0)
                {
                    var before = centerLine.Last();
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
                        isPartial = true;
                        break;
                    }

                    centerLine.Add(nearPoint);
                    candidates.RemoveAll(x => (x - nearPoint).sqrMagnitude <= float.Epsilon);
                }

                // 構築するwayがすべて目的地が同じ場合、中心線に目的地のBorderを追加する
                if (targetWays.All(t => t.nextLaneIndex == targetWays[0].nextLaneIndex))
                {
                    var lastBorder = borders.FirstOrDefault(e => e.neighborLaneIndex == targetWays[0].nextLaneIndex);
                    if (lastBorder != null)
                    {
                        if (lastBorder.TryGetCenterVertex(out var end))
                            centerLine.Add(end);
                    }

                    visitedBorder.Add(targetWays[0].nextLaneIndex);
                }
            }
            // 自己交差があれば削除する
            PolygonUtil.RemoveSelfCrossing(centerLine, t => t.Xz(), (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));
        }
    }

}