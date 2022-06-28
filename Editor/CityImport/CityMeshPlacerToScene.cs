using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Behaviour;
using PLATEAU.CityMeta;
using UnityEditor;
using UnityEngine;
using static PLATEAU.CityMeta.ScenePlacementConfig;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// インポートされた都市の3Dモデルファイルをロードし、シーンに配置します。
    /// </summary>
    internal static class CityMeshPlacerToScene
    {
        /// <summary>
        /// <paramref name="generatedObjs"/> の3Dモデルをロードし、
        /// <paramref name="parentGameObjName"/> の子オブジェクトとしてシーンに配置します。
        /// 親ゲームオブジェクトには <see cref="CityBehaviour"/> をアタッチし、与えられた<see cref="CityMetaData"/> をリンクします。
        /// </summary>
        public static void Place(ScenePlacementConfig placementConf, List<ObjInfo> generatedObjs, string parentGameObjName, CityMetaData metaData, GmlType gmlType)
        {
            // 設定に基づいてシーンに配置すべき obj ファイルを決めます。
            var objsToPlace = new List<ObjInfo>();
            var typeConf = placementConf.PerTypeConfigs[gmlType];
            int selectedLod = typeConf.selectedLod;
            switch (typeConf.placeMethod)
            {
                case PlaceMethod.PlaceAllLod:
                {
                    foreach (var obj in generatedObjs)
                    {
                        objsToPlace.Add(new ObjInfo(obj));
                    }
                    break;
                }
                case PlaceMethod.PlaceMaxLod:
                {
                    var maxLod = generatedObjs.Max(obj => obj.Lod);
                    var maxLodObj = generatedObjs.Find(obj => obj.Lod == maxLod);
                    objsToPlace = new List<ObjInfo> { maxLodObj };
                    break;
                }
                case PlaceMethod.PlaceMinLod:
                {
                    var minLod = generatedObjs.Min(obj => obj.Lod);
                    var minLodObj = generatedObjs.Find(obj => obj.Lod == minLod);
                    objsToPlace = new List<ObjInfo> { minLodObj };
                    break;
                }
                case PlaceMethod.PlaceSelectedLodOrDoNotPlace:
                {
                    var found = generatedObjs.Find(obj => obj.Lod == selectedLod);
                    if (found == null)
                    {
                        objsToPlace.Clear();
                        break;
                    }

                    objsToPlace = new List<ObjInfo> { found };
                    break;
                }
                case PlaceMethod.PlaceSelectedLodOrMax:
                {
                    var found = generatedObjs.Find(obj => obj.Lod == selectedLod);
                    if (found == null)
                    {
                        var maxLod = generatedObjs.Max(obj => obj.Lod);
                        var maxLodObj = generatedObjs.Find(obj => obj.Lod == maxLod);
                        objsToPlace = new List<ObjInfo> { maxLodObj };
                    }
                    break;
                }
                case PlaceMethod.DoNotPlace:
                {
                    objsToPlace.Clear();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // 配置すべきものがないなら、ここでメソッド終了
            if (objsToPlace.Count <= 0) return;
            
            // 親を配置
            var parent = GameObject.Find(parentGameObjName);
            if (parent == null)
            {
                parent = new GameObject(parentGameObjName);
            }
            
            // 親に CityBehaviour をアタッチしてメタデータをリンク
            var cityBehaviour = parent.GetComponent<CityBehaviour>();
            if ( cityBehaviour == null)
            {
                cityBehaviour = parent.AddComponent<CityBehaviour>();
            }
            cityBehaviour.CityMetaData = metaData;

            
            // 配置するobjごとのループ
            foreach (var placingObj in objsToPlace)
            {
                var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(placingObj.AssetsPath);
                if (assetObj == null)
                {
                    // TODO Errorのほうがいい（ユニットテストが通るなら）
                    Debug.LogWarning($"Failed to load '.obj' file.\nobjAssetPath = {placingObj.AssetsPath}");
                    return;
                }
            
                // 古い同名の GameObject を削除
                var oldObj = FindRecursive(parent.transform, assetObj.name);
                if (oldObj != null)
                {
                    Object.DestroyImmediate(oldObj.gameObject);
                }

                // 変換後モデルの配置
                var placedObj = (GameObject)PrefabUtility.InstantiatePrefab(assetObj);
                placedObj.name = assetObj.name;
                placedObj.transform.parent = parent.transform;
            }
            
        }
        
        
        private static Transform FindRecursive(Transform target, string name)
        {
            if (target.name == name) return target;
            foreach (Transform child in target)
            {
                Transform found = FindRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}