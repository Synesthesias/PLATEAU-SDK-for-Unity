using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Editor.Converters;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    internal static class GmlImporter
    {
        /// <summary>
        /// GML群のインポートで、objファイル変換と <see cref="CityMetadata"/> への書き込みをします。
        /// </summary>
        public static void ImportGmls(out int successCount, out int failureCount, string[] gmlRelativePaths, CityImportConfig importConfig, CityMetadata metadata)
        {
            successCount = 0;
            int loopCount = 0;
            int numGml = gmlRelativePaths.Length;
            Vector3? referencePoint = null; // referencePoint は最初に合わせます
            var generatedObjs = new List<ObjInfo>();
            // gmlごとのループ
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                loopCount++;
                ProgressBar($"gml変換中 : [{loopCount}/{numGml}] {gmlRelativePath}", loopCount, numGml );
                bool result = ImportGml(gmlRelativePath, importConfig, ref referencePoint, generatedObjs, metadata);
                if (result) successCount++;
            }

            // 情報書き込み
            metadata.gmlRelativePaths = gmlRelativePaths;
            importConfig.generatedObjFiles = generatedObjs;
            importConfig.rootDirName = PlateauSourcePath.RootDirName(importConfig.sourcePath.RootDirAssetPath);
            
            failureCount = loopCount - successCount;
        }


        /// <summary>
        /// Gmlファイルを1つインポートします。成否をboolで返します。
        /// <paramref name="referencePoint"/> は null の場合は新しく設定し、値がある場合はその値を利用します。
        /// </summary>
        private static bool ImportGml(string gmlRelativePath, CityImportConfig importConfig,
            ref Vector3? referencePoint, List<ObjInfo> generatedObjs, CityMetadata metadata)
        {

            string gmlFullPath = importConfig.sourcePath.UdxRelativeToFullPath(gmlRelativePath);

            // gmlをロードします。
            bool isCityGmlLoaded = TryLoadCityGml(out var cityModel, gmlFullPath, importConfig);

            // cityModel が 必ず Dispose されるように using で囲みます。
            // そうしないと Unityが終了するまでファイルの利用プロセスが解放されなくなります。
            using (cityModel)
            {
                if (!isCityGmlLoaded) return false;
                var importDest = importConfig.importDestPath;
                // objに変換します。
                if (!TryConvertToObj(cityModel, ref referencePoint, importConfig, gmlFullPath, importDest.DirFullPath,
                        out string[] exportedFilePaths))
                {
                    // 出力されるモデルがなければ、ここで終了します。
                    return false;
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
                foreach (string objAssetPath in objAssetPaths)
                {
                    // CityMetadata に情報を記録します。
                    if (!TryWriteConfigsToMetadata(metadata, gmlFileName, objAssetPath, importConfig))
                    {
                        Debug.LogError($"Failed to generate meta data.\nobjAssetPath = {objAssetPath}");
                    }
                }
            }

            return true;
        }

        private static void ProgressBar(string info, int currentCount, int maxCount)
        {
            EditorUtility.DisplayProgressBar("gmlファイル変換中", info, ((float)currentCount)/maxCount);
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
                Vector3? manualReferencePoint;
                // Reference Pointは最初のものに合わせます。
                // 最初は referencePoint == null なので新しく値を設定、以降は値が設定されているのでその値を利用します。
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
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlFullPath);
                var converterConf = importConfig.ToConverterConfig(gmlType, manualReferencePoint);
                
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
        private static bool TryWriteConfigsToMetadata(CityMetadata cityMetadata, string gmlFileName,
            string dstMeshAssetPath, CityImportConfig importConf)
        {
            var metaGenConfig = new CityMapMetadataGeneratorConfig
            {
                CityImportConfig = importConf,
                DoClearIdToGmlTable = false, // 新規ではなく上書き。gmlファイルごとに情報を追記していくため。
                ParserParams = new CitygmlParserParams(),
            };
            
            bool isSucceed = CityMetadataGenerator.WriteConfigs(metaGenConfig, dstMeshAssetPath , gmlFileName, cityMetadata, doSaveFile: false);
            
            return isSucceed;
        }
    }
}