using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 道路ネットワーク関係のテスト用クラス
    /// ストレージ周りのテスト向けに利用していた
    /// 他の道路ネットワークのテストが記載されたところに移植する予定
    /// </summary>
    public class TestTrafficManager : MonoBehaviour
    {
        [SerializeField]
        private RoadNetworkStorage storage = new RoadNetworkStorage();

        [SerializeField]
        private GameObject testObj;

        bool isApply = false;
        public RoadNetworkDataPoint[] newPostion;
        public List<Debug2DArrayElement<RnID<RoadNetworkDataPoint>>> newPointIds;

        private void Start()
        {
            if (testObj == null)
                return;

            // テスト対象のデータを取得
            var dynamicEditable = storage as IRoadNetworkDynamicEditable;

            // ストレージのクリア　前のデータが残っている場合にクリアする
            dynamicEditable.ClearAll();

            // デバッグ用のデータを作成
            int numPoint = 100;
            List<RoadNetworkDataPoint> points = new List<RoadNetworkDataPoint>(numPoint);
            for (int i = 0; i < numPoint; i++)
            {
                points.Add(new RoadNetworkDataPoint(new Vector3(i / (numPoint / 2), 0, i * 0.5f)));
            }

            // ストレージの初期化
            storage.InitializeStorage();

        }

        private void Update()
        {
            if (isApply)
            {

            }
        }

        [Serializable]
        public struct Debug2DArrayElement<_ElementType>
        {
            public Debug2DArrayElement(int capacity)
            {
                A = new _ElementType[capacity];
            }

            public _ElementType this[int idx]
            {
                get { return A[idx]; }
                set { A[idx] = value; }
            }

            public _ElementType[] A;
        }
    }
}
