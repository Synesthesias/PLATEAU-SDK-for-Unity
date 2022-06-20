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
    /// ここでいうインポートとは、次の複数の処理をまとめたものを指します:
    /// ・Plateau元データを StreamingAssets/PLATEAU 内にコピーします。GUI設定で選んだ一部のみをコピーすることもできます。
    ///   テクスチャなど関連するフォルダがあればそれもコピーします。
    /// ・Plateau元データから、複数のobj および 1つの <see cref="CityMetaData"/> を作成します。
    /// ・変換したモデルを現在のシーンに配置します。
    /// </summary>
    internal class CityImporter
    {
        
        /// <summary> このインスタンスが最後に出力した <see cref="CityMetaData"/> です。 </summary>
        public CityMetaData LastConvertedCityMetaData { get; private set; }
        
        /// <summary>
        /// gmlデータ群をインポートします。
        /// コピーおよび変換されるのは <paramref name="gmlRelativePaths"/> のリストにある gmlファイルに関連するデータのみです。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。パスの起点は udx フォルダです。</param>
        /// <param name="config">変換設定です。</param>
        public void Import(string[] gmlRelativePaths, CityImporterConfig config)
        {
            // 元フォルダを StreamingAssets/PLATEAU にコピーします。すでに StreamingAssets内にある場合を除きます。 
            string prevSourceUdxPath = config.sourceUdxFolderPath;
            string rootGmlFolderName = CopyPlateauSrcFiles.UdxPathToGmlRootFolderName(prevSourceUdxPath);
            if (!IsInStreamingAssets(prevSourceUdxPath))
            {
                string copyDest = PlateauPath.StreamingGmlFolder;
                CopyPlateauSrcFiles.SelectCopy(config.sourceUdxFolderPath, copyDest, gmlRelativePaths);
                // configの変換元パスをコピー先に再設定します。
                config.sourceUdxFolderPath = Path.Combine(copyDest, $"{rootGmlFolderName}/udx");
            }
            
            string destMetaDataPath = Path.Combine(PathUtil.FullPathToAssetsPath(config.exportFolderPath), CityMetaDataGenerator.MetaDataFileName);
            var metaData = CityMetaDataGenerator.LoadOrCreateMetaData(destMetaDataPath, true);

            // gmlファイルごとのループを始めます。
            int successCount = 0;
            int loopCount = 0;
            int numGml = gmlRelativePaths.Length;
            Vector3? referencePoint = null;
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;
                ProgressBar($"gml変換中 : [{loopCount}/{numGml}] {gmlRelativePath}", loopCount, numGml );

                string gmlFullPath = Path.GetFullPath(Path.Combine(config.sourceUdxFolderPath, gmlRelativePath));

                // gmlをロードします。
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(config.exportFolderPath, gmlFileName + ".obj");
                if (!TryLoadCityGml(out var cityModel, gmlFullPath, config))
                {
                    cityModel?.Dispose();
                    continue;
                }
                
                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, config, gmlFullPath, objPath))
                {
                    cityModel?.Dispose();
                    continue;
                }

                // CityMapMetaData を生成します。
                string objAssetPath = PathUtil.FullPathToAssetsPath(objPath);
                if (!referencePoint.HasValue) throw new Exception($"{nameof(referencePoint)} is null.");
                config.referencePoint = referencePoint.Value;
                if (!TryGenerateMetaData(metaData, gmlFileName, objAssetPath, config))
                {
                    cityModel?.Dispose();
                    continue;
                }
                cityModel?.Dispose();

                // シーンに配置します。
                PlaceToScene(objAssetPath, rootGmlFolderName, metaData);
                
                LastConvertedCityMetaData = metaData;
                successCount++;
            }
            // gmlファイルごとのループ　ここまで
            
            // 後処理
            EditorUtility.SetDirty(metaData);
            AssetDatabase.ImportAsset(PathUtil.FullPathToAssetsPath(config.exportFolderPath));
            AssetDatabase.SaveAssets();
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
            EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// Gmlファイルをロードします。
        /// 成否を bool で返します。
        /// </summary>
        private static bool TryLoadCityGml(out CityModel cityModel, string gmlFullPath, CityImporterConfig config)
        {
            cityModel = null;
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
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Loading gml failed.\ngml path = {gmlFullPath}\n{e}");
                cityModel?.Dispose();
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
        /// 変換に関する情報をメタデータに記録します。
        /// 成否を bool で返します。
        /// 高速化の都合上、シリアライズの回数を削減するため、このメソッドではメタデータはファイルに保存されず、メモリ内でのみ変更が保持されます。
        /// ファイルの保存はインポートの終了時に別途行われることが前提です。
        /// </summary>
        private static bool TryGenerateMetaData(CityMetaData cityMetaData, string gmlFileName,
            string dstMeshAssetPath, CityImporterConfig importerConf)
        {
            var metaGen = new CityMetaDataGenerator();
            var metaGenConfig = new CityMapMetaDataGeneratorConfig
            {
                CityImporterConfig = importerConf,
                DoClearIdToGmlTable = false, // 新規ではなく上書き。gmlファイルごとに情報を追記していくため。
                ParserParams = new CitygmlParserParams(),
            };
            
            bool isSucceed = metaGen.Generate(metaGenConfig, dstMeshAssetPath , gmlFileName, cityMetaData, doSaveFile: false);
            
            if (!isSucceed)
            {
                return false;
            }
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
        
        public static bool IsInStreamingAssets(string path)
        {
            return PathUtil.IsSubDirectory(path, Application.streamingAssetsPath);
        }

        private static void ProgressBar(string info, int currentCount, int maxCount)
        {
            EditorUtility.DisplayProgressBar("gmlファイル変換中", info, ((float)currentCount)/maxCount);
        }
    }
}