using System;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Config
{
    /// <summary>
    /// パッケージごとの設定項目に、土地特有の設定項目を追加したクラスです。
    /// これに対応するGUIクラスは ReliefLoadSettingGUI を参照してください。
    /// </summary>
    internal class ReliefImportConfig : PackageImportConfig
    {
        public bool AttachMapTile { get; set; }

        private int mapTileZoomLevel;
        public const int MinZoomLevel = 1;

        /// <summary> 地理院地図やGoogleMapPlatformで存在するズームレベルの最大値です。2023年8月に調べたところ18でした。 </summary>
        public const int MaxZoomLevel = 18;

        public const string DefaultMapTileUrl = "https://cyberjapandata.gsi.go.jp/xyz/seamlessphoto/{z}/{x}/{y}.jpg";

        public int MapTileZoomLevel
        {
            get => this.mapTileZoomLevel;
            set
            {
                if (value < MinZoomLevel || value > MaxZoomLevel)
                {
                    throw new ArgumentOutOfRangeException(nameof(MapTileZoomLevel));
                }

                this.mapTileZoomLevel = value;
            }
        }

        private string mapTileURL;

        public string MapTileURL
        {
            get => this.mapTileURL;
            set
            {
                if ((!value.StartsWith("http://")) && (!value.StartsWith("https://")))
                    throw new ArgumentException("URL must start with https:// or http://.");
                this.mapTileURL = value;
            }
        }

        public ReliefImportConfig(PackageImportConfig baseConfig) :
            base(baseConfig)
        {
            // 初期値を決めます。
            AttachMapTile = true;
            MapTileZoomLevel = 18;
            MapTileURL = DefaultMapTileUrl;
        }

        public override MeshExtractOptions ConvertToNativeOption(PlateauVector3d referencePoint, int coordinateZoneID)
        {
            var nativeOption = base.ConvertToNativeOption(referencePoint, coordinateZoneID);
            nativeOption.AttachMapTile = AttachMapTile;
            nativeOption.MapTileZoomLevel = MapTileZoomLevel;
            nativeOption.MapTileURL = MapTileURL;
            nativeOption.TexturePackingResolution = GetTexturePackingResolution();
            return nativeOption;
        }

        private uint GetTexturePackingResolution()
        {
            return ConfExtendable.GetTexturePackingResolution();
        }
    }
}