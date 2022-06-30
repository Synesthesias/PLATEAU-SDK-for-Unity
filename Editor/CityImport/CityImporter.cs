using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.Editor.Converters;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Editor.CityImport
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

        /// <summary>
        /// gmlデータ群をインポートします。
        /// コピーおよび変換されるのは <paramref name="gmlRelativePaths"/> のリストにある gmlファイルに関連するデータのみです。
        /// 変換に成功したgmlファイルの数を返します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。パスの起点は udx フォルダです。</param>
        /// <param name="config">変換設定です。</param>
        /// <param name="metaData">インポートによって生成されたメタデータです。</param>
        public int Import(string[] gmlRelativePaths, CityImporterConfig config, out CityMetaData metaData)
        {
            // 元フォルダを StreamingAssets/PLATEAU にコピーします。すでに StreamingAssets内にある場合を除きます。 
            // 設定のインポート元パスをコピー後のパスに変更します。
            var sourcePathConf = config.sourcePath;
            sourcePathConf.FullUdxPath = CopyImportSrcToStreamingAssets(config.UdxPathBeforeImport, gmlRelativePaths);
            var importDest = config.importDestPath;
            metaData = CityMetaDataGenerator.LoadOrCreateMetaData(importDest.MetaDataAssetPath, true);

            // gmlファイルごとのループを始めます。
            int successCount = 0;
            int loopCount = 0;
            int numGml = gmlRelativePaths.Length;
            Vector3? referencePoint = null;
            var generatedObjs = new List<ObjInfo>();
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;
                ProgressBar($"gml変換中 : [{loopCount}/{numGml}] {gmlRelativePath}", loopCount, numGml );

                string gmlFullPath = sourcePathConf.UdxRelativeToFullPath(gmlRelativePath);

                // gmlをロードします。
                if (!TryLoadCityGml(out var cityModel, gmlFullPath, config))
                {
                    cityModel?.Dispose();
                    continue;
                }
                
                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, config, gmlFullPath, importDest.DirFullPath))
                {
                    // 出力されるモデルがなければ、ここで終了します。
                    cityModel?.Dispose();
                    continue;
                }
                
                // 変換した結果、どのLODのobjが生成されたかを調べます。

                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlFileName);
                var objConvertLodConf = config.objConvertLodConfig;
                (int minLod, int maxLod) = objConvertLodConf.GetMinMaxLodForType(gmlType);
                int lodCountForThisGml = 0;
                for (int l = minLod; l <= maxLod; l++)
                {
                    string objFullPath = Path.Combine(importDest.DirFullPath, $"LOD{l}_{gmlFileName}.obj");
                    if (File.Exists(objFullPath))
                    {
                        string objAssetsPath = PathUtil.FullPathToAssetsPath(objFullPath);
                        generatedObjs.Add(new ObjInfo(objAssetsPath, l, gmlType));
                        lodCountForThisGml++;
                    }
                }
                if (lodCountForThisGml <= 0)
                {
                    Debug.LogError($"No 3d models are found for the lod in the gml file.\ngml file = {gmlFullPath}");
                    cityModel?.Dispose();
                    continue;
                }
                
                // 基準座標は最初のものに合わせます。
                if (!referencePoint.HasValue) throw new Exception($"{nameof(referencePoint)} is null.");
                config.referencePoint = referencePoint.Value;

                // 1つのgmlから LODごとに 0個以上の .obj ファイルが生成されます。
                // .obj ファイルごとのループを始めます。
                
                var objNames = objConvertLodConf.ObjFileNamesForGml(gmlFileName);
                var objAssetPaths = objNames.Select(name => Path.Combine(importDest.dirAssetPath, name + ".obj"));
                foreach(string objAssetPath in objAssetPaths)
                {
                    // CityMapMetaData を生成します。
                    if (!TryGenerateMetaData(metaData, gmlFileName, objAssetPath, config))
                    {
                        Debug.LogError($"Failed to generate meta data.\nobjAssetPath = {objAssetPath}");
                    }
                }
                
                
                cityModel?.Dispose();
                successCount++;
            }
            // gmlファイルごとのループ　ここまで
            
            // シーンに配置します。
            string parentGameObjName = PlateauSourcePath.RootDirName(sourcePathConf.udxAssetPath);
            CityMeshPlacerToScene.Place(
                config.scenePlacementConfig, generatedObjs, parentGameObjName, metaData
            );
            config.generatedObjFiles = generatedObjs;
            config.rootDirName = parentGameObjName;
            
            // 後処理
            EditorUtility.SetDirty(metaData);
            AssetDatabase.ImportAsset(importDest.dirAssetPath);
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
            return successCount;
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

        private string CopyImportSrcToStreamingAssets(string udxPathBeforeImport, string[] gmlRelativePaths)
        {
            string newUdxFullPath = udxPathBeforeImport; // デフォルト値
            if (!IsInStreamingAssets(udxPathBeforeImport))
            {
                string copyDest = PlateauUnityPath.StreamingGmlFolder;
                CopyPlateauSrcFiles.SelectCopy(udxPathBeforeImport, copyDest, gmlRelativePaths);
                newUdxFullPath = Path.Combine(copyDest, $"{PlateauSourcePath.RootDirName(udxPathBeforeImport)}/udx");
            }
            return newUdxFullPath;
        }

        /// <summary>
        /// <see cref="CityModel"/> を obj形式の3Dモデルに変換します。
        /// 成否を bool で返します。
        /// </summary>
        private static bool TryConvertToObj(CityModel cityModel, ref Vector3? referencePoint, CityImporterConfig importerConfig, string gmlFullPath, string objDestDirFullPath)
        {
            using (var objConverter = new GmlToObjConverter())
            {
                // configを作成します。
                var converterConf = new GmlToObjConverterConfig
                {
                    MeshGranularity = importerConfig.meshGranularity,
                    LogLevel = importerConfig.logLevel,
                    DoAutoSetReferencePoint = false,
                    ExportAppearance = true
                };
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlFullPath);
                (int minLod, int maxLod) = importerConfig.objConvertLodConfig.GetMinMaxLodForType(gmlType);
                converterConf.MinLod = minLod;
                converterConf.MaxLod = maxLod;

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

                bool isSuccess = objConverter.ConvertWithoutLoad(cityModel, gmlFullPath, objDestDirFullPath);

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