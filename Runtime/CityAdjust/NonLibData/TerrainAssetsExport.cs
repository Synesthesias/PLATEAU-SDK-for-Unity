using PLATEAU.Util;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// Terrain データをPrefabに保存する際に必要な各データを保存
    /// </summary>
    internal class TerrainAssetsExport : INonLibData
    {
        private bool convertTerrains;
        private string assetPath;

        public TerrainAssetsExport(string assetPath, bool convertTerrains)
        {
            this.assetPath = assetPath;
            this.convertTerrains = convertTerrains;
        }

        public void ComposeFrom(UniqueParentTransformList src)
        {
            if (!convertTerrains) return;
#if UNITY_EDITOR
            src.BfsExec(trans =>
            {
                var terrain = trans.GetComponent<Terrain>();
                if (terrain != null)
                {
                    // Addressables生成に必要なアセットの保存

                    var terrainData = terrain.terrainData;
                    var fullpath = AssetPathUtil.GetFullPath(AssetPathUtil.GetAssetPathFromRelativePath(assetPath));
                    AssetPathUtil.CreateDirectoryIfNotExist(fullpath);

                    List<TerrainLayer> newLayers = new();
                    int layerIndex = 0;
                    foreach (var layer in terrainData.terrainLayers)
                    {
                        if (layer.diffuseTexture != null)
                        {
                            string originalTexturePath = AssetDatabase.GetAssetPath(layer.diffuseTexture);
                            string saveTexturePath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}_{layerIndex}.png"); //新規作成時のパス(temp folder)
                            if (string.IsNullOrEmpty(originalTexturePath))
                            {
                                //　DiffuseTexture保存
                                byte[] pngData = layer.diffuseTexture.EncodeToPNG();
                                File.WriteAllBytes(AssetPathUtil.GetFullPath(saveTexturePath), pngData);
                            }
                            else
                            {
                                //　既存アセットが存在する場合
                                File.Copy(AssetPathUtil.GetFullPath(originalTexturePath), AssetPathUtil.GetFullPath(saveTexturePath), overwrite: true); // ソースフォルダからtempフォルダにコピー
                            }

                            AssetDatabase.ImportAsset(saveTexturePath);
                            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(saveTexturePath);

                            string layerPath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}_layer{layerIndex}.asset");
                            var targetLayer = CanCreateAsset(layer, layerPath)
                                ? layer
                                : UnityEngine.Object.Instantiate(layer);
                            targetLayer.diffuseTexture = savedTexture;

                            // Terrain Layer 保存
                            AssetDatabase.CreateAsset(targetLayer, layerPath);
                            newLayers.Add(targetLayer);

                            layerIndex++;
                        }
                    }

                    // Terrain Data保存
                    var terrainPath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}.asset");
                    if(!CanCreateAsset(terrainData, terrainPath)) // 既存アセットがある場合
                    {
                        TerrainData newTerrainData = UnityEngine.Object.Instantiate(terrainData);
                        newTerrainData.terrainLayers = newLayers.ToArray();
                        AssetDatabase.CreateAsset(newTerrainData, terrainPath);
                        TerrainData loadedTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainPath);
                        terrain.terrainData = loadedTerrainData;
                        EditorUtility.SetDirty(terrain);
                    } 
                    else
                    {
                        terrainData.terrainLayers = newLayers.ToArray();
                        AssetDatabase.CreateAsset(terrainData, terrainPath);
                    }

                    var collider = trans.GetComponent<TerrainCollider>();
                    if(collider != null)
                    {
                        collider.terrainData = terrain.terrainData;
                    }
                }
                return NextSearchFlow.Continue;
            });
            AssetDatabase.SaveAssets();
#endif
        }

        public void RestoreTo(UniqueParentTransformList target)
        {
        }

#if UNITY_EDITOR
        bool CanCreateAsset(UnityEngine.Object obj, string path)
        {
            bool isUnregistered = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj));
            bool pathIsFree = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null;
            return isUnregistered && pathIsFree;
        }

#endif
    }
}
