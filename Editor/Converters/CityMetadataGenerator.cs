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
    /// <see cref="CityMetadata"/> を生成します。
    /// </summary>
    internal class CityMetadataGenerator
    {

        /// <summary>
        /// <see cref="CityMetadata"/> を生成します。
        /// gml元データのインポート時に利用されることを想定しています。
        ///
        /// <paramref name="metadata"/> が null のとき、既存のメタデータをファイルからロード、なければ新規作成した上でそのメタデータの情報を書き換えます。
        /// <paramref name="metadata"/> が null でないとき、そのメモリ上のメタデータに対して情報を書き換えます。
        /// <paramref name="doSaveFile"/> が false のとき、ファイルには保存せずメモリ内でのみ変更が保持されます。
        /// これを false にする動機は、シリアライズするタイミングを手動でコントロールすることでシリアライズ回数を減らし、高速化することです。
        /// </summary>
        /// <returns>成功したかどうかをboolで返します。</returns>
        public bool Generate(
            CityMapMetadataGeneratorConfig config, string meshAssetPath, string gmlFileName,
            CityMetadata metadata = null, bool doSaveFile = true)
        {
            try
            {
                // ロードします。
                var importConf = config.CityImportConfig;
                var importDestPath = importConf.importDestPath;
                string srcRootAssetsPath = importConf.sourcePath.RootDirAssetPath;
                if (metadata == null)
                {
                    metadata = LoadOrCreateMetadata(importDestPath.MetadataAssetPath, config.DoClearIdToGmlTable);
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
                    if (metadata.DoGmlTableContainsKey(id)) continue;
                    metadata.AddToGmlTable(id, gmlFileName);
                }

                // 変換時の設定を書き込みます。
                importConf.sourcePath.RootDirAssetPath = srcRootAssetsPath;

                importConf.importDestPath.DirAssetsPath = importDestPath.DirAssetsPath;
                metadata.cityImportConfig = importConf;
                
                // ファイルに保存します。
                if (doSaveFile)
                {
                    EditorUtility.SetDirty(metadata);
                    AssetDatabase.SaveAssets();
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating {nameof(CityMetadata)}.\n{e}");
                return false;
            }
        }
        
        /// <summary>
        /// 指定パスの <see cref="CityMetadata"/> をロードします。
        /// ファイルが存在しない場合、新しく作成します。
        /// ファイルが存在する場合、それをロードします。
        /// ロード時、<paramref name="doClearIdToGmlTable"/> がtrueなら idToGmlTable を消去します。
        /// </summary>
        public static CityMetadata LoadOrCreateMetadata(string dstAssetPath, bool doClearIdToGmlTable)
        {
            bool doFileExists = File.Exists(PathUtil.AssetsPathToFullPath(dstAssetPath));
            if (!doFileExists)
            {
                var instance = ScriptableObject.CreateInstance<CityMetadata>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            var loadedMetadata = AssetDatabase.LoadAssetAtPath<CityMetadata>(dstAssetPath);
            if (doClearIdToGmlTable)
            {
                loadedMetadata.ClearIdToGmlTable();
            }
            return loadedMetadata;
        }
    }   
}