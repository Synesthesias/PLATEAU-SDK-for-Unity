using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.Client.Common;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapMeta;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// 複数のgmlファイルから、複数のobj および 1つの <see cref="CityMapMetaData"/>テーブルを生成します。
    /// </summary>
    public class MultiGmlConverter
    {
        
        /// <summary> このインスタンスが最後に出力した <see cref="CityMapMetaData"/> です。 </summary>
        public CityMapMetaData LastConvertedCityMapMetaData { get; private set; }
        
        /// <summary>
        /// 複数のgmlファイルを変換します。
        /// 変換元が StreamingAssets の配下にない場合、StreamingAssets配下である <see cref="defaultImportDstPath"/> にコピーした上で
        /// 変換元をコピー先に変更します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。</param>
        /// <param name="config">変換設定です。</param>
        public void Convert(IEnumerable<string> gmlRelativePaths, CityModelImportConfig config)
        {
            CopySrcFolderToStreamingAssets(config);
            
            int successCount = 0;
            int loopCount = 0;
            Vector3? referencePoint = null;
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;

                string gmlFullPath = Path.GetFullPath(Path.Combine(config.sourceUdxFolderPath, gmlRelativePath));

                // gmlをロードします。
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(config.exportFolderPath, gmlFileName + ".obj");
                if (!TryLoadCityGml(out var cityModel, gmlFullPath, config))
                {
                    continue;
                }
                
                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, config, gmlFullPath, objPath))
                {
                    continue;
                }

                // CityMapMetaData を生成します。
                string objAssetPath = PathUtil.FullPathToAssetsPath(objPath);
                if (!referencePoint.HasValue) throw new Exception($"{nameof(referencePoint)} is null.");
                config.referencePoint = referencePoint.Value;
                if (!TryGenerateMetaData(out var cityMapInfo, gmlFileName, objAssetPath, loopCount==1, config))
                {
                    continue;
                }
                LastConvertedCityMapMetaData = cityMapInfo;

                successCount++;
            }
            
            AssetDatabase.ImportAsset(PathUtil.FullPathToAssetsPath(config.exportFolderPath));
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

        /// <summary>
        /// 変換元を StreamingAssets内のデフォルトパスにコピーします。
        /// 変換元がすでに StreamingAssets 内にある場合は何もしません。
        /// <paramref name="config"/> の変換元をコピー先のパスに設定し直します。
        /// </summary>
        private static void CopySrcFolderToStreamingAssets(CityModelImportConfig config)
        {
            string prevSrc = Path.GetFullPath(Path.Combine(config.sourceUdxFolderPath, "../"));
            string srcFolderName = Path.GetFileName(Path.GetDirectoryName(prevSrc));
            bool isInStreamingAssets =
                PathUtil.IsSubDirectory(prevSrc, Application.streamingAssetsPath);
            if (isInStreamingAssets) return;
            
            string nextSrc = PlateauPath.StreamingGmlFolder;
            PathUtil.CloneDirectory(prevSrc, nextSrc);
            config.sourceUdxFolderPath = Path.Combine(nextSrc, $"{srcFolderName}/udx");
        }


        private static bool TryLoadCityGml(out CityModel cityModel, string gmlFullPath, CityModelImportConfig config)
        {
            try
            {
                if (!File.Exists(gmlFullPath))
                {
                    throw new FileNotFoundException($"Gml file is not found.\ngmlPath = {gmlFullPath}");
                }

                // 設定の parserParams.tessellate は true にしないとポリゴンにならない部分があるので true で固定します。
                CitygmlParserParams parserParams = new CitygmlParserParams(config.optimizeFlag);
                cityModel = CityGml.Load(gmlFullPath, parserParams, DllLogCallback.UnityLogCallbacks, config.logLevel);
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError($"{e}");
                cityModel = null;
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading gml failed.\ngml path = {gmlFullPath}\n{e}");
                cityModel = null;
                return false;
                
            }

            return true;
        }

        private static bool TryConvertToObj(CityModel cityModel, ref Vector3? referencePoint, CityModelImportConfig importConfig, string gmlFullPath, string objPath)
        {
            using (var objConverter = new GmlToObjFileConverter())
            {
                // configを作成します。
                var converterConf = new GmlToObjFileConverterConfig();
                converterConf.MeshGranularity = importConfig.meshGranularity;
                converterConf.LogLevel = importConfig.logLevel;
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

        private static bool TryGenerateMetaData(out CityMapMetaData cityMapMetaData, string gmlFileName,
            string dstMeshAssetPath, bool isFirstFile, CityModelImportConfig importConf)
        {
            cityMapMetaData = null;
            var metaGen = new CityMapMetaDataGenerator();
            var metaGenConfig = new CityMapMetaDataGeneratorConfig
            {
                CityModelImportConfig = importConf,
                DoClearIdToGmlTable = isFirstFile,
                ParserParams = new CitygmlParserParams(),
            };
            
            bool isSucceed = metaGen.Generate(metaGenConfig, dstMeshAssetPath , gmlFileName);
            
            if (!isSucceed)
            {
                return false;
            }
            cityMapMetaData = metaGen.LastConvertedCityMapMetaData;
            return true;
        }
    }
}