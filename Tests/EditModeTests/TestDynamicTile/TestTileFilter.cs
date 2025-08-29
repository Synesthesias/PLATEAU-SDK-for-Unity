using NUnit.Framework;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Native;
using PLATEAU.Texture;


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
        }

        private void AssertPackage(string address, PredefinedCityModelPackage expected)
        {
            var pkg = PLATEAUDynamicTileFilter.GetPackage(address);
            Assert.AreEqual(expected, pkg);
        }

    }
}
