using System;
using System.IO;
using System.Linq;
using PLATEAU.CityMeta;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Editor.Converters
{
    /// <summary>
    /// <see cref="CityMetaData"/> を生成します。
    /// </summary>
    internal class CityMetaDataGenerator
    {
        public CityMetaData LastConvertedCityMetaData { get; private set; }
        public const string MetaDataFileName = "CityMapMetaData.asset";

        /// <summary>
        /// <see cref="CityMetaData"/> を生成します。
        /// gml元データのインポート時に利用されることを想定しています。
        ///
        /// <paramref name="metaData"/> が null のとき、既存のメタデータをファイルからロード、なければ新規作成した上でそのメタデータの情報を書き換えます。
        /// <paramref name="metaData"/> が null でないとき、そのメモリ上のメタデータに対して情報を書き換えます。
        /// <paramref name="doSaveFile"/> が false のとき、ファイルには保存せずメモリ内でのみ変更が保持されます。
        /// これを false にする動機は、シリアライズするタイミングを手動でコントロールすることでシリアライズ回数を減らし、高速化することです。
        /// </summary>
        public bool Generate(
            CityMapMetaDataGeneratorConfig config, string meshAssetPath, string gmlFileName,
            CityMetaData metaData = null, bool doSaveFile = true)
        {
            try
            {
                // ロードします。
                string exportFolderFullPath = config.CityImporterConfig.exportFolderPath;
                string udxFullPath = config.CityImporterConfig.sourceUdxFolderPath;
                string dstMetaDataAssetPath =
                    Path.Combine(PathUtil.FullPathToAssetsPath(exportFolderFullPath), MetaDataFileName);
                if (metaData == null)
                {
                    metaData = LoadOrCreateMetaData(dstMetaDataAssetPath, config.DoClearIdToGmlTable);
                }
                var assets = AssetDatabase.LoadAllAssetsAtPath(meshAssetPath);
                if (assets == null || assets.Length <= 0)
                {
                    Debug.LogWarning($"Failed to load mesh asset.\nmeshAssetPath = {meshAssetPath}");
                    // TODO ここがtrueなのは仮。ちゃんとLODごとにできたら false にして失敗通知すべき。
                    return true;
                }
                
                // 各メッシュの名前とgmlファイル名を紐付けます。
                var meshes = assets.OfType<Mesh>().ToArray();
                foreach (var mesh in meshes)
                {
                    string id = mesh.name;
                    if (metaData.DoGmlTableContainsKey(id)) continue;
                    metaData.AddToGmlTable(id, gmlFileName);
                }

                // 変換時の設定を書き込みます。
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
                
                LastConvertedCityMetaData = metaData;
                // ファイルに保存します。
                if (doSaveFile)
                {
                    EditorUtility.SetDirty(metaData);
                    AssetDatabase.SaveAssets();
                }

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
        public static CityMetaData LoadOrCreateMetaData(string dstFullPath, bool doClearIdToGmlTable)
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
                loadedMetaData.ClearIdToGmlTable();
            }
            return loadedMetaData;
        }
    }   
}