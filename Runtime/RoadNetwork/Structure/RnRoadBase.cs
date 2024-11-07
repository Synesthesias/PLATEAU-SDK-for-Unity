using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    /// <summary>
    /// Serialize時にnewする必要があるのでabstractにはできない
    /// </summary>
    [Serializable]
    public class RnRoadBase : ARnParts<RnRoadBase>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        // 自分が所属するRoadNetworkModel
        public RnModel ParentModel { get; set; }

        /// <summary>
        ///  これに紐づくtranオブジェクトリスト(統合なので複数存在する場合がある)
        /// </summary>
        public List<PLATEAUCityObjectGroup> TargetTrans { get; set; } = new List<PLATEAUCityObjectGroup>();

        /// <summary>
        /// 歩道情報
        /// </summary>
        protected List<RnSideWalk> sideWalks = new List<RnSideWalk>();


        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnSideWalk> SideWalks => sideWalks;

        /// <summary>
        /// 歩道sideWalkを追加する.
        /// sideWalkの親情報も書き換える
        /// </summary>
        /// <param name="sideWalk"></param>
        public void AddSideWalk(RnSideWalk sideWalk)
        {
            if (sideWalk == null)
                return;
            if (sideWalks.Contains(sideWalk))
                return;
            // 以前の親からは削除
            sideWalk?.ParentRoad?.RemoveSideWalk(sideWalk);
            sideWalk.SetParent(this);
            sideWalks.Add(sideWalk);
        }

        /// <summary>
        /// 歩道sideWalkを削除する.
        /// sideWalkの親情報は変更しない
        /// </summary>
        /// <param name="sideWalk"></param>
        public void RemoveSideWalk(RnSideWalk sideWalk)
        {
            if (sideWalk == null)
                return;
            sideWalk.SetParent(null);
            sideWalks.Remove(sideWalk);
        }

        // 境界線情報を取得
        public virtual IEnumerable<RnBorder> GetBorders() { yield break; }

        // 隣接するRoadを取得
        public virtual IEnumerable<RnRoadBase> GetNeighborRoads() { yield break; }


        /// <summary>
        /// 対象のTargetTranを追加
        /// </summary>
        /// <param name="targetTran"></param>
        public void AddTargetTran(PLATEAUCityObjectGroup targetTran)
        {
            if (TargetTrans.Contains(targetTran) == false)
                TargetTrans.Add(targetTran);
        }

        public void AddTargetTrans(IEnumerable<PLATEAUCityObjectGroup> targetTrans)
        {
            foreach (var t in targetTrans)
                AddTargetTran(t);
        }

        /// <summary>
        /// otherをつながりから削除する. other側の接続は消えない
        /// </summary>
        /// <param name="other"></param>
        public virtual void UnLink(RnRoadBase other) { }

        /// <summary>
        /// 自身の接続を切断する.
        /// removeFromModel=trueの場合、RnModelからも削除する
        /// </summary>
        public virtual void DisConnect(bool removeFromModel)
        {
            if (removeFromModel)
            {
                foreach (var sw in sideWalks)
                    ParentModel?.RemoveSideWalk(sw);
            }
        }


        /// <summary>
        /// selfの全頂点の重心を返す
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 GetCenter()
        {
            return Vector3.zero;
        }

        public virtual void ReplaceNeighbor(RnRoadBase from, RnRoadBase to) { }
    }

    public static class RnRoadBaseEx
    {
        /// <summary>
        /// 相互に接続を解除する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        public static void UnLinkEachOther(this RnRoadBase self, RnRoadBase other)
        {
            self?.UnLink(other);
            other?.UnLink(self);
        }
    }
}