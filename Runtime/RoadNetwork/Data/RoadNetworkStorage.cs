using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// 道路ネットワークを所持したオブジェクトが継承するクラス
    /// </summary>
    public interface IRoadNetworkObject
    {
        public RnModel RoadNetwork { get; }
    }

    /// <summary>
    /// 道路ネットワークの変更機能を提供する
    /// ストレージの構造を変更する可能性があるものはこちらのインターフェイスに用意する
    /// 重めの処理が多い
    /// これの理由としてはIDの更新が必要になったり配列リアロケーションを行う必要があったりするため
    /// </summary>
    public interface IRoadNetworkDynamicEditable
    {
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
    public class RoadNetworkStorage : IRoadNetworkDynamicEditable
    {
        public PrimitiveDataStorage PrimitiveDataStorage { get => primitiveDataStorage; }

        [SerializeField]
        private PrimitiveDataStorage primitiveDataStorage = new PrimitiveDataStorage();

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

        /// <summary>
        /// ストレージの初期化
        /// </summary>
        public void InitializeStorage()
        {
            // ストレージの初期化  ←　たぶん必要ない シリアライズされたデータが読み込めていればok
            if (primitiveDataStorage == null)
            {
                primitiveDataStorage = new PrimitiveDataStorage();
            }
        }

        public void FinalizeStorage()
        {
            // 未使用のハンドルをストレージに登録する
            // ハンドルマネージャーの終了処理
            // ストレージの終了処理
        }

        void IRoadNetworkDynamicEditable.ClearAll()
        {
            primitiveDataStorage.Points.ClearAll();
            primitiveDataStorage.LineStrings.ClearAll();
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

        public PrimitiveStorage<RnDataTrafficLightController> TrafficLightControllers { get => trafficLightControllers; }
        public PrimitiveStorage<RnDataTrafficLight> TrafficLights { get => trafficLights; }
        public PrimitiveStorage<RnDataTrafficSignalPattern> TrafficSignalPatterns { get => trafficSignalPatterns; }
        public PrimitiveStorage<RnDataTrafficSignalPhase> TrafficSignalPhases { get => trafficSignalPhases; }

        [field: SerializeField]
        private PrimitiveStorage<RnDataTrafficLightController> trafficLightControllers = new PrimitiveStorage<RnDataTrafficLightController>();

        [field: SerializeField] 
        private PrimitiveStorage<RnDataTrafficLight> trafficLights = new PrimitiveStorage<RnDataTrafficLight>();

        [field: SerializeField] 
        private PrimitiveStorage<RnDataTrafficSignalPattern> trafficSignalPatterns = new PrimitiveStorage<RnDataTrafficSignalPattern>();

        [field: SerializeField] 
        private PrimitiveStorage<RnDataTrafficSignalPhase> trafficSignalPhases = new PrimitiveStorage<RnDataTrafficSignalPhase>();

        public PrimitiveStorage<RnDataPoint> Points { get => points; }
        public PrimitiveStorage<RnDataLineString> LineStrings { get => lineStrings; }


        //[field: SerializeField]
        //public PrimitiveStorage<TrafficRegulationInfoData> regulationCollection =
        //    new PrimitiveStorage<TrafficRegulationInfoData>();

        [field: SerializeField] private PrimitiveStorage<RnDataPoint> points = new PrimitiveStorage<RnDataPoint>();

        [field: SerializeField]
        private PrimitiveStorage<RnDataLineString> lineStrings =
            new PrimitiveStorage<RnDataLineString>();

        [field: SerializeField]
        public PrimitiveStorage<RnDataLane> Lanes { get; set; }
            = new PrimitiveStorage<RnDataLane>();

        [field: SerializeField]
        public PrimitiveStorage<RnDataBlock> Blocks { get; set; } =
            new PrimitiveStorage<RnDataBlock>();

        [field: SerializeField]
        public PrimitiveStorage<RnDataWay> Ways { get; set; } = new PrimitiveStorage<RnDataWay>();

        [field: SerializeField]
        public PrimitiveStorage<RnDataRoadBase> RoadBases { get; set; } = new PrimitiveStorage<RnDataRoadBase>();

        [field: SerializeField]
        public PrimitiveStorage<RnDataSideWalk> SideWalks { get; set; } = new();


        // #NOTE : structだとgetterでとってきたときにコピーが返る -> それに対してWriteしても元データが書き換わらないのでclassにする
        [Serializable]
        public class PrimitiveStorage<TPrimType> : IRnIDGeneratable
            where TPrimType : IPrimitiveData
        {
            [SerializeField, SerializeReference] private List<TPrimType> dataList = new List<TPrimType>();
            // 未使用のデータインデックス アプリを次回起動した際にインデックスを利用できるように残す
            [SerializeField] private List<int> freeDataIndices = new List<int>();

            public IReadOnlyList<TPrimType> DataList => dataList;

            /// <summary>
            /// IDの要求
            /// 同一のhandleIndexを渡した時　同じIDが返ることを保証する
            /// </summary>
            /// <param name="handleIndex"></param>
            /// <returns></returns>
            public RnID<TPrimType> RequsetID(int handleIndex)
            {
                return new RnID<TPrimType>(handleIndex, this);
            }

            /// <summary>
            /// ストレージのID群を取得
            /// </summary>
            /// <returns></returns>
            public RnID<TPrimType>[] GetIds()
            {
                var ids = new RnID<TPrimType>[dataList.Count];
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = RequsetID(i);
                }
                return ids;
            }

            /// <summary>
            /// 新しくデータを書き込む
            /// 容量の確保も行う
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public RnID<TPrimType>[] WriteNew(in TPrimType[] values)
            {
                var start = dataList.Count;
                dataList.AddRange(values);
                var ret = new RnID<TPrimType>[values.Length];
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = RequsetID(start + i);
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
            public TPrimType Read(in RnID<TPrimType> id)
            {
                return dataList[id.ID];
            }

            /// <summary>
            /// 点の情報を書き込む
            /// </summary>
            /// <param name="id"></param>
            /// <param name="val"></param>
            public void Write(in RnID<TPrimType> id, in TPrimType val)
            {
                dataList[id.ID] = val;
            }

            /// <summary>
            /// 未使用のデータのIDを登録する
            /// 未使用のデータIDは未登録である必要がある
            /// </summary>
            /// <param name="ids"></param>
            public void RegisterFreeIDs(in RnID<TPrimType>[] ids)
            {
                Assert.IsNull(freeDataIndices);
                freeDataIndices = new List<int>(ids.Length);
                foreach (var item in ids)
                {
                    freeDataIndices.Add(item.ID);
                }
            }

            /// <summary>
            /// 未使用のデータのIDを取得する
            /// 未使用のデータIDのリストが無効になる
            /// </summary>
            /// <returns></returns>
            public RnID<TPrimType>[] RequestFreeIDs()
            {
                Assert.IsNotNull(freeDataIndices);
                var numFree = freeDataIndices.Count;
                var ret = new RnID<TPrimType>[numFree];
                int i = 0;
                foreach (var item in freeDataIndices)
                {
                    ret[i++] = RequsetID(freeDataIndices[i]);
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
}
