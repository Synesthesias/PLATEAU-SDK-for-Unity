using System;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityImport.Config.PackageImportConfigs
{

    /// <summary>
    /// PLATEAUCityModelLoader の設定のうち、パッケージごとの設定です。
    /// <see cref="PackageImportConfigDict"/> によって保持されます。
    /// このクラスに対応するGUIクラスは PackageLoadConfigGUI です。
    /// 設定のうち、一括設定可能な部分は<see cref="PackageImportConfigExtendable"/>が担います。
    ///
    /// 実装上の注意：
    /// ・特定のパッケージ種で追加の設定項目がある場合はサブクラスで実装します。
    /// ・この設定項目を追加・削除する場合、<see cref="ConvertToNativeOption"/>も合わせて実装しないと反映されないことに注意してください。 
    /// </summary>
    public class PackageImportConfig
    {
        public PredefinedCityModelPackage Package { get; }
        public PackageImportConfigExtendable ConfExtendable { get; }
        public bool ImportPackage { get; set; }

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


        private PackageImportConfig(PredefinedCityModelPackage package, bool importPackage, bool includeTexture,
            LODRange lodRange, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo,
            Material fallbackMaterial, bool enableTexturePacking, TexturePackingResolution texturePackingResolution)
        {
            Package = package;
            ImportPackage = importPackage;
            ConfExtendable = new PackageImportConfigExtendable(
                includeTexture, meshGranularity, doSetMeshCollider,
                doSetAttrInfo, enableTexturePacking, texturePackingResolution
            );
            LODRange = lodRange;
            FallbackMaterial = fallbackMaterial;
        }

        /// <summary> コピーコンストラクタ </summary>
        protected PackageImportConfig(PackageImportConfig src) : this(
            src.Package, src.ImportPackage, src.IncludeTexture, src.LODRange,
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
        /// <paramref name="package"/> に対応した<see cref="PackageImportConfig"/> またはそのサブクラスを返します。
        /// 具体的には、
        /// 土地以外の場合は<see cref="PackageImportConfig"/>を返します。
        /// 土地の場合は追加の設定項目があるので、<see cref="PackageImportConfig"/>のサブクラスである<see cref="ReliefImportConfig"/>を返します。
        /// </summary>
        public static PackageImportConfig CreateConfigFor(PredefinedCityModelPackage package, int availableMaxLOD)
        {
            // 範囲選択の結果が引数に入っているので、それをもとに初期値を決めます。
            var predefined = CityModelPackageInfo.GetPredefined(package);
            int minLOD = predefined.minLOD;
            var lodRange = availableMaxLOD >= 0
                ? new LODRange(minLOD, Math.Max(availableMaxLOD, minLOD), Math.Max(availableMaxLOD, minLOD))
                : new LODRange(0, 0, 0);
            var val = new PackageImportConfig(
                package: package,
                importPackage: availableMaxLOD >= 0, // 存在しないものはロードしません
                includeTexture: predefined.hasAppearance,
                lodRange: lodRange,
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
                PredefinedCityModelPackage.Relief => new ReliefImportConfig(val),
                _ => new PackageImportConfig(val)
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
                mapTileURL: ReliefImportConfig.DefaultMapTileUrl); // 土地専用の設定
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