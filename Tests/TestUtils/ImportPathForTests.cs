using PLATEAU.Util;

namespace PLATEAU.Tests.TestUtils
{
    /// <summary>
    /// テストでインポートの入出力に使うパスのセットです。
    /// </summary>
    public class ImportPathForTests
    {
        /// <summary>
        /// シンプルな建物を含む gmlファイル 1つだけを対象とするテストのパスのセットです。
        /// </summary>
        public static readonly ImportPathForTests Simple =
            new ImportPathForTests(
                DirectoryUtil.TestDataSimpleSrcPath,
                new[] { "bldg/53392642_bldg_6697_op2.gml" },
                PathUtil.FullPathToAssetsPath(DirectoryUtil.TempAssetFolderPath)
            );


        /// <summary>
        /// ミニ東京のうち、bldg と dem の2つのgmlファイルを対象とするテストのパスのセットです。
        /// </summary>
        public static readonly ImportPathForTests Tokyo2 =
            new ImportPathForTests(
                DirectoryUtil.TestTokyoMiniSrcPath,
                new[]
                {
                    "bldg/53394525_bldg_6697_2_op.gml",
                    "dem/533925_dem_6697_op.gml"
                },
                PathUtil.FullPathToAssetsPath(DirectoryUtil.TempAssetFolderPath)
            );


        public readonly string SrcRootFullPath;
        public readonly string[] GmlRelativePaths;
        public readonly string OutputDirAssetsPath;

        public ImportPathForTests(string srcRootFullPath, string[] gmlRelativePaths, string outputDirAssetsPath)
        {
            this.SrcRootFullPath = srcRootFullPath;
            this.GmlRelativePaths = gmlRelativePaths;
            this.OutputDirAssetsPath = outputDirAssetsPath;
        }
    }
}