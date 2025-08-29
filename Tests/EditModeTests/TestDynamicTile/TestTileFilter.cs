using NUnit.Framework;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Native;
using PLATEAU.Texture;
using UnityEngine.TestTools;
using UnityEngine;
using PLATEAU.CityGML;
using System.Net;


namespace PLATEAU.Tests.EditModeTests
{

    [TestFixture]
    [Category("DynamicTile"), Category("TileFilter")]
    public class TestTileFilter
    {
        [TestCase("tile_zoom_10_grid_53394507_bldg_6697_op", PredefinedCityModelPackage.Building)]
        [TestCase("tile_zoom_10_grid_53394517_tran_6697_op", PredefinedCityModelPackage.Road)]
        [TestCase("tile_zoom_11_grid_53394517_veg_6697_op", PredefinedCityModelPackage.Vegetation)]
        [TestCase("tile_zoom_9_grid_53393660_brid_6697_op", PredefinedCityModelPackage.Bridge)]
        [TestCase("tile_zoom_10_grid_08EE754_tran_6697_op", PredefinedCityModelPackage.Road)] // Grid Code
        [TestCase("tile_zoom_10_grid_53394507_aaaa_6697_op", PredefinedCityModelPackage.Unknown)]
        public void GetPackage_ReturnsExpected(string address, PredefinedCityModelPackage expected)
            => AssertPackage(address, expected);

        [TestCase("tile_zoom_9_grid_53393660_brid_6697_op", PredefinedCityModelPackage.Building)]
        [TestCase("tile_zoom_10_grid_53394517_tran_6697_op", PredefinedCityModelPackage.Railway)]
        public void GetPackage_DoesNotReturnUnexpected(string address, PredefinedCityModelPackage notExpected)
            => AssertPackageNotEqual(address, notExpected);

        [TestCase("wrong_format")]
        [TestCase("12345678")]
        [TestCase("zoom_10_grid_53394517_tran_6697_op")]
        [TestCase("tile_10_grid_53394517_tran_6697_op")]
        [TestCase("tile_zoom_10_53394517_tran_6697_op")]
        public void GetPackage_ReturnsLogError(string address)
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex($"アドレスからパッケージ名が取得できませんでした。Address: {address}"));
            AssertPackage(address, PredefinedCityModelPackage.None);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase("")]
        [TestCase(null)]
        public void GetPackage_ReturnsNoneForNullOrEmpty(string address)
        {
            AssertPackage(address, PredefinedCityModelPackage.None);
            LogAssert.NoUnexpectedReceived();
        }

        private void AssertPackage(string address, PredefinedCityModelPackage expected)
        {
            var pkg = PLATEAUDynamicTileFilter.GetPackage(address);
            Assert.AreEqual(expected, pkg, $"Address: {address}");
        }

        private void AssertPackageNotEqual(string address, PredefinedCityModelPackage expected)
        {
            var pkg = PLATEAUDynamicTileFilter.GetPackage(address);
            Assert.AreNotEqual(expected, pkg);  
        }

    }
}
