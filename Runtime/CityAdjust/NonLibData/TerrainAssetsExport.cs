using PLATEAU.Util;
using UnityEngine;
using UnityEditor;
using System.IO;

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

                    int layerIndex = 0;
                    foreach (var layer in terrainData.terrainLayers)
                    {
                        if (layer.diffuseTexture != null)
                        {
                            //　DiffuseTexture保存
                            byte[] pngData = layer.diffuseTexture.EncodeToPNG();
                            string texturePath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}_{layerIndex}.png");
                            File.WriteAllBytes(AssetPathUtil.GetFullPath(texturePath), pngData);
                            AssetDatabase.ImportAsset(texturePath);
                            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                            layer.diffuseTexture = savedTexture;

                            // Terrain Layer 保存
                            string layerPath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}_layer{layerIndex}.asset");
                            AssetDatabase.CreateAsset(layer, layerPath);
                            layerIndex++;
                        }
                    }

                    // Terrain Data保存
                    var terrainPath = AssetPathUtil.GetAssetPath($"{assetPath}/{terrain.name}.asset");
                    AssetDatabase.CreateAsset(terrainData, terrainPath);
                }
                return NextSearchFlow.Continue;
            });
            AssetDatabase.SaveAssets();
#endif
        }

        public void RestoreTo(UniqueParentTransformList target)
        {
        }
    }
}
