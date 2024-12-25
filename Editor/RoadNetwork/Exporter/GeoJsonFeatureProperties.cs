namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// GeoJSONフィーチャに関連付けられるプロパティを定義する抽象クラス
    /// </summary>
    public abstract class GeoJsonFeatureProperties
    {
        /// <summary>
        /// 通行禁止の種類を定義するクラス
        /// </summary>
        public class ProhiBit
        {
            /// <summary>左方向禁止</summary>
            public const string Left = "left";

            /// <summary>直進禁止</summary>
            public const string Straight = "straight";

            /// <summary>右方向禁止</summary>
            public const string Right = "right";

            /// <summary>Uターン禁止</summary>
            public const string UTurn = "u_turn";

            /// <summary>前左禁止</summary>
            public const string LeftFront = "leftfront";

            /// <summary>前右禁止</summary>
            public const string RightFront = "rightfront";

            /// <summary>後左禁止</summary>
            public const string RightRear = "rightrear";

            /// <summary>後右禁止</summary>
            public const string LeftRear = "leftrear";
        }

        /// <summary>
        /// 進行可能な方向
        /// </summary>
        public enum TurnConfig
        {
            None = 0,         // 進行できない
            Left = 1,         // 左折
            Straight = 2,     // 直進
            Right = 4,        // 右折
            UTurn = 8,        // Uターン
            ForwardLeft = 16, // 前左
            ForwardRight = 32,// 前右
            BackRight = 64,   // 後右
            BackLeft = 128    // 後左
        }

        /// <summary>
        /// 車両の種類
        /// </summary>
        public enum TypeConfig
        {
            Small = 1,    // 小型
            Large = 2,    // 大型
            Bus = 4       // バス
        }
    }
}