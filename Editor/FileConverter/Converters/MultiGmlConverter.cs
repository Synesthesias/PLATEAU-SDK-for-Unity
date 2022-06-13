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
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// 複数のgmlファイルから、複数のobj および 1つの <see cref="CityMapInfo"/>テーブルを生成します。
    /// </summary>
    public class MultiGmlConverter
    {
        /// <summary>
        /// 複数のgmlファイルを変換します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。</param>
        /// <param name="baseFolderPath"><paramref name="gmlRelativePaths"/>の相対パスの基準となるパスです。</param>
        /// <param name="exportFolderFullPath">出力先のフォルダの絶対パスです。</param>
        public void Convert(IEnumerable<string> gmlRelativePaths, string baseFolderPath, string exportFolderFullPath, CityModelImportConfig config)
        {
            int failureCount = 0;
            int loopCount = 0;
            Vector3 referencePoint = Vector3.negativeInfinity;
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;

                string gmlFullPath = Path.GetFullPath(Path.Combine(baseFolderPath, gmlRelativePath));
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(exportFolderFullPath, gmlFileName + ".obj");
                // TODO ファイル名は変更できるようにしたい
                string idTablePath = Path.Combine(exportFolderFullPath, "CityMapInfo.asset");

                // gmlをロードします。
                CityModel cityModel;
                try
                {
                    // 設定の parserParams.tessellate は true にしないとポリゴンにならない部分があるので true で固定します。
                    CitygmlParserParams parserParams = new CitygmlParserParams(config.OptimizeFlg, true);
                    cityModel = CityGml.Load(gmlFullPath, parserParams, DllLogCallback.UnityLogCallbacks, config.LogLevel);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Loading gml failed.\ngml path = {gmlFullPath}\n{e}");
                    failureCount++;
                    continue;
                }
                
                // objに変換します。
                using (var objConverter = new GmlToObjFileConverter())
                {
                    var converterConf = new GmlToObjFileConverterConfig();
                    converterConf.MeshGranularity = config.MeshGranularity;
                    converterConf.LogLevel = config.LogLevel;
                    objConverter.Config = converterConf;
                    if (referencePoint != Vector3.negativeInfinity)
                    {
                        objConverter.ReferencePoint = referencePoint;
                    }
                    bool isObjSucceed = objConverter.ConvertWithoutLoad(cityModel, gmlFullPath, objPath);
                    Debug.Log(objConverter.ReferencePoint.ToString());
                    // referencePoint は最初の変換時の座標にすべて合わせます。
                    if (loopCount == 1)
                    {
                        referencePoint = objConverter.ReferencePoint;
                    }
                    if (!isObjSucceed)
                    {
                        failureCount++;
                        continue;
                    }
                }
                
                // idTableを生成します。
                var idTableConverter = new GmlToCityMapInfoConverter();
                bool isTableSucceed = idTableConverter.ConvertWithoutLoad(cityModel, gmlFullPath, idTablePath);
                if (!isTableSucceed)
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
    }
}