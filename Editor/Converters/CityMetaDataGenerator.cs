using System;
using System.IO;
using System.Linq;
using PlateauUnitySDK.Runtime.CityMeta;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.Converters
{
    /// <summary>
    /// <see cref="CityMetaData"/> を生成します。
    /// 古い版である <see cref="GmlToCityMetaDataConverter"/> の置き換えとなるクラスです。
    /// </summary>
    internal class CityMetaDataGenerator
    {
        public CityMetaData LastConvertedCityMetaData { get; set; }

        public bool Generate(CityMapMetaDataGeneratorConfig config, string meshAssetPath, string gmlFileName)
        {
            try
            {
                // ロードします。
                string exportFolderFullPath = config.CityImporterConfig.exportFolderPath;
                string udxFullPath = config.CityImporterConfig.sourceUdxFolderPath;
                string dstMetaDataAssetPath =
                    Path.Combine(PathUtil.FullPathToAssetsPath(exportFolderFullPath), "CityMapMetaData.asset");
                var metaData = LoadOrCreateMetaData(dstMetaDataAssetPath, config.DoClearIdToGmlTable);
                var assets = AssetDatabase.LoadAllAssetsAtPath(meshAssetPath);
                
                // 各メッシュの名前とgmlファイル名を紐付けます。
                var meshes = assets.OfType<Mesh>();
                foreach (var mesh in meshes)
                {
                    string id = mesh.name;
                    if (metaData.DoGmlTableContainsKey(id)) continue;
                    metaData.AddToGmlTable(id, gmlFileName);    
                }
                
                // 追加情報を書き込みます。
                var importConf = config.CityImporterConfig;
                importConf.sourceUdxFolderPath = PathUtil.FullPathToAssetsPath(exportFolderFullPath);
                if (PathUtil.IsSubDirectoryOfAssets(udxFullPath))
                {
                    importConf.sourceUdxFolderPath = PathUtil.FullPathToAssetsPath(udxFullPath);
                }
                else
                {
                    importConf.sourceUdxFolderPath = udxFullPath;
                }
                
                importConf.exportFolderPath = PathUtil.FullPathToAssetsPath(exportFolderFullPath);
                metaData.cityImporterConfig = importConf;
                
                // ファイルに保存します。
                LastConvertedCityMetaData = metaData;
                EditorUtility.SetDirty(metaData);
                AssetDatabase.SaveAssets();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating {nameof(CityMetaData)}.\n{e}");
                return false;
            }
        }
        
        /// <summary>
        /// 指定パスの <see cref="CityMetaData"/> をロードします。
        /// ファイルが存在しない場合、新しく作成します。
        /// ファイルが存在する場合、それをロードします。
        /// ロード時、<paramref name="doClearIdToGmlTable"/> がtrueなら idToGmlTable を消去します。
        /// </summary>
        private static CityMetaData LoadOrCreateMetaData(string dstFullPath, bool doClearIdToGmlTable)
        {
            bool doFileExists = File.Exists(dstFullPath);
            string dstAssetPath = PathUtil.FullPathToAssetsPath(dstFullPath);
            if (!doFileExists)
            {
                var instance = ScriptableObject.CreateInstance<CityMetaData>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            var loadedMetaData = AssetDatabase.LoadAssetAtPath<CityMetaData>(dstAssetPath);
            if (doClearIdToGmlTable)
            {
                loadedMetaData.DoClearIdToGmlTable();
            }
            return loadedMetaData;
        }
    }   
}