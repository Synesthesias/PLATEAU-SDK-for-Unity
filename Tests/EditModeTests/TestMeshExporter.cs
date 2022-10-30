using System.Collections;
using System.IO;
using NUnit.Framework;
using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.MeshWriter;
using PLATEAU.Tests.TestUtils;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestMeshExporter
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TestCityImporter.DeleteFetchedTestDir();
            DirectoryUtil.SetUpTempCacheFolder();
            yield return TestCityDefinition.MiniTokyo.Import(out var config).AsIEnumerator();
        }

        [TearDown]
        public void TearDown()
        {
            TestCityImporter.DeleteFetchedTestDir();
            DirectoryUtil.DeleteTempCacheFolder();
        }

        [Test]
        public void ExportMiniTokyo()
        {
            Debug.Log("cachepath=" + Application.temporaryCachePath);
            string destDirPath = Path.Combine(DirectoryUtil.TempCacheFolderPath);
            var instancedCityModel = Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            var options = new MeshExportOptions(
                MeshExportOptions.MeshTransformType.Local,
                true, true, MeshFileFormat.OBJ, CoordinateSystem.ENU,
                new GltfWriteOptions(GltfFileFormat.GLB, destDirPath));
            MeshExporter.Export(destDirPath, instancedCityModel, options);
            // TODO
        }
    }
}
