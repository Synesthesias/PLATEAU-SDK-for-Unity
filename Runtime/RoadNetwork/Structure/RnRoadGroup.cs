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

        /// <summary>
        /// dirで指定した側のレーンをnum個に分割する.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="dir"></param>
        /// <param name="getSplitRate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 中央分離帯を考慮したレーン分割
        /// </summary>
        /// <param name="leftCount"></param>
        /// <param name="rightCount"></param>
        /// <param name="medianWidthRate"></param>
        public void SetLaneCountWithMedian(int leftCount, int rightCount, float medianWidthRate)
        {
            if (IsValid == false)
                return;

            // 両方とも0はダメ
            if (leftCount <= 0 && rightCount <= 0)
            {
                DebugEx.LogWarning($"両方の車線数に0を入れることはできません");
                return;
            }


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

                // 右車線の場合は反対にする
                for (var j = leftCount + 1; j < lanes.Count; ++j)
                    lanes[j].Reverse();

                var median = lanes[leftCount];
                lanes.RemoveAt(leftCount);
                road.ReplaceLanes(lanes);
                road.SetMedianLane(median);
            }
        }
        public void SetLaneCount(int leftCount, int rightCount)
        {
            if (IsValid == false)
                return;

            // 両方とも0はダメ
            if (leftCount <= 0 && rightCount <= 0)
            {
                DebugEx.LogWarning($"両方の車線数に0を入れることはできません");
                return;
            }

            // 向きをそろえる
            Align();

            var nowLeft = GetLeftLaneCount();
            var nowRight = GetRightLaneCount();

            // すでにどっちのレーンもある場合
            // or 0個の車線の道路の指定が0のままの場合は独立して分割する
            if ((nowLeft > 0 || leftCount == 0) &&
                (nowRight > 0 || rightCount == 0))
            {
                SetLeftLaneCount(leftCount);
                SetRightLaneCount(rightCount);
                return;
            }

            SetLaneCountWithoutMedian(leftCount, rightCount);
        }

        /// <summary>
        /// レーン数を変更する. 中央分離帯は削除する
        /// </summary>
        /// <param name="leftCount"></param>
        /// <param name="rightCount"></param>
        private void SetLaneCountWithoutMedian(int leftCount, int rightCount)
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
                    NextIntersection?.ReplaceBorder(road, lanes.Select(l => l.NextBorder).ToList());
                if (i == 0)
                    PrevIntersection?.ReplaceBorder(road, lanes.Select(l => l.PrevBorder).ToList());
                // 右車線の分
                for (var j = leftCount; j < lanes.Count; ++j)
                    lanes[j].Reverse();

                road.ReplaceLanes(lanes);
            }

            // 中央分離帯を削除する
            foreach (var l in Roads)
                l.SetMedianLane(null);
        }



        /// <summary>
        /// 中央分離帯の幅を設定する. 非推奨. 個別にWayを動かすことを推奨
        /// </summary>
        /// <param name="width"></param>
        /// <param name="moveOption"></param>
        // [Obsolete("非推奨. 個別にWayを動かす or ExpandMedianWidthを使うこと")]
        public bool SetMedianWidth(float width, LaneWayMoveOption moveOption)
        {
            if (HasMedian() == false)
                return false;

            Align();
            foreach (var road in Roads)
            {
                var nowWidth = road.GetMedianWidth();
                var deltaWidth = width - nowWidth;
                //// サイズ0の中央分離帯だと法線方向が定まらないので、すでに幅の存在するレーンを動かすことで
                //// 中央分離帯の幅を設定する
                var centerLeft = road.GetLeftLanes().Last();
                var centerRight = road.GetRightLanes().First();
                switch (moveOption)
                {
                    case LaneWayMoveOption.MoveBothWay:
                        centerLeft.RightWay?.MoveAlongNormal(-deltaWidth * 0.5f);
                        centerRight.RightWay?.MoveAlongNormal(-deltaWidth * 0.5f);
                        break;
                    case LaneWayMoveOption.MoveLeftWay:
                        centerLeft.RightWay?.MoveAlongNormal(-deltaWidth);
                        break;
                    case LaneWayMoveOption.MoveRightWay:
                        centerRight.RightWay?.MoveAlongNormal(-deltaWidth);
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// 中央分離帯の幅を拡縮する. deltaWidthが正なら幅が増加する
        /// </summary>
        /// <param name="deltaWidth"></param>
        /// <param name="moveOption"></param>
        /// <returns></returns>
        public bool ExpandMedianWidth(float deltaWidth, LaneWayMoveOption moveOption)
        {
            if (HasMedian() == false)
                return false;
            Align();
            foreach (var road in Roads)
            {
                //// サイズ0の中央分離帯だと法線方向が定まらないので、すでに幅の存在するレーンを動かすことで
                //// 中央分離帯の幅を設定する
                var centerLeft = road.GetLeftLanes().Last();
                var centerRight = road.GetRightLanes().First();
                switch (moveOption)
                {
                    case LaneWayMoveOption.MoveBothWay:
                        centerLeft.RightWay?.MoveAlongNormal(-deltaWidth * 0.5f);
                        centerRight.RightWay?.MoveAlongNormal(-deltaWidth * 0.5f);
                        break;
                    case LaneWayMoveOption.MoveLeftWay:
                        centerLeft.RightWay?.MoveAlongNormal(-deltaWidth);
                        break;
                    case LaneWayMoveOption.MoveRightWay:
                        centerRight.RightWay?.MoveAlongNormal(-deltaWidth);
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// 中央分離帯を削除する
        /// </summary>
        /// <param name="moveOption"></param>
        public void RemoveMedian(LaneWayMoveOption moveOption = LaneWayMoveOption.MoveBothWay)
        {
            if (IsValid == false)
            {
                DebugEx.LogError($"不正な道路に対してRemoveMedianが呼ばれました");
                return;
            }

            foreach (var road in Roads)
            {
                if (road.MedianLane == null)
                    continue;

                // 中央線との境界の左車線を取得
                var leftLane = road.GetLeftLanes().LastOrDefault();
                var rightLane = road.GetRightLanes().FirstOrDefault();
                if (leftLane == null || rightLane == null)
                {
                    DebugEx.LogError($"中央分離帯があるのに, 片側車線しかない道路です");
                    continue;
                }

                var borderTypes = new[] { RnLaneBorderType.Prev, RnLaneBorderType.Next };

                // laneの境界線に中央分離帯の境界線を追加する
                // dirは境界線を取得するときの方向
                // 左車線の場合は左->右の方向でとってきて後ろに追加する
                // 右車線の場合は右->左の方向でとってきて後ろに追加する
                void MergeBorder(RnLane lane, RnLaneBorderDir dir)
                {
                    var median = road.MedianLane;
                    foreach (var borderType in borderTypes)
                    {
                        // 左車線に追加するときも右車線に追加するときも
                        // 必ず中央分離帯は右側にあるのでそれ前提
                        // レーンの境界線をLeft->Right方向で求める
                        var way = road.GetBorderWay(lane, borderType, dir);
                        var medianWay = road.GetBorderWay(median, borderType, dir);
                        way.Append2LineString(medianWay);
                    }
                }

                switch (moveOption)
                {
                    case LaneWayMoveOption.MoveLeftWay:
                        {
                            leftLane.SetSideWay(RnDir.Right, rightLane.RightWay.ReversedWay());
                            MergeBorder(leftLane, RnLaneBorderDir.Left2Right);

                            break;
                        }
                    case LaneWayMoveOption.MoveRightWay:
                        {
                            rightLane.SetSideWay(RnDir.Right, leftLane.RightWay.ReversedWay());
                            MergeBorder(rightLane, RnLaneBorderDir.Right2Left);
                            break;
                        }
                    case LaneWayMoveOption.MoveBothWay:
                        {
                            var median = road.MedianLane;
                            foreach (var borderType in borderTypes)
                            {
                                var mWay = road.GetBorderWay(median, borderType, RnLaneBorderDir.Left2Right);
                                var mWays = mWay.Split(2, false);
                                var leftBorder = road.GetBorderWay(leftLane, borderType, RnLaneBorderDir.Left2Right);
                                leftBorder.Append2LineString(mWays[0]);

                                var rightBorder = road.GetBorderWay(rightLane, borderType, RnLaneBorderDir.Right2Left);
                                rightBorder.Append2LineString(mWays[1].ReversedWay());
                            }
                            var left = leftLane.RightWay;
                            var right = rightLane.RightWay.ReversedWay();

                            // leftLaneから中央分離帯側の端点を取得
                            RnPoint GetEdgePoint(RnLaneBorderType type)
                            {
                                if (leftLane.GetBorderDir(type) == RnLaneBorderDir.Left2Right)
                                {
                                    // 左 -> 右の場合は最後の点
                                    return leftLane.GetBorder(type).GetPoint(-1);
                                }
                                else
                                {
                                    // 右 -> 左の場合は最初の点
                                    return leftLane.GetBorder(type).GetPoint(0);
                                }
                            }

                            var centerLine = RnEx.CreateInnerLerpLineString(
                                left.Vertices.ToList()
                                , right.Vertices.ToList()
                                , GetEdgePoint(RnLaneBorderType.Prev)
                                , GetEdgePoint(RnLaneBorderType.Next)
                                , RnWayEx.CreateMergedWay(leftLane.PrevBorder, rightLane.PrevBorder)
                                , RnWayEx.CreateMergedWay(leftLane.NextBorder, rightLane.NextBorder)
                                , 0.5f);

                            leftLane.SetSideWay(RnDir.Right, new RnWay(centerLine, false, true));
                            rightLane.SetSideWay(RnDir.Right, new RnWay(centerLine, true, true));
                            break;
                        }

                }
                road.SetMedianLane(null);
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

            // 左車線が無い場合は全車線含めて変更する
            if (GetLeftLaneCount() == 0 || count == 0)
            {
                SetLaneCountWithoutMedian(count, GetRightLaneCount());
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

            // 右車線が無い場合は全車線含めて変更する
            if (GetRightLaneCount() == 0 || count == 0)
            {
                SetLaneCountWithoutMedian(GetLeftLaneCount(), count);
            }
            // すでに右車線がある場合はそれだけで変更する
            else
            {
                SetLaneCount(count, RnDir.Right);
            }
        }

        /// <summary>
        /// まだ中央分離帯がない場合は作成する.
        /// medianWidthは作成時の中央分離帯の幅. 最低1m. ただし目安であり、実際の幅は最小のレーン幅に合わせる
        /// </summary>
        /// <param name="medianWidth"></param>
        /// <param name="maxMedianLaneRate">中央分離帯の割合が全体の道のこれを超えないようにする</param>
        public bool CreateMedianOrSkip(float medianWidth = 1f, float maxMedianLaneRate = 0.5f)
        {
            // 全ての道路に中央分離帯がある場合は何もしない
            if (HasMedian())
                return false;
            // 両方にレーンが無いと作成しない
            if (GetLeftLaneCount() == 0 || GetRightLaneCount() == 0)
            {
                DebugEx.LogWarning($"中央分離帯を作成するには左右の車線が必要");
                return false;
            }

            medianWidth = Mathf.Max(1, medianWidth);
            // 1番小さい幅レーンの道幅が1[m]になるように中央分離帯を作成する
            var width = Roads.Min(r => r.AllLanesWithMedian.Sum(l => l.CalcWidth()));
            var medianRate = Mathf.Min(maxMedianLaneRate, medianWidth / width);
            SetLaneCountWithMedian(GetLeftLaneCount(), GetRightLaneCount(), medianRate);
            return true;
        }

        /// <summary>
        /// 中央分離帯があるかどうか.
        /// 構成するすべての道路に中央分離帯がある場合にtrueを返す
        /// </summary>
        /// <returns></returns>
        public bool HasMedian()
        {
            return Roads.All(r => r.MedianLane != null);
        }

        /// <summary>
        /// 中央を通るスプラインとその幅を取得する
        /// 幅は中間点から取得するので完璧な値ではない
        /// </summary>
        /// <param name="spline"></param>
        /// <param name="width"></param>
        /// <param name="tangentLength"></param>
        /// <param name="pointSkipDistance"></param>
        /// <returns></returns>
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

                HashSet<float> indices = new();
                foreach (var p in leftWay.Points)
                {
                    line.GetNearestPoint(p.Vertex, out var v, out var index, out var distance);
                    indices.Add(index);
                }

                foreach (var p in rightWay.Points)
                {
                    line.GetNearestPoint(p.Vertex, out var v, out var index, out var distance);
                    indices.Add(index);
                }

                foreach (var i in Enumerable.Range(0, line.Count))
                    indices.Add(i);

                // 左右それぞれで最も小さい幅の２倍にする
                // 各点に置けるwl+wrの最小値だと、wl << wrの場合があったりすると中心線をずらす必要があるので苦肉の策
                var leftWidth = float.MaxValue;
                var rightWidth = float.MaxValue;
                foreach (var i in indices)
                {
                    var v = line.GetLerpPoint(i);
                    leftWay.LineString.GetNearestPoint(v, out var nl, out var il, out var wl);
                    leftWidth = Mathf.Min(leftWidth, wl);
                    rightWay.LineString.GetNearestPoint(v, out var nr, out var ir, out var wr);
                    rightWidth = Mathf.Min(rightWidth, wr);
                }
                width = Mathf.Min(Mathf.Min(rightWidth, leftWidth) * 0.5f, width);
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

                for (var i = 0; i < Roads.Count; ++i)
                {
                    // 自分のNextがi+1番目じゃない場合は向きが逆
                    if (i < Roads.Count - 1 && Roads[i].Next != Roads[i + 1])
                        return false;

                    // 自分のPrevがi-1番目のRoadsじゃない場合は向きが逆
                    if (i > 0 && Roads[i].Prev != Roads[i - 1])
                        return false;
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


            // まずはRoadsのPrev/Nextの向きをそろえる
            // Roads.Count <= 1の場合はIsAligned=trueなのでここでは
            // インデックス範囲外チェックはしなくてよい
            // 0番目が逆かどうかチェック
            if (Roads[0].Next != Roads[1])
                Roads[0].Reverse();
            // 1番目以降が逆かどうかチェック
            for (var i = 1; i < Roads.Count; ++i)
            {
                // 自分のPrevがi-1番目のRoadsじゃない場合は向きが逆
                if (Roads[i].Prev != Roads[i - 1])
                    Roads[i].Reverse();
            }

            // 次にLaneのPrev/Nextの向きをそろえる
            // 0番目を基準にして, 1番目以降の道路をひとつ前の道路に合わせていく

            // １車線しかない道路の場合それが左車線になるようにそろえるようにする
            if (Roads[0].MainLanes.Count == 0 && Roads[0].GetLeftLaneCount() == 0)
            {
                foreach (var lane in Roads[0].AllLanesWithMedian)
                    lane.Reverse();
            }
            for (var i = 1; i < Roads.Count; ++i)
            {
                var nowLanes = Roads[i].AllLanesWithMedian.ToList();
                var prevLanes = Roads[i - 1].AllLanesWithMedian.ToList();
                for (var j = 0; j < Mathf.Min(nowLanes.Count, prevLanes.Count); ++j)
                {
                    if (nowLanes[j].PrevBorder.IsSameLine(prevLanes[j].NextBorder) == false)
                    {
                        // #TODO : この結果RoadsのMainLanesの中身が左->右の規則が崩れないかのチェックが必要
                        nowLanes[j].Reverse();
                    }
                }
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