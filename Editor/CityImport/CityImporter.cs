using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.Converters;
using PlateauUnitySDK.Runtime.Behaviour;
using PlateauUnitySDK.Runtime.CityMeta;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PlateauUnitySDK.Editor.CityImport
{

    /// <summary>
    /// 都市モデルをインポートします。
    /// 複数のgmlファイルから、複数のobj および 1つの <see cref="CityMetaData"/>テーブルを生成します。
    /// モデルを現在のシーンに配置します。
    /// </summary>
    internal class CityImporter
    {
        
        /// <summary> このインスタンスが最後に出力した <see cref="CityMetaData"/> です。 </summary>
        public CityMetaData LastConvertedCityMetaData { get; private set; }
        
        /// <summary>
        /// 複数のgmlファイルを変換します。
        /// 変換元が StreamingAssets の配下にない場合、StreamingAssets配下である <see cref="PlateauPath.StreamingGmlFolder"/> にコピーした上で
        /// 変換元をコピー先に変更します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。</param>
        /// <param name="config">変換設定です。</param>
        public void Import(string[] gmlRelativePaths, CityImporterConfig config)
        {
            // 元フォルダを StreamingAssets にコピーします。 
            CopySrcFolderToStreamingAssets(config, out string srcFolderName, gmlRelativePaths);
            
            // gmlファイルごとのループを始めます。
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
                if (!TryGenerateMetaData(out var cityMapMetaData, gmlFileName, objAssetPath, loopCount==1, config))
                {
                    continue;
                }

                // シーンに配置します。
                PlaceToScene(objAssetPath, srcFolderName, cityMapMetaData);
                
                LastConvertedCityMetaData = cityMapMetaData;
                successCount++;
            }
            // gmlファイルごとのループ　ここまで
            
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
        private static void CopySrcFolderToStreamingAssets(CityImporterConfig config, out string srcFolderName, string[] gmlRelativePaths)
        {
            string prevSrc = Path.GetFullPath(Path.Combine(config.sourceUdxFolderPath, "../"));
            srcFolderName = Path.GetFileName(Path.GetDirectoryName(prevSrc));
            if (IsInStreamingAssets(prevSrc)) return;
            if (srcFolderName == null) throw new FileNotFoundException($"{nameof(srcFolderName)} is null.");
            
            
            string nextSrc = PlateauPath.StreamingGmlFolder;
            
            // コピー先のルートフォルダを作成します。
            string dstRootFolder = Path.Combine(nextSrc, srcFolderName);
            if (!Directory.Exists(dstRootFolder))
            {
                AssetDatabase.CreateFolder(nextSrc, srcFolderName);
            }
            
            // codelists をコピーします。
            const string codelistsFolderName = "codelists";
            PathUtil.CloneDirectory(Path.Combine(prevSrc, codelistsFolderName), nextSrc);
            
            // udxフォルダを作ります。
            const string udxFolderName = "udx";
            string dstUdxFolder = Path.Combine(dstRootFolder, udxFolderName);
            if (!Directory.Exists(dstUdxFolder))
            {
                AssetDatabase.CreateFolder(dstRootFolder, udxFolderName);
            }
            
            // udxフォルダのうち対象のgmlファイルをコピーします。
            foreach (string gml in gmlRelativePaths)
            {
                GmlFileNameParser.Parse(gml, out int _, out string objTypeStr, out int _, out string _);
                // 地物タイプのディレクトリを作ります。
                string dstObjTypeFolder = dstUdxFolder + objTypeStr;
                if (!Directory.Exists(dstObjTypeFolder))
                {
                    AssetDatabase.CreateFolder(dstUdxFolder, objTypeStr);
                }
                // gmlファイルをコピーします。
                string gmlName = gml + ".gml";
                string srcObjTypeFolder = Path.Combine(prevSrc, "udx", objTypeStr);
                File.Copy(Path.Combine(srcObjTypeFolder, gmlName), Path.Combine(dstObjTypeFolder, gmlName), true);
                // gmlファイルに関連するフォルダをコピーします。
                // gmlの名称からオプションと拡張子を除いた文字列がフォルダ名に含まれていれば、コピー対象のディレクトリとみなします。
                string gmlIdentity = GmlFileNameParser.NameWithoutOption(gml);
                foreach (var dir in Directory.GetDirectories(srcObjTypeFolder))
                {
                    string srcDirName = new DirectoryInfo(dir).Name;
                    if (srcDirName.Contains(gmlIdentity))
                    {
                        PathUtil.CloneDirectory(srcDirName, dstObjTypeFolder);
                    }
                }
            }
            
            // configの変換元パスを再設定します。
            config.sourceUdxFolderPath = Path.Combine(nextSrc, $"{srcFolderName}/udx");
        }

        public static bool IsInStreamingAssets(string path)
        {
            return PathUtil.IsSubDirectory(path, Application.streamingAssetsPath);
        }


        private static bool TryLoadCityGml(out CityModel cityModel, string gmlFullPath, CityImporterConfig config)
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

        private static bool TryConvertToObj(CityModel cityModel, ref Vector3? referencePoint, CityImporterConfig importerConfig, string gmlFullPath, string objPath)
        {
            using (var objConverter = new GmlToObjConverter())
            {
                // configを作成します。
                var converterConf = new GmlToObjConverterConfig();
                converterConf.MeshGranularity = importerConfig.meshGranularity;
                converterConf.LogLevel = importerConfig.logLevel;
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

        private static bool TryGenerateMetaData(out CityMetaData cityMetaData, string gmlFileName,
            string dstMeshAssetPath, bool isFirstFile, CityImporterConfig importerConf)
        {
            cityMetaData = null;
            var metaGen = new CityMetaDataGenerator();
            var metaGenConfig = new CityMapMetaDataGeneratorConfig
            {
                CityImporterConfig = importerConf,
                DoClearIdToGmlTable = isFirstFile,
                ParserParams = new CitygmlParserParams(),
            };
            
            bool isSucceed = metaGen.Generate(metaGenConfig, dstMeshAssetPath , gmlFileName);
            
            if (!isSucceed)
            {
                return false;
            }
            cityMetaData = metaGen.LastConvertedCityMetaData;
            return true;
        }

        private static void PlaceToScene(string objAssetPath, string srcFolderName, CityMetaData metaData)
        {
            // 親を配置
            var parent = GameObject.Find(srcFolderName);
            if (parent == null)
            {
                parent = new GameObject(srcFolderName);
            }
            
            // 親に CityMapBehaviour をアタッチ
            var cityMapBehaviour = parent.GetComponent<CityBehaviour>();
            if ( cityMapBehaviour == null)
            {
                cityMapBehaviour = parent.AddComponent<CityBehaviour>();
            }
            cityMapBehaviour.CityMetaData = metaData;

            var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(objAssetPath);
            
            // 古い同名の GameObject を削除
            var oldObj = FindRecursive(parent.transform, assetObj.name);
            if (oldObj != null)
            {
                Object.DestroyImmediate(oldObj.gameObject);
            }

            // 変換後モデルの配置
            var placedObj = (GameObject)PrefabUtility.InstantiatePrefab(assetObj);
            placedObj.name = assetObj.name;
            placedObj.transform.parent = parent.transform;
        }

        private static Transform FindRecursive(Transform target, string name)
        {
            if (target.name == name) return target;
            foreach (Transform child in target)
            {
                Transform found = FindRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}