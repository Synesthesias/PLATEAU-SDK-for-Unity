using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork;
using System;
using System.Collections;
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
    public class EditorData<_BaseData>
        where _BaseData : class
    {
        public EditorData(_BaseData point)
        {
            Assert.IsNotNull(point);
            this.Ref = point;
            this.userData = new List<object>();
        }

        /// <summary>
        /// 頻繁に呼ばないように注意する
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <returns></returns>
        public _Type GetSubData<_Type>()
            where _Type : class
        {
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
        public bool TryAdd<_Type>(_Type data)
            where _Type : class
        {
            var isNull = GetSubData<_Type>() == null;
            if (isNull)
            {
                userData.Add(data);
                return true;
            }
            return false;
        }

        public _BaseData Ref { get; private set; }
        private List<object> userData;

        /// <summary>
        /// 編集可能なデータか？
        /// 整合性や前提条件を満たしていない場合もfalse
        /// </summary>
        public bool IsEditable { get; set; } = false;
    }

    public class PointEditorData
    {
        public PointEditorData(RnPoint point)
        {
            Assert.IsNotNull(point);
            refPoint = point;
            UpdateBasePosition();
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

        // 参照中のポイント
        private RnPoint refPoint;
        // 基準の座標
        private Vector3 basePosition;

        private Vector3 cacheTranslate;
        //private Vector3 cacheScale;
    }

    public class NodeEditorData
    {
        public NodeEditorData(GameObject gameObject)
        {
            Assert.IsNotNull(gameObject);
            this.RefGameObject = gameObject;
            UpdateBaseTransform();
            PointWithWeight.Clear();
        }

        public void UpdateBaseTransform()
        {
            baseTransform = new CacheTransform(RefGameObject.transform);
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

        public void ApplyTranslate()
        {
            var translate = RefGameObject.transform.position - baseTransform.position;
            foreach (var item in PointWithWeight)
            {
                var ptData = item.Value.data;
                ptData.Translate(translate * item.Value.weight);
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
        public GameObject RefGameObject;
        private CacheTransform baseTransform;

        private Dictionary<EditorData<RnPoint>, (PointEditorData data, float weight)> PointWithWeight { get; set; } =
            new Dictionary<EditorData<RnPoint>, (PointEditorData, float)>();

        public List<LinkGroupEditorData> Connections { get; private set; } = new List<LinkGroupEditorData>();

        public bool IsIntersection { get => Connections.Count >= 3; }

    }
    public class LinkGroupEditorData
    {
        public LinkGroupEditorData(NodeEditorData a, NodeEditorData b, IReadOnlyCollection<RnRoad> links, RnRoadGroup linkGroup)
        {
            Assert.IsNotNull(linkGroup);
            LinkGroup = linkGroup;

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
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

            Assert.IsNotNull(links);
            ConnectionLinks = links;
            // LinksがNode A,Bに接続されているかチェックするデバッグ機能
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
            var nR = LinkGroup.GetRightLaneCount();
            var nL = LinkGroup.GetLeftLaneCount();
            LinkGroup.SetLaneCount(nL, nR);
        }



        public NodeEditorData A { get; private set; }
        public NodeEditorData B { get; private set; }

        public IReadOnlyCollection<RnRoad> ConnectionLinks { get; private set; }

        public RnRoadGroup LinkGroup { get; private set; }

        public List<Vector3> CacheRoadPosList { get; set; }

        public IReadOnlyList<IEnumerable<RnLane>> RigthLanesGroup
        {
            get
            {
                // Link間でレーン数が違う場合に処理が出来ない　合わせる処理が必要？
                throw new NotImplementedException("仕様が未定のため未実装");
                //return null;
            }
        }

        public IEnumerable<RnLane> RightLanes
        {
            get
            {
                return LinkGroup.GetRightLanes();
            }
        }
        public IEnumerable<RnLane> LeftLanes
        {
            get
            {
                return LinkGroup.GetLeftLanes();
            }
        }


        // Person オブジェクトの比較をカスタマイズするクラス
        public class Comparer : IEqualityComparer<LinkGroupEditorData>
        {
            public bool Equals(LinkGroupEditorData x, LinkGroupEditorData y)
            {
                // 名前が同じ場合は同一とみなす
                return x.A == y.A && x.B == y.B;
            }

            public int GetHashCode(LinkGroupEditorData obj)
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
    /// Wayをスライドさせるためのデータ
    /// </summary>
    public class WayEditorData
    {
        public WayEditorData(RnWay target)
        {
            Assert.IsNotNull(target);
            BaseWay = target.Vertices.ToList();
            Ref = target;
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

        /// <summary>
        /// 選択可能か？
        /// </summary>
        public bool IsSelectable { get; set; } = true;
    }

}
