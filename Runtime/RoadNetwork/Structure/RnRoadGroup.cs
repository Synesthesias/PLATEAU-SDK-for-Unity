using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

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

        private Dictionary<RnRoad, List<RnLane>> SplitLane(int num, RnDir? dir, Func<int, float> getSplitRate = null)
        {
            if (num <= 0)
                return null;

            var mergedBorders = Roads.Select(l => l.GetMergedBorder(RnLaneBorderType.Prev, dir)).ToList();
            mergedBorders.Add(Roads[^1].GetMergedBorder(RnLaneBorderType.Next, dir));

            var borderWays = new List<List<RnWay>>(Roads.Count + 1);

            foreach (var b in mergedBorders)
            {
                var split = b.Split(num, false, getSplitRate);
                borderWays.Add(split);
            }

            var afterLanes = new Dictionary<RnRoad, List<RnLane>>(Roads.Count);
            for (var i = 0; i < Roads.Count; ++i)
            {
                var road = Roads[i];
                var prevBorders = borderWays[i];
                var nextBorders = borderWays[i + 1];

                road.TryGetMergedSideWay(dir, out var leftWay, out var rightWay);

                var leftVertices = leftWay.Vertices.ToList();
                var rightVertices = rightWay.Vertices.ToList();
                var left = leftWay;
                var lanes = new List<RnLane>(num);
                var rate = 0f;
                for (var n = 0; n < num; ++n)
                {
                    var right = rightWay;
                    if (n < num - 1)
                    {
                        rate += getSplitRate?.Invoke(n) ?? (1f / num);
                        var prevBorder = prevBorders[n];
                        var nextBorder = nextBorders[n];
                        var line = RnEx.CreateInnerLerpLineString(
                            leftVertices
                            , rightVertices
                            , prevBorder.GetPoint(-1)
                            , nextBorder.GetPoint(-1)
                            , prevBorder
                            , nextBorder,
                            rate);

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


        private void SetLaneCount(int count, RnDir dir)
        {
            if (IsValid == false)
                return;

            // 既に指定の数になっている場合は何もしない
            if (Roads.All(l => l.GetLanes(dir).Count() == count))
                return;
            // 向きをそろえる
            Align();

            var afterLanes = SplitLane(count, dir);
            if (afterLanes == null)
                return;

            // Roadsに変更を加えるのは最後にまとめて必要がある
            // (RnRoads.IsLeftLane等が隣のRoadに依存するため. 途中で変更すると、後続の処理が破綻する可能性がある)
            for (var i = 0; i < Roads.Count; ++i)
            {
                var road = Roads[i];
                var lanes = afterLanes[road];

                var beforeLanes = road.GetLanes(dir).ToList();
                if (i == Roads.Count - 1 && NextIntersection != null)
                {
                    foreach (var l in beforeLanes)
                        NextIntersection.RemoveNeighbor(road, l);
                    foreach (var l in lanes)
                        NextIntersection.AddEdge(road, l.NextBorder);
                }
                if (i == 0 && PrevIntersection != null)
                {
                    foreach (var l in beforeLanes)
                        PrevIntersection.RemoveNeighbor(road, l);
                    foreach (var l in lanes)
                        PrevIntersection.AddEdge(road, l.PrevBorder);
                }

                // 右車線の場合は反対にする
                // #NOTE : 隣接情報変更後に反転させる
                if (dir == RnDir.Right)
                {
                    foreach (var l in lanes)
                        l.Reverse();
                }

                Roads[i].ReplaceLanes(lanes, dir);
            }
        }

        public void SetLaneCountWithMedian(int leftCount, int rightCount, float medianWidthRate)
        {
            if (IsValid == false)
                return;
            // 向きをそろえる
            Align();

            var num = leftCount + rightCount + 1;
            var laneRate = (1f - medianWidthRate) / (num - 1);

            var afterLanes = SplitLane(num, null, i =>
            {
                if (i == leftCount)
                    return medianWidthRate;
                return laneRate;
            });
            //var afterLanes = SplitLane(num, null);
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

                for (var j = leftCount + 1; j < lanes.Count; ++j)
                    lanes[j].Reverse();

                var median = lanes[leftCount];
                lanes.RemoveAt(leftCount);
                Roads[i].ReplaceLanes(lanes);
                Roads[i].SetMedianLane(median);
            }
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
            var afterLanes = SplitLane(num, null);
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
                //// サイズ0の中央分離帯だと法線方向が定まらないので、すでに幅の存在するレーンを動かすことで
                //// 中央分離帯の幅を設定する
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

                //l.MedianLane?.TrySetWidth(width, moveOption);
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

            // 左車線が無い場合は左車線のサイズも含めて変更する
            if (GetLeftLaneCount() == 0)
            {
                SetLaneCount(count, GetRightLaneCount());
            }
            // すでに左車線がある場合はそれだけで変更する
            else
            {
                SetLaneCount(count, RnDir.Left);
            }
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

            // 右車線が無い場合は左車線のサイズも含めて変更する
            if (GetRightLaneCount() == 0)
            {
                SetLaneCount(GetLeftLaneCount(), count);
            }
            // すでに右車線がある場合はそれだけで変更する
            else
            {
                SetLaneCount(count, RnDir.Right);
            }
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

                var prevBorder = new RnWay(prevLineString, isReverseNormal: true);
                var nextBorder = new RnWay(nextLineString);
                var medianLane = new RnLane(leftWay, rightWay, prevBorder, nextBorder);
                l.SetMedianLane(medianLane);

                // 交差点側にも情報を埋め込む(外形が崩れるので)
                if (l.Prev is RnIntersection prevInter)
                {
                    prevInter.AddEdge(l, prevBorder.ReversedWay());
                    prevInter.Align();
                }

                if (l.Next is RnIntersection nextInter)
                {
                    nextInter.AddEdge(l, nextBorder.ReversedWay());
                    nextInter.Align();
                }

            }
        }

        public bool TryCreateSpline(out Spline spline, out float width, float tangentLength = 1f, float pointSkipDistance = 1e-3f)
        {
            Align();
            spline = new Spline();
            width = float.MaxValue;
            var points = new List<Vector3>();
            foreach (var r in Roads)
            {
                if (r.TryGetMergedSideWay(null, out var leftWay, out var rightWay) == false)
                    return false;
                var prevBorder = r.GetMergedBorder(RnLaneBorderType.Prev, null);
                var nextBorder = r.GetMergedBorder(RnLaneBorderType.Next, null);
                var start = new RnPoint(prevBorder.GetLerpPoint(0.5f));
                var end = new RnPoint(nextBorder.GetLerpPoint(0.5f));
                var line = RnEx.CreateInnerLerpLineString(leftWay.Vertices.ToList(), rightWay.Vertices.ToList(), start, end, prevBorder, nextBorder, 0.5f, pointSkipDistance);

                points.AddRange(line.Points.Select(p => p.Vertex));
                width = Mathf.Min(width, prevBorder.CalcLength(), nextBorder.CalcLength());
                // #TODO : 途中のポイントごとに幅を計算する必要がある
            }

            for (var i = 0; i < points.Count; i++)
            {
                var dirIn = (i == 0 ? (points[i + 1] - points[i]) : (points[i] - points[i - 1])).normalized;
                var dirOut = i == (points.Count - 1) ? dirIn : (points[i + 1] - points[i]).normalized;
                spline.Add(new BezierKnot(points[i], dirIn * tangentLength, dirOut * tangentLength));
            }


            return true;
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