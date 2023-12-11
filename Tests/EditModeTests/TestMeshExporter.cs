using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PLATEAU.CityExport;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts;
using PLATEAU.Geometries;
using PLATEAU.MeshWriter;
using PLATEAU.Tests.TestUtils;
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
            // TestCityImporter.DeleteFetchedTestDir();
            LogAssert.ignoreFailingMessages = true;
            DirectoryUtil.SetUpTempCacheFolder();
            yield return TestCityDefinition.MiniTokyo.ImportLocal().AsIEnumerator();
            LogAssert.ignoreFailingMessages = false;
        }

        [TearDown]
        public void TearDown()
        {
            // TestCityImporter.DeleteFetchedTestDir();
            // DirectoryUtil.DeleteTempCacheFolder();
        }

        [Test]
        public void ExportMiniTokyoToObjFiles()
        {
            string destDirPath = Path.Combine(DirectoryUtil.TempCacheFolderPath);
            var instancedCityModel = Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            var options = new MeshExportOptions(
                MeshExportOptions.MeshTransformType.Local,
                true, true, MeshFileFormat.OBJ, CoordinateSystem.ENU,
                new ExportConfigGuiObj().GetExporter());
            UnityModelExporter.Export(destDirPath, instancedCityModel, options);

            var expectedObjFiles = TestCityDefinition.MiniTokyo.GmlDefinitions
                .Where(def => def.ContainsMesh)
                .Select(def => Path.GetFileNameWithoutExtension(def.GmlPath) + $"_LOD{def.MaxLod}.obj")
                .ToList();
            var expectedMtlFiles = expectedObjFiles
                .Select(path => path.Replace(".obj", ".mtl"));
            
            AssertFilesExist(destDirPath, expectedObjFiles);
            AssertFilesExist(destDirPath, expectedMtlFiles);
            AssertObjFilesHaveMesh(expectedObjFiles, destDirPath);
        }

        [Test]
        public void ExportMiniTokyoToFbxFiles()
        {
            string destDirPath = Path.Combine(DirectoryUtil.TempCacheFolderPath);
            var instancedCityModel = Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            var options = new MeshExportOptions(
                MeshExportOptions.MeshTransformType.Local,
                true, false, MeshFileFormat.FBX, CoordinateSystem.ENU,
                new ExportConfigGuiFbx().GetExporter());
            UnityModelExporter.Export(destDirPath, instancedCityModel, options);
            var expectedFbxFiles = TestCityDefinition.MiniTokyo.GmlDefinitions
                .Where(def => def.ContainsMesh)
                .Select(def => Path.GetFileNameWithoutExtension(def.GmlPath) + ".fbx")
                .ToList();
            
            AssertFilesExist(destDirPath, expectedFbxFiles);
        }
        
        [Test]
        public void ExportMiniTokyoToGltfFiles()
        {
            string destDirPath = Path.Combine(DirectoryUtil.TempCacheFolderPath);
            var instancedCityModel = Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            var gltfExportGui = new ExportConfigGuiGltf();
            ((CityExporterGltf)gltfExportGui.GetExporter()).GltfFileFormat = GltfFileFormat.GLTF;
            var options = new MeshExportOptions(
                MeshExportOptions.MeshTransformType.Local,
                true, false, MeshFileFormat.GLTF, CoordinateSystem.ENU,
                gltfExportGui.GetExporter());
            UnityModelExporter.Export(destDirPath, instancedCityModel, options);
            var expectedGltfFiles = TestCityDefinition.MiniTokyo.GmlDefinitions
                .Where(def => def.ContainsMesh)
                .Select(def =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(def.GmlPath);
                    return  $"{fileName}/{fileName}"+ ".gltf";
                })
                .ToList();
            var expectedBinFiles = expectedGltfFiles.Select(path => path.Replace(".gltf", ".bin"));
            
            AssertFilesExist(destDirPath, expectedGltfFiles);
            AssertFilesExist(destDirPath, expectedBinFiles);
        }

        private static void AssertFilesExist(string basePath, IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(Path.Combine(basePath, file));
                var fileInfo = new FileInfo(fullPath);
                Assert.IsTrue(fileInfo.Exists, $"{fileInfo.Name} が存在する");
                Assert.Greater(fileInfo.Length, 0, $"{fileInfo.Name} のサイズが0より大きい");
            }
        }

        private static void AssertObjFilesHaveMesh(IEnumerable<string> objFiles, string basePath)
        {
            var objFilePaths = objFiles
                .Select(file => Path.GetFullPath(Path.Combine(basePath, file)));
            foreach (string objFilePath in objFilePaths)
            {
                string fileContent = File.ReadAllText(objFilePath);
                string objFileName = Path.GetFileName(objFilePath);
            
                // objファイルの中身を見て、メッシュを構成するのに必要な要素が存在するかを確認します。
                // objフォーマットの文法に則った正規表現で検索します。
                AssertStringMatchRegexes(fileContent,new []
                {
                    // 正規表現の実装上の注意:
                    // Windowsの場合、改行は \n ではなく \r\n になります。
                    // そのため、行末でマッチさせるには $ ではなく \r?$ と表記する必要があります。
                    
                    ( // 文法 : matlib (materialファイル名).mtl　でマテリアル定義ファイルを参照
                        @"^mtllib .+\.mtl\s*\r?$",
                        $"{objFileName} にマテリアル定義ファイルへの参照が含まれる"),
                    ( // 文法 : g (group名)　でグループを定義
                        @"^g .+\s*\r?$",
                        $"{objFileName} にグループが含まれる"),
                    ( // 文法 : v (x座標) (y座標) (z座標) で頂点を定義
                        @"^v [0-9.\-]+ [0-9.\-]+ [0-9.\-]+\s*\r?$",
                        $"{objFileName} に頂点が含まれる"),
                    ( // 文法 : vt (x座標) (y座標) でUV座標を定義
                        @"^vt [0-9.\-]+ [0-9.\-]+\s*\r?$",
                        $"{objFileName} にUVが含まれる"),
                    ( // 文法 : f (頂点インデックス)/(UVインデックス) (頂点インデックス)/(UVインデックス) (頂点インデックス)/(UVインデックス) で面を定義
                        @"^f [0-9\/]+ [0-9\/]+ [0-9\/]+\s*\r?$",
                        $"{objFileName} に面が含まれる")
                });
            }
           
        }
        
        private static void AssertStringMatchRegexes(string str, IEnumerable<(string regex, string message)> regexAndMessages)
        {
            foreach (var (regex, message) in regexAndMessages)
            {
                Assert.IsTrue(Regex.IsMatch(str, regex, RegexOptions.Multiline), message);
            }
        }
    }
}
