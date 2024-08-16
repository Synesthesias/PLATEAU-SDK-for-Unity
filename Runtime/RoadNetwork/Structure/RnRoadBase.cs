using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;

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
        private List<RnSideWalk> sideWalks = new List<RnSideWalk>();


        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnSideWalk> SideWalks => sideWalks;

        public void AddSideWalk(RnSideWalk sideWalk)
        {
            if (sideWalk == null)
                return;
            if (sideWalks.Contains(sideWalk))
                return;
            sideWalk.SetParent(this);
            sideWalks.Add(sideWalk);
        }

        public void RemoveSideWalk(RnSideWalk sideWalk)
        {
            if (sideWalk == null)
                return;
            sideWalk.SetParent(null);
            sideWalks.Remove(sideWalk);
        }

        public virtual PLATEAUCityObjectGroup CityObjectGroup => null;

        // 境界線情報を取得
        public virtual IEnumerable<RnBorder> GetBorders() { yield break; }

        // 隣接するRoadを取得
        public virtual IEnumerable<RnRoadBase> GetNeighborRoads() { yield break; }

        // 所持全レーンを取得
        public virtual IEnumerable<RnLane> AllLanes { get { yield break; } }

        /// <summary>
        /// otherをつながりから削除する. other側の接続は消えない
        /// </summary>
        /// <param name="other"></param>
        public virtual void UnLink(RnRoadBase other) { }

        /// <summary>
        /// 自身の接続を切断する.
        /// removeFromModel=trueの場合、RnModelからも削除する
        /// </summary>
        public virtual void DisConnect(bool removeFromModel) { }
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