using PLATEAU.CityAdjust.NonLibData;
using System;
using System.Collections.Generic;
using System.IO;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
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
            
            var srcTransforms = new UniqueParentTransformList(conf.SrcGameObj.transform);
            var srcTrans = conf.SrcGameObj.transform;
            
            progress.Display("都市モデルの情報を記録中...", 0.1f);
            
            // 属性情報、都市情報、マテリアルを覚えておきます。
            var nonLibDataHolder = new NonLibData.NonLibDataHolder(
                new NameToAttrsDict(),
                new InstancedCityModelDict(),
                new NameToMaterialsDict()
            );
            nonLibDataHolder.ComposeFrom(srcTransforms);
            
            progress.Display("共通ライブラリのモデルに変換中...", 0.35f);
            
            // 共通ライブラリのModelに変換します。
            using var model = UnityMeshToDllModelConverter.Convert(
                srcTransforms,
                new UnityMeshToDllSubMeshWithTexture(true),
                false,
                VertexConverterFactory.LocalCoordinateSystemConverter(CoordinateSystem.WUN, srcTrans.position),
                true);
            
            progress.Display("FBXに出力中...", 0.6f);
            // FBXに出力します。
            var fullPath = Path.GetFullPath(conf.AssetPath);
            string fbxNameWithoutExtension = conf.SrcGameObj.name;
            
            new CityExporterFbx().Export(Path.GetFullPath(conf.AssetPath), fbxNameWithoutExtension, model);
            AssetDatabase.Refresh();
            
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
                return;
            }
            var dstParent = new GameObject("Asset_" + conf.SrcGameObj.name);
            dstParent.transform.parent = conf.SrcGameObj.transform.parent;
            dstParent.transform.SetPositionAndRotation(srcTrans.position, srcTrans.rotation);
            var newTransforms = new UniqueParentTransformList();
            foreach (var fbx in fbxs)
            {
                var srcObj = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtil.FullPathToAssetsPath(fbx));
                if (srcObj == null) continue;
                var newObj = Object.Instantiate(srcObj, srcTrans.position, srcTrans.rotation, dstParent.transform);
                newObj.name = srcObj.name;
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

            Dialogue.Display("Assetsへの保存が完了しました！", "OK");
            
#else
            throw new NotImplementedException("ConvertToAssetはランタイムでの実行には未対応です。");
#endif
        }
        
    }
}