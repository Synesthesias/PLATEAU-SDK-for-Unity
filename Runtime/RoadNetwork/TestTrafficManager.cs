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
    public class TestTrafficManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject testObj;

        private PointHandle[] pointHandles;
        private ILineStringsHandle[] lineStringsHandles;

        bool isApply = false;
        public Point[] newPostion;
        public List<Debug2DArrayElement<RoadNetworkPrimID<Point>>> newPointIds;

        private void Start()
        {
            if (testObj == null)
                return;

            // テスト対象のデータを取得
            var storage = testObj.GetComponent<RoadNetworkStorage>();
            var editable = storage as IRoadNetworkEditable;
            var dynamicEditable = storage as IRoadNetworkDynamicEditable;

            // ストレージのクリア　前のデータが残っている場合にクリアする
            dynamicEditable.ClearAll();

            // デバッグ用のデータを作成
            int numPoint = 100;
            List<Point> points = new List<Point>(numPoint);
            for (int i = 0; i < numPoint; i++)
            {
                points.Add(new Point(new Vector3(i / (numPoint / 2), 0, i * 0.5f)));
            }

            // ストレージの初期化
            storage.InitializeStorage();

            // RoadNetworkStorageを介してストレージを確保、書き込み
            pointHandles = dynamicEditable.WriteNewPoints(points.ToArray());

            // Pointの値の取得テスト
            newPostion = new Point[numPoint];
            for (int i = 0; i < numPoint; i++)
            {
                newPostion[i] = pointHandles[i].Val;
            }


            // LineStringsのテストデータ
            int numLineStrigns = numPoint / 10;
            List<LineStrings> lineStrings = new List<LineStrings>(numLineStrigns);
            for (int i = 0; i < numLineStrigns; i++)
            {
                int numData = (numLineStrigns + 1) - i;
                //int numData = 10;
                Assert.IsTrue(numData < numPoint);
                var listStringsData = new List<RoadNetworkPrimID<Point>>(numData);
                for (int j = 0; j < numData; j++)
                {
                    listStringsData.Add(pointHandles[j].ID);
                };

                lineStrings.Add(new LineStrings()
                {
                    points = listStringsData
                });

            }

            // LineStringsの書き込み
            lineStringsHandles = dynamicEditable.WriteNewLineStrings(lineStrings.ToArray());

            // テスト　LineStringsからPointを表示する
            newPointIds = new List<Debug2DArrayElement<RoadNetworkPrimID<Point>>>(lineStringsHandles.Length);
            int idxOffset = 0;
            foreach (var lineStringsHnd in lineStringsHandles)
            {
                var idsBuf = new Debug2DArrayElement<RoadNetworkPrimID<Point>>(lineStringsHnd.Val.Count);
                newPointIds.Add(idsBuf);
                int i = 0;
                foreach (var pointID in lineStringsHnd.Val)
                {
                    idsBuf[i++] = pointID;
                }
                idxOffset++;
            }

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
