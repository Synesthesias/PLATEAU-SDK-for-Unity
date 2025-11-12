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
        private bool convertTerrain;
        private string assetPath;

        public TerrainAssetsExport(string assetPath, bool convertTerrains)
        {
            this.assetPath = assetPath;
            this.convertTerrain = convertTerrains;
        }

        public void ComposeFrom(UniqueParentTransformList src)
        {
            if (!convertTerrain) return;
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
                                File.Copy(originalTexturePath, saveTexturePath, overwrite: true); // ソースフォルダからtempフォルダにコピー
                            }

                            AssetDatabase.ImportAsset(saveTexturePath);
                            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(saveTexturePath);
                            layer.diffuseTexture = savedTexture;

                            // Terrain Layer 保存
                            string layerPath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}_layer{layerIndex}.asset");
                            if (!CanCreateAsset(layer, layerPath)) // 既存アセットがある場合
                            {
                                TerrainLayer newTerrainLayer = UnityEngine.Object.Instantiate(layer);
                                AssetDatabase.CreateAsset(newTerrainLayer, layerPath);
                                TerrainLayer loadedTerrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
                                string texturePath2 = AssetDatabase.GetAssetPath(loadedTerrainLayer.diffuseTexture);
                                newLayers.Add(loadedTerrainLayer);
                                EditorUtility.SetDirty(layer);
                            }         
                            else
                            {
                                AssetDatabase.CreateAsset(layer, layerPath);
                                newLayers.Add(layer);
                            }

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
