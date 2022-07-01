using System.Collections.Generic;
using PLATEAU.CityMeta;
using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.Tests.TestUtils
{
    /// <summary>
    /// PLATEAUのインポート処理のテストを手助けするユーティリティクラスです。
    /// テスト用にインポート時の設定を提供します。
    /// </summary>
    internal static class ImportConfigFactoryForTests
    {
        /// <summary>
        /// PLATEAUインポート設定でなるべくシンプルなものを作って返します。
        /// </summary>
        public static CityImportConfig MinimumConfig(string udxFullPathBeforeImport, string outputDirAssetsPath)
        {
            var conf = new CityImportConfig
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
            convertTypeConf.SetExportLowerLodForAllTypes(true);
            // タイプ別 シーン配置設定
            var placeTypeConf = conf.scenePlacementConfig;
            placeTypeConf.SetPlaceMethodForAllTypes(ScenePlacementConfig.PlaceMethod.DoNotPlace);
            placeTypeConf.SetSelectedLodForAllTypes(0);
            
            return conf;
        }
        
        /// <summary>
        /// objファイル変換において、辞書によってLOD範囲を指定します。
        /// </summary>
        public static CityImportConfig SetConvertLods(this CityImportConfig conf, Dictionary<GmlType, MinMax<int>> argDict)
        {
            var lodDict = conf.objConvertTypesConfig.TypeLodDict;
            foreach (var type in argDict.Keys)
            {
                var argLodRange = argDict[type];
                lodDict[type] = argLodRange;
            }

            return conf;
        }

        /// <summary>
        /// objファイル変換において、各タイプ一括で <paramref name="minLod"/>, <paramref name="maxLod"/> を指定します。
        /// </summary>
        public static CityImportConfig SetConvertLods(this CityImportConfig conf, int minLod, int maxLod)
        {
            conf.objConvertTypesConfig.SetLodRangeByFunc(_ => new MinMax<int>(minLod, maxLod));
            return conf;
        }
    }
}