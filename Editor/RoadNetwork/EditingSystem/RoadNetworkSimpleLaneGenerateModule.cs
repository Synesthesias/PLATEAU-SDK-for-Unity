using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 単純なレーン生成機能を提供するクラス
    /// </summary>
    public class RoadNetworkSimpleLaneGenerateModule
    {
        public RoadNetworkSimpleLaneGenerateModule()
        {
            Init();
        }

        private RnWay baseStartWay;
        private RnWay baseEndWay;

        private List<RnPoint> startPoints = new List<RnPoint>(2);
        private List<RnPoint> endPoints = new List<RnPoint>(2);

        private RnRoad link;


        public void Init()
        {
            baseStartWay = null;
            baseEndWay = null;
            startPoints.Clear();
            endPoints.Clear();
            link = null;
        }

        public void Reset()
        {
            Debug.Log("設定をリセット");
            Init();
        }

        /// <summary>
        /// 時計回りに選択して追加する
        /// 追加出来ないpointは予め非表示にしたい
        /// </summary>
        /// <param name="way"></param>
        /// <returns></returns>
        public bool AddBorder(RnRoad link, RnWay way, RnPoint point)
        {
            if (startPoints.Count == 2 && endPoints.Count == 2)
            {
                Debug.Log("start, endのポイントがすでに設定されている");
                Reset();
                return false;
            }

            if (way.Points.Contains(point) == false)
            {
                Debug.Log("wayに含まれないPointを指定した");
                Reset();
                return false;
            }

            if (startPoints.Contains(point) || endPoints.Contains(point))
            {
                Debug.Log("すでに設定されているPointを指定した");
                Reset();
                return false;
            }

            if (this.link != null)
            {
                if (this.link != link)
                {
                    Debug.Log("すでに設定されているPointが属さないLinkのPointを選択した");
                    Reset();
                    return false;
                }
            }


            if (this.link == null)
            {
                this.link = link;
            }

            // start or end
            bool isStart = true;
            if (startPoints.Count == 2)
            {
                isStart = false;
            }

            RnWay baseWay;
            List<RnPoint> points;
            if (isStart)
            {
                baseWay = baseStartWay;
                points = startPoints;
            }
            else
            {
                baseWay = baseEndWay;
                points = endPoints;
            }

            // baseWayの設定
            var isSuc = SelectBaseWay(way, point, ref baseWay);
            if (isSuc == false)
            {
                Reset();
                return false;
            }

            // baseWayの更新
            if (isStart)
            {
                baseStartWay = baseWay;
            }
            else
            {
                baseEndWay = baseWay;
            }

            // pointを追加する
            points.Add(point);

            var debugMsg = isStart ? "start" : "end";
            Debug.Log($"{debugMsg} {points.Count}つめのpointを追加しました");

            return true;
        }

        private bool SelectBaseWay(RnWay way, RnPoint point, ref RnWay baseWay)
        {
            // nullなので設定
            if (baseWay == null)
            {
                baseWay = way;
                return true;
            }

            // そのまま
            if (baseWay.Points.Contains(point))
            {
                return true;
            }

            // baseWayに含まれていない場合はbaseWayを新規のwayに更新する
            // ただし新規のwayに追加済みのポイントが含まれていない場合は失敗
            Assert.IsTrue(startPoints.Count > 0);
            if (way.Points.Contains(startPoints[0]) == false)
            {
                Debug.Log("wayに含まれないPointを指定した");
                return false;
            }

            baseWay = way;
            return true;
        }

        public bool CanBuild()
        {
            if (startPoints.Count != 2 || endPoints.Count != 2)
            {
                return false;
            }

            if (link == null)
            {
                return false;
            }

            return true;
        }

        public RnLane Build()
        {
            if (startPoints.Count != 2 || endPoints.Count != 2)
            {
                Debug.Log("pointの設定が足りない");
                Reset();
                return null;
            }

            if (link == null)
            {
                Debug.Log("linkの設定が足りない");
                Reset();
                return null;
            }

            var start = CreateBorderWay(baseStartWay, startPoints);
            var end = CreateBorderWay(baseEndWay, endPoints);

            var leftLine = new RnLineString();
            var rightLine = new RnLineString();

            // 時計回りで startPoint,endPointが設定されていることが前提
            // TODO　交差していたら参照順を変える処理を追加する
            var leftWay = new RnWay(RnLineString.Create(new RnPoint[] { startPoints[0], endPoints[1] }));
            var rightWay = new RnWay(RnLineString.Create(new RnPoint[] { startPoints[1], endPoints[0] }));

            var lane = new RnLane(leftWay, rightWay, start, end);
            link.AddMainLane(lane);
            return lane;
        }

        private static RnWay CreateBorderWay(RnWay baseWay, List<RnPoint> points)
        {
            var idx0 = baseWay.FindPoint(points[0]);
            var idx1 = baseWay.FindPoint(points[1]);
            return CreateBorderWay(baseWay, ref idx0, ref idx1);
        }

        private static RnWay CreateBorderWay(RnWay baseWay, ref int idx0, ref int idx1)
        {
            bool wasReversed = false;
            if (idx0 > idx1)
            {
                var tmp = idx0;
                idx0 = idx1;
                idx1 = tmp;
                wasReversed = true;
            }

            List<RnPoint> wayPoints = null;

            var n = idx1 - idx0 + 1;
            wayPoints = new List<RnPoint>(n);
            for (int i = idx0; i <= idx1; i++)
            {
                wayPoints.Add(baseWay.LineString.Points[i]);
            }

            if (wasReversed)
            {
                wayPoints.Reverse();
            }

            var way = new RnWay(RnLineString.Create(wayPoints));
            return way;
        }
    }
}