using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using Object = UnityEngine.Object;

namespace PLATEAU.DynamicTile
{
    public class MultiResolutionPrefabCreator
    {
        /// <summary>
        /// 生成したPrefabの情報を保持する構造体
        /// </summary>
        public class Result
        {
            public GameObject Prefab;
            public Bounds Bounds;
            public string SavePath;
            public int ZoomLevel;
        }

        /// <summary>
        /// 保存先パス
        /// </summary>
        private string savePath;

        /// <summary>
        /// Material, Textureの保存用フォルダ生成
        /// 存在する場合は連番を付与して生成
        /// </summary>
        /// <param name="srcName"></param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public string CreateUniqueResourcePath(string srcName, int zoomLevel)
        {
            // 保存先のディレクトリを作成
            string directoryPath = AssetPathUtil.GetFullPath(Path.Combine(savePath, srcName) + $"_{zoomLevel}");
            return AssetPathUtil.GetAssetPath(AssetPathUtil.CreateDirectoryWithIncrementalNameIfExist(AssetPathUtil.GetFullPath(directoryPath)));
        }

        /// <summary>
        /// 生成したPrefab, Bounds, Pathのリスト
        /// </summary>
        private List<Result> createdResults;
        public List<Result> CreatedResults => createdResults;

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：GameObject）
        /// 解像度、ZoomLevelを指定して生成
        /// 名前はGameObject名を利用
        /// </summary>
        /// <param name="target">元GameObject</param>
        /// <param name="savePath">保存先パス</param>
        /// <param name="denominator">テクスチャ解像度分母 (1/2 : 2 , 1/4 : 4 )</param>
        /// <param name="zoomLevel"> 9, 10 , 11 </param>
        /// <returns></returns>
        public static Result CreateFromGameObject(GameObject target, string savePath, int denominator, int zoomLevel)
        {
            var creator = new MultiResolutionPrefabCreator(savePath);
            return creator.CreateFromGameObject(target, denominator, zoomLevel);
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：Prefab）
        /// 解像度、ZoomLevelを指定siteして生成
        /// 名前はGameObject名を利用
        /// </summary>
        public static Result CreateFromPrefab(GameObject target, string savePath, int denominator, int zoomLevel)
        {
            var creator = new MultiResolutionPrefabCreator(savePath);
            return creator.CreateFromPrefab(target, denominator, zoomLevel);
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：Prefab）複数ターゲット対応版
        /// </summary>
        public static List<Result> CreateFromPrefabs(List<GameObject> prefabs, string savePath, int denominator, int zoomLevel)
        {
            var creator = new MultiResolutionPrefabCreator(savePath);
            return creator.CreateFromPrefabs(prefabs, denominator, zoomLevel);
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="savePath_">保存先パス</param>
        public MultiResolutionPrefabCreator(string savePath_)
        {
            savePath = savePath_;
            createdResults = new();
        }

        /// <summary>
        /// 生成したResultリストをクリアする
        /// </summary>
        public void ClearResults()
        {
            createdResults.Clear();
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：Prefab）
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public Result CreateFromPrefab(GameObject prefab, int denominator, int zoomLevel)
        {
            var path = AssetDatabase.GetAssetPath(prefab);
            var content = PrefabUtility.LoadPrefabContents(path);

            // 保存先のディレクトリを作成
            string directoryPath = AssetPathUtil.GetFullPath(savePath);
            AssetPathUtil.CreateDirectoryIfNotExist(directoryPath);

            // リソース保存先のディレクトリを作成
            var resourcePath = CreateUniqueResourcePath(content.name, zoomLevel);

            var materialList = CreateMaterialList(content, resourcePath, denominator, zoomLevel);
            var result = SavePrefabFromPrefab(prefab, materialList, zoomLevel, Path.GetFileName(resourcePath), savePath);

            Debug.Log($"Prefabs Created for : {content.name}");

            //破棄
            PrefabUtility.UnloadPrefabContents(content);

            return result;
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：Prefab）複数ターゲット対応版
        /// </summary>
        public List<Result> CreateFromPrefabs(List<GameObject> prefabs, int denominator, int zoomLevel)
        {
            foreach (var targetPrefab in prefabs)
            {
                CreateFromPrefab(targetPrefab, denominator, zoomLevel);
            }

            AssetDatabase.Refresh();

            return createdResults;
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：GameObjectインスタンス）
        /// </summary>
        /// <param name="target"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public Result CreateFromGameObject(GameObject target, int denominator, int zoomLevel)
        {
            // 保存先のディレクトリを作成
            string directoryPath = AssetPathUtil.GetFullPath(savePath);
            AssetPathUtil.CreateDirectoryIfNotExist(directoryPath);

            // リソース保存先のディレクトリを作成
            var resourcePath = CreateUniqueResourcePath(target.name, zoomLevel);

            var clone = Object.Instantiate(target);
            clone.name = target.name;
            var materialList = CreateMaterialList(clone, resourcePath, denominator, zoomLevel);
            var result = SavePrefabFromGameObject(clone, materialList, zoomLevel, Path.GetFileName(resourcePath), savePath);

            Debug.Log($"Prefab Created for : {target.name}");

            //破棄
            Object.DestroyImmediate(clone);

            return result;
        }

        /// <summary>
        /// PrefabのMaterialからTextureを探し、存在する場合は解像度を変更して画像を保存しMaterialを生成
        /// 変更するマテリアルを差替えた各解像度ごとのMatrialリストを返す
        /// </summary>
        /// <param name="source">プレハブ content</param>
        /// <param name="saveDirectory">保存パス</param>
        /// <param name="denominator">分母</param>
        /// <returns>各解像度ごとのsharedMaterialsリスト</returns>
        private Material[] CreateMaterialList(GameObject source, string saveDirectory, int denominator, int zoomLevel)
        {
            Material[] materials = null; // 変更後のマテリアルを格納する配列

            var renderer = source.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                if (renderer.sharedMaterials.Length > 0)
                {
                    //元のMaterial複製
                    materials = new Material[renderer.sharedMaterials.Length];
                    Array.Copy(renderer.sharedMaterials, materials, renderer.sharedMaterials.Length);

                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        var material = renderer.sharedMaterials[i];
                        if (material != null && material.HasMainTextureAttribute())
                        {
                            Texture2D albedoTexture = material.mainTexture as Texture2D;
                            if (albedoTexture != null)
                            {
                                // Texture生成
                                var textureResizer = new PrefabTextureResizer(saveDirectory);
                                var newTexture = textureResizer.CreateSingleResizedTexture(albedoTexture, denominator, zoomLevel);

                                // Material差替え 保存　
                                var newMaterial = new Material(material);
                                newMaterial.mainTexture = newTexture;

                                var materialName = $"{newMaterial.name}_{zoomLevel}";
                                SaveMaterialAsset(newMaterial, saveDirectory, materialName);

                                materials[i] = newMaterial; // 変更後のマテリアルをセット    
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"renderer is null!!　{source.name}");
            }

            return materials;
        }

        /// <summary>
        /// GameObjectから生成したPrefabを保存
        /// </summary>
        /// <param name="target"></param>
        /// <param name="materialList"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="prefabName"></param>
        /// <param name="saveDirectory"></param>
        private Result SavePrefabFromGameObject(GameObject target, Material[] materialList, int zoomLevel, string prefabName, string saveDirectory)
        {
            // missing script削除
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(target);

            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError($"Renderer is null in {target.name}. Cannot save prefab without Renderer.");
                return null;
            }

            // マテリアルアサイン
            if (materialList != null)
                renderer.sharedMaterials = materialList;

            // 保存
            var newName = $"{prefabName}.prefab";
            var newPath = AssetPathUtil.GetFullPath(Path.Combine(saveDirectory, newName));
            var uniquePath = AssetPathUtil.CreateIncrementalPathName(newPath);
            var created = PrefabUtility.SaveAsPrefabAsset(target, uniquePath);
            var bounds = renderer == null ? default : target.GetComponentInChildren<Renderer>().bounds;

            var result = new Result() { Bounds = bounds, Prefab = created, SavePath = uniquePath, ZoomLevel = zoomLevel };
            createdResults.Add(result);
            return result;
        }

        /// <summary>
        /// Prefabから生成したPrefabを保存
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="materialList"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="prefabName"></param>
        /// <param name="saveDirectory"></param>
        private Result SavePrefabFromPrefab(GameObject prefab, Material[] materialList, int zoomLevel, string prefabName, string saveDirectory)
        {
            var path = AssetDatabase.GetAssetPath(prefab);
            var content = PrefabUtility.LoadPrefabContents(path);
            var result = SavePrefabFromGameObject(content, materialList, zoomLevel, prefabName, saveDirectory);

            //破棄
            PrefabUtility.UnloadPrefabContents(content);
            return result;
        }

        /// <summary>
        /// Material保存
        /// </summary>
        /// <param name="material">保存するMaterial</param>
        /// <param name="relativeSaveDirectory">保存先パス</param>
        /// <param name="materialName">Material名</param>
        private void SaveMaterialAsset(Material material, string relativeSaveDirectory, string materialName)
        {
            var basePath = AssetPathUtil.GetAssetPath($"{relativeSaveDirectory}/{materialName}.mat");
            AssetDatabase.CreateAsset(material, basePath);
            AssetDatabase.SaveAssets();
        }
    }
}