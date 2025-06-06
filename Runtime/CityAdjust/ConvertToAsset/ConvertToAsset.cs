using PLATEAU.CityAdjust.NonLibData;
using System;
using System.Collections.Generic;
using System.IO;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.Geometries;
using PLATEAU.Util;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityAdjust.ConvertToAsset
{
    /// <summary>
    /// シーン内に保存された都市モデルを、属性情報等を保ったままFBXに出力します。
    /// </summary>
    public class ConvertToAsset
    {

        public void Convert(ConvertToAssetConfig conf)
        {
#if UNITY_EDITOR
            if (Directory.GetFileSystemEntries(Path.GetFullPath(conf.AssetPath)).Length > 0)
            {
                Debug.LogError("失敗：出力先は空のディレクトリを指定してください");
                return;
            }

            using var progress = new ProgressBar();
            ConvertCore(conf, progress);
            Dialogue.Display("Assetsへの保存が完了しました！", "OK");
#else
            throw new NotImplementedException("ConvertToAssetはランタイムでの実行には未対応です。");
#endif
        }
        
        public List<GameObject> ConvertCore(ConvertToAssetConfig conf, IProgressBar progress)
        {
#if UNITY_EDITOR
            var srcTransforms = new UniqueParentTransformList(conf.SrcGameObj.transform);
            var srcTrans = conf.SrcGameObj.transform;

            progress.Display("都市モデルの情報を記録中...", 0.1f);

            using var subMeshConverter = new UnityMeshToDllSubMeshWithTexture(true);
            
            // 属性情報、都市情報、マテリアルを覚えておきます。
            var nonLibDataHolder = new NonLibData.NonLibDataHolder(
                new PositionRotationDict(),
                new NameToAttrsDict(),
                new NameToExportedMaterialsDict(subMeshConverter),
                new NonLibComponentsDict()
            );
            nonLibDataHolder.ComposeFrom(srcTransforms);

            progress.Display("共通ライブラリのモデルに変換中...", 0.35f);

            // 共通ライブラリのModelに変換します。
            using var model = UnityMeshToDllModelConverter.Convert(
                srcTransforms,
                subMeshConverter,
                false,
                VertexConverterFactory.LocalCoordinateSystemConverter(CoordinateSystem.WUN, srcTrans.position),
                true);

            progress.Display("FBXに出力中...", 0.6f);

            // FBXに出力します。
            var fullPath = Path.GetFullPath(conf.AssetPath);
            string fbxNameWithoutExtension = conf.SrcGameObj.name;
            
            new CityExporterFbx().Export(Path.GetFullPath(conf.AssetPath), fbxNameWithoutExtension, model);
            
            // FBXのインポート設定を適切に直したうえでインポートします。
            var assetPath = PathUtil.FullPathToAssetsPath(fullPath);
            PLATEAUAssetPostProcessor.PostProcessHandler assetProcess = () =>
            {
                AdjustFbxImportSettings(assetPath);
            };
            PLATEAUAssetPostProcessor.OnPostProcess += assetProcess;
            AssetDatabase.Refresh();
            PLATEAUAssetPostProcessor.OnPostProcess -= assetProcess;
            
            // FBXのインポート設定をします。
            string fbxPath = Path.Combine(conf.AssetPath, fbxNameWithoutExtension + ".fbx");
            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath)  as ModelImporter;
            if (modelImporter != null)
            {
                modelImporter.globalScale = 100;
                modelImporter.isReadable = true;
                modelImporter.SaveAndReimport();
            }

            progress.Display("FBXをシーンに配置中...", 0.8f);

            // FBXをシーンに配置します。
            var fbxs = Directory.GetFiles(fullPath, "*.fbx", SearchOption.TopDirectoryOnly);

            if (fbxs.Length == 0)
            {
                Debug.LogError("失敗： fbxファイルが生成されませんでした。");
                return new List<GameObject>();
            }

            var dstParent = srcTrans.parent;
            var newTransforms = new UniqueParentTransformList();
            foreach (var fbx in fbxs)
            {
                var srcObj = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtil.FullPathToAssetsPath(fbx));
                if (srcObj == null) continue;
                var newObj = Object.Instantiate(srcObj, srcTrans.position, srcTrans.rotation, dstParent);
                newObj.name = srcObj.name;
                AdjustGameObjectNames(newObj.transform);
                newTransforms.Add(newObj.transform);
            }

            progress.Display("都市の情報を復元中...", 0.9f);

            // 覚えておいたマテリアル、属性情報、都市情報を復元します。
            nonLibDataHolder.RestoreTo(newTransforms);
            
            // Colliderの付与
            var newRenderers = new List<Renderer>();
            foreach (var newTrans in newTransforms.Get)
            {
                newRenderers.AddRange(newTrans.GetComponentsInChildren<Renderer>());
            }
            foreach (var r in newRenderers)
            {
                if (r.GetComponent<MeshCollider>() != null) continue;
                r.gameObject.AddComponent<MeshCollider>();
            }
            
            // 元のゲームオブジェクトの削除
            var srcArr = srcTransforms.Get.ToArray();
            foreach (var s in srcArr)
            {
                Object.DestroyImmediate(s.gameObject);
            }

            Selection.objects = newTransforms.Get.Select(t => (Object)t.gameObject).ToArray();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
            return newTransforms.Get.Select(t => t.gameObject).ToList();
#else 
            throw new NotImplementedException("ConvertToAssetはランタイムでの実行には未対応です。");
#endif
        }
        
        /// <summary>
        /// FBX設定を調整します。
        /// </summary>
        private static void AdjustFbxImportSettings(string targetFolderPath)
        {
            #if UNITY_EDITOR
            string[] fbxFiles = Directory.GetFiles(targetFolderPath, "*.fbx", SearchOption.AllDirectories);

            foreach (string fbxPath in fbxFiles)
            {
                ModelImporter importer = AssetImporter.GetAtPath(PathUtil.FullPathToAssetsPath(fbxPath)) as ModelImporter;
                if (importer != null)
                {
                    // エクスポートしたFBXにはNormalがないので、そのままインポートすると「ノーマルがないので計算します」という警告がたくさん出ます。
                    // これを抑制するため、インポート設定のノーマルを「計算する」に変更します。
                    importer.importNormals = ModelImporterNormals.Calculate;

                    // Unityの最適化を切る設定にします。
                    // これをしないと、PLATEAUの標準作業手順書の仕様を満たさなくなります。
                    // 例えば、luseの頂点の順番は、頂点を順番に繋いだときに交差しない多角形になることが手順書で定められています。
                    // ここで最適化が働いてしまうと、頂点の順番が変わってしまい、順番上はぐちゃぐちゃに交差する多角形になってしまいます。
                    importer.optimizeMeshVertices = false;
                    importer.optimizeMeshPolygons = false;
                }
            }
#else
            throw new NotImplementedException("ConvertToAssetはランタイムでの実行には未対応です。");
#endif
        }

        private void AdjustGameObjectNames(Transform target)
        {
            new UniqueParentTransformList(target).BfsExec(trans =>
            {
                // 同名のgmlがあるとき、FBXにすると "xxx.gml 1" のように末尾に数字が付いてしまうのを修正します。
                if (Regex.IsMatch(trans.name, @"^.+\.gml\s[0-9]+$"))
                {
                    trans.name = Regex.Replace(trans.name, @"\.gml\s[0-9]+$", ".gml");
                }

                return NextSearchFlow.Continue;
            });
        }
    }
}