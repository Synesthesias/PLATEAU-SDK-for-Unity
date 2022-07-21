using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.Editor.Converters;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
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
    /// ・Plateau元データから、複数のobj および 1つの <see cref="CityMetadata"/> を作成します。
    /// ・変換したモデルを現在のシーンに配置します。
    /// </summary>
    internal static class CityImporterModel
    {

        /// <summary>
        /// gmlデータ群をインポートします。
        /// コピーおよび変換されるのは <paramref name="gmlRelativePaths"/> のリストにある gmlファイルに関連するデータのみです。
        /// 変換に成功したgmlファイルの数を返します。
        /// </summary>
        /// <param name="gmlRelativePaths">gmlファイルの相対パスのリストです。パスの起点は udx フォルダです。</param>
        /// <param name="importConfig">変換設定です。</param>
        /// <param name="metadata">インポートによって生成されたメタデータです。</param>
        public static int Import(string[] gmlRelativePaths, CityImportConfig importConfig, out CityMetadata metadata)
        {
            // 元フォルダを StreamingAssets/PLATEAU にコピーします。すでに StreamingAssets内にある場合を除きます。 
            // 設定のインポート元パスをコピー後のパスに変更します。
            var sourcePathConf = importConfig.sourcePath;
            sourcePathConf.SetRootDirFullPath(CopyImportSrcToStreamingAssets(importConfig.SrcRootPathBeforeImport, gmlRelativePaths));
            var importDest = importConfig.importDestPath;
            metadata = CityMetadataGenerator.LoadOrCreateMetadata(importDest.MetadataAssetPath, true);
            EditorUtility.SetDirty(metadata);

            metadata.gmlRelativePaths = gmlRelativePaths;
            
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
                if (!TryLoadCityGml(out var cityModel, gmlFullPath, importConfig))
                {
                    cityModel?.Dispose();
                    continue;
                }
                
                
                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, importConfig, gmlFullPath, importDest.DirFullPath, out string[] exportedFilePaths))
                {
                    // 出力されるモデルがなければ、ここで終了します。
                    cityModel?.Dispose();
                    continue;
                }

                // 生成されたファイルをインポートします。
                foreach (string fullPath in exportedFilePaths)
                {
                    string objAssetsPath = PathUtil.FullPathToAssetsPath(fullPath);
                    var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
                    int lod = ModelFileNameParser.GetLod(objAssetsPath);
                    generatedObjs.Add(new ObjInfo(objAssetsPath, lod, gmlType));
                    
                    // mtlファイルが存在すればインポートします。
                    string mtlAssetsPath = PathUtil.RemoveExtension(objAssetsPath) + ".mtl";
                    if (File.Exists(PathUtil.AssetsPathToFullPath(mtlAssetsPath)))
                    {
                        AssetDatabase.ImportAsset(mtlAssetsPath);
                    }
                        
                    // objファイルをインポートします。
                    AssetDatabase.ImportAsset(objAssetsPath);
                }
                
                
                // 基準座標は最初のものに合わせます。
                if (!referencePoint.HasValue) throw new Exception($"{nameof(referencePoint)} is null.");
                importConfig.referencePoint = referencePoint.Value;
                

                // 1つのgmlから LODごとに 0個以上の .obj ファイルが生成されます。
                // .obj ファイルごとのループを始めます。
                
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                var objConvertLodConf = importConfig.objConvertTypesConfig;
                var objNames = objConvertLodConf.ObjFileNamesForGml(gmlFileName);
                var objAssetPaths = objNames.Select(name => Path.Combine(importDest.DirAssetsPath, name + ".obj"));
                foreach(string objAssetPath in objAssetPaths)
                {
                    // CityMetadata を生成します。
                    if (!TryGenerateMetadata(metadata, gmlFileName, objAssetPath, importConfig))
                    {
                        Debug.LogError($"Failed to generate meta data.\nobjAssetPath = {objAssetPath}");
                    }
                }
                
                
                cityModel?.Dispose();
                successCount++;
            }
            // gmlファイルごとのループ　ここまで
            
            importConfig.generatedObjFiles = generatedObjs;
            
            // シーンに配置します。
            importConfig.rootDirName = PlateauSourcePath.RootDirName(sourcePathConf.RootDirAssetPath);
            CityMeshPlacerModel.Place(metadata.cityImportConfig.cityMeshPlacerConfig, metadata);
            
            // 後処理
            EditorUtility.SetDirty(metadata);
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
        private static bool TryLoadCityGml(out CityModel cityModel, string gmlFullPath, CityImportConfig config)
        {
            cityModel = null;
            if (!File.Exists(gmlFullPath))
            {
                Debug.LogError($"Gml file is not found.\ngmlPath = {gmlFullPath}");
                return false;
            }

            // 設定の parserParams.tessellate は true にしないとポリゴンにならない部分があるので true で固定します。
            // 　　　 最適化は常に true にします。false にする意味が無いので。
            CitygmlParserParams parserParams = new CitygmlParserParams(true);
            cityModel = CityGml.Load(gmlFullPath, parserParams, DllLogCallback.UnityLogCallbacks, config.logLevel);
            if (cityModel == null)
            {
                Debug.LogError($"Loading gml failed.\ngml path = {gmlFullPath}");
                cityModel?.Dispose();
                return false;

            }

            return true;
        }

        private static string CopyImportSrcToStreamingAssets(string srcRootPathBeforeImport, string[] gmlRelativePaths)
        {
            string newRootFullPath = srcRootPathBeforeImport; // デフォルト値
            if (!IsInStreamingAssets(srcRootPathBeforeImport))
            {
                string copyDest = PlateauUnityPath.StreamingGmlFolder;
                CopyPlateauSrcFiles.SelectCopy(srcRootPathBeforeImport, copyDest, gmlRelativePaths);
                newRootFullPath = Path.Combine(copyDest, $"{PlateauSourcePath.RootDirName(srcRootPathBeforeImport)}");
            }
            return newRootFullPath;
        }

        /// <summary>
        /// <see cref="CityModel"/> を obj形式の3Dモデルに変換します。
        /// 成否を bool で返します。
        /// </summary>
        private static bool TryConvertToObj(CityModel cityModel, ref Vector3? referencePoint,
            CityImportConfig importConfig, string gmlFullPath, string objDestDirFullPath, out string[] exportedFilePaths)
        {
            using (var objConverter = new GmlToObjConverter())
            {
                
                // configの値を作ります。
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlFullPath);
                (int minLod, int maxLod) = importConfig.objConvertTypesConfig.GetMinMaxLodForType(gmlType);
                Vector3? manualReferencePoint;
                // Reference Pointは最初のものに合わせます。
                if (referencePoint == null)
                {
                    referencePoint = objConverter.SetValidReferencePoint(cityModel);
                    manualReferencePoint = referencePoint;
                }
                else
                {
                    manualReferencePoint = referencePoint.Value;
                }
                
                // 変換設定を作成します。この設定は gml 1つに対する変換に関して利用されます。
                var converterConf = new GmlToObjConverterConfig(
                    exportAppearance:        importConfig.exportAppearance,
                    meshGranularity:         importConfig.meshGranularity,
                    minLod:                  minLod,
                    maxLod:                  maxLod,
                    exportLowerLod:          importConfig.objConvertTypesConfig.TypeExportLowerLodDict[gmlType],
                    doAutoSetReferencePoint: false,
                    manualReferencePoint:    manualReferencePoint,
                    logLevel:               importConfig.logLevel
                );
                
                objConverter.Config = converterConf;
                
                bool isSuccess = objConverter.ConvertWithoutLoad(cityModel, gmlFullPath, objDestDirFullPath, out exportedFilePaths);

                return isSuccess;
            }
        }

        /// <summary>
        /// 変換に関する情報をメタデータに記録します。
        /// 成否を bool で返します。
        /// 高速化の都合上、シリアライズの回数を削減するため、このメソッドではメタデータはファイルに保存されず、メモリ内でのみ変更が保持されます。
        /// ファイルの保存はインポートの終了時に別途行われることが前提です。
        /// </summary>
        private static bool TryGenerateMetadata(CityMetadata cityMetadata, string gmlFileName,
            string dstMeshAssetPath, CityImportConfig importConf)
        {
            var metaGen = new CityMetadataGenerator();
            var metaGenConfig = new CityMapMetadataGeneratorConfig
            {
                CityImportConfig = importConf,
                DoClearIdToGmlTable = false, // 新規ではなく上書き。gmlファイルごとに情報を追記していくため。
                ParserParams = new CitygmlParserParams(),
            };
            
            bool isSucceed = metaGen.Generate(metaGenConfig, dstMeshAssetPath , gmlFileName, cityMetadata, doSaveFile: false);
            
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