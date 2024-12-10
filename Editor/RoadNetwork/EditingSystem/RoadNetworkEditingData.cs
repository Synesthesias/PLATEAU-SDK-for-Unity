using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="_ListElement"></typeparam>
    public class EditorDataList<_ListElement> : List<_ListElement>
    {
        /// <summary>
        /// 頻繁に呼ばないように注意する
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <returns></returns>
        public bool TryGetCache<_ListElementVal>(string key, out IEnumerable<_ListElementVal> o)
        {
            if (cacheDataMap.TryGetValue(key, out var data) == false)
            {
                o = default;
                return false;
            }

            var castedData = data as CacheData<_ListElementVal>;
            var d = castedData.data as IEnumerable<_ListElementVal>;
            if (d == null)
            {
                o = default;
                return false;
            }

            o = d;
            return true;
        }

        /// <summary>
        /// 追加する
        /// 同型で用途が異なるデータにも対応するため keyを必要とする   例　右車線コレクション、左車線コレクション　どちらもList<Lane>
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddCache<_ListElementVal>(string key, Func<_ListElement, _ListElementVal> selectMethod)
        {
            Assert.IsNotNull(key);
            Assert.IsNotNull(selectMethod);
            if (cacheDataMap.ContainsKey(key))
            {
                Debug.LogError("追加済みのキャッシュデータキーを使用しようとした");
                return false;
            }

            var e = this.Select(selectMethod);
            cacheDataMap.Add(key, new CacheData<_ListElementVal>() { selecter = selectMethod, data = e });
            return true;
        }

        public bool Recache<_ListElementVal>(string key)
        {
            Assert.IsNotNull(key);
            if (cacheDataMap.TryGetValue(key, out var cacheData) == false)
            {
                return false;
            }

            var d = cacheData as CacheData<_ListElementVal>;
            Assert.IsNotNull(d);

            d.data = this.Select(d.selecter);
            return true;
        }

        public void ClearCache()
        {
            cacheDataMap.Clear();
        }

        private class CacheData<_Val>
        {
            public Func<_ListElement, _Val> selecter;
            public IEnumerable<_Val> data;
        }
        Dictionary<string, object> cacheDataMap = new Dictionary<string, object>();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="_BaseData"></typeparam>
    internal class EditorData<_BaseData>
        where _BaseData : class
    {
        public EditorData(_BaseData baseData)
        {
            Assert.IsNotNull(baseData);
            this.Ref = baseData;
            this.userData = new List<object>();
        }

        /// <summary>
        /// サブデータをリクエストする。
        /// 存在すればそれを返して、なければ生成して返す
        /// 頻繁に呼ばないように注意する
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <returns></returns>
        public _Type ReqSubData<_Type>()
            where _Type : EditorSubData<_BaseData>, new()
        {
            // 既存データの探索
            foreach (var item in userData)
            {
                if (item is _Type)
                    return (_Type)item;
            }

            // データが存在しないので生成する
            var d = new _Type();
            var isSuc = d.Construct(this);
            if (isSuc)
            {
                Add(d);
                return d;
            }

            return null;
        }

        /// <summary>
        /// サブデータの新規追加（初期化時の最適化用）
        /// 初めてのサブテータを追加する時のみ利用可能
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <returns></returns>
        public _Type Add<_Type>()
            where _Type : EditorSubData<_BaseData>, new()
        {
            // 既存データの探索
            foreach (var item in userData)
            {
                Assert.IsFalse(item is _Type);   // 重複登録は不可
            }

            // データ作成
            var d = new _Type();
            var isSuc = d.Construct(this);
            if (isSuc)
            {
                Add(d);
                return d;
            }

            return null;
        }


        /// <summary>
        /// サブデータを取得する
        /// nullの可能性あり
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <returns></returns>
        public _Type GetSubData<_Type>()
            where _Type : EditorSubData<_BaseData>, new()
        {
            // 既存データの探索
            foreach (var item in userData)
            {
                if (item is _Type)
                    return (_Type)item;
            }

            return null;
        }

        /// <summary>
        /// 追加を試みる
        /// 既に同じ型が登録されている場合は不可
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private _Type Add<_Type>(_Type data)
            where _Type : EditorSubData<_BaseData>, new()
        {
            Assert.IsFalse(userData.Contains(data));
            userData.Add(data);
            return data;
        }

        /// <summary>
        /// 内部データのクリア
        /// データの構成に変更が出て既存のデータが古くなった際に使ったりする
        /// 例　EditorData<RnRoadGroup>　内部データでwayのリストを持っている。この時Laneが削除などされればこの関数を呼ぶ必要が出てくる
        /// </summary>
        public void ClearSubData()
        {
            userData.Clear();
        }

        public void ClearSubData<_Type>()
            where _Type : EditorSubData<_BaseData>, new()
        {
            var subData = GetSubData<_Type>();
            if (subData != null)
            {
                userData.Remove(subData);
            }
        }

        public _BaseData Ref { get; private set; }
        private List<object> userData;

        /// <summary>
        /// 編集可能なデータか？
        /// 整合性や前提条件を満たしていない場合もfalse
        /// </summary>
        public bool IsEditable { get; set; } = false;
    }

    internal class PointEditorData : EditorSubData<RnPoint>
    {
        public PointEditorData()
            : base()
        {
        }

        /// <summary>
        /// 基準位置の更新
        /// </summary>
        public void UpdateBasePosition()
        {
            basePosition = refPoint.Vertex;
        }

        /// <summary>
        /// キャッシュに変更を適用する
        /// </summary>
        /// <param name="translate"></param>
        public void Translate(in Vector3 translate)
        {
            cacheTranslate += translate;
        }

        //public void Scale(in Vector3 scale)
        //{
        //    cacheScale += scale;
        //}

        public void Apply()
        {
            refPoint.Vertex = basePosition + cacheTranslate;
        }

        protected override bool Construct()
        {
            refPoint = Parent.Ref;
            UpdateBasePosition();
            return true;
        }

        // 参照中のポイント
        private RnPoint refPoint;
        // 基準の座標
        private Vector3 basePosition;

        private Vector3 cacheTranslate;
        //private Vector3 cacheScale;
    }

    internal class NodeEditorData
    {
        public NodeEditorData()
        {
            PointWithWeight.Clear();
        }

        public bool AddPoint(EditorData<RnPoint> point, float weight)
        {
            Assert.IsTrue(0 <= weight && weight <= 1.0f, "weight " + weight.ToString());
            Assert.IsNotNull(point);
            var subData = point.GetSubData<PointEditorData>();
            Assert.IsNotNull(subData);
            return PointWithWeight.TryAdd(point, (subData, weight));
        }

        public void RemovePoint(EditorData<RnPoint> point)
        {
            PointWithWeight.Remove(point);
        }

        public void SetPointWeight(EditorData<RnPoint> point, float weight)
        {
            var isSuc = PointWithWeight.TryGetValue(point, out var w);
            if (isSuc)
            {
                w.weight = weight;
            }
        }

        private struct CacheTransform
        {
            public CacheTransform(Transform transform)
            {
                position = transform.position;
            }

            public Vector3 position;
        }
        private CacheTransform baseTransform;

        private Dictionary<EditorData<RnPoint>, (PointEditorData data, float weight)> PointWithWeight { get; set; } =
            new Dictionary<EditorData<RnPoint>, (PointEditorData, float)>();

        public List<RoadGroupEditorData> Connections { get; private set; } = new List<RoadGroupEditorData>();

        public bool IsIntersection { get => Connections.Count >= 3; }

    }

    internal abstract class EditorSubData<_Parent>
        where _Parent : class
    {
        public bool Construct(EditorData<_Parent> parent)
        {
            Assert.IsNull(Parent);  // 再初期化を許可しない

            Assert.IsNotNull(parent);
            Assert.IsNotNull(parent.Ref);
            Parent = parent;

            // 派生クラスのコンストラクト
            Construct();

            return true;
        }

        protected abstract bool Construct();

        protected EditorData<_Parent> Parent { get; private set; }
    }

    internal class RoadGroupEditorData : EditorSubData<RnRoadGroup>
    {
        public RoadGroupEditorData()
            : base()
        {
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() + B.GetHashCode();
        }

        /// <summary>
        /// レーン数を統一する
        /// </summary>
        public void AlignNumLane()
        {
            var nR = RoadGroup.Ref.GetRightLaneCount();
            var nL = RoadGroup.Ref.GetLeftLaneCount();
            RoadGroup.Ref.SetLaneCount(nL, nR);
        }

        protected override bool Construct()
        {
            RoadGroup = Parent;

            var a = Parent.Ref.PrevIntersection;
            var b = Parent.Ref.NextIntersection;

            if (a != null && b != null)         // 両方がnullでない場合
            {
                if (a.GetHashCode() > b.GetHashCode())
                {
                    A = a;
                    B = b;
                }
                else
                {
                    A = b;
                    B = a;
                }
            }
            else if (a != null || b != null)    // 片方がnullである場合
            {
                if (a == null)
                {
                    A = b;
                    B = null;
                }
                else
                {
                    B = a;
                    A = null;
                }
            }
            else
            {
                A = null;
                B = null;
            }

            ConnectionLinks = Parent.Ref.Roads;
            // LinksがNode A,Bに接続されているかチェックするデバッグ機能

            return true;
        }

        public RnIntersection A { get; private set; }
        public RnIntersection B { get; private set; }

        public IReadOnlyCollection<RnRoad> ConnectionLinks { get; private set; }

        public EditorData<RnRoadGroup> RoadGroup { get; private set; }

        public List<Vector3> CacheRoadPosList { get; set; }

        /// <summary>
        /// 中心点の計算
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCenter()
        {
            var p1 = RoadGroup.Ref.Roads.First().GetCentralVertex();
            var p2 = RoadGroup.Ref.Roads.Last().GetCentralVertex();
            return (p1 + p2) * 0.5f;
        }

        public IEnumerable<RnLane> RightLanes
        {
            get
            {
                return RoadGroup.Ref.GetRightLanes();
            }
        }
        public IEnumerable<RnLane> LeftLanes
        {
            get
            {
                return RoadGroup.Ref.GetLeftLanes();
            }
        }


        // Person オブジェクトの比較をカスタマイズするクラス
        public class Comparer : IEqualityComparer<RoadGroupEditorData>
        {
            public bool Equals(RoadGroupEditorData x, RoadGroupEditorData y)
            {
                // 名前が同じ場合は同一とみなす
                return x.A == y.A && x.B == y.B;
            }

            public int GetHashCode(RoadGroupEditorData obj)
            {
                // 名前のハッシュコードを返す
                return obj.GetHashCode();
            }
        }

    }

    public class LaneGroupEditorData
    {
        public LaneGroupEditorData(RnRoadGroup target)
        {
            Assert.IsNotNull(target);
            group = target;
            var fLink = group.Roads.First();
            var lLink = group.Roads.Last();

            // link一つで構成されているため Laneの接続を確認する必要がない
            if (fLink == lLink)
            {
                foreach (var lane in fLink.MainLanes)
                {
                    LaneGroupListCache.Add(new List<RnLane>() { lane });
                }
                return;
            }

            var flLanes = fLink.GetLanes(RnDir.Left);
            var llLanes = fLink.GetLanes(RnDir.Left);

            var frLanes = fLink.GetLanes(RnDir.Right);
            var lrLanes = fLink.GetLanes(RnDir.Right);

            var nGroup = target.GetLeftLaneCount() + target.GetRightLaneCount();
            var nLanes = target.Roads.Count;

            LaneGroupListCache = new List<List<RnLane>>(nGroup);
            foreach (var lane in flLanes)
            {
                var errCheckCnt = nLanes + 3;   // 無限ループ防止

                var laneGroup = new List<RnLane>(nLanes);
                LaneGroupListCache.Add(laneGroup);

                var prevLane = lane;
                laneGroup.Add(prevLane);
                while (true)
                {
                    // 最後のレーンに到達した
                    if (llLanes.Contains(prevLane))
                    {
                        break;
                    }

                    if (errCheckCnt == 0)
                    {
                        Assert.IsTrue(false, "無限ループ");
                        break;
                    }

                    // borderを共有しているので直線状に繋がったLane
                    var nextLane = prevLane.GetNextLanes().First(v => v.PrevBorder == prevLane.NextBorder);
                    Assert.IsNotNull(nextLane);
                    if (nextLane != null)
                    {
                        laneGroup.Add(nextLane);
                        prevLane = nextLane;
                    }

                    errCheckCnt--;
                }
            }
        }

        private RnRoadGroup group;

        // 連結レーンのリスト
        public List<List<RnLane>> LaneGroupListCache { get; set; } = new List<List<RnLane>>();
    }

    // refresh時にClaer()必要
    public class LaneEditCache
    {
        public float Scale { get; set; }
        public RnLane BaseLane { get; set; }
    }

    /// <summary>
    /// WayEditorDataのリストをサブデータ化したクラス
    /// </summary>
    internal class WayEditorDataList : EditorSubData<RnRoadGroup>
    {
        public WayEditorDataList()
        {
        }

        public IReadOnlyList<WayEditorData> Raw { get => wayEditorDataList; }
        private List<WayEditorData> wayEditorDataList;

        protected override bool Construct()
        {
            CreateWayEditorData(Parent);
            return true;
        }

        public void SetSelectable(bool enable)
        {
            foreach (var wayEditorData in wayEditorDataList)
            {
                wayEditorData.IsSelectable = enable;
            }
        }

        private void CreateWayEditorData(EditorData<RnRoadGroup> roadGroupEditorData)
        {
            // wayを重複無しでコレクションする
            Dictionary<RnLane, HashSet<RnWay>> laneWays = new();
            foreach (var road in roadGroupEditorData.Ref.Roads)
            {
                foreach (var lane in road.AllLanes)
                {
                    var ways = NewOrGetWays(laneWays, lane);
                    if (lane.LeftWay != null)
                        ways.Add(lane.LeftWay);
                    if (lane.RightWay != null)
                        ways.Add(lane.RightWay);
                }
            }

            // 歩道のwayを重複無しでコレクションする
            Dictionary<RnRoad, HashSet<RnWay>> sideWalkWays = new();    // wayはlaneに紐づいていたないためroadに紐づける
            foreach (var road in roadGroupEditorData.Ref.Roads)
            {
                foreach (var sideWalk in road.SideWalks)
                {
                    var ways = NewOrGetWays(sideWalkWays, road);
                    if (sideWalk.OutsideWay != null)
                        ways.Add(sideWalk.OutsideWay); // 歩道の外側の線を組み込みたい
                }
            }

            // 中央分離帯のwayを重複無しでコレクションする
            Dictionary<RnRoad, HashSet<RnWay>> medianWays = new();
            foreach (var road in roadGroupEditorData.Ref.Roads)
            {
                var ways = NewOrGetWays(medianWays, road);
                roadGroupEditorData.Ref.GetMedians(out var leftWays, out var rightWays);
                foreach (var way in leftWays)
                {
                    if (way != null)
                        ways.Add(way);
                }
                foreach (var way in rightWays)
                {
                    if (way != null)
                        ways.Add(way);
                }
            }

            // way用の編集データの作成準備
            if (wayEditorDataList == null)
                wayEditorDataList = new List<WayEditorData>(laneWays.Count);
            wayEditorDataList?.Clear();

            // 車線のwayから中央分離帯のwayを除外
            foreach (var ways in laneWays.Values)
            {
                foreach (var editingTarget in medianWays)
                {
                    if (editingTarget.Value == null)
                    {
                        continue;
                    }

                    foreach (var medianWay in editingTarget.Value)
                    {
                        ways.Remove(medianWay);
                    }
                }
            }

            // 車線の編集用データを作成
            CreateWayEditorData(wayEditorDataList, laneWays, WayEditorData.WayType.Main);

            // 歩道の編集用データを作成
            CreateWayEditorData(wayEditorDataList, sideWalkWays, WayEditorData.WayType.SideWalk);

            // 中央分離帯の編集用データを作成
            CreateWayEditorData(wayEditorDataList, medianWays, WayEditorData.WayType.Median);

            static HashSet<RnWay> NewOrGetWays<_RnRoadNetworkClass>(Dictionary<_RnRoadNetworkClass, HashSet<RnWay>> wayCollection, _RnRoadNetworkClass parent)
            {
                HashSet<RnWay> ways = null;
                if (wayCollection.TryGetValue(parent, out ways) == false)
                {
                    wayCollection.Add(parent, ways = new HashSet<RnWay>());
                }

                return ways;
            }

            static void CreateWayEditorData<_RnRoadNetworkClass>(List<WayEditorData> wayEditorDataList, Dictionary<_RnRoadNetworkClass, HashSet<RnWay>> wayCollection, WayEditorData.WayType wayType)
            {
                foreach (var editingTarget in wayCollection)
                {
                    if (editingTarget.Value == null)
                    {
                        continue;
                    }

                    foreach (var way in editingTarget.Value)
                    {
                        var wayEditorData = new WayEditorData(way, editingTarget.Key as RnLane);
                        wayEditorData.Type = wayType;
                        wayEditorDataList.Add(wayEditorData);
                    }
                }
            }

            //// 道路端のwayを編集不可能にする
            //wayEditorDataList.First().IsSelectable = false;
            //wayEditorDataList.Last().IsSelectable = false;

            // 下　もしかしたらwayを結合して扱う必要があるかも
            // 道路端のwayを編集不可能にする
            //var firstRoad = roadGroupEditorData.Ref.Roads.First();
            //var leftEdgeLane = firstRoad.MainLanes.First();
            //wayEditorDataList.Find(x => x.Ref == leftEdgeLane.LeftWay).IsSelectable = false;
            //var rightEdgeLane = firstRoad.MainLanes.Last();
            //if (leftEdgeLane == rightEdgeLane)  // レーンが一つしかない時は反対側のレーンを参照する
            //{
            //    wayEditorDataList.Find(x => x.Ref == rightEdgeLane.RightWay).IsSelectable = false;
            //}
            //else
            //{
            //    if (rightEdgeLane.LeftWay != null)
            //    {
            //        rightEdgeLane.GetBorderDir(RnLaneBorderType.)
            //        wayEditorDataList.Find(x => x.Ref == rightEdgeLane.LeftWay).IsSelectable = false;

            //    }
            //    wayEditorDataList.Find(x => x.Ref == rightEdgeLane.LeftWay).IsSelectable = false;
            //}

        }

    }

    /// <summary>
    /// ScriptableObjectを保持用
    /// 旧仕様との差分を吸収するためのクラス
    /// </summary>
    internal class ScriptableObjectFolder : EditorSubData<RnRoadGroup>
    {
        protected override bool Construct()
        {
            // UIへバインドするモデルオブジェクトの生成
            var testObj = ScriptableObject.CreateInstance<UIDocBind.ScriptableRoadMdl>();
            testObj.Construct(Parent.Ref);
            Item = new UIDocBind.SerializedScriptableRoadMdl(testObj, Parent);

            return true;
        }

        public UIDocBind.SerializedScriptableRoadMdl Item { get; private set; }
    }

    /// <summary>
    /// Wayをスライドさせるためのデータ
    /// </summary>
    public class WayEditorData
    {
        public enum WayType
        {
            Undefind = 0,   // 無効値　システムエラー
            Main,
            SideWalk,
            Median
        }

        public WayEditorData(RnWay target, RnLane parent)
        {
            Assert.IsNotNull(target);
            BaseWay = target.Vertices.ToList();
            Ref = target;

            ParentLane = parent;    // null許容
        }

        float sliderVarVals;
        public float SliderVarVals
        {
            get
            {
                return sliderVarVals;
            }
            set
            {
                if (sliderVarVals == value)
                {
                    return;
                }
                IsChanged = true;
                sliderVarVals = value;
            }
        }
        public bool IsChanged { get; private set; }

        public List<Vector3> BaseWay { get; private set; } = new List<Vector3>();
        public RnWay Ref { get; private set; } = null;
        public RnLane ParentLane { get; private set; } = null;
        /// <summary>
        /// 選択可能か？
        /// </summary>
        public bool IsSelectable { get; set; } = true;

        public WayType Type { get; set; } = WayEditorData.WayType.Undefind;
    }

    /// <summary>
    /// 変更前の結果保持用
    /// </summary>
    internal abstract class CacheSideWalkGroupEditorData : EditorSubData<RnRoadGroup>
    {
        public IReadOnlyCollection<RnRoadGroup.RnSideWalkGroup> Group { get; protected set; }
        public bool HasSideWalk { get => Group?.Count > 0; }

    }

    internal class CacheLeftSideWalkGroupEditorData : CacheSideWalkGroupEditorData
    {
        protected override bool Construct()
        {
            Parent.Ref.GetSideWalkGroups(out var left, out var right);  // 最適化でrightを取得しないようにする
            Group = left;
            return true;
        }
    }
    internal class CacheRightSideWalkGroupEditorData : CacheSideWalkGroupEditorData
    {
        protected override bool Construct()
        {
            Parent.Ref.GetSideWalkGroups(out var left, out var right);  // 最適化でrightを取得しないようにする
            Group = right;
            return true;
        }
    }

    internal abstract class NeighborPointEditarData : EditorSubData<RnIntersection>
    {
        public IReadOnlyCollection<RnNeighbor> Points { get => points; }

        protected IReadOnlyCollection<RnNeighbor> points = null;

    }


    internal class EnterablePointEditorData : NeighborPointEditarData
    {
        protected override bool Construct()
        {
            points = CollectEnterablePoints(Parent);
            return true;
        }

        private static IReadOnlyCollection<RnNeighbor> CollectEnterablePoints(EditorData<RnIntersection> data)
        {
            var enterablePoints = new List<RnNeighbor>(data.Ref.Neighbors.Count());
            foreach (var neighbor in data.Ref.Neighbors)
            {
                if (CheckEnterablePoint(neighbor))
                    enterablePoints.Add(neighbor);
            }
            return enterablePoints;
        }

        private static bool CheckEnterablePoint(RnNeighbor neighbor)
        {
            var isInboud = (neighbor.GetFlowType() & RnFlowTypeMask.Inbound) > 0;
            return isInboud;
        }


    }

    internal class ExitablePointEditorData : NeighborPointEditarData
    {
        protected override bool Construct()
        {
            points = CollectExitablePoints(Parent);
            return true;
        }

        private static IReadOnlyCollection<RnNeighbor> CollectExitablePoints(EditorData<RnIntersection> data)
        {
            var exitablePoints = new List<RnNeighbor>(data.Ref.Neighbors.Count());
            foreach (var neighbor in data.Ref.Neighbors)
            {
                if (CheckExitablePoint(neighbor))
                    exitablePoints.Add(neighbor);
            }
            return exitablePoints;
        }

        private static bool CheckExitablePoint(RnNeighbor neighbor)
        {
            var isOutbound = (neighbor.GetFlowType() & RnFlowTypeMask.Outbound) > 0;
            return isOutbound;
        }


    }

}
