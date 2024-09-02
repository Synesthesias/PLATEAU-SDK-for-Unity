using NUnit.Framework;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Texture;
using PLATEAU.Util;
using System.IO;

namespace PLATEAU.Tests.EditModeTests
{

    [TestFixture]
    public class TestHeightmapGenerator
    {
        public string ImageDirPath => Path.GetFullPath(Path.Combine(testDataDir, "日本語パステスト/TestDataImages"));
        private static readonly string testDataDir = Path.Combine(PathUtil.SdkBasePath, "./Tests/TestData");

        [Test]
        public void TestGenerateFromMesh()
        {
            SimpleMeshInfo(out var vertices, out var indices, out var uv1, out var uv4, out var subMeshes);
            var mesh = PolygonMesh.Mesh.Create(vertices, indices, uv1, uv4, subMeshes);

            int TextureWidth = 513;
            int TextureHeight = 513;
            PlateauVector2d margin = new PlateauVector2d(0, 0);

            HeightmapGenerator Gen = new HeightmapGenerator();
            Gen.GenerateFromMesh(mesh, TextureWidth, TextureHeight, margin, true, true, out var Min, out var Max, out var MinUV, out var MaxUV, out var outData);
            Assert.AreEqual(TextureWidth * TextureHeight, outData.Length);
        }


        [Test]
        public void TestReadWritePng()
        {
            Assert.IsTrue(Directory.Exists(ImageDirPath), "Directory Exists");
            Assert.IsTrue(File.Exists(Path.Combine(ImageDirPath, "test_heightmap.png")), "File Exists");
            //Read
            int TextureWidth = 513;
            int TextureHeight = 513;
            HeightmapGenerator.ReadPngFile(Path.Combine(ImageDirPath, "test_heightmap.png"), TextureWidth, TextureHeight, out var imageData);
            Assert.AreEqual(TextureWidth * TextureHeight, imageData.Length);
            //Write
            HeightmapGenerator.SavePngFile(Path.Combine(ImageDirPath, "test_heightmap_saved.png"), TextureWidth, TextureHeight, imageData);
        }

        [Test]
        public void TestReadWriteRaw()
        {
            Assert.IsTrue(Directory.Exists(ImageDirPath), "Directory Exists");
            //Read
            int TextureWidth = 513;
            int TextureHeight = 513;
            HeightmapGenerator.ReadRawFile(Path.Combine(ImageDirPath, "test_heightmap.raw"), TextureWidth, TextureHeight, out var imageData);
            Assert.AreEqual(TextureWidth * TextureHeight, imageData.Length);

            //Write
            HeightmapGenerator.SaveRawFile(Path.Combine(ImageDirPath, "test_heightmap_saved.raw"), TextureWidth, TextureHeight, imageData);
        }

        [Test]
        public void TestConvertTo2DFloatArray()
        {
            ushort[] data = new ushort[4] { 65535, 0, 0, 65535 };
            float[,] outData = HeightmapGenerator.ConvertTo2DFloatArray(data, 2, 2);
            Assert.AreEqual(0f, outData[0, 0]);
            Assert.AreEqual(1f, outData[0, 1]);
            Assert.AreEqual(1f, outData[1, 0]);
            Assert.AreEqual(0f, outData[1, 1]);
        }

        private static PolygonMesh.Mesh CreateSimpleMesh()
        {
            SimpleMeshInfo(out var vertices, out var indices, out var uv1, out var uv4, out var subMeshes);
            var mesh = PolygonMesh.Mesh.Create(vertices, indices, uv1, uv4, subMeshes);
            return mesh;
        }

        private static void SimpleMeshInfo(out PlateauVector3d[] vertices, out uint[] indices,
            out PlateauVector2f[] uv1, out PlateauVector2f[] uv4, out SubMesh[] subMeshes)
        {
            vertices = new[]
            {
                new PlateauVector3d(0, 0, 0),
                new PlateauVector3d(0, 100, 10),
                new PlateauVector3d(100, 100, 30),
                new PlateauVector3d(100, 0, 10),
            };
            indices = new uint[]
            {
                0, 1, 2, 3, 2, 0
            };
            uv1 = new[]
            {
                new PlateauVector2f(1.1f, 1.2f),
                new PlateauVector2f(2.1f, 2.2f),
                new PlateauVector2f(3.1f, 3.2f)
            };
            uv4 = new[]
            {
                new PlateauVector2f(0, 0),
                new PlateauVector2f(0, 0),
                new PlateauVector2f(0, 0),
                new PlateauVector2f(0, 0)
            };
            subMeshes = new[]
            {
                SubMesh.Create(0, 2, "testTexturePath.png")
            };
        }
    }
}
