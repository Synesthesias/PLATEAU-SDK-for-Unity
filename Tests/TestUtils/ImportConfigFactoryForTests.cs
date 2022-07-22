using System.Collections.Generic;
using PLATEAU.CityMeta;
using PLATEAU.CommonDataStructure;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;

namespace PLATEAU.Tests.TestUtils
{
    /// <summary>
    /// PLATEAUのインポート処理のテストを手助けするユーティリティクラスです。
    /// テスト用にインポート時の設定を提供します。
    /// </summary>
    internal static class ImportConfigFactoryForTests
    {
        /// <summary>
        /// PLATEAUインポート設定で標準的なものを作って返します。
        /// </summary>
        public static CityImportConfig StandardConfig(string srcFullPathBeforeImport, string outputDirAssetsPath)
        {
            var conf = new CityImportConfig
            {
                exportAppearance = false,
                meshGranularity = MeshGranularity.PerCityModelArea,
                SrcRootPathBeforeImport = srcFullPathBeforeImport,
                importDestPath =
                {
                    DirAssetsPath = outputDirAssetsPath
                }
            };
            
            // タイプ別 3Dモデル変換設定
            var convertTypeConf = conf.objConvertTypesConfig;
            convertTypeConf.SetLodRangeToAllRange();
            convertTypeConf.SetExportLowerLodForAllTypes(true);
            // タイプ別 シーン配置設定
            var placeTypeConf = conf.cityMeshPlacerConfig;
            placeTypeConf.SetPlaceMethodForAllTypes(CityMeshPlacerConfig.PlaceMethod.DoNotPlace);
            placeTypeConf.SetSelectedLodForAllTypes(0);

            GmlSearcherModel gmlSearcher = new GmlSearcherModel(srcFullPathBeforeImport);
            conf.gmlSearcherConfig.GenerateAreaTree(gmlSearcher.AreaIds, false);
            return conf;
        }
        
        /// <summary>
        /// objファイル変換において、辞書によってLOD範囲を指定します。
        /// </summary>
        public static void SetConvertLods(this CityImportConfig conf, Dictionary<GmlType, MinMax<int>> argDict)
        {
            var typeConf = conf.objConvertTypesConfig;
            foreach (var type in argDict.Keys)
            {
                var argLodRange = argDict[type];
                typeConf.SetLodRangeForType(type, argLodRange);
            }
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