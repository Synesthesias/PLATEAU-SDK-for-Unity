using PLATEAU.Native;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// レーンを表すクラス
    /// </summary>
    public class RoadNetworkElementLane : RoadNetworkElement
    {
        /// <summary>
        /// レーン識別子のプレフィックス
        /// </summary>
        public static readonly string IDPrefix = "Lane";

        /// <summary>
        /// 地球の半径 (km)
        /// </summary>
        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// 元となる道路ネットワーク上のレーン
        /// </summary>
        public RnDataLane OriginLane { get; private set; } = null;

        /// <summary>
        /// レーンの順序
        /// </summary>
        public int Order { get; private set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">道路ネットワークのコンテキスト</param>
        /// <param name="id">レーンのID</param>
        /// <param name="lane">元のレーンデータ</param>
        /// <param name="order">レーンの順序</param>
        public RoadNetworkElementLane(RoadNetworkContext context, string id, RnDataLane lane, int order) : base(context, CreateID(id, order))
        {
            OriginLane = lane;

            Order = order;
        }

        /// <summary>
        /// レーン識別子を生成します
        /// </summary>
        /// <param name="id">元のID</param>
        /// <param name="order">レーンの順序</param>
        /// <returns>生成されたID</returns>
        private static string CreateID(string id, int order)
        {
            var newID = IDPrefix + id + "_" + order;

            return newID;
        }

        /// <summary>
        /// ジオメトリ情報を取得します
        /// </summary>
        /// <param name="isCenter">中心線を使用するかどうか</param>
        /// <returns>ジオメトリ情報のリスト</returns>
        public List<GeoJSON.Net.Geometry.Position> GetGeometory(bool isCenter = true)
        {
            var coods = new List<GeoJSON.Net.Geometry.Position>();

            var ways = roadNetworkContext.RoadNetworkGetter.GetWays();

            var linestrings = roadNetworkContext.RoadNetworkGetter.GetLineStrings();

            var points = roadNetworkContext.RoadNetworkGetter.GetPoints();

            var isReverse = OriginLane.IsReversed;

            var way = isCenter ? OriginLane.CenterWay : OriginLane.RightWay;

            if (way.IsValid)
            {
                var linestring = ways[way.ID].LineString;

                //　コピーを作成
                var vertices = new List<Vector3>();

                foreach (var point in linestrings[linestring.ID].Points)
                {
                    if (!point.IsValid) continue;

                    vertices.Add(points[point.ID].Vertex);
                }

                if (isReverse)
                {
                    vertices.Reverse();
                }

                foreach (var cood in vertices)
                {
                    var geoCood = roadNetworkContext.GeoReference.Unproject(new PlateauVector3d(cood.x, cood.y, cood.z));

                    coods.Add(new GeoJSON.Net.Geometry.Position(geoCood.Latitude, geoCood.Longitude));
                }
            }

            if (coods.Count < 2)
            {
                // フォールバック

                Debug.LogWarning("Link geometry is invalid. Fallback to straight line." + ID);

                var geoCood = roadNetworkContext.GeoReference.Unproject(new PlateauVector3d(0, 0, 0));

                if (coods.Count == 0)
                {
                    coods.Add(new GeoJSON.Net.Geometry.Position(geoCood.Latitude, geoCood.Longitude));
                }

                coods.Add(new GeoJSON.Net.Geometry.Position(geoCood.Latitude, geoCood.Longitude));
            }

            return coods;
        }

        /// <summary>
        /// レーンの長さを取得します
        /// </summary>
        /// <returns>レーンの長さ</returns>
        public float GetLength()
        {
            return (float)CalculateTotalDistance(GetGeometory());
        }

        /// <summary>
        /// ジオメトリ情報のリストから総距離を計算します
        /// </summary>
        /// <param name="positions">ジオメトリ情報のリスト</param>
        /// <returns>総距離</returns>
        private double CalculateTotalDistance(List<GeoJSON.Net.Geometry.Position> positions)
        {
            double totalDistance = 0.0;

            for (int i = 0; i < positions.Count - 1; i++)
            {
                var start = positions[i];
                var end = positions[i + 1];
                totalDistance += HaversineDistance(start, end);
            }

            return totalDistance;
        }

        /// <summary>
        /// 2つの位置間のハーバーサイン距離を計算します
        /// </summary>
        /// <param name="pos1">開始位置</param>
        /// <param name="pos2">終了位置</param>
        /// <returns>ハーバーサイン距離</returns>
        private double HaversineDistance(GeoJSON.Net.Geometry.Position pos1, GeoJSON.Net.Geometry.Position pos2)
        {
            double lat1 = pos1.Latitude * Math.PI / 180.0;
            double lon1 = pos1.Longitude * Math.PI / 180.0;
            double lat2 = pos2.Latitude * Math.PI / 180.0;
            double lon2 = pos2.Longitude * Math.PI / 180.0;

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c * 1000;
        }

        /// <summary>
        /// レーンの幅員を取得します
        /// </summary>
        /// <returns>レーンの幅員</returns>
        public float GetLaneWidth()
        {
            var ways = roadNetworkContext.RoadNetworkGetter.GetWays();

            var linestrings = roadNetworkContext.RoadNetworkGetter.GetLineStrings();

            var points = roadNetworkContext.RoadNetworkGetter.GetPoints();

            var wayleft = OriginLane.LeftWay;

            var wayRight = OriginLane.RightWay;

            if (wayleft.IsValid == false || wayRight.IsValid == false)
            {
                return 0;
            }

            var linestringleft = ways[wayleft.ID].LineString;

            var linestringright = ways[wayRight.ID].LineString;

            var pointsleft = linestrings[linestringleft.ID].Points;

            var pointsright = linestrings[linestringright.ID].Points;

            var left = new List<Vector3>();

            var right = new List<Vector3>();

            foreach (var point in pointsleft)
            {
                if (!point.IsValid) continue;

                var cood = points[point.ID].Vertex;

                left.Add(cood);
            }

            foreach (var point in pointsright)
            {
                if (!point.IsValid) continue;

                var cood = points[point.ID].Vertex;

                right.Add(cood);
            }

            return GetMinimumDistanceBetweenPointSets(left, right);
        }

        /// <summary>
        /// 2つの点集合間の最短距離を計算します
        /// </summary>
        /// <param name="pointSetA">点集合A</param>
        /// <param name="pointSetB">点集合B</param>
        /// <returns>最短距離</returns>
        private float GetMinimumDistanceBetweenPointSets(List<Vector3> pointSetA, List<Vector3> pointSetB)
        {
            float minDistance = float.MaxValue;

            // 点集合Aのすべての点
            foreach (Vector3 pointA in pointSetA)
            {
                // 点集合Bのすべての点
                foreach (Vector3 pointB in pointSetB)
                {
                    // 点Aと点B間の距離
                    float distance = Vector3.Distance(pointA, pointB);

                    // 最短距離を更新
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;
        }
    }
}