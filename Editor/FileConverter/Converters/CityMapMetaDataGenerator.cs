using System;
using System.IO;
using System.Linq;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    /// <summary>
    /// <see cref="CityMapMetaData"/> を生成します。
    /// 古い版である <see cref="GmlToCityMapInfoConverter"/> の置き換えとなるクラスです。
    /// </summary>
    public class CityMapMetaDataGenerator
    {
        public CityMapMetaData LastConvertedCityMapMetaData { get; set; }
        public CityMapMetaDataGeneratorConfig Config { get; set; }

        public CityMapMetaDataGenerator()
        {
            Config = new CityMapMetaDataGeneratorConfig();
        }

        public bool Generate(string meshAssetPath, string dstMetaDataAssetPath, string gmlFileName, bool doClearOldMapInfo)
        {
            try
            {
                // ロードします。
                var metaData = LoadOrCreateMetaData(dstMetaDataAssetPath, doClearOldMapInfo);
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
                metaData.cityModelImportConfig = Config.CityModelImportConfig;

                // ファイルに保存します。
                LastConvertedCityMapMetaData = metaData;
                EditorUtility.SetDirty(metaData);
                AssetDatabase.SaveAssets();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating {nameof(CityMapMetaData)}.\n{e}");
                return false;
            }
        }
        
        /// <summary>
        /// 指定パスの <see cref="CityMapMetaData"/> をロードします。
        /// ファイルが存在しない場合、新しく作成します。
        /// ファイルが存在する場合、<paramref name="doClearOldMapInfo"/> が false ならばそれをロードします。
        /// true ならば中身のデータを消してからロードします。
        /// </summary>
        private static CityMapMetaData LoadOrCreateMetaData(string dstAssetPath, bool doClearOldMapInfo)
        {
            bool doFileExists = File.Exists(dstAssetPath);
            if (!doFileExists)
            {
                var instance = ScriptableObject.CreateInstance<CityMapMetaData>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            var loadedMetaData = AssetDatabase.LoadAssetAtPath<CityMapMetaData>(dstAssetPath);
            if (doClearOldMapInfo)
            {
                loadedMetaData.ClearData();
            }
            return loadedMetaData;
        }
    }   
}