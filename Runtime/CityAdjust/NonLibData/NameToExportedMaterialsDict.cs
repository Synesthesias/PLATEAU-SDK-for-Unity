using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.Util;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// ゲームオブジェクト名と、エクスポートしたマテリアルの辞書です。
    /// </summary>
    internal class NameToExportedMaterialsDict : INonLibData
    {
        private NonLibDictionary<Material[]> data = new();
        private UnityMeshToDllSubMeshWithTexture subMeshConverter;
        private string assetPath;
        private bool isRebuild = false;

        private static readonly int PropIdBaseMap = Shader.PropertyToID("_BaseMap");

        public NameToExportedMaterialsDict(UnityMeshToDllSubMeshWithTexture subMeshConverter, string assetPath, bool isRebuild)
        {
            this.subMeshConverter = subMeshConverter;
            this.assetPath = assetPath;
            this.isRebuild = isRebuild;
        }

        /// <summary>
        /// ゲームオブジェクトとその子から、マテリアルの辞書を構築します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                var renderer = trans.GetComponent<Renderer>();
                Material[] materials;
                if (renderer != null)
                {
                    materials = renderer.sharedMaterials;
                }
                else
                {
                    materials = null;
                }
                data.Add(trans, src.Get.ToArray(), materials);

                return NextSearchFlow.Continue;
            });
        }
        

        /// <summary>
        /// <paramref name="target"/>とその子に対して、ゲームオブジェクト名からマテリアルを復元します。
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
                    string[] fbxMaterialNames = renderer.sharedMaterials.Select(mat => mat.name).ToArray();
                    var nextMaterials = renderer.sharedMaterials;
                    var srcMaterials = data.GetNonRestoredAndMarkRestored(dst.transform, target.Get.ToArray());
                    if (srcMaterials != null)
                    {
                        for (int i = 0; i < renderer.sharedMaterials.Length && i < srcMaterials.Length; i++)
                        {
                            int gameMaterialId = FbxMaterialNameToGameMaterialId(fbxMaterialNames[i]);
                            if (gameMaterialId < 0 || gameMaterialId >= subMeshConverter.GameMaterials.Count) continue;
                            var srcMat = subMeshConverter.GameMaterials[gameMaterialId];

                            if (srcMat == null)
                            {
                                nextMaterials[i] = null;
                                continue;
                            }

                            // マテリアル復元の分岐

                            // trueならFBXのマテリアルを利用し、falseなら元のマテリアルを利用します。
                            bool shouldUseFbxMaterial = false;
                            // 元のマテリアルがデフォルトマテリアルの場合は、アセットとして保存する処理をスキップします。
                            bool isDefaultMaterial = false;

                            string shaderName = srcMat.shader.name;
                            if (shaderName is "Weather/Building_URP" or "Weather/Building_HDRP")
                            {
                                // Rendering ToolkitのAuto Textureを利用している場合
                                // マテリアルは元からコピーします、ただしテクスチャはfbxのものに差し替えます。
                                shouldUseFbxMaterial = false;
                                var nextMaterial = new Material(srcMat);
                                var fbxTex = nextMaterials[i].mainTexture;
                                nextMaterial.SetTexture(PropIdBaseMap, fbxTex);
                                srcMat = nextMaterial;
                            }
                            else if (shaderName is "Shader Graphs/ObstacleLight_URP"
                                     or "Shader Graphs/ObstacleLight_HDRP")
                            {
                                // Rendering ToolkitのAuto Textureで生成されるライトの場合
                                shouldUseFbxMaterial = false;
                                srcMat = new Material(srcMat);
                            }
                            else
                            {
                                // Rendering Toolkitでない場合

                                // mainTextureがないシェーダー、またはmainTextureがない、またはデフォルトマテリアルなら、元のマテリアルを利用します。
                                isDefaultMaterial =
                                    srcMat.mainTexture != null &&
                                    FallbackMaterial.ByMainTextureName(srcMat.mainTexture.name) != null;
                                if (!srcMat.HasMainTextureAttribute() || srcMat.mainTexture == null || isDefaultMaterial)
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
                                    if(isRebuild)
                                        shouldUseFbxMaterial = true; // FBXから変換しているなら、テクスチャのパスに関わらずFBXのマテリアルを使います。
                                    else
                                        shouldUseFbxMaterial = srcTexPath == "";
                                }
                            }

                            if (!shouldUseFbxMaterial)
                            {
                                if (!isDefaultMaterial)
                                {
#if UNITY_EDITOR
                                    try
                                    {
                                        // マテリアルが保存されていない場合は、アセットとして保存します。
                                        // Import時　(Veg等のマテリアル色変更アセット用）
                                        // Rebuild時（Rendering ToolkitのAuto Texture等のマテリアル用）
                                        var srcMatPath = AssetDatabase.GetAssetPath(srcMat);
                                        if (string.IsNullOrEmpty(srcMatPath))
                                        {
                                            var fullpath = AssetPathUtil.GetFullPath(AssetPathUtil.GetAssetPathFromRelativePath(assetPath));
                                            AssetPathUtil.CreateDirectoryIfNotExist(fullpath);
                                            var matName = srcMat.name.Replace("/", "_").Replace("\\", "_").Replace(" ", "").Replace(".", "");
                                            var matRealtivePath = AssetPathUtil.NormalizeAssetPath($"{assetPath}/{matName}.mat");

                                            var currentMat = AssetDatabase.LoadAssetAtPath<Material>(matRealtivePath); // 既に同じ名前で保存されているマテリアルがある場合は同一とみなす
                                            if (currentMat == null)
                                            {
                                                var newMat = new Material(srcMat);
                                                AssetDatabase.CreateAsset(newMat, matRealtivePath);
                                                AssetDatabase.SaveAssets();
                                                srcMat = newMat;
                                            }
                                            else
                                            {
                                                srcMat = currentMat;
                                            }
                                        }
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.LogError($"Could not save material asset for {dst.name} : {e.Message}\n{e.StackTrace}");
                                    }
#endif
                                }
                                nextMaterials[i] = srcMat;
                            }
                        }
                    }

                    renderer.sharedMaterials = nextMaterials;
                }
                return NextSearchFlow.Continue;
            });


        }

        /// <summary>
        /// FBXのマテリアル名からゲームエンジンのマテリアルIDを取得します。
        /// FBXのマテリアル名の末尾が"-(マテリアルID)"であることが前提です。
        /// 失敗時は-1を返します。
        /// </summary>
        private int FbxMaterialNameToGameMaterialId(string name)
        {
            int hyphen = name.LastIndexOf('-');
            if (hyphen < 0 || hyphen >= name.Length - 1)
            {
                Debug.LogWarning($"Could not associate material name with game material ID. name = {name}");
                return -1;
            }
            string idStr = name.Substring(hyphen + 1);
            if (string.IsNullOrEmpty(idStr))
            {
                Debug.LogWarning($"Could not associate material name with game material ID. name = {name}");
                return -1;
            }
            if (int.TryParse(idStr, out var id))
            {
                return id;
            }
            return -1;
        }
    }
}