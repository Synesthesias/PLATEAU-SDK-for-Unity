using System;
using System.IO;
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
        /// gmlデータ群をインポートします。
        /// 以下の作業をいっぺんに行います。
        /// ・利用するファイルを StreamingAssets 内にコピーします（すでに同フォルダ内にある場合は除きます）。コピー先を変換元として設定します。
        /// ・objファイルに変換します。
        /// ・メタデータを保存します。
        /// ・変換後モデルをシーンに配置します。
        /// コピーおよび変換されるのは <paramref name="gmlRelativePaths"/> のリストにある gmlファイルに関連するデータのみです。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。パスの起点は udx フォルダです。</param>
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
            
            // 後処理
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
            // 例: Tokyoをコピーする場合のパスの例を以下に示します。
            //     Assets/StreamingAssets/PLATEAU/Tokyo　フォルダを作ります。
            string dstRootFolder = Path.Combine(nextSrc, srcFolderName);
            if (!Directory.Exists(dstRootFolder))
            {
                AssetDatabase.CreateFolder(nextSrc, srcFolderName);
            }
            
            // codelists をコピーします。
            // 例: Assets/StreamingAssets/PLATEAU/Tokyo/codelists/****.xml をコピーにより作成します。
            const string codelistsFolderName = "codelists";
            PathUtil.CloneDirectory(Path.Combine(prevSrc, codelistsFolderName), dstRootFolder);
            
            // udxフォルダを作ります。
            // 例: Assets/StreamingAssets/PLATEAU/Tokyo/udx　ができます。
            const string udxFolderName = "udx";
            string dstUdxFolder = Path.Combine(dstRootFolder, udxFolderName);
            if (!Directory.Exists(dstUdxFolder))
            {
                AssetDatabase.CreateFolder(PathUtil.FullPathToAssetsPath(dstRootFolder), udxFolderName);
            }
            
            // udxフォルダのうち対象のgmlファイルをコピーします。
            foreach (string gml in gmlRelativePaths)
            {
                GmlFileNameParser.Parse(gml, out int _, out string objTypeStr, out int _, out string _);
                // 地物タイプのディレクトリを作ります。
                // 例: gml のタイプが bldg なら、
                //     Assets/StreamingAssets/PLATEAU/Tokyo/udx/bldg　ができます。
                string dstObjTypeFolder = Path.Combine(dstUdxFolder, objTypeStr);
                if (!Directory.Exists(dstObjTypeFolder))
                {
                    AssetDatabase.CreateFolder(PathUtil.FullPathToAssetsPath(dstUdxFolder), objTypeStr);
                }
                
                // gmlファイルをコピーします。
                // 例: Assets/StreamingAssets/PLATEAU/Tokyo/bldg/1234.gml　ができます。
                string gmlName = Path.GetFileName(gml);
                string srcObjTypeFolder = Path.Combine(prevSrc, "udx", objTypeStr);
                File.Copy(Path.Combine(srcObjTypeFolder, gmlName), Path.Combine(dstObjTypeFolder, gmlName), true);
                
                // gmlファイルに関連するフォルダをコピーします。
                // gmlの名称からオプションと拡張子を除いた文字列がフォルダ名に含まれていれば、コピー対象のディレクトリとみなします。
                // 例: Assets/StreamingAssets/PLATEAU/Tokyo/bldg/1234_appearance/texture_number.jpg　などがコピーされます。 
                string gmlIdentity = GmlFileNameParser.NameWithoutOption(gml);
                foreach (var srcDir in Directory.GetDirectories(srcObjTypeFolder))
                {
                    string srcDirName = new DirectoryInfo(srcDir).Name;
                    if (srcDirName.Contains(gmlIdentity))
                    {
                        PathUtil.CloneDirectory(srcDir, dstObjTypeFolder);
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


        /// <summary>
        /// Gmlファイルをロードします。
        /// 成否を bool で返します。
        /// </summary>
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

        /// <summary>
        /// <see cref="CityModel"/> を obj形式の3Dモデルに変換します。
        /// 成否を bool で返します。
        /// </summary>
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

        /// <summary>
        /// 変換に関する情報をメタデータに保存します。
        /// 成否を bool で返します。
        /// </summary>
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

        /// <summary>
        /// 変換後の3Dモデルをシーンに配置します。
        /// </summary>
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