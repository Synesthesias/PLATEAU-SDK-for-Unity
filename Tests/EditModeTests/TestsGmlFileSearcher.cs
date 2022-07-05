using NUnit.Framework;
using PLATEAU.Editor.CityImport;
using PLATEAU.Tests.TestUtils;
using UnityEngine;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestsGmlFileSearcher
    {
        [Test]
        public void AreaIds_Returns_File_AreaIds()
        {
            var searcher = new GmlSearcher(DirectoryUtil.TestTokyoSrcPath);
            var areaIds = searcher.AreaIds;
            Assert.Contains(53394525, areaIds, "地域IDを検索して返す");
            Assert.Contains(533925, areaIds);
            Debug.Log(searcher);
        }
    }
}