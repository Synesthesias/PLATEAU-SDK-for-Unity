using System;
using PLATEAU.CityMeta;
using PLATEAU.Editor.Converters;
using PLATEAU.Editor.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Editor.CityImportOLD
{

    /// <summary>
    /// 都市モデルをインポートします。
    /// ここでいうインポートとは、次の複数の処理をまとめたものを指します:
    /// ・Plateau元データを StreamingAssets/PLATEAU 内にコピーします。GUI設定で選んだ一部のみをコピーすることもできます。
    ///   テクスチャなど関連するフォルダがあればそれもコピーします。
    /// ・Plateau元データから、複数のobj および 1つの <see cref="CityMetadata"/> を作成します。
    /// ・変換したモデルを現在のシーンに配置します。
    /// </summary>
    [Obsolete]
    internal static class CityImporterModel
    {

        /// <summary>
        /// gmlデータ群をインポートします。
        /// コピーおよび変換されるのは <paramref name="gmlRelativePaths"/> のリストにある gmlファイルに関連するデータのみです。
        /// 変換に成功したgmlファイルの数を返します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。パスの起点は udx フォルダです。</param>
        /// <param name="importConfig">変換設定です。</param>
        /// <param name="metadata">インポートによって生成されたメタデータです。</param>
        public static int Import(string[] gmlRelativePaths, CityImportConfig importConfig, out CityMetadata metadata)
        {
            var timer = new TimeDiagnosticsTable(); // 時間計測

            // データを StreamingAssets にコピーします。
            timer.Start("ImportCopySrc", "all");
            CopyPlateauSrcFiles.ImportCopy(importConfig, gmlRelativePaths);

            // メタデータを作成またはロードします。
            timer.Start("LoadOrCreateMetadata", "all");
            metadata = CityMetadataGenerator.LoadOrCreateMetadata(importConfig.importDestPath.MetadataAssetPath, true);
            // TODO この metadata にどのように設定が書き込まれていくかが分かりにくく、そのあたりの処理でGmlImporterが無駄に複雑化している気がするので整理したい


            // gmlファイル群をインポートします。
            timer.Start("ImportGmls", "all");

            // このキャッシュの実装意図については CityMeshPlacerModel.Place() のコメントを参照してください。
            using (var gmlToCityModelCache = new GmlToCityModelDict())
            {

                GmlImporter.ImportGmls(out int successCount, out int failureCount, gmlRelativePaths, importConfig,
                    metadata, gmlToCityModelCache);


                // シーンに配置します。
                timer.Start("PlaceToScene", "all");
                CityMeshPlacerModel.Place(importConfig.cityMeshPlacerConfig, metadata, gmlToCityModelCache);

                // 後処理
                timer.Start("PostProcess", "all");
                SaveAssets(metadata);
                Debug.Log($"[インポート 処理時間ログ]\n{timer.SummaryByProcess()}");
                PrintResult(successCount, failureCount, gmlRelativePaths.Length);
                EditorUtility.ClearProgressBar();
                return successCount;
            }

        }

        private static void SaveAssets(CityMetadata metadata)
        {
            EditorUtility.SetDirty(metadata);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void PrintResult(int successCount, int failureCount, int numGml)
        {
            if (failureCount == 0)
            {
                Debug.Log($"Convert Success. {successCount} gml files are converted.");
            }
            else
            {
                Debug.LogError($"Convert end with error. {failureCount} of {numGml} gml files are not converted.");
            }
        }



    }
}