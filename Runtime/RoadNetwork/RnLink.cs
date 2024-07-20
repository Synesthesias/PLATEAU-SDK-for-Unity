using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.RoadNetwork
{
    //[Serializable]
    public class RnLink : RnRoadBase
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RnModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 接続先
        public RnRoadBase Next { get; set; }

        // 接続元
        public RnRoadBase Prev { get; set; }

        // レーンリスト
        private List<RnLane> mainLanes = new List<RnLane>();

        // 双方向フラグ
        public bool IsBothWay { get; set; } = true;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 本線レーン(参照のみ)
        // 追加/削除はAddMainLane/RemoveMainLaneを使うこと
        public IReadOnlyList<RnLane> MainLanes => mainLanes;

        // 全レーン
        public override IEnumerable<RnLane> AllLanes => MainLanes;


        public RnLink() { }

        public RnLink(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            if (Next != null)
                yield return Next;
            if (Prev != null)
                yield return Prev;
        }

        public void AddMainLane(RnLane lane)
        {
            AddLane(mainLanes, lane);
        }

        public void RemoveMainLane(RnLane lane)
        {
            RemoveLane(mainLanes, lane);
        }

        public void RemoveLane(RnLane lane)
        {
            RnEx.RemoveLane(mainLanes, lane);
        }

        public void ReplaceLane(RnLane before, RnLane after)
        {
            RnEx.ReplaceLane(mainLanes, before, after);
        }

        public void ReplaceLane(RnLane before, IEnumerable<RnLane> newLanes)
        {
            var index = mainLanes.IndexOf(before);
            if (index < 0)
                return;
            var lanes = newLanes.ToList();
            mainLanes.InsertRange(index, lanes);
            foreach (var lane in lanes)
                lane.Parent = this;
            RemoveLane(before);
        }
        /// <summary>
        /// lanesにlaneを追加する. ParentLink情報も更新する
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        private void AddLane(List<RnLane> lanes, RnLane lane)
        {
            if (lanes.Contains(lane))
                return;
            lane.Parent = this;
            lanes.Add(lane);
        }

        /// <summary>
        /// lanesからlaneを削除するParentLink情報も更新する
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        private void RemoveLane(List<RnLane> lanes, RnLane lane)
        {
            if (lanes.Remove(lane))
                lane.Parent = null;
        }

        // ---------------
        // Static Methods
        // ---------------
        /// <summary>
        /// 完全に孤立したリンクを作成
        /// </summary>
        /// <param name="targetTran"></param>
        /// <param name="way"></param>
        /// <returns></returns>
        public static RnLink CreateIsolatedLink(PLATEAUCityObjectGroup targetTran, RnWay way)
        {
            var lane = RnLane.CreateOneWayLane(way);
            var ret = new RnLink(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }

        public static RnLink CreateOneLaneLink(PLATEAUCityObjectGroup targetTran, RnLane lane)
        {
            var ret = new RnLink(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }
    }

    public static class RnLinkEx
    {
        /// <summary>
        /// laneの向きがLinkの進行方向と逆かどうか(左車線/右車線の判断に使う)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="lane"></param>
        /// <returns></returns>
        public static bool IsReverseLane(this RnLink self, RnLane lane)
        {
            if (lane.Parent != self)
                return false;

            return lane.GetNextLanes().Any(a => a.Parent == self.Next);
        }

        /// <summary>
        /// selfのPrev/Nextのうち, otherじゃない方を返す.
        /// 両方ともotherじゃない場合は例外を投げる
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static RnRoadBase GetOppositeLink(this RnLink self, RnRoadBase other)
        {
            if (self.Prev == other)
            {
                return self.Next;
            }
            if (self.Next == other)
            {
                return self.Prev;
            }

            throw new InvalidDataException($"{self.DebugMyId} is not linked {other.DebugMyId}");
        }

        /// <summary>
        /// selfと隣接しているLinkをすべてまとめたLinkGroupを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RnLinkGroup CreateLinkGroup(this RnLink self)
        {
            var links = new List<RnLink> { self };
            RnNode Search(RnRoadBase src, RnRoadBase target, bool isPrev)
            {
                while (target is RnLink link)
                {
                    if (isPrev)
                        links.Insert(0, link);
                    else
                        links.Add(link);
                    // linkの接続先でselfじゃない方
                    target = link.GetOppositeLink(src);
                    src = link;
                }
                return target as RnNode;
            }
            var prevNode = Search(self, self.Prev, true);
            var nextNode = Search(self, self.Next, false);
            return new RnLinkGroup(prevNode, nextNode, links);
        }

    }
}