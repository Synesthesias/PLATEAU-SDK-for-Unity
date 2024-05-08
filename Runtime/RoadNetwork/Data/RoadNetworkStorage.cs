using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// ノード、リンク、レーン、ブロックなどのデータを所持するオブジェクトが継承するインターフェイス
    /// 道路ネットワークエディタのパラメータで型指定するために作成
    /// （仮でこのファイルに配置している）
    /// </summary>
    public class RoadNetworkObject : MonoBehaviour
    {

    }

    /// <summary>
    /// 道路ネットワークの変更機能を提供する
    /// </summary>
    public interface IRoadNetworkEditable
    {
        public PointHandle GetRef(in RoadNetworkID<RoadNetworkPoint> id);
        public ILineStringsHandle GetRef(in RoadNetworkID<RoadNetworkDataLineString> id);

        public void RelaseHandle(ref PointHandle hnd);

    }

    /// <summary>
    /// 道路ネットワークの変更機能を提供する
    /// ストレージの構造を変更する可能性があるものはこちらのインターフェイスに用意する
    /// 重めの処理が多い
    /// これの理由としてはIDの更新が必要になったり配列リアロケーションを行う必要があったりするため
    /// </summary>
    public interface IRoadNetworkDynamicEditable
    {
        /// <summary>
        /// ストレージを確保して値を書き込む
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public PointHandle[] WriteNewPoints(in RoadNetworkPoint[] points);

        public ILineStringsHandle[] WriteNewLineStrings(in RoadNetworkDataLineString[] lineStrings);


        public void ClearAll();
        /// <summary>
        /// ストレージのサイズを再調整する
        /// 未使用の領域を削除して詰める
        /// この関数の後に既存のIDには更新処理を掛ける必要がある
        /// </summary>
        /// <returns></returns>
        //public bool FitPointStorage();
        //public bool FitLineStringsStorage();

    }

    [Serializable]
    public class RoadNetworkStorage : IRoadNetworkEditable, IRoadNetworkDynamicEditable
    {
        public TrafficRegulationStorage TrafficRegulationStorage { get => trafficRegulationStorage; }
        public PrimitiveDataStorage PrimitiveDataStorage { get => primitiveDataStorage; }

        public IReadOnlyList<RoadNetworkDataLane> Lanes => lanes;

        public IReadOnlyList<RoadNetworkDataBlock> Blocks => blocks;

        public IReadOnlyList<RoadNetworkDataLink> Links => links;

        public IReadOnlyList<RoadNetworkDataNode> Nodes => nodes;

        // 頂点情報
        [SerializeField] private List<RoadNetworkPoint> points = new List<RoadNetworkPoint>();

        [SerializeField] private List<RoadNetworkDataLineString> lineStrings = new List<RoadNetworkDataLineString>();

        // レーンリスト
        [SerializeField] private List<RoadNetworkDataLane> lanes = new List<RoadNetworkDataLane>();

        [SerializeField] private List<RoadNetworkDataBlock> blocks = new List<RoadNetworkDataBlock>();

        [SerializeField] private List<RoadNetworkDataLink> links = new List<RoadNetworkDataLink>();

        [SerializeField] private List<RoadNetworkDataNode> nodes = new List<RoadNetworkDataNode>();


        [SerializeField/*, PersistentAmongPlayMode*/] private TrafficRegulationStorage trafficRegulationStorage;
        [SerializeField/*, PersistentAmongPlayMode*/] private PrimitiveDataStorage primitiveDataStorage;

        private PrimitiveDataHandleManager primitiveDataHandleManager;

        private PointHandle[] initHandles;

        void OnEnable()
        {
        }

        void Reset()
        {

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InitializeStorage()
        {
            // ストレージの初期化  ←　たぶん必要ない シリアライズされたデータが読み込めていればok
            if (primitiveDataStorage == null)
            {
                primitiveDataStorage = new PrimitiveDataStorage();
            }

            // ハンドルマネージャーの初期化
            var pointIds = primitiveDataStorage.Points.GetIds();
            var lineStringsIds = new RoadNetworkID<RoadNetworkDataLineString>[0];
            primitiveDataHandleManager =
                new PrimitiveDataHandleManager(this, pointIds.Length, lineStringsIds.Length);

            // ハンドルマネージャーのハンドルのインスタンス化
            initHandles = new PointHandle[pointIds.Length];
            int initHandlesOffset = 0;
            foreach (var item in pointIds)
            {
                initHandles[initHandlesOffset++] = primitiveDataHandleManager.RequestHandle(item);
            }

            // 未使用のストレージのデータのIDをハンドルマネージャーに登録する
            var freeIds = primitiveDataStorage.Points.RequestFreeIDs();
            foreach (var item in freeIds)
            {
                var freeHnd = primitiveDataHandleManager.RequestHandle(item);
                primitiveDataHandleManager.ReleaseHandle(freeHnd);
            }
        }

        public void FinalizeStorage()
        {
            // 未使用のハンドルをストレージに登録する
            // ハンドルマネージャーの終了処理
            // ストレージの終了処理
        }

        PointHandle IRoadNetworkEditable.GetRef(in RoadNetworkID<RoadNetworkPoint> id)
        {
            return primitiveDataHandleManager.RequestHandle(id);
        }

        ILineStringsHandle IRoadNetworkEditable.GetRef(in RoadNetworkID<RoadNetworkDataLineString> id)
        {
            return primitiveDataHandleManager.RequestHandle(id);
        }

        /// <summary>
        /// ハンドルを解放する
        /// </summary>
        /// <param name="hnd"></param>
        /// <exception cref="NotImplementedException"></exception>
        void IRoadNetworkEditable.RelaseHandle(ref PointHandle hnd)
        {
            primitiveDataHandleManager.ReleaseHandle(hnd);
        }

        PointHandle[] IRoadNetworkDynamicEditable.WriteNewPoints(in RoadNetworkPoint[] points)
        {
            //PointHandle[] ret = null;
            //Point[] newPoints = null;

            // memo freeハンドルが残ったままアプリを閉じると？永遠に使われないストレージが出現する？
            // 未使用領域もストレージのシリアライズ時に残す
            // 初期化時にフリーハンドルを作成する

            var numFreeHandle = primitiveDataHandleManager.NumFreePointHandle();
            if (numFreeHandle >= points.Length)
            {
                var freeHandles = primitiveDataHandleManager.RequestFreeHandles(points.Length);
                for (int i = 0; i < points.Length; i++)
                {
                    freeHandles[i].Val = points[i];
                }
                return freeHandles.ToArray();
            }
            else
            {
                var ids = primitiveDataStorage.Points.WriteNew(points);
                var hnds = primitiveDataHandleManager.RequestHandle(ids);
                return hnds;
            }

            //// フリーハンドルがある場合はそれを割り当てる
            //if (numFreeHandle > 0)
            //{
            //    // フリーハンドルの数が足りない。足りない分は新規で作成
            //    int pointsOffsetIdx = 0;
            //    // フリーハンドルをすべて利用する
            //    var freeHandles = primitiveDataHandleManager.RequestFreeHandles(numFreeHandle);

            //    foreach (var item in freeHandles)
            //    {
            //        if (pointsOffsetIdx >= numFreeHandle)
            //            break;
            //        item.Point = points[pointsOffsetIdx];
            //        pointsOffsetIdx++;
            //    }
            //    var numRemaining = points.Length - pointsOffsetIdx;
            //    if (numRemaining > 0)
            //    {
            //        // 足りない分を新規で作成
            //        newPoints = points.AsSpan(pointsOffsetIdx, numRemaining).ToArray();
            //    }
            //}
            //else // フリーハンドルがないのですべて新規で割り当て
            //{
            //    newPoints = points;
            //}

            //Assert.IsNotNull(newPoints);
            //if (newPoints.Length > 0)
            //{
            //    var ids = primitiveDataStorage.Points.WriteNew(newPoints);
            //    ret.comb( = primitiveDataHandleManager.RequestHandle(ids);
            //    ret = hnds;
            //}

            //return ret;

        }

        ILineStringsHandle[] IRoadNetworkDynamicEditable.WriteNewLineStrings(in RoadNetworkDataLineString[] lineStrings)
        {
            var ids = primitiveDataStorage.LineStrings.WriteNew(lineStrings);
            var hnds = primitiveDataHandleManager.RequestHandle(ids);
            return hnds;
        }

        void IRoadNetworkDynamicEditable.ClearAll()
        {
            primitiveDataStorage.Points.ClearAll();
            primitiveDataStorage.LineStrings.ClearAll();
        }

        //bool IRoadNetworkDynamicEditable.FitPointStorage()
        //{
        //    throw new NotImplementedException();
        //}

        //bool IRoadNetworkDynamicEditable.FitLineStringsStorage()
        //{
        //    throw new NotImplementedException();
        //}

    }

    [System.Serializable]
    public struct TrafficRegulationStorage
    {
        public TrafficRegulationInfo[] regulationCollection;
    }

    /// <summary>
    /// シリアライズしない
    /// それぞれの道路モデルネットワークの初期化される際に再構築されるはず
    /// </summary>
    public class PrimitiveDataHandleManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="pointHandlesCapacity"></param>
        /// <param name="lineStringsHandlesCapacity"></param>
        public PrimitiveDataHandleManager(
            in RoadNetworkStorage storage, int pointHandlesCapacity, int lineStringsHandlesCapacity)
        {
            Assert.IsNotNull(storage);
            Assert.IsTrue(pointHandlesCapacity >= 0);
            Assert.IsTrue(lineStringsHandlesCapacity >= 0);

            this.storage = storage;

            pointHandles = new Dictionary<RoadNetworkID<RoadNetworkPoint>, PointHandle>(pointHandlesCapacity);
            freePointHandles = new List<PointHandle>(pointHandlesCapacity);

            lineStringsHandles =
                new Dictionary<RoadNetworkID<RoadNetworkDataLineString>, LineStringsHandle>(lineStringsHandlesCapacity);
            freeLineStringsHandles = new List<RoadNetworkDataLineString>(lineStringsHandlesCapacity);
        }

        private RoadNetworkStorage storage;
        private Dictionary<RoadNetworkID<RoadNetworkPoint>, PointHandle> pointHandles;
        private List<PointHandle> freePointHandles;
        private Dictionary<RoadNetworkID<RoadNetworkDataLineString>, LineStringsHandle> lineStringsHandles;
        private List<RoadNetworkDataLineString> freeLineStringsHandles;

        /// <summary>
        /// Handleの生成or取得を行う
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PointHandle RequestHandle(in RoadNetworkID<RoadNetworkPoint> id)
        {
            PointHandle pointHnd = null;
            if (pointHandles.TryGetValue(id, out pointHnd) == false)
            {
                pointHnd = new PointHandle(storage, id);
                pointHandles.Add(id, pointHnd);
            }
            return pointHnd;
        }

        public PointHandle[] RequestHandle(in RoadNetworkID<RoadNetworkPoint>[] ids)
        {
            var ret = new PointHandle[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var hnd = RequestHandle(ids[i]);
                ret[i] = hnd;
            }
            return ret;
        }

        public ILineStringsHandle RequestHandle(in RoadNetworkID<RoadNetworkDataLineString> id)
        {
            LineStringsHandle lineStringsHnd = null;
            if (lineStringsHandles.TryGetValue(id, out lineStringsHnd) == false)
            {
                lineStringsHnd = new LineStringsHandle(storage, id);
                lineStringsHandles.Add(id, lineStringsHnd);
            }
            return lineStringsHnd;
        }
        public ILineStringsHandle[] RequestHandle(in RoadNetworkID<RoadNetworkDataLineString>[] ids)
        {
            var ret = new ILineStringsHandle[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var hnd = RequestHandle(ids[i]);
                ret[i] = hnd;
            }
            return ret;
        }

        /// <summary>
        /// ハンドルを解放する
        /// 解放したハンドルは未使用のハンドルとして登録されてハンドルがリクエストされた際に再利用される
        /// </summary>
        /// <param name="pointHandle"></param>
        public void ReleaseHandle(in PointHandle pointHandle)
        {
            Assert.IsTrue(freePointHandles.Contains(pointHandle) == false);
            freePointHandles.Add(pointHandle);
        }

        /// <summary>
        /// 未使用のハンドルの数を取得する
        /// </summary>
        /// <returns></returns>
        public int NumFreePointHandle()
        {
            return freePointHandles.Count;
        }

        /// <summary>
        /// 未使用のハンドルを取得する
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<PointHandle> RequestFreeHandles(int size)
        {
            Debug.Assert(freePointHandles.Count >= size);
            var ret = freePointHandles.GetRange(0, size);
            freePointHandles.RemoveRange(0, size);
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class PrimitiveDataStorage
    {
        // 最適化方法案
        // 大きくメモリを確保してそこから切り出す形に修正する
        // Listで確保しているストレージは配列にして初期化処理、解放処理の最適化

        public PrimitiveDataStorage()
        {
            points = new PrimitiveStorage<RoadNetworkPoint>();
            lineStrings = new PrimitiveStorage<RoadNetworkDataLineString>();
        }

        public PrimitiveStorage<RoadNetworkPoint> Points { get => points; }
        public PrimitiveStorage<RoadNetworkDataLineString> LineStrings { get => lineStrings; }

        [SerializeField] private PrimitiveStorage<RoadNetworkPoint> points;
        [SerializeField] private PrimitiveStorage<RoadNetworkDataLineString> lineStrings;

        [System.Serializable]
        public class PrimitiveStorage<TPrimType>
            where TPrimType : IPrimitiveData
        {
            [SerializeField] private List<TPrimType> dataList;
            // 未使用のデータインデックス アプリを次回起動した際にインデックスを利用できるように残す
            [SerializeField] private List<int> freeDataIndices;

            /// <summary>
            /// ストレージのID群を取得
            /// </summary>
            /// <returns></returns>
            public RoadNetworkID<TPrimType>[] GetIds()
            {
                var ids = new RoadNetworkID<TPrimType>[dataList.Count];
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = new RoadNetworkID<TPrimType>(i);
                }
                return ids;
            }

            /// <summary>
            /// 新しくデータを書き込む
            /// 容量の確保も行う
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public RoadNetworkID<TPrimType>[] WriteNew(in TPrimType[] values)
            {
                var start = dataList.Count;
                dataList.AddRange(values);
                var ret = new RoadNetworkID<TPrimType>[values.Length];
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = new RoadNetworkID<TPrimType>(start + i);
                return ret;
            }

            /// <summary>
            /// ストレージの解放
            /// </summary>
            /// <param name="size"></param>
            public void Release(int size)
            {
                Assert.IsTrue(dataList.Count >= size);
                dataList.RemoveRange(dataList.Count - size, size);
                //ストレージでは連続したメモリ領域途中の解放は扱わない　ハンドルで解放する際に未使用のHandleとして保持。新しくデータが欲しい時にそのハンドルを渡す
            }

            /// <summary>
            /// 点の情報を読み込む
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public TPrimType Read(in RoadNetworkID<TPrimType> id)
            {
                return dataList[id];
            }

            /// <summary>
            /// 点の情報を書き込む
            /// </summary>
            /// <param name="id"></param>
            /// <param name="val"></param>
            public void Write(in RoadNetworkID<TPrimType> id, in TPrimType val)
            {
                dataList[id] = val;
            }

            /// <summary>
            /// 未使用のデータのIDを登録する
            /// 未使用のデータIDは未登録である必要がある
            /// </summary>
            /// <param name="ids"></param>
            public void RegisterFreeIDs(in RoadNetworkID<TPrimType>[] ids)
            {
                Assert.IsNull(freeDataIndices);
                freeDataIndices = new List<int>(ids.Length);
                foreach (var item in ids)
                {
                    freeDataIndices.Add(item);
                }
            }

            /// <summary>
            /// 未使用のデータのIDを取得する
            /// 未使用のデータIDのリストが無効になる
            /// </summary>
            /// <returns></returns>
            public RoadNetworkID<TPrimType>[] RequestFreeIDs()
            {
                Assert.IsNotNull(freeDataIndices);
                var numFree = freeDataIndices.Count;
                var ret = new RoadNetworkID<TPrimType>[numFree];
                int i = 0;
                foreach (var item in freeDataIndices)
                {
                    ret[i++] = new RoadNetworkID<TPrimType>(freeDataIndices[i]);
                }
                freeDataIndices.Clear();
                return ret;
            }

            /// <summary>
            /// ストレージのクリア
            /// すべてのデータが消えるので注意
            /// </summary>
            public void ClearAll()
            {
                dataList.Clear();
            }
        }
    }

    public abstract class PrimitiveDataHandle<TPrimType>
        where TPrimType : IPrimitiveData
    {
        protected RoadNetworkStorage storage;
        protected RoadNetworkID<TPrimType> id;
    }

    /// <summary>
    /// Pointデータへの参照
    /// 読み書き機能を提供
    /// </summary>
    public class PointHandle : PrimitiveDataHandle<RoadNetworkPoint>
    {
        public PointHandle(RoadNetworkStorage storage, RoadNetworkID<RoadNetworkPoint> id)
        {
            Assert.IsNotNull(storage);
            this.storage = storage;
            this.id = id;
        }


        public RoadNetworkID<RoadNetworkPoint> ID { get { return id; } }

        public RoadNetworkPoint Val
        {
            get
            {
                return storage.PrimitiveDataStorage.Points.Read(id);
            }
            set
            {
                storage.PrimitiveDataStorage.Points.Write(id, value);
            }
        }
    }

    /// <summary>
    /// LineStrings系統への参照
    /// 読み書き機能を提供
    /// </summary>
    public interface ILineStringsHandle
    {
        public RoadNetworkPoint this[RoadNetworkID<RoadNetworkPoint> id] { get; set; }
        public RoadNetworkID<RoadNetworkPoint> this[int index] { get; set; }
        public RoadNetworkDataLineString Val { get; set; }
    }

    public class LineStringsHandle : PrimitiveDataHandle<RoadNetworkDataLineString>, ILineStringsHandle
    {
        public LineStringsHandle(RoadNetworkStorage storage, RoadNetworkID<RoadNetworkDataLineString> id)
        {
            Assert.IsNotNull(storage);
            Assert.IsTrue(id.Id >= 0);
            this.storage = storage;
            this.id = id;
        }
        public RoadNetworkID<RoadNetworkDataLineString> ID { get { return id; } }

        public RoadNetworkPoint this[RoadNetworkID<RoadNetworkPoint> id]
        {
            get
            {
                var pointId = storage.PrimitiveDataStorage.LineStrings.Read(this.id).points[id.Id];
                return storage.PrimitiveDataStorage.Points.Read(pointId);
            }
            set
            {
                var pointId = storage.PrimitiveDataStorage.LineStrings.Read(this.id).points[id.Id];
                storage.PrimitiveDataStorage.Points.Write(pointId, value);
            }
        }
        RoadNetworkID<RoadNetworkPoint> ILineStringsHandle.this[int index]
        {
            get
            {
                return storage.PrimitiveDataStorage.LineStrings.Read(this.id).points[index];
            }
            set
            {
                storage.PrimitiveDataStorage.LineStrings.Read(this.id).points[index] = value;
            }
        }

        public RoadNetworkDataLineString Val
        {
            get
            {
                return storage.PrimitiveDataStorage.LineStrings.Read(id);
            }
            set
            {
                storage.PrimitiveDataStorage.LineStrings.Write(id, value);
            }
        }

    }
}
