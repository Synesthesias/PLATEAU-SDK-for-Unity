using NUnit.Framework;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Native;
using PLATEAU.Texture;
using UnityEngine.TestTools;
using UnityEngine;
using PLATEAU.CityGML;


namespace PLATEAU.Tests.EditModeTests
{

    [TestFixture]
    public class TestTileFilter
    {
        [Test]
        public void TestGetPackage()
        {
            AssertPackage("tile_zoom_10_grid_53394507_bldg_6697_op", PredefinedCityModelPackage.Building);
            AssertPackage("tile_zoom_10_grid_53394517_tran_6697_op", PredefinedCityModelPackage.Road);
            AssertPackage("tile_zoom_11_grid_53394517_veg_6697_op", PredefinedCityModelPackage.Vegetation);
            AssertPackage("tile_zoom_9_grid_53393660_brid_6697_op", PredefinedCityModelPackage.Bridge);

            AssertPackage("tile_zoom_10_grid_08EE754_tran_6697_op", PredefinedCityModelPackage.Road); //　Grid Code

            AssertPackage("tile_zoom_10_grid_53394507_aaaa_6697_op", PredefinedCityModelPackage.Unknown);

            LogAssert.Expect(LogType.Error, "アドレスからパッケージ名が取得できませんでした。Address: wrong_format");
            AssertPackage("wrong_format", PredefinedCityModelPackage.None);
            LogAssert.NoUnexpectedReceived();

            // Fail
            AssertPackageFail("tile_zoom_9_grid_53393660_brid_6697_op", PredefinedCityModelPackage.Building);
            AssertPackageFail("tile_zoom_10_grid_53394517_tran_6697_op", PredefinedCityModelPackage.Railway);
        }

        private void AssertPackage(string address, PredefinedCityModelPackage expected)
        {
            var pkg = PLATEAUDynamicTileFilter.GetPackage(address);
            Assert.AreEqual(expected, pkg);
        }

        private void AssertPackageFail(string address, PredefinedCityModelPackage expected)
        {
            var pkg = PLATEAUDynamicTileFilter.GetPackage(address);
            Assert.AreNotEqual(expected, pkg);  
        }

    }
}
