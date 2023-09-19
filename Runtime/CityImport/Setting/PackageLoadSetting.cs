using System;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// テクスチャ結合後の解像度です。
    /// </summary>
    internal enum TexturePackingResolution : int
    {
        W2048H2048 = 0,
        W4096H4096 = 1,
        W8192H8192 = 2,
    }

    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、パッケージごとの設定です。
    /// <see cref="CityLoadConfig"/> によって保持されます。
    /// このクラスに対応するGUIクラスは PackageLoadSettingGUI です。
    ///
    /// 実装上の注意：
    /// ・特定のパッケージ種で追加の設定項目がある場合はサブクラスで実装します。
    /// ・この設定項目を追加・削除する場合、<see cref="ConvertToNativeOption"/>も合わせて実装しないと反映されないことに注意してください。 
    /// </summary>
    internal class PackageLoadSetting
    {
        public PredefinedCityModelPackage Package { get; }
        public bool LoadPackage { get; set; }
        public bool IncludeTexture { get; set; }
        
        public LODRange LODRange { get; set; }
        
        public MeshGranularity MeshGranularity { get; set; }
        public bool DoSetMeshCollider { get; set; }
        public bool DoSetAttrInfo { get; set; }
        public Material FallbackMaterial { get; set; }
        public bool EnableTexturePacking { get; set; }
        public TexturePackingResolution TexturePackingResolution { get; set; }
        
        public PackageLoadSetting(PredefinedCityModelPackage package, bool loadPackage, bool includeTexture, LODRange lodRange, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo, Material fallbackMaterial, bool enableTexturePacking, TexturePackingResolution texturePackingResolution)
        {
            Package = package;
            LoadPackage = loadPackage;
            IncludeTexture = includeTexture;
            LODRange = lodRange;
            MeshGranularity = meshGranularity;
            DoSetMeshCollider = doSetMeshCollider;
            DoSetAttrInfo = doSetAttrInfo;
            EnableTexturePacking = enableTexturePacking;
            TexturePackingResolution = texturePackingResolution;
            FallbackMaterial = fallbackMaterial;
        }

        /// <summary> コピーコンストラクタ </summary>
        protected PackageLoadSetting(PackageLoadSetting src) : this(
            src.Package, src.LoadPackage, src.IncludeTexture, src.LODRange, src.MeshGranularity, src.DoSetMeshCollider, src.DoSetAttrInfo, src.FallbackMaterial, src.EnableTexturePacking, src.TexturePackingResolution
            )
        {
        }
        
        // インポート設定のうち、Unityでは必ずこうなるという定数部分です。
        public const CoordinateSystem MeshAxes = CoordinateSystem.EUN;
        public const float UnitScale = 1.0f;

        /// <summary>
        /// <paramref name="package"/> に対応した<see cref="PackageLoadSetting"/> またはそのサブクラスを返します。
        /// 具体的には、
        /// 土地以外の場合は<see cref="PackageLoadSetting"/>を返します。
        /// 土地の場合は追加の設定項目があるので、<see cref="PackageLoadSetting"/>のサブクラスである<see cref="ReliefLoadSetting"/>を返します。
        /// </summary>
        public static PackageLoadSetting CreateSettingFor(PredefinedCityModelPackage package, PackageLoadSetting baseSetting)
        {
            // パッケージ種による場合分けです。
            // これと似たロジックが PackageLoadSettingGUI.PackageLoadSettingGUIList にあるので、変更時はそちらも合わせて変更をお願いします。
            return package switch
            {
                PredefinedCityModelPackage.Relief => new ReliefLoadSetting(true, baseSetting),
                _ => new PackageLoadSetting(baseSetting)
            };
        }

        /// <summary>
        /// インポート設定について、C++のstructに変換します。
        /// </summary>
        public virtual MeshExtractOptions ConvertToNativeOption(PlateauVector3d referencePoint, int coordinateZoneID, Extent extent)
        {

            return new MeshExtractOptions(
                referencePoint: referencePoint,
                meshAxes: MeshAxes,
                meshGranularity: MeshGranularity,
                minLOD: (uint)LODRange.MinLOD,
                maxLOD: (uint)LODRange.MaxLOD,
                exportAppearance: IncludeTexture,
                gridCountOfSide: 10,
                unitScale: UnitScale,
                coordinateZoneID: coordinateZoneID,
                excludeCityObjectOutsideExtent: ShouldExcludeCityObjectOutsideExtent(Package),
                excludePolygonsOutsideExtent: ShouldExcludePolygonsOutsideExtent(Package),
                enableTexturePacking: EnableTexturePacking,
                texturePackingResolution: 1, // 土地専用の設定は ReliefLoadSetting で行うので、ここでは仮の値にします。
                extent: extent,
                attachMapTile: false, // 土地専用の設定は ReliefLoadSetting で行うので、ここでは false に固定します。
                mapTileZoomLevel: 15,
                mapTileURL: ReliefLoadSetting.DefaultMapTileUrl);
        }
        
        private static bool ShouldExcludeCityObjectOutsideExtent(PredefinedCityModelPackage package)
        {
            if (package == PredefinedCityModelPackage.Relief) return false;
            return true;
        }

        private static bool ShouldExcludePolygonsOutsideExtent(PredefinedCityModelPackage package)
        {
            return !ShouldExcludeCityObjectOutsideExtent(package);
        }

    }

    /// <summary>
    /// パッケージごとの設定項目に、土地特有の設定項目を追加したクラスです。
    /// これに対応するGUIクラスは ReliefLoadSettingGUI を参照してください。
    /// </summary>
    internal class ReliefLoadSetting : PackageLoadSetting
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
                if ((!value.StartsWith("http://")) && (!value.StartsWith("https://"))) throw new ArgumentException("URL must start with https:// or http://.");
                this.mapTileURL = value;
            }
        }

        public ReliefLoadSetting(bool attachMapTile, PackageLoadSetting baseSetting) :
            base(baseSetting)
        {
            // 初期値を決めます。
            AttachMapTile = attachMapTile;
            MapTileZoomLevel = 15;
            MapTileURL = DefaultMapTileUrl;
        }

        public override MeshExtractOptions ConvertToNativeOption(PlateauVector3d referencePoint, int coordinateZoneID, Extent extent)
        {
            var nativeOption = base.ConvertToNativeOption(referencePoint, coordinateZoneID, extent);
            nativeOption.AttachMapTile = AttachMapTile;
            nativeOption.MapTileZoomLevel = MapTileZoomLevel;
            nativeOption.MapTileURL = MapTileURL;
            nativeOption.TexturePackingResolution = GetTexturePackingResolution();
            return nativeOption;
        }
        
        private uint GetTexturePackingResolution()
        {
            return TexturePackingResolution switch
            {
                TexturePackingResolution.W2048H2048 => 2048,
                TexturePackingResolution.W4096H4096 => 4096,
                TexturePackingResolution.W8192H8192 => 8192,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    internal struct LODRange
    {
        /// <summary> ユーザーが選択したLOD範囲の下限 </summary>
        public int MinLOD { get; }
        /// <summary> ユーザーが選択したLODの上限 </summary>
        public int MaxLOD { get; }
        /// <summary> ユーザーが選択したデータのなかで存在するLODの最大値 </summary>
        public int AvailableMaxLOD { get; }

        public LODRange(int minLOD, int maxLOD, int availableMaxLOD)
        {
            if (minLOD > maxLOD) throw new ArgumentException("Condition minLOD <= maxLOD does not meet.");
            MinLOD = minLOD;
            MaxLOD = maxLOD;
            AvailableMaxLOD = availableMaxLOD;
// =======
//         /// <summary> ユーザーが選択した範囲のなかで存在するLODの最大値 </summary>
//         public readonly int AvailableMaxLOD;
//         
//         public MeshGranularity MeshGranularity;
//         public bool DoSetMeshCollider;
//         public bool DoSetAttrInfo;
//         public Material FallbackMaterial;
//         public bool EnableTexturePacking;
//         public TexturePackingResolution TexturePackingResolution;
//
//         public PackageLoadSetting(PredefinedCityModelPackage package, bool loadPackage, bool includeTexture, int minLOD, int maxLOD, int availableMaxLOD, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo, bool enableTexturePacking, TexturePackingResolution texturePackingResolution)
//         {
//             Package = package;
//             this.LoadPackage = loadPackage;
//             this.IncludeTexture = includeTexture;
//             this.MinLOD = minLOD;
//             this.MaxLOD = maxLOD;
//             this.AvailableMaxLOD = availableMaxLOD;
//             this.MeshGranularity = meshGranularity;
//             this.DoSetMeshCollider = doSetMeshCollider;
//             this.DoSetAttrInfo = doSetAttrInfo;
//             this.EnableTexturePacking = enableTexturePacking;
//             this.TexturePackingResolution = texturePackingResolution;
//             this.FallbackMaterial = MaterialPathUtil.LoadDefaultFallbackMaterial(package);;
// >>>>>>> dev/v2
        }

    }

}
