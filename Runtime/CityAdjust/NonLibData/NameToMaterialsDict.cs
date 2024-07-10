using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// ゲームオブジェクト名とマテリアルの辞書です。
    /// </summary>
    internal class NameToMaterialsDict : INonLibData
    {
        private Dictionary<NonLibKeyName, Material[]> data = new();
        
        private static readonly int PropIdBaseMap = Shader.PropertyToID("_BaseMap");

        /// <summary>
        /// ゲームオブジェクトとその子から、マテリアルの辞書を構築します。
        /// ただしヒエラルキー上で非アクティブのものは対象外とします。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                if (!trans.gameObject.activeInHierarchy) return NextSearchFlow.SkipChildren;
                var renderer = trans.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Add(trans, renderer.sharedMaterials, src);
                }

                return NextSearchFlow.Continue;
            });
        }

        private void Add(Transform trans, Material[] materials, UniqueParentTransformList baseTransforms)
        {
            var key = new NonLibKeyName(trans, baseTransforms.Get.ToArray());
            if (data.TryAdd(key, materials))
            {
                return;
            }

            // 重複時はログを出します。ただし、ToolkitsのAutoTexturingで多数出てくる名前はよしとします。
            if (key.ObjName != "FloorEmission" && key.ObjName != "ObstacleLight")
            {
                Debug.Log($"{nameof(NameToMaterialsDict)} : Skipping duplicate game object name: {key}");
            }

        }

        /// <summary>
        /// <paramref name="dst"/>とその子に対して、ゲームオブジェクト名からマテリアルを復元します。
        /// ただし、FBXのマテリアルを使った方が良い状況ではそれを使います。
        /// </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(dst =>
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
                    if (data.TryGetValue(new NonLibKeyName(dst.transform, target.Get.ToArray()), out var materials))
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
                            }
                            else if (shaderName is "Shader Graphs/ObstacleLight_URP"
                                     or "Shader Graphs/ObstacleLight_HDRP")
                            {
                                // Rendering ToolkitのAuto Textureで生成されるライトの場合
                                shouldUseFbxMaterial = false;
                                nextMaterials[i] = new Material(srcMat);
                            }
                            else
                            {
                                // Rendering Toolkitでない場合

                                // mainTextureがないシェーダー、またはmainTextureがないなら、元のマテリアルを利用します。
                                if (!srcMat.HasMainTextureAttribute() || srcMat.mainTexture == null)
                                {
                                    shouldUseFbxMaterial = false;
                                }
                                else
                                {
                                    #if UNITY_EDITOR
                                    var srcTexPath = AssetDatabase.GetAssetPath(srcMat.mainTexture);
                                    #else
                                    var srcTexPath = "";
                                    #endif
                                    // 元のテクスチャがシーン内に保存されているなら、FBXに出力されたマテリアルを利用します。
                                    // 元のテクスチャがシーン外に保存されているなら、元のマテリアルを利用します。
                                    shouldUseFbxMaterial = srcTexPath == "";
                                }
                            }


                            if (!shouldUseFbxMaterial) nextMaterials[i] = srcMat;
                        }
                    }

                    renderer.sharedMaterials = nextMaterials;
                }
                return NextSearchFlow.Continue;
            });


        }
    }
}