using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    [System.Flags]
    public enum RnSideWalkWayTypeMask
    {
        None = 0,
        Outside = 1 << 0,
        Inside = 1 << 1,
        StartEdge = 1 << 2,
        EndEdge = 1 << 3,
    }

    /// <summary>
    /// 道路専用. 手動編集で道路の左側/右側どっちに所属するかが取りたいみたいなので専用フラグを用意する.
    /// 交差点だと意味がない
    /// </summary>
    public enum RnSideWalkLaneType
    {
        // 交差点 or その他(デフォルト値)
        Undefined,
        // 左レーン
        LeftLane,
        // 右レーン
        RightLane,
    }

    public partial class RnSideWalk : ARnParts<RnSideWalk>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        private RnRoadBase parentRoad;

        // 道路と反対側のWay
        // シリアライズ化の為にフィールドに
        private RnWay outsideWay;

        // 道路と同じ側のWay
        // シリアライズ化の為にフィールドに
        private RnWay insideWay;

        // outsideWayとinsideWayの始点を繋ぐWay
        // シリアライズ化の為にフィールドに
        private RnWay startEdgeWay;

        // outsideWayとinsideWayの終点を繋ぐWay
        // シリアライズ化の為にフィールドに
        private RnWay endEdgeWay;

        /// <summary>
        /// 親道路に対して左側にあるか右側にあるか(自動生成時だけに存在するので編集すると壊れる可能性あり)
        /// </summary>
        private RnSideWalkLaneType laneType = RnSideWalkLaneType.Undefined;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public RnRoadBase ParentRoad => parentRoad;

        public RnWay OutsideWay => outsideWay;

        public RnWay InsideWay => insideWay;

        public RnWay StartEdgeWay => startEdgeWay;

        public RnWay EndEdgeWay => endEdgeWay;

        public RnSideWalkLaneType LaneType
        {
            get => laneType;
            set => laneType = value;
        }

        /// <summary>
        /// 左右のWay(OutsideWay, InsideWay)を列挙
        /// </summary>
        public IEnumerable<RnWay> SideWays
        {
            get
            {
                if (OutsideWay != null)
                    yield return OutsideWay;
                if (InsideWay != null)
                    yield return insideWay;
            }
        }

        /// <summary>
        /// 開始/終了の境界線のWay(StartEdgeWay, EndEdgeWay)を列挙
        /// </summary>
        public IEnumerable<RnWay> EdgeWays
        {
            get
            {
                if (StartEdgeWay != null)
                    yield return startEdgeWay;
                if (EndEdgeWay != null)
                    yield return endEdgeWay;
            }
        }

        /// <summary>
        /// 全てのWay(nullは含まない)
        /// </summary>
        public IEnumerable<RnWay> AllWays => SideWays.Concat(EdgeWays);

        /// <summary>
        /// Inside/OutsideのWayが両方ともValidかどうか. (Edgeは角の道だとnullの場合もあり得るのでチェックしない)
        /// </summary>
        public bool IsValid => InsideWay.IsValidOrDefault() && OutsideWay.IsValidOrDefault();

        public RnSideWalk() { }

        private RnSideWalk(RnRoadBase parent, RnWay outsideWay, RnWay insideWay, RnWay startEdgeWay, RnWay endEdgeWay, RnSideWalkLaneType laneType)
        {
            this.parentRoad = parent;
            this.outsideWay = outsideWay;
            this.insideWay = insideWay;
            this.startEdgeWay = startEdgeWay;
            this.endEdgeWay = endEdgeWay;
            this.laneType = laneType;
            Align();
        }

        public RnSideWalkWayTypeMask GetValidWayTypeMask()
        {
            var mask = RnSideWalkWayTypeMask.None;
            if (outsideWay != null)
                mask |= RnSideWalkWayTypeMask.Outside;
            if (insideWay != null)
                mask |= RnSideWalkWayTypeMask.Inside;
            if (startEdgeWay != null)
                mask |= RnSideWalkWayTypeMask.StartEdge;
            if (endEdgeWay != null)
                mask |= RnSideWalkWayTypeMask.EndEdge;
            return mask;
        }

        /// <summary>
        /// 強制的に親を変更する. 構造壊れるので扱い注意.
        /// 基本的にはChangeParentを使うこと
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(RnRoadBase parent)
        {
            this.parentRoad = parent;
        }

        /// <summary>
        /// 左右のWayを再設定(使い方によっては構造壊れるので注意)
        /// </summary>
        /// <param name="outsideWay"></param>
        /// <param name="insideWay"></param>
        public void SetSideWays(RnWay outsideWay, RnWay insideWay)
        {
            this.outsideWay = outsideWay;
            this.insideWay = insideWay;
            Align();
        }

        /// <summary>
        /// 境界のWayを再設定(使い方によっては構造壊れるので注意)
        /// </summary>
        /// <param name="startWay"></param>
        /// <param name="endWay"></param>
        public void SetEdgeWays(RnWay startWay, RnWay endWay)
        {
            this.startEdgeWay = startWay;
            this.endEdgeWay = endWay;
            Align();
        }

        /// <summary>
        /// 境界のWayを再設定(使い方によっては構造壊れるので注意)
        /// </summary>
        /// <param name="startWay"></param>
        public void SetStartEdgeWay(RnWay startWay)
        {
            this.startEdgeWay = startWay;
        }

        /// <summary>
        /// 境界のWayを再設定(使い方によっては構造壊れるので注意)
        /// </summary>
        /// <param name="endWay"></param>
        public void SetEndEdgeWay(RnWay endWay)
        {
            this.endEdgeWay = endWay;
        }

        /// <summary>
        /// レーンタイプを入れ替え
        /// </summary>
        public void ReverseLaneType()
        {
            // レーンタイプを入れ替え
            if (LaneType == RnSideWalkLaneType.LeftLane)
                LaneType = RnSideWalkLaneType.RightLane;
            else if (LaneType == RnSideWalkLaneType.RightLane)
                LaneType = RnSideWalkLaneType.LeftLane;
        }

        /// <summary>
        /// InSideWay/OutSideWayの方向を, StartEdgeWay/EndEdgeWayを見て合わせる.
        /// これらのWayがない場合は何もしない
        /// </summary>
        /// <returns></returns>
        public void Align()
        {
            void Impl(RnWay way)
            {
                if (way.IsValidOrDefault() == false)
                    return;

                if (StartEdgeWay.IsValidOrDefault())
                {
                    // StartEdgeWay上にway[0]がある場合は整列されているので何もしない
                    var st = way.GetPoint(0);
                    if (StartEdgeWay.Points.Any(p => p.IsSamePoint(st)))
                        return;

                    // StartEdgeWay上にway[^1]がある場合は逆向きなので反転する
                    var en = way.GetPoint(-1);
                    if (StartEdgeWay.Points.Any(p => p.IsSamePoint(en)))
                    {
                        way.Reverse(true);
                        return;
                    }
                }

                if (EndEdgeWay.IsValidOrDefault())
                {

                    // EndEdgeWay上にway[^1]がある場合は整列されているので何もしない
                    var en = way.GetPoint(-1);
                    if (EndEdgeWay.Points.Any(p => p.IsSamePoint(en)))
                        return;

                    // EndEdgeWay上にway[0]がある場合は逆向きなので反転する
                    var st = way.GetPoint(0);
                    if (EndEdgeWay.Points.Any(p => p.IsSamePoint(st)))
                    {
                        way.Reverse(true);
                        return;
                    }

                }
            }
            Impl(InsideWay);
            Impl(OutsideWay);
        }

        /// <summary>
        /// 不正地チェック
        /// </summary>
        public bool Check()
        {
            return true;
        }

        /// <summary>
        /// 自身とsrcSideWalkが連結している場合に結合する
        /// </summary>
        /// <param name="srcSideWalk"></param>
        /// <returns></returns>
        public bool TryMergeNeighborSideWalk(RnSideWalk srcSideWalk)
        {
            if (srcSideWalk == null)
                return false;
            Align();
            srcSideWalk.Align();

            bool IsMatch(RnWay a, RnWay b)
            {
                return a != null && b != null && a.IsSameLineReference(b);
            }

            static void MergeSideWays(RnSideWalk srcSw, RnSideWalk dstSw)
            {
                // #TODO : 現状の使い方だと問題ないが, 今後OutsideWay同士, InsideWay同士が繋がっているかの保証が無いのでポイント単位でチェックすべき
                dstSw.SetSideWays(
                    RnWayEx.CreateMergedWay(srcSw.OutsideWay, dstSw.OutsideWay, false)
                    , RnWayEx.CreateMergedWay(srcSw.InsideWay, dstSw.InsideWay, false)
                );
                dstSw.SetStartEdgeWay(srcSw.StartEdgeWay);
            }

            //  Dst  -  Src
            // Start - Startで繋がっている場合
            if (IsMatch(StartEdgeWay, srcSideWalk.StartEdgeWay))
            {
                // src側を反転させる
                MergeSideWays(srcSideWalk.ReversedSideWalk(), this);
                return true;
            }
            // Start - Endで繋がっている場合
            else if (IsMatch(StartEdgeWay, srcSideWalk.EndEdgeWay))
            {
                MergeSideWays(srcSideWalk, this);
                return true;
            }
            // End - Endで繋がっている場合
            else if (IsMatch(EndEdgeWay, srcSideWalk.EndEdgeWay))
            {
                Reverse();
                MergeSideWays(srcSideWalk, this);
                Reverse();
                return true;
            }
            // End - Startで繋がっている場合
            else if (IsMatch(EndEdgeWay, srcSideWalk.StartEdgeWay))
            {
                Reverse();
                // src側を反転させる
                MergeSideWays(srcSideWalk.ReversedSideWalk(), this);
                Reverse();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Start-Endを反転させる
        /// </summary>
        /// <returns></returns>
        public void Reverse()
        {
            (startEdgeWay, endEdgeWay) = (endEdgeWay, startEdgeWay);
            Align();
        }

        public RnSideWalk ReversedSideWalk()
        {
            var ret = new RnSideWalk(ParentRoad, OutsideWay, InsideWay, StartEdgeWay, EndEdgeWay, LaneType);
            ret.Reverse();
            return ret;
        }

        /// <summary>
        /// 歩道作成 ParentRoadへの追加も行う
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="outsideWay"></param>
        /// <param name="insideWay"></param>
        /// <param name="startEdgeWay"></param>
        /// <param name="endEdgeWay"></param>
        /// <param name="laneType"></param>
        /// <returns></returns>
        public static RnSideWalk Create(RnRoadBase parent, RnWay outsideWay, RnWay insideWay, RnWay startEdgeWay, RnWay endEdgeWay, RnSideWalkLaneType laneType = RnSideWalkLaneType.Undefined)
        {
            var sideWalk = new RnSideWalk(parent, outsideWay, insideWay, startEdgeWay, endEdgeWay, laneType);
            parent?.AddSideWalk(sideWalk);
            return sideWalk;
        }
    }

    public static class RnSideWalkEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector3 GetCentralVertex(this RnSideWalk self)
        {
            if (self == null)
                return Vector3.zero;
            return Vector3Ex.Centroid(self
                .SideWays
                .Select(w => w.GetLerpPoint(0.5f)));
        }

        /// <summary>
        /// selfとotherが隣接しているかどうか(境界線が重なっているかどうか)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsNeighboring(this RnSideWalk self, RnSideWalk other)
        {
            return self.AllWays.Any(x => other.AllWays.Any(b => x.IsSameLineReference(b)));
        }


        /// <summary>
        /// 歩道swとotherの距離スコアを計算する(近いほど低い)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float? CalcRoadProximityScore(this RnSideWalk self, RnRoadBase other)
        {
            if (other == null || self == null)
                return null;

            // otherを構成するWayのうち比較対象のもののみを取得
            var targetWays = new List<RnWay>();
            if (other is RnRoad road)
            {
                targetWays = road.GetMergedSideWays().ToList();
            }
            else if (other is RnIntersection intersection)
            {
                targetWays = intersection.Edges.Select(e => e.Border).ToList();
            }

            if (targetWays.Count == 0)
                return null;

            // swのInsideWayの各点に対して, targetWaysとの最近傍点を計算し, その平均をスコアとする
            // 最も小さい距離だと道路同士の境界部分に繋がっている歩道がどっちの評価も0になるため全体平均で見る
            float GetNearestDistance(Vector3 v)
            {
                return targetWays.Select(w =>
                {
                    w.GetNearestPoint(v, out var nearest, out float pointIndex, out float distance);
                    return distance;
                }).Min();
            }

            return self.InsideWay
                .Select(GetNearestDistance)
                .Average();
        }
    }
}