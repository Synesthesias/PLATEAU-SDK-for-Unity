using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 各ズームレベルは、カメラからの距離が何メートルのときに利用するかを設定します。
    /// </summary>
    [Serializable]
    public class TileLoadDistanceCollection
    {
        /// <summary>
        /// ロード距離設定のリスト
        /// </summary>
        public List<TileLoadDistance> LoadDistances = new List<TileLoadDistance>
        {
            new TileLoadDistance(11, -10000f, 500f),
            new TileLoadDistance(10, 500f, 1500f),
            new TileLoadDistance(9, 1500f, 10000f)
        };


        /// <summary>
        /// 指定されたズームレベルのロード距離設定を取得します。
        /// </summary>
        /// <param name="zoomLevel">ズームレベル</param>
        /// <returns>ロード距離設定、見つからない場合はnull</returns>
        public TileLoadDistance GetLoadDistance(int zoomLevel)
        {
            return LoadDistances.FirstOrDefault(ld => ld.ZoomLevel == zoomLevel);
        }

        /// <summary>
        /// 指定された距離が指定されたズームレベルのロード範囲内にあるかどうかを判定します。
        /// </summary>
        /// <param name="distance">判定する距離</param>
        /// <param name="zoomLevel">ズームレベル</param>
        /// <returns>範囲内の場合true</returns>
        public bool IsWithinRange(float distance, int zoomLevel)
        {
            var loadDistance = GetLoadDistance(zoomLevel);
            return loadDistance?.IsWithinRange(distance) ?? false;
        }

        
        
        /// <summary>
        /// 各ズームレベルごとのロード距離範囲を表現するクラス。
        /// </summary>
        [Serializable]
        public class TileLoadDistance
        {
            /// <summary>
            /// ズームレベル
            /// </summary>
            public int ZoomLevel { get; set; }

            /// <summary>
            /// 最小ロード距離
            /// </summary>
            public float MinDistance { get; set; }

            /// <summary>
            /// 最大ロード距離
            /// </summary>
            public float MaxDistance { get; set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="zoomLevel">ズームレベル</param>
            /// <param name="minDistance">最小ロード距離</param>
            /// <param name="maxDistance">最大ロード距離</param>
            public TileLoadDistance(int zoomLevel, float minDistance, float maxDistance)
            {
                ZoomLevel = zoomLevel;
                MinDistance = minDistance;
                MaxDistance = maxDistance;
            }

            /// <summary>
            /// 指定された距離がこのロード範囲内にあるかどうかを判定します。
            /// </summary>
            /// <param name="distance">判定する距離</param>
            /// <returns>範囲内の場合true</returns>
            public bool IsWithinRange(float distance)
            {
                return distance >= MinDistance && distance <= MaxDistance;
            }

        }
    }
} 