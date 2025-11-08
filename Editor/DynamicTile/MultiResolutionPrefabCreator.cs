using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.Editor.AdjustModel;
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
        /// 既存のファイルを上書きするかどうか
        /// </summary>
        private bool overwriteExisting;

        /// <summary>
        /// Material, Textureの保存用フォルダ生成
        /// 存在する場合は連番を付与して生成
        /// </summary>
        /// <param name="srcName"></param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public string CreateUniqueResourcePath(string srcName, int zoomLevel)
        {
            //上書きする場合はフォルダ生成しない
            if (overwriteExisting) 
                return string.Empty;

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
        public static Result CreateFromGameObject(GameObject target, string savePath, int denominator, int zoomLevel, bool overwrite = false)
        {
            var creator = new MultiResolutionPrefabCreator(savePath, overwrite);
            return creator.CreateFromGameObject(target, denominator, zoomLevel);
        }

        /// <summary>
        /// 既存のGameObjectのTextureを異なる解像度に変更して上書き
        /// 解像度、ZoomLevelを指定して生成
        /// </summary>
        public static Result OverwriteGameObject(GameObject target, int denominator, int zoomLevel)
        {
            var creator = new MultiResolutionPrefabCreator();
            return creator.CreateFromGameObject(target, denominator, zoomLevel);
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：Prefab）
        /// 解像度、ZoomLevelを指定して生成
        /// 名前はGameObject名を利用
        /// </summary>
        public static Result CreateFromPrefab(GameObject target, string savePath, int denominator, int zoomLevel, bool overwrite = false)
        {
            var creator = new MultiResolutionPrefabCreator(savePath, overwrite);
            return creator.CreateFromPrefab(target, denominator, zoomLevel);
        }

        /// <summary>
        /// 異なる解像度のTextureを持つPrefabを生成（ソース：Prefab）複数ターゲット対応版
        /// </summary>
        public static List<Result> CreateFromPrefabs(List<GameObject> prefabs, string savePath, int denominator, int zoomLevel, bool overwrite = false)
        {
            var creator = new MultiResolutionPrefabCreator(savePath, overwrite);
            return creator.CreateFromPrefabs(prefabs, denominator, zoomLevel);
        }

        /// <summary>
        /// 既存のPrefabのTextureを異なる解像度に変更して上書き
        /// 解像度、ZoomLevelを指定して生成
        /// </summary>
        public static List<Result> OverwritePrefabs(List<GameObject> prefabs, int denominator, int zoomLevel)
        {
            var creator = new MultiResolutionPrefabCreator();
            return creator.CreateFromPrefabs(prefabs, denominator, zoomLevel);
        }

        /// <summary>
        /// constructor (上書きのみ）
        /// </summary>
        public MultiResolutionPrefabCreator()
        {
            savePath = string.Empty;
            overwriteExisting = true;
            createdResults = new();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="savePath_">保存先パス</param>
        /// <param name="overwrite">上書きするかどうか</param>
        public MultiResolutionPrefabCreator(string savePath_, bool overwrite)
        {
            savePath = savePath_;
            overwriteExisting = overwrite;
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

            var materialList = new List<Material[]>();
            bool lod1Processed = false;
            // リソース保存先のディレクトリを作成
            var resourcePath = CreateUniqueResourcePath(content.name, zoomLevel);
            
            // 倍率が4分の1の場合かつ建物の場合は5面図キャプチャでLOD1テクスチャを生成し、Materialを適用する
            if (denominator is 4 && content.name.Contains("bldg"))
            {
                lod1Processed = CreateLod1TextureAndMaterialIfRequired(ref content);
            }
            
            if(!lod1Processed)
            {
                materialList = CreateMaterialList(content, resourcePath, denominator, zoomLevel);
            }
            var result = SavePrefabFromGameObject(content, materialList, zoomLevel, Path.GetFileName(resourcePath),
                savePath, lod1Processed);

            if (result != null)
                Debug.Log($"Prefab Created : {content.name}　zoomLevel{zoomLevel}");

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

            var materialList = new List<Material[]>();
            bool lod1Processed = false;

            // 倍率が4分の1の場合かつ建物の場合は5面図キャプチャでLOD1テクスチャを生成し、Materialを適用する
            if (denominator is 4 && target.name.Contains("bldg"))
            {
                //撮影の邪魔になるのでcloneを非表示
                target.SetActive(false);
                try
                {
                    lod1Processed = CreateLod1TextureAndMaterialIfRequired(ref clone);
                }
                finally
                {
                    target.SetActive(true);
                }
            }
            
            if(!lod1Processed)
            {
                materialList = CreateMaterialList(clone, resourcePath, denominator, zoomLevel);
            }

            var result = SavePrefabFromGameObject(clone, materialList, zoomLevel, Path.GetFileName(resourcePath), savePath, lod1Processed);

            if (result != null)
                Debug.Log($"Prefab Created : {target.name}　zoomLevel{zoomLevel}"); 
            
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
        private List<Material[]> CreateMaterialList(GameObject source, string saveDirectory, int denominator, int zoomLevel)
        {
            List<Material[]> materialsInRenderers = new List<Material[]>();
            var renderers = source.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogError($"No Renderer found in {source.name}");
                return materialsInRenderers;
            }
            else
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                    {
                        if (renderer.sharedMaterials.Length > 0)
                        {
                            //元のMaterial複製
                            Material[] materials = new Material[renderer.sharedMaterials.Length];
                            Array.Copy(renderer.sharedMaterials, materials, renderer.sharedMaterials.Length);

                            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                            {
                                var material = renderer.sharedMaterials[i];
                                if (material != null && material.HasMainTextureAttribute() && material.mainTexture != null && !FallbackMaterial.ByMainTextureName(material.mainTexture.name))
                                {
                                    Texture2D albedoTexture = material.mainTexture as Texture2D;
                                    if (albedoTexture != null)
                                    {
                                        // 上書きかつ等倍の場合はスキップ
                                        if (overwriteExisting && denominator == 1)
                                            continue;

                                        // Texture生成
                                        var textureResizer = new PrefabTextureResizer(saveDirectory, overwriteExisting);
                                        var newTexture = textureResizer.CreateSingleResizedTexture(albedoTexture, denominator, zoomLevel);

                                        // 上書きする場合はTexture更新のみ
                                        if(overwriteExisting)
                                            continue;

                                        // Material差替え 保存　
                                        var newMaterial = new Material(material);
                                        newMaterial.mainTexture = newTexture;

                                        var materialName = $"{newMaterial.name}_{zoomLevel}";
                                        SaveMaterialAsset(newMaterial, saveDirectory, materialName);

                                        materials[i] = newMaterial; // 変更後のマテリアルをセット    
                                    }
                                    else
                                        Debug.LogWarning($"MainTexture is null for material {material.name} in {source.name}");
                                }
                                else if (material == null)
                                    Debug.LogWarning($"Material at index {i} is null in {source.name}");
                            }
                            materialsInRenderers.Add(materials); // 各Rendererのマテリアルリストを追加
                        }
                        else
                        {
                            // Rendererにマテリアルがない場合は空の配列を追加
                            materialsInRenderers.Add(Array.Empty<Material>());
                            Debug.LogWarning($"No shared materials found in renderer for {source.name}");
                        }     
                    }
                }
            }
            return materialsInRenderers;
        }

        /// <summary>
        /// GameObjectから生成したPrefabを保存
        /// </summary>
        /// <param name="target"></param>
        /// <param name="materialList"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="prefabName"></param>
        /// <param name="saveDirectory"></param>
        /// <param name="isLod1TextureProcessed">LOD1にテクスチャを貼る処理を実行している場合はtrue</param>
        private Result SavePrefabFromGameObject(GameObject target, List<Material[]> materialList, int zoomLevel, string prefabName, string saveDirectory, bool isLod1TextureProcessed = false)
        {
            // missing script削除
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(target);

            List<Bounds> boundsList = new List<Bounds>();
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                Debug.Log($"Skipping {target.name} because there is no Renderer component.");
                return null;
            }
            else
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    if (renderer != null)
                    {
                        // マテリアルアサイン
                        if (materialList != null && i < materialList.Count && materialList[i] != null)
                            renderer.sharedMaterials = materialList[i];
                        boundsList.Add(renderer.bounds);
                    }
                }
            }

            // 保存
            var bounds = new Bounds();
            foreach (var b in boundsList)
            {
                if (bounds.size == Vector3.zero)
                    bounds = b;
                else
                    bounds.Encapsulate(b);
            }

            string uniquePath = string.Empty;
            GameObject created;
            if (overwriteExisting && !isLod1TextureProcessed)
            {
                // 上書き時は新規保存しない（ただしLOD1テクスチャの処理をしていた場合は保存する必要がある）
                created = target;
            }
            else
            {
                // prefabNameが空の場合はtarget.nameを使用（overwriteモード時にresourcePathが空になるため）
                var baseName = string.IsNullOrEmpty(prefabName) ? target.name : prefabName;
                var newName = $"{baseName}.prefab";
                var newPath = AssetPathUtil.GetFullPath(Path.Combine(saveDirectory, newName));
                uniquePath = AssetPathUtil.CreateIncrementalPathName(newPath);
                created = PrefabUtility.SaveAsPrefabAsset(target, uniquePath);
            }

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
        private Result SavePrefabFromPrefab(GameObject prefab, List<Material[]> materialList, int zoomLevel, string prefabName, string saveDirectory)
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

        private bool CreateLod1TextureAndMaterialIfRequired(ref GameObject target)
        {
            if (!target.name.Contains("bldg")) return false;

            var lod1 = target.transform.Find("LOD1");
            var lod2 = target.transform.Find("LOD2");
            if (lod1 != null && lod2 != null)
            {
                var tmpCamera = new GameObject("TempCamera").AddComponent<Camera>();
                try
                {
                    var service =
                        new Lod1TextureCaptureService(Path.Combine(FileUtil.GetProjectRelativePath(savePath),target.name), 1,
                            true);
                    service.Execute(tmpCamera, target);
                    service.SwitchLod1Visible(target, true);
                    //LOD2を削除してLOD1(Material反映済み）のみ残す
                    Object.DestroyImmediate(lod2.gameObject);
                }
                finally
                {
                    //シーンが汚れないように削除
                    Object.DestroyImmediate(tmpCamera.gameObject);
                }
                return true;
            }
            return false;
        }
    }
}