using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.Util;
using System;
using System.IO;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTile処理で必要なデータを保持するコンテキストクラス
    /// </summary>
    public class DynamicTileProcessingContext : IDisposable
    {
        /// <summary>
        /// プレハブ保存パス
        /// </summary>
        private const string PrefabsSavePath = "Assets/PLATEAUPrefabs";
        
        /// <summary>
        /// Addressableグループのベース名
        /// </summary>
        private const string AddressableGroupBaseName = "PLATEAUCityObjectGroup";
        
        /// <summary>
        /// Addressableグループ名
        /// </summary>
        public string AddressableGroupName { get; set; }
        
        /// <summary>
        /// メタデータストア
        /// </summary>
        public PLATEAUDynamicTileMetaStore MetaStore { get; set; }
        
        /// <summary>
        /// アセット変換設定
        /// </summary>
        public ConvertToAssetConfig AssetConfig { get; set; }
        
        /// <summary>
        /// DynamicTileインポート設定
        /// </summary>
        public DynamicTileImportConfig Config { get; set; }
        
        /// <summary>
        /// ビルドフォルダパス（リモートビルド用）
        /// </summary>
        public string BuildFolderPath { get; set; }
        
        /// <summary>
        /// プログレスバー
        /// </summary>
        public ProgressBar ProgressBar { get; set; }
        
        /// <summary>
        /// Assets外のパスかどうか
        /// </summary>
        public bool IsExcludeAssetFolder { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config">DynamicTileインポート設定</param>
        public DynamicTileProcessingContext(DynamicTileImportConfig config)
        {
            Config = config;
            if (config != null)
            {
                AssetConfig = ConvertToAssetConfig.DefaultValue;
                AssetConfig.AssetPath = PrefabsSavePath;
                BuildFolderPath = config.OutputPath;
                IsExcludeAssetFolder = !string.IsNullOrEmpty(config.OutputPath) && !IsAssetPath(config.OutputPath);
                
                // AddressableGroupNameを生成
                AddressableGroupName = GenerateAddressableGroupName();
                
                // MetaStoreを生成
                MetaStore = ScriptableObject.CreateInstance<PLATEAUDynamicTileMetaStore>();
                
                // ProgressBarを生成
                ProgressBar = new ProgressBar();
            }
        }
        
        /// <summary>
        /// Addressableグループ名を生成します
        /// </summary>
        private string GenerateAddressableGroupName()
        {
            var groupName = AddressableGroupBaseName;
            if (IsExcludeAssetFolder)
            {
                // Assets外のパスを指定する場合はグループを分ける
                var directoryName = Path.GetFileName(
                    BuildFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                groupName += "_" + directoryName;
            }
            return groupName;
        }
        
        /// <summary>
        /// パスがAssetsフォルダー内かどうかを判定します
        /// </summary>
        private static bool IsAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;
            
            var normalizedPath = path.Replace('\\', '/');
            return normalizedPath.StartsWith("Assets/");
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
        
        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            // マネージドリソースの解放
            ProgressBar?.Dispose();
            ProgressBar = null;
            
            // MetaStoreの解放（ScriptableObjectなのでDestroyが必要）
            if (MetaStore != null)
            {
                UnityEngine.Object.DestroyImmediate(MetaStore);
                MetaStore = null;
            }
            
            // その他のリソースをnullに設定
            AssetConfig = null;
            Config = null;
            AddressableGroupName = null;
            BuildFolderPath = null;
        }
    }
}