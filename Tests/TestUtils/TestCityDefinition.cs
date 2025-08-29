using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.CityImport.Import;
using PLATEAU.Dataset;
#if UNITY_EDITOR
using PLATEAU.Editor.Window.ProgressDisplay;
#endif
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Network;
using PLATEAU.Util;

namespace PLATEAU.Tests.TestUtils
{
    /// <summary>
    /// ユニットテストにおいて、都市データに期待する状態を定義します。
    /// どのようなGMLファイルがあることが期待され、どの地域メッシュコードがあるかを記述します。
    /// </summary>
    internal class TestCityDefinition
    {
        public string SrcRootDirPathLocal => Path.GetFullPath(Path.Combine(testDataDir, this.rootDirName));
        public GridCodeList AreaGridCodes { get; }
        public int CoordinateZoneId { get; set; }
        public TestGmlDefinition[] GmlDefinitions { get; set; }

        private string rootDirName;

        private static readonly string testDataDir = Path.Combine(PathUtil.SdkBasePath, "./Tests/TestData/日本語パステスト");

        public TestCityDefinition(string rootDirName, TestGmlDefinition[] gmlDefs, GridCodeList areaGridCodes, int coordinateZoneId)
        {
            this.rootDirName = rootDirName;
            GmlDefinitions = gmlDefs;
            AreaGridCodes = areaGridCodes;
            CoordinateZoneId = coordinateZoneId;
        }

        /// <summary>
        /// <see cref="TestCityDefinition"/> に記述されたパスをもとに、ローカルモードでインポートします。
        /// </summary>
        public Task ImportLocal()
        {
            
            #if UNITY_EDITOR
            var progressDisplay = new ProgressDisplayGUI(null);
            #else
            IProgressDisplay progressDisplay = null;
            #endif
            
            var conf = MakeConfig(false);
            var task = CityImporter.ImportAsync(conf, progressDisplay, new System.Threading.CancellationTokenSource().Token );
            return task;
        }

        /// <summary>
        /// <see cref="TestCityDefinition"/> に記述されたデータセットID をもとに、サーバーモードでインポートします。
        /// </summary>
        public Task ImportServer()
        {
            #if UNITY_EDITOR
            var progressDisplay = new ProgressDisplayGUI(null);
            #else
            IProgressDisplay progressDisplay = null;
            #endif
            var task = CityImporter.ImportAsync(MakeConfig(true), progressDisplay, new System.Threading.CancellationTokenSource().Token);
            return task;
        }

        /// <summary>
        /// インポートするための設定を返します。
        /// </summary>
        private CityImportConfig MakeConfig(bool isServer)
        {
            // TODO どのパッケージと何が対応するかは要テスト
            var allPackages =
                EnumUtil.EachFlags(PredefinedCityModelPackageExtension.All());
            var allPackageLods = new PackageToLodDict();
            foreach (var package in allPackages)
            {
                allPackageLods.MergePackage(package, 3);
            }
            
            IDatasetSourceConfig datasetSourceConfig =
                isServer
                    ? new DatasetSourceConfigRemote(this.rootDirName, NetworkConfig.MockServerUrl, "")
                    : new DatasetSourceConfigLocal(SrcRootDirPathLocal);
            
            var dummyAreaSelectResult = new AreaSelectResult(new ConfigBeforeAreaSelect(datasetSourceConfig, CoordinateZoneId), AreaGridCodes, AreaSelectResult.ResultReason.Confirm);
            var conf = CityImportConfig.CreateWithAreaSelectResult(dummyAreaSelectResult);
            
            
            // メッシュコードがあるあたりに基準点を設定します。 Extent.Allの中心を基準点にすると極端な座標になるため。  
            using var geoRef = GeoReference.Create(new PlateauVector3d(0, 0, 0), 1.0f, CoordinateSystem.EUN,
                conf.ConfBeforeAreaSelect.CoordinateZoneID);
            using var grid = AreaGridCodes.At(0); 
            conf.ReferencePoint = geoRef.Project(grid.Extent.Center);
            
            foreach (var packageConf in conf.PackageImportConfigDict.ForEachPackagePair)
            {
                packageConf.Value.IncludeTexture = true;
            }
            return conf;
        }

        /// <summary>
        /// <see cref="TestCityDefinition"/> に記述されたファイルについて、
        /// GMLファイルとその関連ファイルが指定フォルダにコピーされることを確認します。
        /// </summary>
        public void AssertFilesExist(string testDataFetchPath)
        {
            var relativePaths = GmlDefinitions
                .Select(def => def.GmlPath)
                .ToArray();
            foreach(string relativePath in relativePaths)
            {
                string path = Path.GetFullPath(Path.Combine(testDataFetchPath, relativePath));
                Assert.IsTrue(File.Exists(path), $"次のパスにファイルが存在する : {path}");
            }
        }

        /// <summary>
        /// <see cref="TestCityDefinition"/> に記述されたゲームオブジェクト名をリストで返します。
        /// </summary>
        public List<string> ExpectedObjNames =>
            GmlDefinitions
                .Where(def => def.ContainsMesh)
                .Select(def => def.GameObjName)
                .ToList();
        

        /// <summary>
        /// ユニットテストにおいて、GMLファイルに期待する状態を定義します。
        /// どのパスにGMLファイルがあることが期待されるか、メッシュは有するか、期待するテクスチャのパスは何か
        /// を定義します。
        /// </summary>
        public class TestGmlDefinition
        {
            public string GmlPath { get; set; }
            public string GameObjName { get; set; }
            public bool ContainsMesh { get; set; }
            public string[] TexturePaths { get; set; }
            public int MaxLod { get; set; }

            public TestGmlDefinition(string gmlPath, string gameObjName, bool containsMesh, string[] texturePaths, int maxLod)
            {
                GmlPath = gmlPath;
                GameObjName = gameObjName;
                ContainsMesh = containsMesh;
                TexturePaths = texturePaths;
                MaxLod = maxLod;
            }
        }

        /// <summary>
        /// テストデータ "Simple" について、
        /// その内容を <see cref="TestCityDefinition"/> 形式で説明したものです。
        /// </summary>
        public static readonly TestCityDefinition Simple =
            new TestCityDefinition(
                "TestDataSimpleGml",
                new[]
                {
                    new TestGmlDefinition("udx/bldg/53392642_bldg_6697_op2.gml", "53392642_bldg_6697_op2.gml", true,
                        null,
                        2)
                }, GridCodeList.CreateFromGridCodesStr(new string[]
                {
                    "53392642"
                }),
                9);

        /// <summary>
        /// テストデータ "MiniTokyo" について、
        /// その内容を <see cref="TestCityDefinition"/> 形式で説明したものです。 
        /// </summary>
        public static readonly TestCityDefinition MiniTokyo =
            new TestCityDefinition("TestDataTokyoMini", new[]
            {
                new TestGmlDefinition("udx/bldg/53392546_bldg_6697_2_op.gml", "53392546_bldg_6697_2_op.gml", true, null, 1),
                new TestGmlDefinition("udx/bldg/53392547_bldg_6697_2_op.gml", "53392547_bldg_6697_2_op.gml", true, null, 1),
                new TestGmlDefinition("udx/brid/53394525_brid_6697_op.gml", "53394525_brid_6697_op.gml", true,
                    new[] { "udx/brid/53394525_brid_6697_appearance/skjp6776.jpg" }, 2),
                new TestGmlDefinition("udx/dem/533925_dem_6697_op.gml", "533925_dem_6697_op.gml", true, null, 1),
                new TestGmlDefinition("udx/frn/53394525_frn_6697_sjkms_op.gml", "53394525_frn_6697_sjkms_op.gml", true,
                    new[] { "udx/frn/53394525_frn_6697_sjkms_appearance/17992.jpg" }, 2),
                new TestGmlDefinition("udx/lsld/533925_lsld_6668_op.gml", "533925_lsld_6668_op.gml", false, null, 0),
                new TestGmlDefinition("udx/luse/533925_luse_6668_2_op.gml", "533925_luse_6668_2_op.gml", true, null, 1),
                new TestGmlDefinition("udx/luse/533925_luse_6697_park_op.gml", "533925_luse_6697_park_op.gml", true, null, 1),
                new TestGmlDefinition("udx/tran/533925_tran_6697_op.gml", "533925_tran_6697_op.gml", true,  null, 1),
                new TestGmlDefinition("udx/urf/533925_urf_6668_boka_op.gml", "533925_urf_6668_boka_op.gml", false, null, 0),
                new TestGmlDefinition("udx/urf/533925_urf_6668_kodo_op.gml", "533925_urf_6668_kodo_op.gml", false, null, 0),
                new TestGmlDefinition("udx/urf/533925_urf_6668_yoto_op.gml", "533925_urf_6668_yoto_op.gml", false, null, 0),
                new TestGmlDefinition("udx/fld/natl/tamagawa_tamagawa-asakawa-etc/53392547_fld_6697_l1_op.gml", "natl/tamagawa_tamagawa-asakawa-etc/53392547_fld_6697_l1_op.gml", true, null, 1),
                new TestGmlDefinition("udx/fld/natl/tamagawa_tamagawa-asakawa-etc/53392547_fld_6697_l2_op.gml", "natl/tamagawa_tamagawa-asakawa-etc/53392547_fld_6697_l2_op.gml", true, null, 1),
            }, GridCodeList.CreateFromGridCodesStr(new string[]
            {
                "53394525", "53392546", "53392547", "533925"
            }), 9);

        /// <summary>
        /// 杉戸町のテストデータの説明
        /// </summary>
        public static readonly TestCityDefinition SugitoMachi =
            new TestCityDefinition("TestDataSugitoMachi", 
                new TestGmlDefinition[]{
                    new TestGmlDefinition("udx/bldg/54390565_bldg_6697_op.gml", "54390565_bldg_6697_op.gml", true,
                        new string[]{
                            "udx/bldg/54390566_bldg_6697_appearance/2f7028a7-51de-447c-827d-d1812ca9995a.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/5e01e573-ad1b-4211-8880-74d9d7dc25a5.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/7e45faf2-6586-4f1d-b74e-78bc424fb74c.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/43b0a174-92a8-4915-b44c-48cdd6091079.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/77de944c-c5fd-4876-a99e-936a198a5579.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/744e9aa5-ac58-40ed-8de9-3f30152cf285.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/985fb2b6-8fd9-4fde-97fe-3dcceffc7e8a.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/32700fb8-8855-4ed8-bbf9-6e255300c926.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/76591d5c-abcf-4dbe-aca0-6b8f6358be68.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/15425414-2694-45ac-b91e-1dc8f7f2732a.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/a5421cd6-2138-45e1-baa2-43ccbce94702.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/c7b7e60b-8988-4ec2-94e4-e2c935cd494c.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/c8362a4b-47e8-4798-a4c8-f565ddfb7de8.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/cd72fe36-70c2-4256-971a-0876e90f73d1.jpg",
                            "udx/bldg/54390566_bldg_6697_appearance/fade1799-6c78-4b74-859d-7774b6a34fa6.jpg"
                            }, 2),
                    new TestGmlDefinition("udx/bldg/54390566_bldg_6697_op.gml", "54390565_bldg_6697_op.gml", true, null, 1)
                },
                GridCodeList.CreateFromGridCodesStr(new string[]{"54390565", "54390566"})
                , 9);

        /// <summary>
        /// テストデータ "TestServer23Ku" について、
        /// その内容を <see cref="TestCityDefinition"/> 形式で説明したものです。
        /// </summary>
        public static readonly TestCityDefinition TestServer23Ku =
            new TestCityDefinition("23ku", new[]
                {
                    new TestGmlDefinition("udx/bldg/53392642_bldg_6697_2_op.gml", "53392642_bldg_6697_2_op.gml", true,
                        null,
                        2),
                    new TestGmlDefinition("udx/bldg/53392670_bldg_6697_2_op.gml", "53392670_bldg_6697_2_op.gml", true, null, 2)
                }, GridCodeList.CreateFromGridCodesStr(new string[]
                    { "53392642", "53392670" }
            ), 9);
    }
}
