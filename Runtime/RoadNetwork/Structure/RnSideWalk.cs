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

    public class RnSideWalk : ARnParts<RnSideWalk>
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
        /// 親からのリンク解除
        /// </summary>
        public void UnLinkFromParent()
        {
            ParentRoad?.RemoveSideWalk(this);
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
        /// 歩道作成
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
            return self.AllWays.Any(x => other.AllWays.Any(b => x.IsSameLine(b)));
        }



        //public static void MergeSideWalk(RnSideWalk srcSw, RnSideWalk dstSw)
        //{
        //    void Merge(bool reverse, Action<RnWay, RnWay> merger)
        //    {
        //        var insideWay = reverse ? srcSw.InsideWay?.ReversedWay() : srcSw.InsideWay;
        //        var outsideWay = reverse ? srcSw.OutsideWay?.ReversedWay() : srcSw.OutsideWay;
        //        if (dstSw.InsideWay != null)
        //        {
        //            if (visited.Contains(dstSw.InsideWay.LineString) == false)
        //            {
        //                merger(dstSw.InsideWay, insideWay);
        //                visited.Add(dstSw.InsideWay.LineString);
        //            }

        //            insideWay = dstSw.InsideWay;
        //        }

        //        if (dstSw.OutsideWay != null)
        //        {
        //            if (visited.Contains(dstSw.OutsideWay.LineString) == false)
        //            {
        //                merger(dstSw.OutsideWay, outsideWay);
        //                visited.Add(dstSw.OutsideWay.LineString);
        //            }

        //            outsideWay = dstSw.OutsideWay;
        //        }

        //        dstSw.SetSideWays(outsideWay, insideWay);
        //        found = true;
        //    }

        //    // start - startで重なっている場合
        //    if (dstSw.StartEdgeWay?.IsSameLine(srcSw.StartEdgeWay) ?? false)
        //    {
        //        Merge(true, RnWayEx.AppendFront2LineString);
        //        dstSw.SetStartEdgeWay(srcSw.EndEdgeWay);
        //    }
        //    // start - endで重なっている場合
        //    else if (dstSw.StartEdgeWay?.IsSameLine(srcSw.EndEdgeWay) ?? false)
        //    {
        //        Merge(false, RnWayEx.AppendFront2LineString);
        //        dstSw.SetStartEdgeWay(srcSw.StartEdgeWay);
        //    }
        //    // end - endで重なっている場合
        //    else if (dstSw.EndEdgeWay?.IsSameLine(srcSw.EndEdgeWay) ?? false)
        //    {
        //        Merge(true, RnWayEx.AppendBack2LineString);
        //        dstSw.SetEndEdgeWay(srcSw.StartEdgeWay);
        //    }
        //    // end - startで重なっている場合
        //    else if (dstSw.EndEdgeWay?.IsSameLine(srcSw.StartEdgeWay) ?? false)
        //    {
        //        Merge(false, RnWayEx.AppendBack2LineString);
        //        dstSw.SetEndEdgeWay(srcSw.EndEdgeWay);
        //    }
        //}
    }
}