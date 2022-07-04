using System;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;

namespace PLATEAU.Tests.TestUtils
{
    internal static class TestImporter
    {
        /// <summary>
        /// テスト用のPlateau元データをインポートします。
        /// </summary>
        /// <param name="pathSet">テストに使うデータに関して、入出力のパスセットを指定します。</param>
        /// <param name="metaData">インポートによって生成されたメタデータを出力します。</param>
        /// <param name="additionalConfigFunc">
        /// インポート設定は通常 MinimumConfig が利用されますが、この設定をベースに追加で
        /// 設定を書き換えたい場合は Action{CityImportConfig} によって設定を書き換える処理を渡します。 
        /// </param>
        /// <returns></returns>
        public static int Import(ImportPathForTests pathSet, out CityMetaData metaData, Action<CityImportConfig> additionalConfigFunc)
        {
            var config = ImportConfigFactoryForTests.MinimumConfig(pathSet.SrcRootFullPath, pathSet.OutputDirAssetsPath);
            additionalConfigFunc?.Invoke(config);
            int successCount = new CityImporter().Import(pathSet.GmlRelativePaths, config, out metaData);
            return successCount;
        }
    }
}