using System.IO;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using static PLATEAU.CityMeta.CityMeshPlacerConfig;
using static PLATEAU.Tests.EditModeTests.Placer.TestPlacerUtil;

namespace PLATEAU.Tests.EditModeTests.Placer
{
    /// <summary>
    /// <see cref="CityMeshPlacerModelV2"/> のテストの基底クラスです。
    /// 
    /// このクラスでは、各 <see cref="PlaceMethod"/> ごとに正しくシーンに配置できるかをテストします。
    /// このテストを 各 <see cref="MeshGranularity"/> ごとに行いたいので、
    /// <see cref="MeshGranularity"/> ごとにサブクラスを実装して確認しています。 
    /// </summary>
    [Ignore("これは基底クラスなので、実際のテストは派生クラスで行ってください。")]
    public abstract class TestCityMeshPlacerModelV2Base
    {
        private string prevDefaultDstPath;
        private static readonly string testDefaultCopyDestPath = Path.Combine(DirectoryUtil.TempAssetFolderPath, "PLATEAU");
        private CityMetadata metaData;
        
        /// <summary>
        /// このプロパティをサブクラスで実装することで、
        /// <see cref="MeshGranularity"/> を変えながらこのテストを使い回すことができます。
        /// </summary>
        protected abstract MeshGranularity MeshGranularity { get; }
        
        
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
            var initialPlaceConf = new CityMeshPlacerConfig().SetPlaceMethodForAllTypes(PlaceMethod.DoNotPlace);
            this.metaData = ImportSimple(initialPlaceConf, MeshGranularity);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DirectoryUtil.DeleteTempAssetFolder();
            PlateauUnityPath.TestOnly_SetStreamingGmlFolder(this.prevDefaultDstPath);
        }

        [TearDown]
        public void TearDown()
        {
            SceneUtil.DestroyAllGameObjectsInEditModeTestScene();
        }
        
        [Test]
        public void When_PlaceMethod_Is_DoNotPlace_Then_No_Model_Is_Placed()
        {
            Place(PlaceMethod.DoNotPlace, 1, this.metaData);
            AssertLodNotPlaced(0,1,2);
        }


        [Test]
        public void When_PlaceMethod_Is_PlaceAllLod_Then_All_Lods_Are_Placed()
        {
            Place(PlaceMethod.PlaceAllLod, -1, this.metaData);
            AssertLodPlaced(0,1,2);
        }

        [Test]
        public void When_PlaceMethod_Is_MaxLod_Then_Only_MaxLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceMaxLod, -1, this.metaData);
            AssertLodPlaced(2);
            AssertLodNotPlaced(0,1);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedLodOrMax_And_Lod_Not_Found_Then_MaxLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceSelectedLodOrMax, 999, this.metaData);
            AssertLodPlaced(2); // 存在するなかで最大
            AssertLodNotPlaced(0,1);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedLodOrMax_And_Lod_Found_Then_TargetLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceSelectedLodOrMax, 1, this.metaData);
            AssertLodPlaced(1);
            AssertLodNotPlaced(0,2);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedLodOrDoNotPlace_And_Lod_Not_Found_Then_Do_Not_Place()
        {
            Place(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 999, this.metaData);
            AssertLodNotPlaced(0,1,2);
        }

        [Test]
        public void When_PlaceMethod_Is_PlaceSelectedOrDoNotPlace_And_Lod_Found_Then_TargetLod_Is_Placed()
        {
            Place(PlaceMethod.PlaceSelectedLodOrDoNotPlace, 1, this.metaData);
            AssertLodPlaced(1);
            AssertLodNotPlaced(0,2);
        }
    }
}