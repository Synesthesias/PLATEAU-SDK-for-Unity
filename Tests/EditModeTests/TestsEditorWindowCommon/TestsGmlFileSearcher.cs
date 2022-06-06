using NUnit.Framework;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEngine;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsEditorWindowCommon
{
    [TestFixture]
    public class TestsGmlFileSearcher
    {
        [Test]
        public void AreaIds_Returns_File_AreaIds()
        {
            var searcher = new GmlFileSearcher(DirectoryUtil.TestTokyoUdxPath);
            string[] areaIds = searcher.AreaIds;
            Assert.Contains("53394525", areaIds);
            Assert.Contains("533925", areaIds);
        }
    }
}