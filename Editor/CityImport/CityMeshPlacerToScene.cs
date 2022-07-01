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
        /// <paramref name="availableObjs"/> の3Dモデルをロードし、
        /// <paramref name="parentGameObjName"/> の子オブジェクトとしてシーンに配置します。
        /// 親ゲームオブジェクトには <see cref="CityBehaviour"/> をアタッチし、与えられた<see cref="CityMetaData"/> をリンクします。
        /// </summary>
        public static void Place(ScenePlacementConfig placementConf, List<ObjInfo> availableObjs, string parentGameObjName, CityMetaData metaData)
        {
            // 設定に基づいてシーンに配置すべき obj ファイルを決めます。
            var objsToPlace = new List<ObjInfo>();
            var allGmlTypes = Enum.GetValues(typeof(GmlType)).OfType<GmlType>();
            
            // 設定は gmlType ごとに行われるので、 gmlType ごとにループして シーンに配置すべきモデル( objsToPlace )を決めます。
            foreach (var gmlType in allGmlTypes)
            {
                var typeConf = placementConf.PerTypeConfigs[gmlType];
                int selectedLod = typeConf.selectedLod;
                var availableObjsOfType = availableObjs.Where(obj => obj.gmlType == gmlType).ToArray();
                switch (typeConf.placeMethod)
                {
                    case PlaceMethod.PlaceAllLod:
                    {
                        foreach (var obj in availableObjsOfType)
                        {
                            objsToPlace.Add(new ObjInfo(obj));
                        }

                        break;
                    }
                    case PlaceMethod.PlaceMaxLod:
                    {
                        var maxLod = availableObjsOfType.Max(obj => obj.lod);
                        var maxLodObj = availableObjsOfType.First(obj => obj.lod == maxLod);
                        objsToPlace.Add(maxLodObj);
                        break;
                    }
                    case PlaceMethod.PlaceSelectedLodOrDoNotPlace:
                    {
                        var found = availableObjsOfType.FirstOrDefault(obj => obj.lod == selectedLod);
                        if (found == null)
                        {
                            break;
                        }

                        objsToPlace.Add(found);
                        break;
                    }
                    case PlaceMethod.PlaceSelectedLodOrMax:
                    {
                        var found = availableObjsOfType.FirstOrDefault(obj => obj.lod == selectedLod);
                        if (found != null)
                        {
                            objsToPlace.Add(found);
                            break;
                        }
                        var maxLod = availableObjsOfType.Max(obj => obj.lod);
                        var maxLodObj = availableObjsOfType.First(obj => obj.lod == maxLod);
                        objsToPlace.Add(maxLodObj);
                        break;
                    }
                    case PlaceMethod.DoNotPlace:
                    {
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // 親を配置
            var parent = GameObject.Find(parentGameObjName);
            if (parent == null)
            {
                parent = new GameObject(parentGameObjName);
            }
            
            // 親に CityBehaviour をアタッチしてメタデータをリンクします。
            var cityBehaviour = parent.GetComponent<CityBehaviour>();
            if ( cityBehaviour == null)
            {
                cityBehaviour = parent.AddComponent<CityBehaviour>();
            }
            cityBehaviour.CityMetaData = metaData;
            
            // 親に古い子が付いているなら、子を全削除します。
            DestroyAllChild(parent.transform);
            
            // 配置するobjごとのループ
            foreach (var placingObj in objsToPlace)
            {
                var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(placingObj.assetsPath);
                if (assetObj == null)
                {
                    Debug.LogError($"Failed to load '.obj' file.\nobjAssetPath = {placingObj.assetsPath}");
                    return;
                }

                // 変換後モデルの配置
                var placedObj = (GameObject)PrefabUtility.InstantiatePrefab(assetObj);
                placedObj.name = assetObj.name;
                placedObj.transform.parent = parent.transform;
            }
        }

        private static void DestroyAllChild(Transform trans)
        {
            int numChild = trans.childCount;
            List<GameObject> objsToRemove = new List<GameObject>(); 
            for (int i = 0; i < numChild; i++)
            {
                objsToRemove.Add(trans.GetChild(i).gameObject);
            }

            foreach (var obj in objsToRemove)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}