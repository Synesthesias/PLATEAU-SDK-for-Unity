using System;
using System.Collections.Generic;
using System.IO;
using PLATEAU.CityExport;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
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
        private static readonly int PropIdBaseMap = Shader.PropertyToID("_BaseMap");

        public void Convert(ConvertToAssetConfig conf)
        {
#if UNITY_EDITOR
            
            if (Directory.GetFileSystemEntries(Path.GetFullPath(conf.AssetPath)).Length > 0)
            {
                Debug.LogError("失敗：出力先は空のディレクトリを指定してください");
                return;
            }

            using var progress = new ProgressBar();
            
            var srcTransforms = new UniqueParentTransformList(new [] {conf.SrcGameObj.transform});
            var srcTrans = conf.SrcGameObj.transform;
            
            progress.Display("都市モデルの情報を記録中...", 0.1f);
            
            // 属性情報、都市情報、マテリアルを覚えておきます。
            var attributes = NameToAttrsDict.ComposeFrom(conf.SrcGameObj);
            var instancedCityModelDict = InstancedCityModelDict.ComposeFrom(srcTransforms);
            var nameToMaterialsDict = NameToMaterialsDict.ComposeFrom(conf.SrcGameObj);
            
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
            var newRenderers = new List<Renderer>();
            foreach (var newTrans in newTransforms.Get)
            {
                nameToMaterialsDict.RestoreTo(newTrans);
                attributes.RestoreTo(newTrans);
                newRenderers.AddRange(newTrans.GetComponentsInChildren<Renderer>());
            }
            instancedCityModelDict.Restore(newTransforms);
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
        
#if UNITY_EDITOR

        /// <summary>
        /// ゲームオブジェクト名と属性情報の辞書です。
        /// </summary>
        internal class NameToAttrsDict
        {
            private Dictionary<string, PLATEAUCityObjectGroup> data = new();

            /// <summary>
            /// ゲームオブジェクトとその子から属性情報の辞書を構築します。
            /// </summary>
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

            /// <summary>
            /// <paramref name="target"/>とその子に対して、
            /// ゲームオブジェクト名を元に覚えておいた属性情報を復元します。
            /// </summary>
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

        /// <summary>
        /// ゲームオブジェクト名とマテリアルの辞書です。
        /// </summary>
        internal class NameToMaterialsDict
        {
            private Dictionary<string,Material[]> data = new();

            /// <summary>
            /// ゲームオブジェクトとその子から、マテリアルの辞書を構築します。
            /// ただしヒエラルキー上で非アクティブのものは対象外とします。
            /// </summary>
            public static NameToMaterialsDict ComposeFrom(GameObject src)
            {
                var ret = new NameToMaterialsDict();
                ComposeRecursive(ret, src);
                return ret;
            }
            
            private static void ComposeRecursive(NameToMaterialsDict dict, GameObject src)
            {
                if (!src.activeInHierarchy) return;
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
                if (!data.TryAdd(name, materials))
                {
                    // 重複時はログを出します。ただし、ToolkitsのAutoTexturingで多数出てくる名前はよしとします。
                    if (name != "FloorEmission" && name != "ObstacleLight")
                    {
                        Debug.LogError($"Duplicate game object name: {name}");
                    }
                };
            }

            /// <summary>
            /// <paramref name="dst"/>とその子に対して、ゲームオブジェクト名からマテリアルを復元します。
            /// ただし、FBXのマテリアルを使った方が良い状況ではそれを使います。
            /// </summary>
            public void RestoreTo(Transform dst)
            {
                
                // Plateau ToolkitのAutoTexturingで生成されるObstacleLight向けの特別処理です。
                // 複数の"ObstacleLight"を含むFBXをインポートするとUnityの仕様で"ObstacleLight 1" "ObstacleLight 2"... という名前に変わってしまいますが、
                // 名前が変わると以下のマテリアルを当てる処理で問題となるので名前を戻します。
                if (dst.name is "ObstacleLight 1" or "ObstacleLight 2" or "ObstacleLight 3")
                {
                    dst.name = "ObstacleLight";
                }
                
                // 以下、マテリアルを当てる処理
                var renderer = dst.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var nextMaterials = renderer.sharedMaterials;
                    if(data.TryGetValue(dst.name, out var materials))
                    {
                        for (int i = 0; i < renderer.sharedMaterials.Length && i < materials.Length; i++)
                        {
                            var srcMat = materials[i];

                            if (srcMat == null)
                            {
                                nextMaterials[i] = null;
                                continue;
                            }
                            
                            // マテリアル復元の分岐
                            
                            // trueならFBXのマテリアルを利用し、falseなら元のマテリアルを利用します。
                            bool shouldUseFbxMaterial = false;


                            string shaderName = srcMat.shader.name;
                            if (shaderName is "Weather/Building_URP" or "Weather/Building_HDRP")
                            {
                                // Rendering ToolkitのAuto Textureを利用している場合
                                // マテリアルは元からコピーします、ただしテクスチャはfbxのものに差し替えます。
                                shouldUseFbxMaterial = false;
                                var nextMaterial = new Material(srcMat);
                                var fbxTex = nextMaterials[i].mainTexture;
                                nextMaterial.SetTexture(PropIdBaseMap, fbxTex);
                            }else if (shaderName is "Shader Graphs/ObstacleLight_URP" or "Shader Graphs/ObstacleLight_HDRP")
                            {
                                // Rendering ToolkitのAuto Textureで生成されるライトの場合
                                shouldUseFbxMaterial = false;
                                nextMaterials[i] = new Material(srcMat);
                            }
                            else
                            {
                                // Rendering Toolkitでない場合
                                
                                // mainTextureがないシェーダーなら、元のマテリアルを利用します。
                                if (!srcMat.HasMainTextureAttribute())
                                {
                                    shouldUseFbxMaterial = false;
                                }
                                else
                                {
                                    var srcTexPath = AssetDatabase.GetAssetPath(srcMat.mainTexture);
                                    // 元のテクスチャがシーン内に保存されているなら、FBXに出力されたマテリアルを利用します。
                                    // 元のテクスチャがシーン外に保存されているなら、元のマテリアルを利用します。
                                    shouldUseFbxMaterial = srcTexPath == "";
                                }
                            }
                            
                            
                            if(!shouldUseFbxMaterial) nextMaterials[i] = srcMat;
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
#endif
    }
}