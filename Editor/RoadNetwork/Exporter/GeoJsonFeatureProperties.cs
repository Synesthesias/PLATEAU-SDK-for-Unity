namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// RoadNetworkのプロパティを保持する基底クラス
    /// </summary>
    public abstract class GeoJsonFeatureProperties
    {
        public class ProhiBit
        {
            public const string Left = "left";
            public const string Straight = "straight";
            public const string Right = "right";
            public const string UTurn = "u_turn";
            public const string LeftFront = "leftfront";
            public const string RightFront = "rightfront";
            public const string RightRear = "rightrear";
            public const string LeftRear = "leftrear";
        }

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

        public enum TypeConfig
        {
            Small = 1,    // 小型
            Large = 2,    // 大型
            Bus = 4       // バス
        }
    }
}