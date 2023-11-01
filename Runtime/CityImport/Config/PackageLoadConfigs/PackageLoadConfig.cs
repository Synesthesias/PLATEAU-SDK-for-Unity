﻿using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityImport.Config.PackageLoadConfigs
{

    /// <summary>
    /// PLATEAUCityModelLoader の設定のうち、パッケージごとの設定です。
    /// <see cref="PackageLoadConfigDict"/> によって保持されます。
    /// このクラスに対応するGUIクラスは PackageLoadConfigGUI です。
    /// 設定のうち、一括設定可能な部分は<see cref="PackageLoadConfigExtendable"/>が担います。
    ///
    /// 実装上の注意：
    /// ・特定のパッケージ種で追加の設定項目がある場合はサブクラスで実装します。
    /// ・この設定項目を追加・削除する場合、<see cref="ConvertToNativeOption"/>も合わせて実装しないと反映されないことに注意してください。 
    /// </summary>
    internal class PackageLoadConfig
    {
        public PredefinedCityModelPackage Package { get; }
        public PackageLoadConfigExtendable ConfExtendable { get; }
        public bool LoadPackage { get; set; }

        public bool IncludeTexture
        {
            get => ConfExtendable.IncludeTexture;
            set => ConfExtendable.IncludeTexture = value;
        }

        public bool EnableTexturePacking
        {
            get => ConfExtendable.EnableTexturePacking;
            set => ConfExtendable.EnableTexturePacking = value;
        }

        public TexturePackingResolution TexturePackingResolution
        {
            get => ConfExtendable.TexturePackingResolution;
            set => ConfExtendable.TexturePackingResolution = value;
        }

        public bool DoSetMeshCollider
        {
            get => ConfExtendable.DoSetMeshCollider;
            set => ConfExtendable.DoSetMeshCollider = value;
        }

        public MeshGranularity MeshGranularity
        {
            get => ConfExtendable.MeshGranularity;
            set => ConfExtendable.MeshGranularity = value;
        }

        public bool DoSetAttrInfo
        {
            get => ConfExtendable.DoSetAttrInfo;
            set => ConfExtendable.DoSetAttrInfo = value;
        }


        public LODRange LODRange { get; set; }


        public Material FallbackMaterial { get; set; }


        private PackageLoadConfig(PredefinedCityModelPackage package, bool loadPackage, bool includeTexture,
            LODRange lodRange, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo,
            Material fallbackMaterial, bool enableTexturePacking, TexturePackingResolution texturePackingResolution)
        {
            Package = package;
            LoadPackage = loadPackage;
            ConfExtendable = new PackageLoadConfigExtendable(
                includeTexture, meshGranularity, doSetMeshCollider,
                doSetAttrInfo, enableTexturePacking, texturePackingResolution
            );
            LODRange = lodRange;
            FallbackMaterial = fallbackMaterial;
        }

        /// <summary> コピーコンストラクタ </summary>
        protected PackageLoadConfig(PackageLoadConfig src) : this(
            src.Package, src.LoadPackage, src.IncludeTexture, src.LODRange,
            src.MeshGranularity, src.DoSetMeshCollider,
            src.DoSetAttrInfo, src.FallbackMaterial,
            src.EnableTexturePacking, src.TexturePackingResolution
        )
        {
        }

        // インポート設定のうち、Unityでは必ずこうなるという定数部分です。
        public const CoordinateSystem MeshAxes = CoordinateSystem.EUN;
        public const float UnitScale = 1.0f;

        /// <summary>
        /// <paramref name="package"/> に対応した<see cref="PackageLoadConfig"/> またはそのサブクラスを返します。
        /// 具体的には、
        /// 土地以外の場合は<see cref="PackageLoadConfig"/>を返します。
        /// 土地の場合は追加の設定項目があるので、<see cref="PackageLoadConfig"/>のサブクラスである<see cref="ReliefLoadConfig"/>を返します。
        /// </summary>
        public static PackageLoadConfig CreateConfigFor(PredefinedCityModelPackage package, int availableMaxLOD)
        {
            // 範囲選択の結果が引数に入っているので、それをもとに初期値を決めます。
            var predefined = CityModelPackageInfo.GetPredefined(package);
            var val = new PackageLoadConfig(
                package: package,
                loadPackage: availableMaxLOD >= 0, // 存在しないものはロードしません
                includeTexture: predefined.hasAppearance,
                lodRange: new LODRange(predefined.minLOD, availableMaxLOD, availableMaxLOD),
                MeshGranularity.PerPrimaryFeatureObject,
                doSetMeshCollider: true,
                doSetAttrInfo: true,
                enableTexturePacking: true,
                texturePackingResolution: TexturePackingResolution.W4096H4096,
                fallbackMaterial: Util.FallbackMaterial.LoadByPackage(package));

            // パッケージ種に応じてクラスを分けます。
            // これと似たロジックが PackageLoadSettingGUI.PackageLoadSettingGUIList にあるので、変更時はそちらも合わせて変更をお願いします。
            return package switch
            {
                PredefinedCityModelPackage.Relief => new ReliefLoadConfig(val),
                _ => new PackageLoadConfig(val)
            };
        }

        /// <summary>
        /// インポート設定について、C++のstructに変換します。
        /// </summary>
        public virtual MeshExtractOptions ConvertToNativeOption(PlateauVector3d referencePoint, int coordinateZoneID)
        {
            return new MeshExtractOptions(
                referencePoint: referencePoint,
                meshAxes: MeshAxes,
                meshGranularity: ConfExtendable.MeshGranularity,
                minLOD: (uint)LODRange.MinLOD,
                maxLOD: (uint)LODRange.MaxLOD,
                exportAppearance: ConfExtendable.IncludeTexture,
                gridCountOfSide: 1,
                unitScale: UnitScale,
                coordinateZoneID: coordinateZoneID,
                excludeCityObjectOutsideExtent: ShouldExcludeCityObjectOutsideExtent(Package),
                excludePolygonsOutsideExtent: ShouldExcludePolygonsOutsideExtent(Package),
                enableTexturePacking: ConfExtendable.EnableTexturePacking,
                texturePackingResolution: (uint)ConfExtendable.TexturePackingResolution.ToPixelCount(),
                attachMapTile: false, // 土地専用の設定は ReliefLoadSetting で行うので、ここでは false に固定します。
                mapTileZoomLevel: 15, // 土地専用の設定は ReliefLoadSetting で行うので、ここでは仮の値にします。
                mapTileURL: ReliefLoadConfig.DefaultMapTileUrl); // 土地専用の設定
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
}