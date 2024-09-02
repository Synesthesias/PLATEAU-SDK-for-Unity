using Codice.CM.Common;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{

    [Flags]
    public enum RnRoadAttribute
    {
        // 1レーンしかない時にそのレーンが両方向かどうか
        BothSide = 1 << 0,
    }
    //[Serializable]
    public class RnRoad : RnRoadBase
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RnModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 接続先
        public RnRoadBase Next { get; private set; }

        // 接続元
        public RnRoadBase Prev { get; private set; }

        // レーンリスト
        private List<RnLane> mainLanes = new List<RnLane>();

        // 中央分離帯
        private RnLane medianLane;

        // 即性情報
        public RnRoadAttribute RnRoadAttribute { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public override PLATEAUCityObjectGroup CityObjectGroup => TargetTran;

        // 本線レーン(参照のみ)
        // 追加/削除はAddMainLane/RemoveMainLaneを使うこと
        public IReadOnlyList<RnLane> MainLanes => mainLanes;

        // 全レーン
        public override IEnumerable<RnLane> AllLanes => MainLanes;

        // 有効なRoadかどうか
        public bool IsValid => MainLanes.Any() && MainLanes.All(l => l.HasBothBorder);

        // 全てのレーンに両方向に接続先がある
        public bool IsAllBothConnectedLane => MainLanes.Any() && MainLanes.All(l => l.IsBothConnectedLane);

        /// <summary>
        /// 左車線/右車線両方あるかどうか
        /// </summary>
        public bool HasBothLane
        {
            get
            {
                var hasLeft = false;
                var hasRight = false;
                foreach (var lane in MainLanes)
                {
                    if (IsLeftLane(lane))
                        hasLeft = true;
                    else if (IsRightLane(lane))
                        hasRight = true;
                    if (hasLeft && hasRight)
                        return true;
                }

                return false;
            }
        }

        public RnLane MedianLane => medianLane;

        public RnRoad() { }

        public RnRoad(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        /// <summary>
        /// 交差点間に挿入された空Roadかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyRoad
        {
            get
            {
                return Prev is RnIntersection && Next is RnIntersection && AllLanes.All(l => l.IsEmptyLane);
            }
        }

        public IEnumerable<RnLane> GetLanes(RnDir dir)
        {
            return dir switch
            {
                RnDir.Left => GetLeftLanes(),
                RnDir.Right => GetRightLanes(),
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null),
            };
        }

        /// <summary>
        /// laneがこのRoadの左車線かどうか(Prev/NextとBorderが共通である前提)
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        private bool IsLeftLane(RnLane lane)
        {
            return lane.IsReverse == false;
        }

        /// <summary>
        /// laneがこのRoadの右車線かどうか(Prev/NextとBorderが共通である前提)
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        private bool IsRightLane(RnLane lane)
        {
            return lane.IsReverse == true;
        }

        /// <summary>
        /// レーンの方向を見る
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        private RnDir GetLaneDir(RnLane lane)
        {
            return IsLeftLane(lane) ? RnDir.Left : RnDir.Right;
        }

        // 境界線情報を取得
        public override IEnumerable<RnBorder> GetBorders()
        {
            foreach (var lane in MainLanes)
            {
                if (lane.PrevBorder != null)
                    yield return new RnBorder(lane.PrevBorder, lane);
                if (lane.NextBorder != null)
                    yield return new RnBorder(lane.NextBorder, lane);
            }
        }

        /// <summary>
        /// 左側のレーン(Prev/NextとBorderが共通である前提)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetLeftLanes()
        {
            // #TODO : Prev == Nextの時にバグる
            return MainLanes.Where(IsLeftLane);
        }

        /// <summary>
        /// 右側のレーン(Prev/NextとBorderが共通である前提)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetRightLanes()
        {
            // #TODO : Prev == Nextの時にバグる
            return MainLanes.Where(IsRightLane);
        }

        /// <summary>
        /// 左側レーン数(Prev/NextとBorderが共通である前提)
        /// </summary>
        /// <returns></returns>
        public int GetLeftLaneCount()
        {
            //return lane.GetNextRoad() == Next;
            return GetLeftLanes().Count();
        }

        /// <summary>
        /// 右側レーン数(Prev/NextとBorderが共通である前提)
        /// </summary>
        /// <returns></returns>
        public int GetRightLaneCount()
        {
            //return lane.GetNextRoad() == Prev;
            return GetRightLanes().Count();
        }

        /// <summary>
        /// 中央分離帯の幅を取得する
        /// </summary>
        /// <returns></returns>
        public float GetMedianWidth()
        {
            if (MedianLane == null)
                return 0f;

            return MedianLane.AllBorders.Select(b => b.CalcLength()).Min();
        }

        /// <summary>
        /// 直接呼ぶの禁止. RnRoadGroupから呼ばれる
        /// </summary>
        /// <param name="lane"></param>
        public void SetMedianLane(RnLane lane)
        {
            medianLane = lane;
        }

        /// <summary>
        /// dirで指定した側の全レーンのBorderを統合した一つの大きなBorderを返す
        /// WayはLeft -> Right方向になっている
        /// dir == nullの時は全レーン
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public RnWay GetMergedBorder(RnLaneBorderType type, RnDir? dir = null)
        {
            var ret = new RnLineString();
            foreach (var l in MainLanes.Where(l => dir == null || GetLaneDir(l) == dir))
            {
                RnWay way = null;
                var t = type;
                var d = RnLaneBorderDir.Left2Right;
                if (IsLeftLane(l) == false)
                {
                    t = t.GetOpposite();
                    d = d.GetOpposite();
                }

                way = l.GetBorder(t);
                if (way == null)
                    continue;
                if (l.GetBorderDir(t) != d)
                    way = way.ReversedWay();

                foreach (var p in way.Points)
                    ret.AddPointOrSkip(p);
            }
            return new RnWay(ret);
        }

        /// <summary>
        /// dirで指定した側の全レーンのSideWayを統合した一つの大きなWayを返す
        /// dir==nullの時は全レーン共通で返す
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="leftWay"></param>
        /// <param name="rightWay"></param>
        /// <returns></returns>
        public bool TryGetMergedSideWay(RnDir? dir, out RnWay leftWay, out RnWay rightWay)
        {
            leftWay = rightWay = null;
            if (IsValid == false)
                return false;
            var targetLanes = MainLanes.Where(l => dir == null || GetLaneDir(l) == dir).ToList();
            if (targetLanes.Any() == false)
                return false;
            var leftLane = targetLanes[0];
            leftWay = IsLeftLane(leftLane) ? leftLane?.LeftWay : leftLane?.RightWay?.ReversedWay();
            var rightLane = targetLanes[^1];
            rightWay = IsLeftLane(rightLane) ? rightLane?.RightWay : rightLane?.LeftWay?.ReversedWay();
            return true;
        }

        /// <summary>
        /// Lanes全体を一つのLaneとしたときのdir側のWayを返す
        /// WayはPrev -> Nextの方向になっている
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public RnWay GetMergedSideWay(RnDir dir)
        {
            if (IsValid == false)
                return null;
            switch (dir)
            {
                case RnDir.Left:
                    {
                        var lane = MainLanes[0];
                        return IsLeftLane(lane) ? lane?.LeftWay : lane?.RightWay?.ReversedWay();
                    }
                case RnDir.Right:
                    {
                        var lane = MainLanes[^1];
                        return IsLeftLane(lane) ? lane?.RightWay : lane?.LeftWay?.ReversedWay();
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }

        /// <summary>
        /// dir方向の一番左のWayと右のWayを取得.
        /// 向きは調整されていない
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="leftWay"></param>
        /// <param name="rightWay"></param>
        /// <returns></returns>
        public bool TryGetSideBorderWay(RnDir dir, out RnWay leftWay, out RnWay rightWay)
        {
            leftWay = rightWay = null;
            if (IsValid == false)
                return false;

            var lanes = GetLanes(dir).ToList();
            if (lanes.Any() == false)
                return false;

            leftWay = lanes[0].LeftWay;
            rightWay = lanes[^1].RightWay;
            return true;
        }

        /// <summary>
        /// dir方向のレーンの最も左のWayと最も右のWayを取得する
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public RnLane GetBorderLane(RnDir dir)
        {
            var prevBorder = new RnLineString();
            var nextBorder = new RnLineString();

            void Merge(RnLineString points, RnWay way)
            {
                foreach (var p in way.Points)
                {
                    points.AddPointOrSkip(p);
                }
            }

            RnLane firstLane = null;
            RnLane lastLane = null;
            foreach (var lane in GetLanes(dir))
            {
                if (lane == null)
                    continue;
                Merge(prevBorder, lane.PrevBorder);
                Merge(nextBorder, lane.NextBorder);
                firstLane ??= lane;
                lastLane = lane;
            }

            return new RnLane(firstLane?.LeftWay, lastLane?.RightWay, new RnWay(prevBorder), new RnWay(nextBorder));
        }

        /// <summary>
        /// 逆転する
        /// </summary>
        public void Reverse()
        {
            (Next, Prev) = (Prev, Next);
            foreach (var lane in mainLanes)
                lane.Reverse();
            mainLanes.Reverse();
        }

        /// <summary>
        /// レーンの境界線の向きをそろえる
        /// </summary>
        /// <param name="borderDir"></param>
        public void AlignLaneBorder(RnLaneBorderDir borderDir = RnLaneBorderDir.Left2Right)
        {
            foreach (var lane in mainLanes)
                lane.AlignBorder(borderDir);
        }

        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            if (Next != null)
                yield return Next;
            if (Prev != null)
                yield return Prev;
        }

        /// <summary>
        /// #TODO : 左右の隣接情報がないので要修正
        /// laneを追加する. ParentRoad情報も更新する
        /// </summary>
        /// <param name="lane"></param>
        public void AddMainLane(RnLane lane)
        {
            if (mainLanes.Contains(lane))
                return;
            OnAddLane(lane);
            mainLanes.Add(lane);
        }

        /// <summary>
        /// laneを追加. indexで指定した位置に追加する
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="index"></param>
        public void AddMainLane(RnLane lane, int index)
        {
            if (mainLanes.Contains(lane))
                return;
            OnAddLane(lane);
            mainLanes.Insert(index, lane);
        }

        /// <summary>
        /// laneを削除するParentRoad情報も更新する
        /// </summary>
        /// <param name="lane"></param>
        public void RemoveLane(RnLane lane)
        {
            if (mainLanes.Remove(lane))
            {
                OnRemoveLane(lane);
            }
        }

        public void ReplaceLane(RnLane before, RnLane after)
        {
            RnEx.ReplaceLane(mainLanes, before, after);
        }

        public void ReplaceLanes(IEnumerable<RnLane> newLanes)
        {
            while (mainLanes.Count > 0)
                RemoveLane(mainLanes[0]);

            foreach (var lane in newLanes)
                AddMainLane(lane);
        }

        public void ReplaceLanes(IEnumerable<RnLane> newLanes, RnDir dir)
        {
            foreach (var l in GetLanes(dir).ToList())
            {
                RemoveLane(l);
            }
            // Leftは先頭に追加
            if (dir == RnDir.Left)
            {
                var i = 0;
                foreach (var l in newLanes)
                {
                    AddMainLane(l, i);
                    i++;
                }
            }
            // Rightは末尾に追加
            else
            {
                foreach (var l in newLanes)
                {
                    AddMainLane(l);
                }
            }
        }

        /// <summary>
        /// 中央分離帯を入れ替える
        /// </summary>
        /// <param name="lane"></param>
        public void ReplaceMedianLane(RnLane lane)
        {
            RemoveLane(lane);
            medianLane = lane;
            OnAddLane(lane);
        }

        public void ReplaceLane(RnLane before, IEnumerable<RnLane> newLanes)
        {
            var index = mainLanes.IndexOf(before);
            if (index < 0)
                return;
            var lanes = newLanes.ToList();
            mainLanes.InsertRange(index, lanes);
            foreach (var lane in lanes)
                OnRemoveLane(lane);
            RemoveLane(before);
        }

        private void OnAddLane(RnLane lane)
        {
            if (lane == null)
                return;
            lane.Parent = this;
        }

        private void OnRemoveLane(RnLane lane)
        {
            if (lane == null)
                return;
            if (lane.Parent == this)
                lane.Parent = null;
        }

        /// <summary>
        /// Factoryからのみ呼ぶ. Prev/Nextの更新
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        public void SetPrevNext(RnRoadBase prev, RnRoadBase next)
        {
            Prev = prev;
            Next = next;
        }

        /// <summary>
        /// 接続を解除する
        /// </summary>
        public override void DisConnect(bool removeFromModel)
        {
            Prev?.UnLink(this);
            Next?.UnLink(this);
            SetPrevNext(null, null);
            if (removeFromModel)
                ParentModel?.RemoveRoad(this);
            foreach (var lane in mainLanes)
            {
                lane.DisConnectBorder();
            }
        }

        /// <summary>
        /// 隣接情報からotherを削除する. other側の接続は消えない
        /// </summary>
        /// <param name="other"></param>
        public override void UnLink(RnRoadBase other)
        {
            if (Prev == other)
                Prev = null;
            if (Next == other)
                Next = null;
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
        public static RnRoad CreateIsolatedRoad(PLATEAUCityObjectGroup targetTran, RnWay way)
        {
            var lane = RnLane.CreateOneWayLane(way);
            var ret = new RnRoad(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }

        public static RnRoad CreateOneLaneRoad(PLATEAUCityObjectGroup targetTran, RnLane lane)
        {
            var ret = new RnRoad(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }
    }

    public static class RnRoadEx
    {
        /// <summary>
        /// laneの向きがRoadの進行方向と逆かどうか(左車線/右車線の判断に使う)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="lane"></param>
        /// <returns></returns>
        public static bool IsReverseLane(this RnRoad self, RnLane lane)
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
        public static RnRoadBase GetOppositeRoad(this RnRoad self, RnRoadBase other)
        {
            if (self.Prev == other)
            {
                return self.Next == other ? null : self.Next;
            }
            if (self.Next == other)
            {
                return self.Prev == other ? null : self.Prev;
            }

            throw new InvalidDataException($"{self.DebugMyId} is not roaded {other.DebugMyId}");
        }

        /// <summary>
        /// selfと隣接しているRoadをすべてまとめたRoadGroupを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RnRoadGroup CreateRoadGroup(this RnRoad self)
        {
            var roads = new List<RnRoad> { self };
            RnIntersection Search(RnRoadBase src, RnRoadBase target, bool isPrev)
            {
                while (target is RnRoad road)
                {
                    // ループしていたら終了
                    if (roads.Contains(road))
                        break;
                    if (isPrev)
                        roads.Insert(0, road);
                    else
                        roads.Add(road);
                    // roadの接続先でselfじゃない方
                    target = road.GetOppositeRoad(src);

                    src = road;
                }
                return target as RnIntersection;
            }
            var prevIntersection = Search(self, self.Prev, true);
            var nextIntersection = Search(self, self.Next, false);
            return new RnRoadGroup(prevIntersection, nextIntersection, roads);
        }

        /// <summary>
        /// selfと隣接しているRoadをすべてまとめたRoadGroupを返す.
        /// 返せない場合はnullを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RnRoadGroup CreateRoadGroupOrDefault(this RnRoad self)
        {
            try
            {
                return CreateRoadGroup(self);
            }
            catch (InvalidDataException)
            {
                return null;
            }
        }

        /// <summary>
        /// selfの全頂点の重心を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector3 GetCenter(this RnRoadBase self)
        {
            var a = self.AllLanes
                .Select(l => l.GetCenter())
                .Aggregate(new { sum = Vector3.zero, i = 0 }, (a, p) => new { sum = a.sum + p, i = a.i + 1 });
            if (a.i == 0)
                return Vector3.zero;
            return a.sum / a.i;
        }
    }
}