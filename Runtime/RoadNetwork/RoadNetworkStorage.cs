using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using static GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Configuration.ConfigurationTreeNodeCheck;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 道路ネットワークの変更機能を提供する
    /// </summary>
    public interface IRoadNetworkEditable
    {
        public PointHandle GetRef(in RoadNetworkPrimID<Point> id);
        public ILineStringsHandle GetRef(in RoadNetworkPrimID<LineStrings> id);

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
        public PointHandle[] WriteNewPoints(in Point[] points);

        public ILineStringsHandle[] WriteNewLineStrings(in LineStrings[] lineStrings);

        /// <summary>
        /// ストレージのサイズを再調整する
        /// 未使用の領域を削除して詰める
        /// この関数の後に既存のIDには更新処理を掛ける必要がある
        /// </summary>
        /// <returns></returns>
        public bool FitPointStorage();
        public bool FitLineStringsStorage();

    }

    public class RoadNetworkStorage : MonoBehaviour,
         IRoadNetworkEditable, IRoadNetworkDynamicEditable
    {
        public TrafficRegulationStorage TrafficRegulationStorage { get => trafficRegulationStorage; }
        public PrimitiveDataStorage PrimitiveDataStorage { get => primitiveDataStorage; }

        [SerializeField] private TrafficRegulationStorage trafficRegulationStorage;
        [SerializeField] private PrimitiveDataStorage primitiveDataStorage;
        private PrimitiveDataHandleManager primitiveDataHandleManager;

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

        /// <summary>
        /// IDの更新
        /// この中でHandle内のIDを更新する
        /// ストレージの容量を最適化した直後に実行する必要がある
        /// </summary>
        /// <param name="id"></param>
        void UpdateID(ref RoadNetworkPrimID<Point> id)
        {
            throw new NotImplementedException();
        }
        void UpdateID(ref RoadNetworkPrimID<LineStrings> id)
        {
            throw new NotImplementedException();
        }


        PointHandle IRoadNetworkEditable.GetRef(in RoadNetworkPrimID<Point> id)
        {
            return primitiveDataHandleManager.GetHandle(id);
        }

        ILineStringsHandle IRoadNetworkEditable.GetRef(in RoadNetworkPrimID<LineStrings> id)
        {
            return primitiveDataHandleManager.GetHandle(id);
        }

        PointHandle[] IRoadNetworkDynamicEditable.WriteNewPoints(in Point[] points)
        {
            var ids = primitiveDataStorage.Points.WriteNew(points);
            var hnds = primitiveDataHandleManager.GetHandle(ids);
            return hnds;
        }

        ILineStringsHandle[] IRoadNetworkDynamicEditable.WriteNewLineStrings(in LineStrings[] lineStrings)
        {
            var ids = primitiveDataStorage.LineStrings.WriteNew(lineStrings);
            var hnds = primitiveDataHandleManager.GetHandle(ids);
            return hnds;
        }

        bool IRoadNetworkDynamicEditable.FitPointStorage()
        {
            throw new NotImplementedException();
        }

        bool IRoadNetworkDynamicEditable.FitLineStringsStorage()
        {
            throw new NotImplementedException();
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
    public struct PrimitiveDataHandleManager
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
            
            pointHandles = new Dictionary<RoadNetworkPrimID<Point>, PointHandle>(pointHandlesCapacity);
            pointHndRefCnt = 0;

            lineStringsHandles = 
                new Dictionary<RoadNetworkPrimID<LineStrings>, LineStringsHandle>(lineStringsHandlesCapacity);
            lineStringsHndRefCnt = 0;
        }

        private RoadNetworkStorage storage;
        private Dictionary<RoadNetworkPrimID<Point>, PointHandle> pointHandles;
        private int pointHndRefCnt;
        private Dictionary<RoadNetworkPrimID<LineStrings>, LineStringsHandle> lineStringsHandles;
        private int lineStringsHndRefCnt;

        /// <summary>
        /// Handleの生成or取得を行う
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PointHandle GetHandle(in RoadNetworkPrimID<Point> id)
        {
            PointHandle pointHnd = null;
            if (pointHandles.TryGetValue(id, out pointHnd) == false)
            {
                pointHnd = new PointHandle(storage, id);
                pointHandles.Add(id, pointHnd);
            }
            pointHndRefCnt++;
            return pointHnd;
        }

        public PointHandle[] GetHandle(in RoadNetworkPrimID<Point>[] ids)
        {
            var ret = new PointHandle[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var hnd = GetHandle(ids[i]);
                ret[i] = hnd;
            }
            lineStringsHndRefCnt++;
            return ret;
        }

        public ILineStringsHandle GetHandle(in RoadNetworkPrimID<LineStrings> id)
        {
            LineStringsHandle lineStringsHnd = null;
            if (lineStringsHandles.TryGetValue(id, out lineStringsHnd) == false)
            {
                lineStringsHnd = new LineStringsHandle(storage, id);
                lineStringsHandles.Add(id, lineStringsHnd);
            }
            return lineStringsHnd;
        }
        public ILineStringsHandle[] GetHandle(in RoadNetworkPrimID<LineStrings>[] ids)
        {
            var ret = new ILineStringsHandle[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var hnd = GetHandle(ids[i]);
                ret[i] = hnd;
            }
            return ret;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public struct PrimitiveDataStorage
    {
        // 最適化方法案
        // 大きくメモリを確保してそこから切り出す形に修正する
        // Listで確保しているストレージは配列にして初期化処理、解放処理の最適化

        public PrimitiveStorage<Point> Points { get => points; }
        public PrimitiveStorage<LineStrings> LineStrings { get => lineStrings; }

        [SerializeField] PrimitiveStorage<Point> points;
        [SerializeField] PrimitiveStorage<LineStrings> lineStrings;
        //[SerializeField] private List<Point> points;
        //[SerializeField] private int pointsUsedCnt;
        //[SerializeField] private List<LineStrings> lineStrings;
        //[SerializeField] private int lineStringsUsedCnt;
        //[SerializeField] private LinkedLineStrings[] linkedLineStrings;

        ///// <summary>
        ///// Linestringsのストレージを確保する
        ///// </summary>
        ///// <param name="size"></param>
        ///// <returns></returns>
        //public bool AllocateLineStrings(int size)
        //{
        //    lineStrings.AddRange(new LineStrings[size]);
        //    return true;
        //}

        ///// <summary>
        ///// Linestringsのストレージを解放する
        ///// </summary>
        ///// <param name="size"></param>
        ///// <returns></returns>
        //private bool RelaseLineStrings(int size)
        //{
        //    lineStrings.RemoveRange((lineStrings.Count - 1) - size, size);
        //    return true;
        //}


        ///// <summary>
        ///// ラインストリングの値を読み込む
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public LineStrings ReadLineString(in RoadNetworkPrimID<LineStrings> id)
        //{
        //    return lineStrings[id.id];
        //}

        ///// <summary>
        ///// Storage内のILineStringsの値を書き換える
        ///// 適切な継承型を利用しないと落ちるので修正する？
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="val"></param>
        //public void WriteLineString(in RoadNetworkPrimID<LineStrings> id, in LineStrings val)
        //{
        //    lineStrings[id.id] = val;
        //}

        [System.Serializable]
        public struct PrimitiveStorage<_PrimType> where _PrimType : struct, IPrimitiveData
        {
            [SerializeField] private List<_PrimType> dataList;

            /// <summary>
            /// 新しくデータを書き込む
            /// 容量の確保も行う
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public RoadNetworkPrimID<_PrimType>[] WriteNew(in _PrimType[] values)
            {
                var start = dataList.Count;
                dataList.AddRange(values);
                var ret = new RoadNetworkPrimID<_PrimType>[start];
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = new RoadNetworkPrimID<_PrimType>(){
                       id = start + i
                    };
                return ret;
            }

            /// <summary>
            /// ストレージの未使用領域を解放する
            /// </summary>
            /// <returns></returns>
            private bool Fit()
            {
                throw new NotImplementedException();

                //var len = dataList.Count - usedCnt;
                //// 使用済みの領域まで解放しようとしている
                //if (len <= 0)
                //{
                //    return true;
                //}
                //dataList.RemoveRange((dataList.Count - 1) - len, len);
                //return true;
            }

            /// <summary>
            /// 点の情報を読み込む
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public _PrimType Read(in RoadNetworkPrimID<_PrimType> id)
            {
                return dataList[id.id];
            }

            /// <summary>
            /// 点の情報を書き込む
            /// </summary>
            /// <param name="id"></param>
            /// <param name="val"></param>
            public void Write(in RoadNetworkPrimID<_PrimType> id, in _PrimType val)
            {
                dataList[id.id] = val;
            }

        }
    }

    public abstract class PrimitiveDataHandle<_PrimType>
        where _PrimType : struct, IPrimitiveData
    {
        protected RoadNetworkStorage storage;
        protected RoadNetworkPrimID<_PrimType> id;
    }

    /// <summary>
    /// Pointデータへの参照
    /// 読み書き機能を提供
    /// </summary>
    public class PointHandle : PrimitiveDataHandle<Point>
    {
        public PointHandle(RoadNetworkStorage storage, RoadNetworkPrimID<Point> id)
        {
            Assert.IsNotNull(storage);
            Assert.IsTrue(id.id >= 0);
            this.storage = storage;
            this.id = id;
        }

        public Point Point 
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
        public Point this[RoadNetworkPrimID<Point> index] { get; set; }
        public LineStrings Val { get; set; }
    }

    public class LineStringsHandle : PrimitiveDataHandle<LineStrings>, ILineStringsHandle
    {
        public LineStringsHandle(RoadNetworkStorage storage, RoadNetworkPrimID<LineStrings> id)
        {
            Assert.IsNotNull(storage);
            Assert.IsTrue(id.id >= 0);
            this.storage = storage;
            this.id = id;
        }

        public Point this[RoadNetworkPrimID<Point> id] 
        { 
            get 
            {
                var pointId = storage.PrimitiveDataStorage.LineStrings.Read(this.id).points[id.id];
                return storage.PrimitiveDataStorage.Points.Read(pointId);
            }
            set
            {
                var pointId = storage.PrimitiveDataStorage.LineStrings.Read(this.id).points[id.id];
                storage.PrimitiveDataStorage.Points.Write(pointId, value);
            }
        }

        public LineStrings Val
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

    //[Serializable]
    //public struct LinkedLineStrings
    //{
    //    public Point[] Points
    //    {
    //        get => throw new NotImplementedException();
    //        set => throw new NotImplementedException();
    //    }

    //    [SerializeField] private RoadNetworkPrimID<ILineStrings>[] lineStringsIDs;


    //    public IEnumerator<Point> GetEnumerator()
    //    {
    //        for (int i = 0; i < lineStringsIDs.Length; i++)
    //        {
    //            for (int j = 0; j < length; j++)
    //            {

    //            }
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public interface IPrimitiveHandle<_PrimDataType>
    //{
    //    public RoadNetworkPrimID<_PrimDataType> ID { get; }
    //}

    //public interface IPrimitiveROHandle<_PrimDataType> : IPrimitiveHandle<_PrimDataType>
    //{
    //    public _PrimDataType Value { get; }
    //}

    //public interface IPrimitiveRWHandle<_PrimDataType> : IPrimitiveHandle<_PrimDataType>
    //{
    //    public _PrimDataType Value { get; set; }

    //}

    //public struct PointROHandle : IPrimitiveROHandle<Point>, IPrimitiveRWHandle<Point>
    //{

    //    public RoadNetworkPrimID<Point> ID { get; set; }
    //    Point IPrimitiveROHandle<Point>.Value { get => storage.ReadPoint(ID); }
    //    Point IPrimitiveRWHandle<Point>.Value { get => ((IPrimitiveROHandle<Point>)this).Value; set => storage.WritePoint(ID, value); }

    //    RoadNetworkStorage storage;
    //}

    //public struct RoadNetworkPrimRWHandle<_PrimDataType>
    //{
    //    public int ID;
    //}


}
