using System.Collections.Generic;
using System.IO;
using PLATEAU.CityExport;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityAdjust.ConvertToAsset
{
    /// <summary>
    /// シーン内に保存された都市モデルを、属性情報等を保ったままFBXに出力します。
    /// </summary>
    public class ConvertToAsset
    {
        public void Convert(ConvertToAssetConfig conf)
        {
            if (Directory.GetFileSystemEntries(Path.GetFullPath(conf.AssetPath)).Length > 0)
            {
                Debug.LogError("失敗：出力先は空のディレクトリを指定してください");
                return;
            }
            var srcGameObjs = new GameObject[] { conf.SrcGameObj };
            var srcTrans = conf.SrcGameObj.transform;
            
            // 属性情報、都市情報、マテリアルを覚えておきます。
            var attributes = NameToAttrsDict.ComposeFrom(conf.SrcGameObj);
            var instancedCityModelDict = InstancedCityModelDict.ComposeFrom(srcGameObjs);
            var nameToMaterialsDict = NameToMaterialsDict.ComposeFrom(conf.SrcGameObj);
            
            // 共通ライブラリのModelに変換します。
            using var model = UnityMeshToDllModelConverter.Convert(
                srcGameObjs,
                new UnityMeshToDllSubMeshWithTexture(),
                false,
                UnityModelExporter.LocalVertexConvertFunc(CoordinateSystem.WUN, srcTrans.position));
            
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
            List<GameObject> newObjs = new List<GameObject>();
            foreach (var fbx in fbxs)
            {
                var srcObj = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtil.FullPathToAssetsPath(fbx));
                if (srcObj == null) continue;
                var newObj = Object.Instantiate(srcObj, srcTrans.position, srcTrans.rotation, dstParent.transform);
                newObj.name = srcObj.name;
                newObjs.Add(newObj);
            }

            // 覚えておいたマテリアル、属性情報、都市情報を復元します。
            var newRenderers = new List<Renderer>();
            foreach (var newObj in newObjs)
            {
                nameToMaterialsDict.RestoreTo(newObj.transform);
                attributes.RestoreTo(newObj.transform);
                newRenderers.AddRange(newObj.transform.GetComponentsInChildren<Renderer>());
            }
            instancedCityModelDict.Restore(newObjs);
            foreach (var r in newRenderers)
            {
                if (r.GetComponent<MeshCollider>() != null) continue;
                r.gameObject.AddComponent<MeshCollider>();
            }

            

        }

        internal class NameToAttrsDict
        {
            private Dictionary<string, PLATEAUCityObjectGroup> data = new();

            public static NameToAttrsDict ComposeFrom(GameObject src)
            {
                var ret = new NameToAttrsDict();
                var attrs = src.transform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var attr in attrs)
                {
                    ret.data.TryAdd(attr.gameObject.name, attr);
                }

                return ret;
            }

            public void RestoreTo(Transform target)
            {
                var existingAttr = target.GetComponent<PLATEAUCityObjectGroup>();
                if (existingAttr == null)
                {
                    if (data.TryGetValue(target.name, out var srcAttr))
                    {
                        var dstAttr = target.gameObject.AddComponent<PLATEAUCityObjectGroup>();
                        dstAttr.Init(srcAttr.CityObjects, srcAttr.InfoForToolkits, srcAttr.Granularity);
                    }
                }
                else
                {
                    Debug.LogWarning("PLATEAUCityObjectGroup is already attached.");
                }

                foreach (Transform child in target)
                {
                    RestoreTo(child);
                }
            }
            
            
        }

        internal class NameToMaterialsDict
        {
            private Dictionary<string,Material[]> data = new();

            public static NameToMaterialsDict ComposeFrom(GameObject src)
            {
                var ret = new NameToMaterialsDict();
                ComposeRecursive(ret, src);
                return ret;
            }

            private static void ComposeRecursive(NameToMaterialsDict dict, GameObject src)
            {
                var renderer = src.GetComponent<Renderer>();
                if (renderer != null)
                {
                    dict.Add(src.name, renderer.sharedMaterials);
                }

                foreach (Transform child in src.transform)
                {
                    ComposeRecursive(dict, child.gameObject);
                }
            }

            private void Add(string name, Material[] materials)
            {
                data.TryAdd(name, materials);
            }

            public void RestoreTo(Transform dst)
            {
                var renderer = dst.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var nextMaterials = renderer.sharedMaterials;
                    if(data.TryGetValue(dst.name, out var materials))
                    {
                        for (int i = 0; i < renderer.sharedMaterials.Length && i < materials.Length; i++)
                        {
                            var srcTexPath = AssetDatabase.GetAssetPath(materials[i].mainTexture);
                            
                            // 元のテクスチャがシーン内に保存されているなら、FBXに出力されたマテリアルを利用します。
                            if (srcTexPath == "") continue;
                            // 元のテクスチャがシーン外に保存されているなら、元のマテリアルを利用します。
                            nextMaterials[i] = materials[i];
                        }
                    }

                    renderer.sharedMaterials = nextMaterials;
                }
                

                foreach (Transform child in dst)
                {
                    RestoreTo(child.transform);
                }
            }

        }
    }
}