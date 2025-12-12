using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTile処理で必要なデータを保持するコンテキストクラス
    /// </summary>
    public class DynamicTileProcessingContext
    {
        /// <summary>
        /// プレハブ一時保存パス
        /// </summary>
        public const string PrefabsTempSavePath = "Assets/PLATEAUPrefabs";

        /// <summary>
        /// Addressableグループのベース名
        /// </summary>
        public const string AddressableGroupBaseName = "PLATEAUCityObjectGroup";

        /// <summary>
        /// Addressableグループ名
        /// </summary>
        public string AddressableGroupName { get; }

        /// <summary>
        /// メタデータストア
        /// </summary>
        public PLATEAUDynamicTileMetaStore MetaStore { get; set; }

        /// <summary>
        /// アセット変換設定
        /// </summary>
        public ConvertToAssetConfig AssetConfig { get; }

        /// <summary>
        /// DynamicTileインポート設定
        /// </summary>
        public DynamicTileImportConfig Config { get; set; }

        /// <summary>
        /// ビルドフォルダパス（リモートビルド用）
        /// </summary>
        public string BuildFolderPath { get; set; }

        /// <summary>
        /// Assets外のパスかどうか
        /// </summary>
        public bool IsExcludeAssetFolder { get; }

        /// <summary>
        /// <see cref="TileRebuilder"/>によるリビルドの時のみ利用します。
        /// 差分ビルド対象とするタイルのアドレス群です。。null または空なら全件対象とします。
        /// </summary>
        public HashSet<string> TargetAddresses { get; set; }

        /// <summary>
        /// GML数
        /// </summary>
        public int GmlCount { get; set; }

        /// <summary>
        /// 読み込み完了したGML数
        /// </summary>
        private int loadedGmlCount;

        /// <summary>
        /// 出力するunitypackageのパスです。（Assets外への出力のみ）
        /// </summary>
        public string UnityPackagePath
        {
            get
            {
                return Path.Combine(BuildFolderPath, UnityPackageFileName);
            }
        }

        /// <summary>
        /// 出力するunitypackageのファイル名です。（Assets外への出力のみ）
        /// </summary>
        public string UnityPackageFileName
        {
            get
            {
                return $"{AddressableGroupName}_Prefabs.unitypackage";
            }
        }

        /// <summary>
        /// 読み込み完了したGML数をインクリメントして返す
        /// </summary>
        public int IncrementAndGetLoadedGmlCount()
        {
            return System.Threading.Interlocked.Increment(ref loadedGmlCount);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config">DynamicTileインポート設定</param>
        public DynamicTileProcessingContext(DynamicTileImportConfig config)
        {
            if (config == null) throw new System.ArgumentNullException(nameof(config));
            var outputPath = config.OutputPath;
            if (string.IsNullOrEmpty(outputPath))
            {
                Debug.LogError("output path is not set.");
                outputPath = "";
            }
            
            Config = config;

            AssetConfig = ConvertToAssetConfig.DefaultValue;
            IsExcludeAssetFolder = !string.IsNullOrEmpty(outputPath) && !IsAssetPath(outputPath);

            // Assetsフォルダー外のパスを指定している場合は、プレハブ一時保存パスを使用
            AssetConfig.AssetPath = IsExcludeAssetFolder ? PrefabsTempSavePath : outputPath;
            BuildFolderPath = outputPath;

            // AddressableGroupNameを生成
            AddressableGroupName = GenerateAddressableGroupName();

            // MetaStoreを生成
            MetaStore = ScriptableObject.CreateInstance<PLATEAUDynamicTileMetaStore>();
        }

        /// <summary>
        /// Addressableグループ名を生成します
        /// </summary>
        private string GenerateAddressableGroupName()
        {
            var groupName = AddressableGroupBaseName;

            var directoryName = Path.GetFileName(
                BuildFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var sanitizedDirectoryName = System.Text.RegularExpressions.Regex.Replace(directoryName, @"[^\w\-_]", "_");
            groupName += "_" + sanitizedDirectoryName;
            return groupName;
        }

        /// <summary>
        /// PLATEAUTileManagerからContextを生成します
        /// </summary>
        public static DynamicTileProcessingContext CreateFrom(PLATEAUTileManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            var importConf = new DynamicTileImportConfig(ImportType.DynamicTile, manager.OutputPath, true);
            return new DynamicTileProcessingContext(importConf);
        }

        /// <summary>
        /// パスがAssetsフォルダー内かどうかを判定します
        /// </summary>
        private static bool IsAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            try
            {
                // Assetsフォルダー内で、かつ相対パスに「..」が含まれないことを確認
                var assetPath = AssetPathUtil.GetAssetPath(path);
                return assetPath.StartsWith("Assets") && !assetPath.Contains("..");
            }
            catch
            {
                // 変換に失敗した場合はプロジェクト外のパスと判定
                return false;
            }
        }

        /// <summary>
        /// コンテキストが有効かどうかを判定
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AddressableGroupName) &&
                   AssetConfig != null &&
                   !string.IsNullOrEmpty(AssetConfig.AssetPath);
        }

        public string AddressName
        {
            get
            {
                string shorterGroupName = AddressableGroupName.Replace(AddressableGroupBaseName + "_", "");
                string addressName = $"{DynamicTileExporter.AddressableAddressBase}_{shorterGroupName}";
                return addressName;
            }
        }

        public string DataPath
        {
            get
            {
                string normalizedAssetPath = AssetPathUtil.NormalizeAssetPath(AssetConfig.AssetPath);
                string dataPath = Path.Combine(normalizedAssetPath, AddressName + ".asset").Replace('\\', '/');
                return dataPath;
            }
        }

        public AddressablesUtility.TileBuildMode BuildMode
        {
            get
            {
                return TileCatalogSearcher.FindCatalogFiles(BuildFolderPath, true).Length == 0
                    ? AddressablesUtility.TileBuildMode.New
                    : AddressablesUtility.TileBuildMode.Add;
            }
        }
    }
}