using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

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
        public PointHandle GetRef(in RnId<RoadNetworkDataPoint> id);
        public ILineStringsHandle GetRef(in RnId<RoadNetworkDataLineString> id);

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
        public PointHandle[] WriteNewPoints(in RoadNetworkDataPoint[] points);

        public ILineStringsHandle[] WriteNewLineStrings(in RoadNetworkDataLineString[] lineStrings);

        public void ClearAll();

#if false
        /// <summary>
        /// ストレージのサイズを再調整する
        /// 未使用の領域を削除して詰める
        /// この関数の後に既存のIDには更新処理を掛ける必要がある
        /// </summary>
        /// <returns></returns>
        public bool FitPointStorage();
        public bool FitLineStringsStorage();
#endif
    }

    [Serializable]
    public class RoadNetworkStorage : IRoadNetworkEditable, IRoadNetworkDynamicEditable
    {
        public TrafficRegulationStorage TrafficRegulationStorage { get => trafficRegulationStorage; }
        public PrimitiveDataStorage PrimitiveDataStorage { get => primitiveDataStorage; }

        [SerializeField /*, PersistentAmongPlayMode*/]
        private TrafficRegulationStorage trafficRegulationStorage;

        [SerializeField /*, PersistentAmongPlayMode*/]
        private PrimitiveDataStorage primitiveDataStorage = new PrimitiveDataStorage();

        private PrimitiveDataHandleManager primitiveDataHandleManager;

        private PointHandle[] initHandles;

        private void OnEnable()
        {
        }

        private void Reset()
        {

        }

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
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
            var lineStringsIds = new RnId<RoadNetworkDataLineString>[0];
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

        PointHandle IRoadNetworkEditable.GetRef(in RnId<RoadNetworkDataPoint> id)
        {
            return primitiveDataHandleManager.RequestHandle(id);
        }

        ILineStringsHandle IRoadNetworkEditable.GetRef(in RnId<RoadNetworkDataLineString> id)
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

        PointHandle[] IRoadNetworkDynamicEditable.WriteNewPoints(in RoadNetworkDataPoint[] points)
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

            pointHandles = new Dictionary<RnId<RoadNetworkDataPoint>, PointHandle>(pointHandlesCapacity);
            freePointHandles = new List<PointHandle>(pointHandlesCapacity);

            lineStringsHandles =
                new Dictionary<RnId<RoadNetworkDataLineString>, LineStringsHandle>(lineStringsHandlesCapacity);
            freeLineStringsHandles = new List<RoadNetworkDataLineString>(lineStringsHandlesCapacity);
        }

        private RoadNetworkStorage storage;
        private Dictionary<RnId<RoadNetworkDataPoint>, PointHandle> pointHandles;
        private List<PointHandle> freePointHandles;
        private Dictionary<RnId<RoadNetworkDataLineString>, LineStringsHandle> lineStringsHandles;
        private List<RoadNetworkDataLineString> freeLineStringsHandles;

        /// <summary>
        /// Handleの生成or取得を行う
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PointHandle RequestHandle(in RnId<RoadNetworkDataPoint> id)
        {
            PointHandle pointHnd = null;
            if (pointHandles.TryGetValue(id, out pointHnd) == false)
            {
                pointHnd = new PointHandle(storage, id);
                pointHandles.Add(id, pointHnd);
            }
            return pointHnd;
        }

        public PointHandle[] RequestHandle(in RnId<RoadNetworkDataPoint>[] ids)
        {
            var ret = new PointHandle[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var hnd = RequestHandle(ids[i]);
                ret[i] = hnd;
            }
            return ret;
        }

        public ILineStringsHandle RequestHandle(in RnId<RoadNetworkDataLineString> id)
        {
            LineStringsHandle lineStringsHnd = null;
            if (lineStringsHandles.TryGetValue(id, out lineStringsHnd) == false)
            {
                lineStringsHnd = new LineStringsHandle(storage, id);
                lineStringsHandles.Add(id, lineStringsHnd);
            }
            return lineStringsHnd;
        }
        public ILineStringsHandle[] RequestHandle(in RnId<RoadNetworkDataLineString>[] ids)
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
    [Serializable]
    public class PrimitiveDataStorage
    {
        // 最適化方法案
        // 大きくメモリを確保してそこから切り出す形に修正する
        // Listで確保しているストレージは配列にして初期化処理、解放処理の最適化

        public PrimitiveStorage<RoadNetworkDataPoint> Points { get => points; }
        public PrimitiveStorage<RoadNetworkDataLineString> LineStrings { get => lineStrings; }


        [SerializeField] private PrimitiveStorage<RoadNetworkDataPoint> points = new PrimitiveStorage<RoadNetworkDataPoint>();

        [SerializeField]
        private PrimitiveStorage<RoadNetworkDataLineString> lineStrings =
            new PrimitiveStorage<RoadNetworkDataLineString>();

        [field: SerializeField]
        public PrimitiveStorage<RoadNetworkDataLane> Lanes { get; set; }
            = new PrimitiveStorage<RoadNetworkDataLane>();

        [field: SerializeField]
        public PrimitiveStorage<RoadNetworkDataBlock> Blocks { get; set; } =
            new PrimitiveStorage<RoadNetworkDataBlock>();

        [field: SerializeField]
        public PrimitiveStorage<RoadNetworkDataLink> Links { get; set; } = new PrimitiveStorage<RoadNetworkDataLink>();

        [field: SerializeField]
        public PrimitiveStorage<RoadNetworkDataNode> Nodes { get; set; } = new PrimitiveStorage<RoadNetworkDataNode>();

        [field: SerializeField]
        public PrimitiveStorage<RoadNetworkDataWay> Ways { get; set; } = new PrimitiveStorage<RoadNetworkDataWay>();

        [field: SerializeField]
        public PrimitiveStorage<RoadNetworkDataTrack> Tracks { get; set; } =
            new PrimitiveStorage<RoadNetworkDataTrack>();

        // #NOTE : structだとgetterでとってきたときにコピーが返る -> それに対してWriteしても元データが書き換わらないのでclassにする
        [Serializable]
        public class PrimitiveStorage<TPrimType>
            where TPrimType : IPrimitiveData
        {
            [SerializeField] private List<TPrimType> dataList = new List<TPrimType>();
            // 未使用のデータインデックス アプリを次回起動した際にインデックスを利用できるように残す
            [SerializeField] private List<int> freeDataIndices = new List<int>();

            public IReadOnlyList<TPrimType> DataList => dataList;

            /// <summary>
            /// ストレージのID群を取得
            /// </summary>
            /// <returns></returns>
            public RnId<TPrimType>[] GetIds()
            {
                var ids = new RnId<TPrimType>[dataList.Count];
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = new RnId<TPrimType>(i);
                }
                return ids;
            }

            /// <summary>
            /// 新しくデータを書き込む
            /// 容量の確保も行う
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public RnId<TPrimType>[] WriteNew(in TPrimType[] values)
            {
                var start = dataList.Count;
                dataList.AddRange(values);
                var ret = new RnId<TPrimType>[values.Length];
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = new RnId<TPrimType>(start + i);
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
            public TPrimType Read(in RnId<TPrimType> id)
            {
                return dataList[id];
            }

            /// <summary>
            /// 点の情報を書き込む
            /// </summary>
            /// <param name="id"></param>
            /// <param name="val"></param>
            public void Write(in RnId<TPrimType> id, in TPrimType val)
            {
                dataList[id] = val;
            }

            /// <summary>
            /// 未使用のデータのIDを登録する
            /// 未使用のデータIDは未登録である必要がある
            /// </summary>
            /// <param name="ids"></param>
            public void RegisterFreeIDs(in RnId<TPrimType>[] ids)
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
            public RnId<TPrimType>[] RequestFreeIDs()
            {
                Assert.IsNotNull(freeDataIndices);
                var numFree = freeDataIndices.Count;
                var ret = new RnId<TPrimType>[numFree];
                int i = 0;
                foreach (var item in freeDataIndices)
                {
                    ret[i++] = new RnId<TPrimType>(freeDataIndices[i]);
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
        protected RnId<TPrimType> id;
    }

    /// <summary>
    /// Pointデータへの参照
    /// 読み書き機能を提供
    /// </summary>
    public class PointHandle : PrimitiveDataHandle<RoadNetworkDataPoint>
    {
        public PointHandle(RoadNetworkStorage storage, RnId<RoadNetworkDataPoint> id)
        {
            Assert.IsNotNull(storage);
            this.storage = storage;
            this.id = id;
        }


        public RnId<RoadNetworkDataPoint> ID { get { return id; } }

        public RoadNetworkDataPoint Val
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
        public RoadNetworkDataPoint this[RnId<RoadNetworkDataPoint> id] { get; set; }
        public RnId<RoadNetworkDataPoint> this[int index] { get; set; }
        public RoadNetworkDataLineString Val { get; set; }
    }

    public class LineStringsHandle : PrimitiveDataHandle<RoadNetworkDataLineString>, ILineStringsHandle
    {
        public LineStringsHandle(RoadNetworkStorage storage, RnId<RoadNetworkDataLineString> id)
        {
            Assert.IsNotNull(storage);
            Assert.IsTrue(id.Id >= 0);
            this.storage = storage;
            this.id = id;
        }
        public RnId<RoadNetworkDataLineString> ID { get { return id; } }

        public RoadNetworkDataPoint this[RnId<RoadNetworkDataPoint> id]
        {
            get
            {
                var pointId = storage.PrimitiveDataStorage.LineStrings.Read(this.id).Points[id.Id];
                return storage.PrimitiveDataStorage.Points.Read(pointId);
            }
            set
            {
                var pointId = storage.PrimitiveDataStorage.LineStrings.Read(this.id).Points[id.Id];
                storage.PrimitiveDataStorage.Points.Write(pointId, value);
            }
        }
        RnId<RoadNetworkDataPoint> ILineStringsHandle.this[int index]
        {
            get
            {
                return storage.PrimitiveDataStorage.LineStrings.Read(this.id).Points[index];
            }
            set
            {
                storage.PrimitiveDataStorage.LineStrings.Read(this.id).Points[index] = value;
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
