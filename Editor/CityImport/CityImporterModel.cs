using PLATEAU.Editor.Converters;
using PLATEAU.CityMeta;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// 都市モデルをインポートします。
    /// ここでいうインポートとは、次の複数の処理をまとめたものを指します:
    /// ・Plateau元データを StreamingAssets/PLATEAU 内にコピーします。GUI設定で選んだ一部のみをコピーすることもできます。
    ///   テクスチャなど関連するフォルダがあればそれもコピーします。
    /// ・Plateau元データから、複数のobj および 1つの <see cref="CityMetadata"/> を作成します。
    /// ・変換したモデルを現在のシーンに配置します。
    /// </summary>
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
         
            // データを StreamingAssets にコピーします。
            CopyPlateauSrcFiles.ImportCopy(importConfig, gmlRelativePaths);
            
            
            // メタデータを作成またはロードします。
            var importDest = importConfig.importDestPath;
            metadata = CityMetadataGenerator.LoadOrCreateMetadata(importDest.MetadataAssetPath, true);
            

            metadata.gmlRelativePaths = gmlRelativePaths;
            
            // gmlファイル群をインポートします。
            int successCount = GmlImporter.ImportGmls(out var generatedObjs, out int failureCount, gmlRelativePaths, importConfig, metadata);
            
            importConfig.generatedObjFiles = generatedObjs;
            
            // シーンに配置します。
            importConfig.rootDirName = PlateauSourcePath.RootDirName(importConfig.sourcePath.RootDirAssetPath);
            CityMeshPlacerModel.Place(metadata.cityImportConfig.cityMeshPlacerConfig, metadata);
            
            // 後処理
            SaveAssets(metadata);
            PrintResult(successCount, failureCount, gmlRelativePaths.Length);
            EditorUtility.ClearProgressBar();
            return successCount;
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