using PLATEAU.CityMeta;
using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.Tests.TestUtils
{
    /// <summary>
    /// PLATEAUのインポート処理のテストを手助けするユーティリティクラスです。
    /// テスト用にインポート時の設定を提供します。
    /// </summary>
    internal static class ImportUtil
    {
        /// <summary>
        /// PLATEAUインポート設定でなるべくシンプルなものを作って返します。
        /// </summary>
        public static CityImporterConfig MinimumConfig(string udxFullPathBeforeImport, string outputDirAssetsPath)
        {
            var conf = new CityImporterConfig
            {
                exportAppearance = false,
                meshGranularity = MeshGranularity.PerCityModelArea,
                UdxPathBeforeImport = udxFullPathBeforeImport,
                importDestPath =
                {
                    dirAssetPath = outputDirAssetsPath
                }
            };
            // タイプ別 3Dモデル変換設定
            var convertTypeConf = conf.objConvertTypesConfig;
            convertTypeConf.SetLodRangeToOnlyMin();
            convertTypeConf.SetExportLowerLodForAllTypes(false);
            // タイプ別 シーン配置設定
            var placeTypeConf = conf.scenePlacementConfig;
            placeTypeConf.SetPlaceMethodForAllTypes(ScenePlacementConfig.PlaceMethod.DoNotPlace);
            placeTypeConf.SetSelectedLodForAllTypes(0);
            
            return conf;
        }
    }
}