using System.IO;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using static PLATEAU.Tests.EditModeTests.Placer.TestPlacerUtil;

namespace PLATEAU.Tests.EditModeTests.Placer
{
    [TestFixture]
    public class TestCityMeshPlacerModelV2ForPrimary
    {
        private string prevDefaultDstPath;
        private static readonly string testDefaultCopyDestPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");
        private CityMetadata metaData;
        
        // インポートには時間がかかるので、 OneTimeSetUp 内でインポートしたものを使い回します。
        // 別のインポート設定でテストしたい場合は、別のソースファイルを作ってください。

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DirectoryUtil.SetUpTempAssetFolder();
            
            // テスト用に一時的に デフォルト出力先を変更します。
            this.prevDefaultDstPath = PlateauUnityPath.StreamingGmlFolder;
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(testDefaultCopyDestPath);
            
            // インポートします。
            var initialPlaceConf = new CityMeshPlacerConfig().SetPlaceMethodForAllTypes(CityMeshPlacerConfig.PlaceMethod.DoNotPlace);
            this.metaData = ImportSimple(initialPlaceConf, MeshGranularity.PerPrimaryFeatureObject);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(this.prevDefaultDstPath);
        }
        
        // TODO ここにテストを書く
    }
}