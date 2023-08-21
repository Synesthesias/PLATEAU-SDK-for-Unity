using System;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using UnityEngine.UI;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、パッケージごとの設定です。
    /// <see cref="CityLoadConfig"/> によって保持されます。
    /// このクラスに対応するGUIクラスは PackageLoadSettingGUI です。
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

        public PackageLoadSetting(PredefinedCityModelPackage package, bool loadPackage, bool includeTexture, LODRange lodRange, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo)
        {
            Package = package;
            LoadPackage = loadPackage;
            IncludeTexture = includeTexture;
            LODRange = lodRange;
            MeshGranularity = meshGranularity;
            DoSetMeshCollider = doSetMeshCollider;
            DoSetAttrInfo = doSetAttrInfo;
        }

        public PackageLoadSetting(PackageLoadSetting src)
        {
            Package = src.Package;
            LoadPackage = src.LoadPackage;
            IncludeTexture = src.IncludeTexture;
            LODRange = src.LODRange;
            MeshGranularity = src.MeshGranularity;
            DoSetMeshCollider = src.DoSetMeshCollider;
            DoSetAttrInfo = src.DoSetAttrInfo;
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
                extent: extent,
                attachMapTile: true,
                mapTileZoomLevel: 15); // TODO ここで定数で決め打っている部分は、ユーザーが選択できるようにすると良い
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
    /// </summary>
    internal class ReliefLoadSetting : PackageLoadSetting
    {
        public bool AttachMapTile { get; set; }
        public ReliefLoadSetting(bool attachMapTile, PackageLoadSetting baseSetting) :
            base(baseSetting)
        {
            AttachMapTile = attachMapTile;
        }

        public override MeshExtractOptions ConvertToNativeOption(PlateauVector3d referencePoint, int coordinateZoneID, Extent extent)
        {
            var nativeOption = base.ConvertToNativeOption(referencePoint, coordinateZoneID, extent);
            nativeOption.AttachMapTile = AttachMapTile;
            return nativeOption;
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
        }
    }

}
