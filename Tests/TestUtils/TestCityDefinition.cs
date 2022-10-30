using System.IO;
using System.Threading.Tasks;
using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Interop;
using PLATEAU.Udx;

namespace PLATEAU.Tests.TestUtils
{
    /// <summary>
    /// ユニットテストにおいて、都市データに期待する状態を定義します。
    /// 何というGMLファイルがあることが期待され、どの地域メッシュコードがあるかを記述します。
    /// </summary>
    internal class TestCityDefinition
    {
        public string SrcRootDirPath => Path.GetFullPath(Path.Combine(testDataDir, this.rootDirName));
        public string[] AreaMeshCodes { get; set; }
        public TestGmlDefinition[] GmlDefinitions { get; set; }

        private string rootDirName;

        private const string testDataDir = "Packages/com.synesthesias.plateau-unity-sdk/Tests/TestData";

        public TestCityDefinition(string rootDirName, TestGmlDefinition[] gmlDefs, string[] areaMeshCodes)
        {
            this.rootDirName = rootDirName;
            GmlDefinitions = gmlDefs;
            AreaMeshCodes = areaMeshCodes;
        }

        public Task Import(out CityLoadConfig outConfig)
        {
            var progressDisplay = new ProgressDisplayGUI();
            outConfig = new CityLoadConfig();
            // TODO どのパッケージと何が対応するかは要テスト
            uint packageFlagsAll = 0b10000000000000000000000011111111;
            outConfig.InitWithPackageFlags((PredefinedCityModelPackage)packageFlagsAll);
            outConfig.Extent = new Extent(new GeoCoordinate(-90, -180, -9999), new GeoCoordinate(90, 180, 9999));
            outConfig.AreaMeshCodes = AreaMeshCodes;
            foreach (var packageConf in outConfig.ForEachPackagePair)
            {
                packageConf.Value.includeTexture = true;
            }

            outConfig.SourcePathBeforeImport = SrcRootDirPath;

            var task = CityImporter.ImportAsync(outConfig, progressDisplay);
            return task;
        }

        /// <summary>
        /// ユニットテストにおいて、GMLファイルに期待する状態を定義します。
        /// どのパスにGMLファイルがあることが期待されるか、メッシュは有するか、期待するテクスチャのパスは何か
        /// を定義します。
        /// </summary>
        public class TestGmlDefinition
        {
            public string GmlPath { get; set; }
            public bool ContainsMesh { get; set; }
            public string[] TexturePaths { get; set; }

            public TestGmlDefinition(string gmlPath, bool containsMesh, string[] texturePaths)
            {
                GmlPath = gmlPath;
                ContainsMesh = containsMesh;
                TexturePaths = texturePaths;
            }
        }


        public static TestCityDefinition MiniTokyo =
            new TestCityDefinition("TestDataTokyoMini", new[]
            {
                new TestGmlDefinition("udx/bldg/53392546_bldg_6697_2_op.gml", true, null),
                new TestGmlDefinition("udx/bldg/53392547_bldg_6697_2_op.gml", true, null),
                new TestGmlDefinition("udx/brid/53394525_brid_6697_op.gml", true,
                    new[] { "udx/brid/53394525_brid_6697_appearance/skjp6776.jpg" }),
                new TestGmlDefinition("udx/dem/533925_dem_6697_op.gml", true, null),
                new TestGmlDefinition("udx/frn/53394525_frn_6697_sjkms_op.gml", true,
                    new[] { "udx/frn/53394525_frn_6697_sjkms_appearance/17992.jpg" }),
                new TestGmlDefinition("udx/lsld/533925_lsld_6668_op.gml", false, null),
                new TestGmlDefinition("udx/luse/533925_luse_6668_2_op.gml", true, null),
                new TestGmlDefinition("udx/luse/533925_luse_6697_park_op.gml", true, null),
                new TestGmlDefinition("udx/tran/533925_tran_6697_op.gml", true,  null),
                new TestGmlDefinition("udx/urf/533925_urf_6668_boka_op.gml", false, null),
                new TestGmlDefinition("udx/urf/533925_urf_6668_kodo_op.gml", false, null),
                new TestGmlDefinition("udx/urf/533925_urf_6668_yoto_op.gml", false, null)
            }, new []
            {
                "53394525", "53392546", "53392547", "533925"
            });
    }
}
