using System;
using System.Collections.Generic;
using System.IO;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.CityModelImportWindow;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// 複数のgmlファイルから、複数のobj および 1つの <see cref="CityMapInfo"/>テーブルを生成します。
    /// </summary>
    public class MultiGmlConverter
    {
        /// <summary> このインスタンスが最後に出力した <see cref="CityMapInfo"/> です。 </summary>
        public CityMapInfo LastConvertedCityMapInfo { get; set; }
        
        /// <summary>
        /// 複数のgmlファイルを変換します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。</param>
        /// <param name="baseFolderPath"><paramref name="gmlRelativePaths"/>の相対パスの基準となるパスです。</param>
        /// <param name="exportFolderFullPath">出力先のフォルダの絶対パスです。</param>
        /// <param name="config">変換設定です。</param>
        public void Convert(IEnumerable<string> gmlRelativePaths, string baseFolderPath, string exportFolderFullPath, CityModelImportConfig config)
        {
            int successCount = 0;
            int loopCount = 0;
            Vector3? referencePoint = null;
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;

                string gmlFullPath = Path.GetFullPath(Path.Combine(baseFolderPath, gmlRelativePath));

                // gmlをロードします。
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(exportFolderFullPath, gmlFileName + ".obj");
                if (!TryLoadCityGml(out var cityModel, gmlFullPath, config))
                {
                    continue;
                }

                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, config, gmlFullPath, objPath))
                {
                    continue;
                }

                // CityMapInfo を生成します。
                // TODO ファイル名は変更できるようにしたい
                string mapInfoPath = Path.Combine(exportFolderFullPath, "CityMapInfo.asset");
                if (!TryGenerateCityMapInfo(out var cityMapInfo, cityModel, gmlFullPath, mapInfoPath, referencePoint, config))
                {
                    continue;
                }
                LastConvertedCityMapInfo = cityMapInfo;

                successCount++;
            }
            AssetDatabase.ImportAsset(FilePathValidator.FullPathToAssetsPath(exportFolderFullPath));
            AssetDatabase.Refresh();
            int failureCount = loopCount - successCount;
            if (failureCount == 0)
            {
                Debug.Log($"Convert Success. {loopCount} gml files are converted.");
            }
            else
            {
                Debug.LogError($"Convert end with error. {failureCount} of {loopCount} gml files are not converted.");
            }
        }

        
        private static bool TryLoadCityGml(out CityModel cityModel, string gmlFullPath, CityModelImportConfig config)
        {
            try
            {
                // 設定の parserParams.tessellate は true にしないとポリゴンにならない部分があるので true で固定します。
                CitygmlParserParams parserParams = new CitygmlParserParams(config.OptimizeFlg);
                cityModel = CityGml.Load(gmlFullPath, parserParams, DllLogCallback.UnityLogCallbacks, config.LogLevel);
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading gml failed.\ngml path = {gmlFullPath}\n{e}");
                cityModel = null;
                return false;
                
            }

            return true;
        }

        private static bool TryConvertToObj(CityModel cityModel, ref Vector3? referencePoint, CityModelImportConfig config, string gmlFullPath, string objPath)
        {
            using (var objConverter = new GmlToObjFileConverter())
            {
                // configを作成します。
                var converterConf = new GmlToObjFileConverterConfig();
                converterConf.MeshGranularity = config.MeshGranularity;
                converterConf.LogLevel = config.LogLevel;
                converterConf.DoAutoSetReferencePoint = false;
                    
                // Reference Pointは最初のものに合わせます。
                if (referencePoint == null)
                {
                    referencePoint = objConverter.SetValidReferencePoint(cityModel);
                    converterConf.ManualReferencePoint = referencePoint;
                }
                else
                {
                    converterConf.ManualReferencePoint = referencePoint.Value;
                }

                objConverter.Config = converterConf;
                    
                bool isSuccess = objConverter.ConvertWithoutLoad(cityModel, gmlFullPath, objPath);

                return isSuccess;
            }
        }

        private static bool TryGenerateCityMapInfo(out CityMapInfo cityMapInfo, CityModel cityModel, string gmlFullPath,
            string mapInfoPath, Vector3? referencePoint, CityModelImportConfig importConf)
        {
            cityMapInfo = null;
            if (referencePoint == null)
            {
                Debug.LogError($"{nameof(referencePoint)} is null.");
                return false;
            }
            var converter = new GmlToCityMapInfoConverter();
            var infoConf = converter.Config;
            infoConf.ReferencePoint = referencePoint.Value;
            infoConf.MeshGranularity = importConf.MeshGranularity;
            infoConf.DoClearOldMapInfo = true;
            converter.Config = infoConf;
            bool isSucceed = converter.ConvertWithoutLoad(cityModel, gmlFullPath, mapInfoPath);
            if (!isSucceed)
            {
                return false;
            }
            cityMapInfo = converter.LastConvertedCityMapInfo;
            return true;
        }
    }
}