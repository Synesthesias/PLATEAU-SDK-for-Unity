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

        private void SetLaneCount(int leftCount, int rightCount)
        {
            if (IsValid == false)
                return;
            // 既に指定の数になっている場合は何もしない
            if (Links.All(l => l.GetLeftLaneCount() == leftCount && l.GetRightLaneCount() == rightCount))
                return;
            // 向きをそろえる
            Align();

            var num = leftCount + rightCount;

            var mergedBorders = Links.Select(l => l.GetMergedBorder(RnLaneBorderType.Prev)).ToList();
            mergedBorders.Add(Links[^1].GetMergedBorder(RnLaneBorderType.Next));

            var borderWays = new List<List<RnWay>>(Links.Count + 1);

            foreach (var b in mergedBorders)
            {
                var split = b.Split(num, false);
                borderWays.Add(split);
            }

            for (var i = 0; i < Links.Count; ++i)
            {
                var link = Links[i];
                var prevBorders = borderWays[i];
                var nextBorders = borderWays[i + 1];
                var leftWay = link.GetMergedSideWay(RnDir.Left);
                var rightWay = link.GetMergedSideWay(RnDir.Right);

                var left = leftWay;
                var lanes = new List<RnLane>(num);
                for (var n = 0; n < num; ++n)
                {
                    var right = rightWay;
                    if (n != num - 1)
                    {
                        var ep = 1e-3f;
                        var prevBorder = prevBorders[n];
                        var nextBorder = nextBorders[n];
                        var line = new RnLineString();
                        line.AddPointOrSkip(prevBorder.GetPoint(-1), ep);
                        var segments = GeoGraphEx.GetInnerLerpSegments(leftWay.Vertices.ToList(), rightWay.Vertices.ToList(), AxisPlane.Xz, (1f + n) / num);
                        foreach (var s in segments)
                            line.AddPointOrSkip(new RnPoint(s.Segment.Start), ep);
                        line.AddPointOrSkip(nextBorder.GetPoint(-1), ep);
                        right = new RnWay(line, false, true);
                    }

                    var l = new RnWay(left.LineString, left.IsReversed, false);
                    var newLane = new RnLane(l, right, prevBorders[n], nextBorders[n]);
                    if (n >= leftCount)
                        newLane.Reverse();
                    lanes.Add(newLane);
                    left = right;
                }

                link.ReplaceLanes(lanes);
            }


        }

        public void SetLeftLaneCount(int count)
        {
            SetLaneCount(count, GetRightLaneCount());
        }

        public void SetRightLaneCount(int count)
        {
            SetLaneCount(GetLeftLaneCount(), count);
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
        }

        public RnLinkGroup(RnNode prevNode, RnNode nextNode, IEnumerable<RnLink> links)
        {
            PrevNode = prevNode;
            NextNode = nextNode;
            this.links = links.ToList();
            // Linkの向きをそろえる
            Align();
        }
    }
}