using PLATEAU.Dataset;

namespace PLATEAU.CityImport.Config
{
    /// <summary>
    /// インポート設定のうち、範囲選択より前に行う部分です。
    /// </summary>
    public class CityLoadConfigBeforeAreaSelect
    {
        private CityLoadConfigBeforeAreaSelect(IDatasetSourceConfig datasetSourceConfig, int coordinateZoneID)
        {
            DatasetSourceConfig = datasetSourceConfig;
            CoordinateZoneID = coordinateZoneID;
        }

        public CityLoadConfigBeforeAreaSelect(){}
        
        /// <summary>
        /// 都市モデル読み込み元に関する設定です。
        /// </summary>
        public IDatasetSourceConfig DatasetSourceConfig { get; set; }
        
        /// <summary>
        /// 平面直角座標系の番号です。
        /// 次のサイトで示される平面直角座標系の番号です。
        /// https://www.gsi.go.jp/sokuchikijun/jpc.html
        /// </summary>
        public int CoordinateZoneID { get; set; } = 9;
    }
}