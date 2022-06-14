using System;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{
    public class GmlToCityMapInfoConverter : IFileConverter
    {
        private GmlToCityMapInfoConverterConfig config;
        public CityMapInfo LastConvertedCityMapInfo { get; set; }

        public GmlToCityMapInfoConverter()
        {
            this.config = new GmlToCityMapInfoConverterConfig();
        }

        /// <summary>
        /// gmlファイルをロードして CityMapInfo に書き込みます。
        /// </summary>
        public bool Convert(string srcGmlPath, string dstTableFullPath)
        {
            return ConvertInner(srcGmlPath, dstTableFullPath, null);
        }

        /// <summary>
        /// ロードを省略し、ロード済みの cityModel をもとに変換します。
        /// </summary>
        public bool ConvertWithoutLoad(CityModel cityModel, string srcGmlPath, string dstTableFullPath)
        {
            if (cityModel == null)
            {
                Debug.LogError("cityModel is null.");
                return false;
            }

            return ConvertInner(srcGmlPath, dstTableFullPath, cityModel);
        }

        /// <summary>
        /// <see cref="CityMapInfo"/> に変換します。
        /// 引数の <paramref name="cityModel"/> が null の場合、<see cref="CityModel"/> を新たにロードして変換します。
        /// <paramref name="cityModel"/> が nullでない場合、ロードを省略してそのモデルを変換します。
        /// </summary>
        public bool ConvertInner(string srcGmlPath, string dstTableFullPath, CityModel cityModel)
        {
            if (!IsPathValid(srcGmlPath, dstTableFullPath)) return false;

            try
            {
                cityModel ??= CityGml.Load(srcGmlPath, this.config.ParserParams, DllLogCallback.UnityLogCallbacks);
                
                // IdToGmlTable を作成します。
                var cityObjectIds = cityModel.RootCityObjects
                    .SelectMany(co => co.CityObjectDescendantsDFS)
                    .Select(co => co.ID);
                var mapInfo = LoadOrCreateCityMapInfo(dstTableFullPath, this.config.DoClearOldMapInfo);
                string gmlFileName = Path.GetFileNameWithoutExtension(srcGmlPath);
                foreach (string id in cityObjectIds)
                {
                    if (mapInfo.DoGmlTableContainsKey(id)) continue;
                    mapInfo.AddToGmlTable(id, gmlFileName);
                }
                
                // 追加情報を書き込みます。
                mapInfo.ReferencePoint = this.config.ReferencePoint;
                mapInfo.MeshGranularity = this.config.MeshGranularity;
                LastConvertedCityMapInfo = mapInfo;
                EditorUtility.SetDirty(mapInfo);
                AssetDatabase.SaveAssets();
                return true;
            }catch (FileLoadException e)
            {
                Debug.LogError($"Failed to load gml file.\n gml path = {srcGmlPath}\n{e}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating {nameof(CityMapInfo)}.\ngml path = {srcGmlPath}\n{e}");
                return false;
            }
        }

        private static bool IsPathValid(string srcGmlPath, string dstTableFullPath)
        {
            if (!FilePathValidator.IsValidInputFilePath(srcGmlPath, "gml", false))
            {
                return false;
            }
            if(!FilePathValidator.IsValidOutputFilePath(dstTableFullPath, "asset"))
            {
                return false;
            }
            // TODO outputPathがAssetsフォルダ内かどうかチェックする
            return true;
        }

        /// <summary>
        /// 指定パスの <see cref="CityMapInfo"/> をロードします。
        /// ファイルが存在しない場合、新しく作成します。
        /// ファイルが存在する場合、<paramref name="doClearOldMapInfo"/> が false ならばそれをロードします。
        /// true ならば中身のデータを消してからロードします。
        /// </summary>
        private static CityMapInfo LoadOrCreateCityMapInfo(string dstMapInfoFullPath, bool doClearOldMapInfo)
        {
            
            
            string dstAssetPath = FilePathValidator.FullPathToAssetsPath(dstMapInfoFullPath);
            bool doFileExists = File.Exists(dstMapInfoFullPath);
            if (!doFileExists)
            {
                var instance = ScriptableObject.CreateInstance<CityMapInfo>();
                AssetDatabase.CreateAsset(instance, dstAssetPath);
                AssetDatabase.SaveAssets();
            }
            var loadedMapInfo = AssetDatabase.LoadAssetAtPath<CityMapInfo>(dstAssetPath);
            if (doClearOldMapInfo)
            {
                loadedMapInfo.ClearData();
            }
            return loadedMapInfo;
        }
        

        public GmlToCityMapInfoConverterConfig Config
        {
            set => this.config = value;
            get => this.config;
        }
    }
}