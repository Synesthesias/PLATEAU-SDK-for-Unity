using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    /// <summary>
    /// 実際にデータ化されるものではない
    /// Intersection -> Intersectionを繋ぐ複数のRoadをまとめるクラス
    /// </summary>
    [Serializable]
    public class RnRoadGroup
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        /// <summary>
        /// 開始ノード
        /// </summary>
        [field: SerializeField]
        public RnIntersection PrevIntersection { get; private set; }

        /// <summary>
        /// 終了ノード
        /// </summary>
        [field: SerializeField]
        public RnIntersection NextIntersection { get; private set; }

        [SerializeField]
        private List<RnRoad> roads;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnRoad> Roads => roads;

        /// <summary>
        /// 有効なRoadGroupかどうか
        /// </summary>
        public bool IsValid => Roads.All(l => l.IsValid);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="prevIntersection"></param>
        /// <param name="nextIntersection"></param>
        /// <param name="roads"></param>
        public RnRoadGroup(RnIntersection prevIntersection, RnIntersection nextIntersection, IEnumerable<RnRoad> roads)
        {
            PrevIntersection = prevIntersection;
            NextIntersection = nextIntersection;
            this.roads = roads.ToList();
            // Roadの向きをそろえる
            Align();
        }

        /// <summary>
        /// 左側のレーン数
        /// </summary>
        /// <returns></returns>
        public int GetLeftLaneCount()
        {
            // 向きをそろえる
            Align();
            if (Roads.Any() == false)
                return 0;

            return roads.Select(l => l.GetLeftLaneCount()).Min();
        }

        /// <summary>
        /// 右側レーン数
        /// </summary>
        /// <returns></returns>
        public int GetRightLaneCount()
        {
            // 向きをそろえる
            Align();
            if (Roads.Any() == false)
                return 0;
            return roads.Select(l => l.GetRightLaneCount()).Min();
        }

        public IEnumerable<RnLane> GetRightLanes()
        {
            // 向きをそろえる
            Align();
            if (Roads.Any() == false)
                return null;
            return roads.SelectMany(l => l.GetRightLanes());
        }

        public IEnumerable<RnLane> GetLeftLanes()
        {
            // 向きをそろえる
            Align();
            if (Roads.Any() == false)
                return null;
            return roads.SelectMany(l => l.GetLeftLanes());
        }

        private RnWay ConnectWays(IEnumerable<RnWay> ways)
        {
            var points = new List<RnPoint>();
            foreach (var way in ways)
            {
                foreach (var p in way.Points)
                {
                    if (points.Any() && points.Last() == p)
                        continue;
                    points.Add(p);
                }
            }

            return new RnWay(RnLineString.Create(points), false, false);
        }

        private Dictionary<RnRoad, List<RnLane>> SplitLane(int num)
        {
            if (num <= 0)
                return null;
            // 向きをそろえる
            Align();
            // #DebugId=127でPrev/Nextが逆になっている
            var mergedBorders = Roads.Select(l => l.GetMergedBorder(RnLaneBorderType.Prev)).ToList();
            mergedBorders.Add(Roads[^1].GetMergedBorder(RnLaneBorderType.Next));

            var borderWays = new List<List<RnWay>>(Roads.Count + 1);

            foreach (var b in mergedBorders)
            {
                var split = b.Split(num, false);
                borderWays.Add(split);
            }

            var afterLanes = new Dictionary<RnRoad, List<RnLane>>(Roads.Count);
            for (var i = 0; i < Roads.Count; ++i)
            {
                var road = Roads[i];
                var prevBorders = borderWays[i];
                var nextBorders = borderWays[i + 1];
                var leftWay = road.GetMergedSideWay(RnDir.Left);
                var rightWay = road.GetMergedSideWay(RnDir.Right);

                var leftVertices = leftWay.Vertices.ToList();
                var rightVertices = rightWay.Vertices.ToList();
                var left = leftWay;
                var lanes = new List<RnLane>(num);
                for (var n = 0; n < num; ++n)
                {
                    var right = rightWay;
                    if (n < num - 1)
                    {
                        var ep = 1e-3f;
                        var prevBorder = prevBorders[n];
                        var nextBorder = nextBorders[n];
                        var line = new RnLineString();
                        line.AddPointOrSkip(prevBorder.GetPoint(-1), ep);
                        var segments = GeoGraphEx.GetInnerLerpSegments(leftVertices, rightVertices, AxisPlane.Xz,
                            (1f + n) / num);
                        // 1つ目の点はボーダーと重複するのでスキップ
                        // #TODO : 実際はボーダーよりも外側にあるのはすべてスキップすべき
                        foreach (var s in segments.Skip(1))
                            line.AddPointOrSkip(new RnPoint(s), ep);
                        line.AddPointOrSkip(nextBorder.GetPoint(-1), ep);


                        // 自己交差があれば削除する
                        var plane = AxisPlane.Xz;
                        GeoGraph2D.RemoveSelfCrossing(line.Points
                            , t => t.Vertex.GetTangent(plane)
                            , (p1, p2, p3, p4, inter, f1, f2) => new RnPoint(Vector3.Lerp(p1, p2, f1)));

                        right = new RnWay(line, false, true);
                    }

                    var l = new RnWay(left.LineString, left.IsReversed, false);
                    var r = new RnWay(right.LineString, right.IsReversed, true);
                    var newLane = new RnLane(l, r, prevBorders[n], nextBorders[n]);
                    lanes.Add(newLane);
                    left = right;
                }

                afterLanes[road] = lanes;
            }

            return afterLanes;
        }

        /// <summary>
        /// レーン数を変更する
        /// </summary>
        /// <param name="leftCount"></param>
        /// <param name="rightCount"></param>
        public void SetLaneCount(int leftCount, int rightCount)
        {
            if (IsValid == false)
                return;
            // 既に指定の数になっている場合は何もしない
            if (Roads.All(l => l.GetLeftLaneCount() == leftCount && l.GetRightLaneCount() == rightCount))
                return;

            // 向きをそろえる
            Align();

            var num = leftCount + rightCount;
            var afterLanes = SplitLane(num);
            if (afterLanes == null)
                return;

            // Roadsに変更を加えるのは最後にまとめて必要がある
            // (RnRoads.IsLeftLane等が隣のRoadに依存するため. 途中で変更すると、後続の処理が破綻する可能性がある)
            for (var i = 0; i < Roads.Count; ++i)
            {
                var road = Roads[i];
                var lanes = afterLanes[road];

                if (i == Roads.Count - 1)
                    NextIntersection?.ReplaceBorder(Roads[^1], lanes.Select(l => l.NextBorder).ToList());
                if (i == 0)
                    PrevIntersection?.ReplaceBorder(Roads[0], lanes.Select(l => l.PrevBorder).ToList());
                for (var j = leftCount; j < lanes.Count; ++j)
                    lanes[j].Reverse();

                Roads[i].ReplaceLanes(lanes);
            }

            if (leftCount == 0 || rightCount == 0)
            {
                foreach (var l in Roads)
                    l.SetMedianLane(null);
            }
            else
            {
                // 中央分離帯があるリンクだけ更新する(ない場合はなにもしない
                CreateMedianOrSkip(l => l.MedianLane != null);
            }
        }


        /// <summary>
        /// 中央分離帯の幅を設定する.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="moveOption"></param>
        public bool SetMedianWidth(float width, LaneWayMoveOption moveOption)
        {
            if (GetLeftLaneCount() == 0 || GetRightLaneCount() == 0)
                return false;
            Align();
            // 中央分離帯がないリンクは一度作成する
            CreateMedianOrSkip(l => l.MedianLane == null);
            foreach (var l in Roads)
            {
                // サイズ0の中央分離帯だと法線方向が定まらないので、すでに幅の存在するレーンを動かすことで
                // 中央分離帯の幅を設定する
                var centerLeft = l.GetLeftLanes().Last();
                var centerRight = l.GetRightLanes().First();
                switch (moveOption)
                {
                    case LaneWayMoveOption.MoveBothWay:
                        centerLeft.RightWay?.MoveAlongNormal(-width * 0.5f);
                        centerRight.RightWay?.MoveAlongNormal(-width * 0.5f);
                        break;
                    case LaneWayMoveOption.MoveLeftWay:
                        centerLeft.RightWay?.MoveAlongNormal(-width);
                        break;
                    case LaneWayMoveOption.MoveRightWay:
                        centerRight.RightWay?.MoveAlongNormal(-width);
                        break;
                }

                l.MedianLane?.TrySetWidth(width, moveOption);
            }
            return true;
        }

        /// <summary>
        /// 中央分離帯を削除する
        /// </summary>
        /// <param name="moveOption"></param>
        public void RemoveMedian(LaneWayMoveOption moveOption = LaneWayMoveOption.MoveBothWay)
        {
            SetMedianWidth(0f, moveOption);
            foreach (var road in Roads)
            {
                if (road.MedianLane == null)
                    continue;
                // l,rを同じにするため, r -> lに差し替え(向きをそろえるためにlを反転させる)
                var before = road.MedianLane.RightWay;
                var after = road.MedianLane.LeftWay.ReversedWay();
                foreach (var lane in road.GetRightLanes())
                {
                    if (lane.RightWay?.IsSameLine(before) ?? false)
                    {
                        lane.SetSideWay(RnDir.Right, after);
                    }
                }
            }
        }

        /// <summary>
        /// 左側レーン数を変更する
        /// </summary>
        /// <param name="count"></param>
        public void SetLeftLaneCount(int count)
        {
            // 既に指定の数になっている場合は何もしない
            if (GetLeftLaneCount() == count)
                return;

            SetLaneCount(count, GetRightLaneCount());
        }

        /// <summary>
        /// 右側レーン数を変更する
        /// </summary>
        /// <param name="count"></param>
        public void SetRightLaneCount(int count)
        {
            // 既に指定の数になっている場合は何もしない
            if (GetRightLaneCount() == count)
                return;
            SetLaneCount(GetLeftLaneCount(), count);
        }

        /// <summary>
        /// 中央分離帯を作成するかスキップする
        /// </summary>
        private void CreateMedianOrSkip(Func<RnRoad, bool> createTarget)
        {
            Dictionary<RnPoint, RnPoint> replace = new Dictionary<RnPoint, RnPoint>();
            foreach (var l in Roads)
            {
                if (createTarget != null && createTarget(l) == false)
                    continue;

                var centerLeft = l.GetLeftLanes().Last();
                var rightWay = centerLeft.Replace2Clone(RnDir.Right, true);
                var leftWay = centerLeft.RightWay;
                leftWay = new RnWay(leftWay.LineString, leftWay.IsReversed, !leftWay.IsReverseNormal);
                var st = rightWay.GetPoint(0);
                var en = rightWay.GetPoint(-1);
                var afterSt = replace.GetValueOrCreate(st, r => st.Clone());
                var afterEn = replace.GetValueOrCreate(en, r => en.Clone());

                var prev = centerLeft.GetBorder(RnLaneBorderType.Prev, RnLaneBorderDir.Left2Right);
                var next = centerLeft.GetBorder(RnLaneBorderType.Next, RnLaneBorderDir.Left2Right);
                prev.LineString.ReplacePoint(st, afterSt);
                next.LineString.ReplacePoint(en, afterEn);
                leftWay.SetPoint(0, prev.GetPoint(-1));
                leftWay.SetPoint(-1, next.GetPoint(-1));

                var prevLineString = RnLineString.Create(new[] { afterSt, st }, false);
                var nextLineString = RnLineString.Create(new[] { afterEn, en }, false);
                var medianLane = new RnLane(leftWay, rightWay, new RnWay(prevLineString, isReverseNormal: true), new RnWay(nextLineString));
                l.SetMedianLane(medianLane);
            }
        }

        /// <summary>
        /// 向きがそろっているかどうか
        /// </summary>
        public bool IsAligned
        {
            get
            {
                if (Roads.Count <= 1)
                    return true;
                var src = Roads[0];
                for (var i = 1; i < Roads.Count; ++i)
                {
                    // 自分のPrevがi-1番目のRoadsじゃない場合は向きが逆
                    if (Roads[i].Prev != src)
                        return false;
                    // #TODO : laneのborderの向きも見る
                    src = Roads[i];
                }

                return true;
            }
        }

        /// <summary>
        /// LaneのPrev/Nextの向きをそろえる
        /// </summary>
        public void Align()
        {
            if (IsAligned)
                return;
            var src = Roads[0];
            for (var i = 1; i < Roads.Count; ++i)
            {
                if (Roads[i].Prev != src)
                    Roads[i].Reverse();
                src = Roads[i];
            }

            // 境界線の向きもそろえる
            foreach (var l in Roads)
                l.AlignLaneBorder();

            if (Roads[0].Prev != PrevIntersection)
                (PrevIntersection, NextIntersection) = (NextIntersection, PrevIntersection);
        }

        // ---------------
        // Static Methods
        // ---------------
        /// <summary>
        /// 2つのノードを繋ぐRoadGroupを作成する
        /// 2つのノードが直接繋がっていない場合はnullを返す
        /// </summary>
        /// <param name="prevIntersection"></param>
        /// <param name="nextIntersection"></param>
        /// <returns></returns>
        public static RnRoadGroup CreateRoadGroupOrDefault(RnIntersection prevIntersection, RnIntersection nextIntersection)
        {
            if (prevIntersection == null || nextIntersection == null)
                return null;

            foreach (var n in prevIntersection.GetNeighborRoads())
            {
                if (n is RnRoad road)
                {
                    var ret = new RnRoadGroup(prevIntersection, nextIntersection, new[] { road });
                    var hasPrev = ret.PrevIntersection == prevIntersection || ret.NextIntersection == prevIntersection;
                    var hasNex = ret.PrevIntersection == nextIntersection || ret.NextIntersection == nextIntersection;
                    if (hasPrev && hasNex)
                        return ret;
                }
            }

            return null;
        }

    }
}