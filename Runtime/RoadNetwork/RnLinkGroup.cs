using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 実際にデータ化されるものではない
    /// Node -> Nodeを繋ぐ複数のLinkをまとめるクラス
    /// </summary>
    [Serializable]
    public class RnLinkGroup
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        /// <summary>
        /// 開始ノード
        /// </summary>
        [field: SerializeField]
        public RnNode PrevNode { get; private set; }

        /// <summary>
        /// 終了ノード
        /// </summary>
        [field: SerializeField]
        public RnNode NextNode { get; private set; }

        [SerializeField]
        private List<RnLink> links;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnLink> Links => links;

        /// <summary>
        /// 有効なLinkGroupかどうか
        /// </summary>
        public bool IsValid => Links.All(l => l.IsValid);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="prevNode"></param>
        /// <param name="nextNode"></param>
        /// <param name="links"></param>
        public RnLinkGroup(RnNode prevNode, RnNode nextNode, IEnumerable<RnLink> links)
        {
            PrevNode = prevNode;
            NextNode = nextNode;
            this.links = links.ToList();
            // Linkの向きをそろえる
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
            if (Links.Any() == false)
                return 0;

            return links.Select(l => l.GetLeftLaneCount()).Min();
        }

        /// <summary>
        /// 右側レーン数
        /// </summary>
        /// <returns></returns>
        public int GetRightLaneCount()
        {
            // 向きをそろえる
            Align();
            if (Links.Any() == false)
                return 0;
            return links.Select(l => l.GetRightLaneCount()).Min();
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

        private Dictionary<RnLink, List<RnLane>> SplitLane(int num)
        {
            if (num <= 0)
                return null;
            // 向きをそろえる
            Align();
            var mergedBorders = Links.Select(l => l.GetMergedBorder(RnLaneBorderType.Prev)).ToList();
            mergedBorders.Add(Links[^1].GetMergedBorder(RnLaneBorderType.Next));

            var borderWays = new List<List<RnWay>>(Links.Count + 1);

            foreach (var b in mergedBorders)
            {
                var split = b.Split(num, false);
                borderWays.Add(split);
            }

            var afterLanes = new Dictionary<RnLink, List<RnLane>>(Links.Count);
            for (var i = 0; i < Links.Count; ++i)
            {
                var link = Links[i];
                var prevBorders = borderWays[i];
                var nextBorders = borderWays[i + 1];
                var leftWay = link.GetMergedSideWay(RnDir.Left);
                var rightWay = link.GetMergedSideWay(RnDir.Right);

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
                        foreach (var s in segments)
                            line.AddPointOrSkip(new RnPoint(s.Segment.Start), ep);
                        line.AddPointOrSkip(nextBorder.GetPoint(-1), ep);
                        right = new RnWay(line, false, true);
                    }

                    var l = new RnWay(left.LineString, left.IsReversed, false);
                    var r = new RnWay(right.LineString, right.IsReversed, true);
                    var newLane = new RnLane(l, r, prevBorders[n], nextBorders[n]);
                    lanes.Add(newLane);
                    left = right;
                }

                afterLanes[link] = lanes;
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
            if (Links.All(l => l.GetLeftLaneCount() == leftCount && l.GetRightLaneCount() == rightCount))
                return;

            // 向きをそろえる
            Align();

            var num = leftCount + rightCount;
            var afterLanes = SplitLane(num);
            if (afterLanes == null)
                return;

            // Linksに変更を加えるのは最後にまとめて必要がある
            // (RnLinks.IsLeftLane等が隣のLinkに依存するため. 途中で変更すると、後続の処理が破綻する可能性がある)
            for (var i = 0; i < Links.Count; ++i)
            {
                var link = Links[i];
                var lanes = afterLanes[link];

                if (i == Links.Count - 1)
                    NextNode?.ReplaceBorder(Links[^1], lanes.Select(l => l.NextBorder).ToList());
                if (i == 0)
                    PrevNode?.ReplaceBorder(Links[0], lanes.Select(l => l.PrevBorder).ToList());
                for (var j = leftCount; j < lanes.Count; ++j)
                    lanes[j].Reverse();


                Links[i].ReplaceLanes(lanes);
            }

            if (leftCount > 0 && rightCount > 0)
            {
                CreateMedian();
            }
            else
            {
                foreach (var l in Links)
                    l.SetMedianLane(null);
            }
        }

        /// <summary>
        /// 左側レーン数を変更する
        /// </summary>
        /// <param name="count"></param>
        public void SetLeftLaneCount(int count)
        {
            SetLaneCount(count, GetRightLaneCount());
        }

        /// <summary>
        /// 右側レーン数を変更する
        /// </summary>
        /// <param name="count"></param>
        public void SetRightLaneCount(int count)
        {
            SetLaneCount(GetLeftLaneCount(), count);
        }

        /// <summary>
        /// 中央分離帯を作成するかスキップする
        /// </summary>
        private void CreateMedian()
        {
            Dictionary<RnPoint, RnPoint> replace = new Dictionary<RnPoint, RnPoint>();
            foreach (var l in Links)
            {
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
                if (Links.Count <= 1)
                    return true;
                var src = Links[0];
                for (var i = 1; i < Links.Count; ++i)
                {
                    // 自分のPrevがi-1番目のLinksじゃない場合は向きが逆
                    if (Links[i].Prev != src)
                        return false;
                    // #TODO : laneのborderの向きも見る
                    src = Links[i];
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
            var src = Links[0];
            for (var i = 1; i < Links.Count; ++i)
            {
                if (Links[i].Prev != src)
                    Links[i].Reverse();
                src = Links[i];
            }

            // 境界線の向きもそろえる
            foreach (var l in Links)
                l.AlignLaneBorder();

            if (Links[0].Prev != PrevNode)
                (PrevNode, NextNode) = (NextNode, PrevNode);
        }

        // ---------------
        // Static Methods
        // ---------------
        /// <summary>
        /// 2つのノードを繋ぐLinkGroupを作成する
        /// 2つのノードが直接繋がっていない場合はnullを返す
        /// </summary>
        /// <param name="prevNode"></param>
        /// <param name="nextNode"></param>
        /// <returns></returns>
        public static RnLinkGroup CreateLinkGroupOrDefault(RnNode prevNode, RnNode nextNode)
        {
            if (prevNode == null || nextNode == null)
                return null;

            foreach (var n in prevNode.GetNeighborRoads())
            {
                if (n is RnLink link)
                {
                    var ret = new RnLinkGroup(prevNode, nextNode, new[] { link });
                    var hasPrev = ret.PrevNode == prevNode || ret.NextNode == prevNode;
                    var hasNex = ret.PrevNode == nextNode || ret.NextNode == nextNode;
                    if (hasPrev && hasNex)
                        return ret;
                }
            }

            return null;
        }

    }
}