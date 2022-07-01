using System;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;

namespace PLATEAU.Tests.TestUtils
{
    internal static class TestImporter
    {
        public static int Import(ImportPathForTests pathSet, out CityMetaData metaData, Action<CityImportConfig> additionalConfigFunc)
        {
            var config = ImportConfigFactoryForTests.MinimumConfig(pathSet.SrcUdxFullPath, pathSet.OutputDirAssetsPath);
            additionalConfigFunc?.Invoke(config);
            int successCount = new CityImporter().Import(pathSet.GmlRelativePaths, config, out metaData);
            return successCount;
        }
    }
}