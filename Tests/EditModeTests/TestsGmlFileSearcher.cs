using NUnit.Framework;
using PlateauUnitySDK.Editor.CityImport;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests
{
    [TestFixture]
    public class TestsGmlFileSearcher
    {
        [Test]
        public void AreaIds_Returns_File_AreaIds()
        {
            var searcher = new GmlSearcher(DirectoryUtil.TestTokyoUdxPath);
            string[] areaIds = searcher.AreaIds;
            Assert.Contains("53394525", areaIds);
            Assert.Contains("533925", areaIds);
            Debug.Log(searcher);
        }
    }
}