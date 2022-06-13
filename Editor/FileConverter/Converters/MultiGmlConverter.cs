using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.CityModelImportWindow;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using UnityEngine.Analytics;
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
            int failureCount = 0;
            int loopCount = 0;
            Vector3? referencePoint = null;
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;

                string gmlFullPath = Path.GetFullPath(Path.Combine(baseFolderPath, gmlRelativePath));
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(exportFolderFullPath, gmlFileName + ".obj");
                // TODO ファイル名は変更できるようにしたい
                string mapInfoPath = Path.Combine(exportFolderFullPath, "CityMapInfo.asset");

                // gmlをロードします。
                if (!TryLoadCityGml(out var cityModel, gmlFullPath, config))
                {
                    failureCount++;
                    continue;
                }

                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, config, gmlFullPath, objPath))
                {
                    failureCount++;
                    continue;
                }
                
                
                // CityMapInfo を生成します。
                if (!TryGenerateCityMapInfo(cityModel, gmlFullPath, mapInfoPath, referencePoint))
                {
                    failureCount++;
                    continue;
                }
                
            }
            AssetDatabase.ImportAsset(FilePathValidator.FullPathToAssetsPath(exportFolderFullPath));
            AssetDatabase.Refresh();
            if (failureCount == 0)
            {
                Debug.Log($"Convert Success. {loopCount} gml files are converted.");
            }
            else
            {
                Debug.LogError($"Convert end with error. {failureCount} of {loopCount} gml files are not converted.");
            }
        }

        
        private bool TryLoadCityGml(out CityModel cityModel, string gmlFullPath, CityModelImportConfig config)
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

        private bool TryGenerateCityMapInfo(CityModel cityModel, string gmlFullPath, string mapInfoPath, Vector3? referencePoint)
        {
            if (referencePoint == null)
            {
                Debug.LogError($"{nameof(referencePoint)} is null.");
                return false;
            }
            var cityMapInfoConverter = new GmlToCityMapInfoConverter();
            var cityMapConf = cityMapInfoConverter.Config;
            cityMapConf.ReferencePoint = referencePoint.Value;
            bool isSucceed = cityMapInfoConverter.ConvertWithoutLoad(cityModel, gmlFullPath, mapInfoPath);
            if (!isSucceed)
            {
                return false;
            }
            LastConvertedCityMapInfo = cityMapInfoConverter.LastConvertedCityMapInfo;
            return true;
        }
    }
}