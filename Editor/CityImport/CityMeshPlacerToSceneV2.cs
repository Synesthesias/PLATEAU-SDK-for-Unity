using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.CM.Common.Serialization;
using PlasticGui.Gluon.WorkspaceWindow.Views.IncomingChanges;
using PLATEAU.Behaviour;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.CityImport
{
    internal static class CityMeshPlacerToSceneV2
    {
        public static void Place(ScenePlacementConfig placeConfig, CityMetaData metaData)
        {
            string[] gmlRelativePaths = metaData.gmlRelativePaths;
            // ループ 1段目 : gmlファイルごと
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                string gmlFullPath = metaData.cityImportConfig.sourcePath.UdxRelativeToFullPath(gmlRelativePath);
                // tessellate を false にすることで、3Dモデルができない代わりにパースが高速になります。3Dモデルはインポート時のものを使います。
                var gmlParserParams = new CitygmlParserParams(true, false);
                var cityModel = CityGml.Load(gmlFullPath, gmlParserParams, DllLogCallback.UnityLogCallbacks, DllLogLevel.Error);
                if (cityModel == null)
                {
                    Debug.LogError($"failed to load city model.\ngmlFullPath = {gmlFullPath}");
                    continue;
                }
                // Debug.Log("first cityObj id = " + cityModel.RootCityObjects[0].ID);
                
                var primaryCityObjs = cityModel.GetCityObjectsByType(PrimaryCityObjectTypes.PrimaryTypeMask);
                // var primaryCityObjs = cityModel.GetCityObjectsByType(CityObjectType.COT_All);
                
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
                int targetLod = metaData.cityImportConfig.scenePlacementConfig.GetPerTypeConfig(gmlType).selectedLod;
                targetLod = 2; // TODO 仮
                
                
                // シーンへの配置先として、親GameObjectを作ります。
                string rootDirName = metaData.cityImportConfig.rootDirName;
                var parentGameObj = GameObject.Find(rootDirName);
                if (parentGameObj == null)
                {
                    parentGameObj = new GameObject(rootDirName);
                }
                
                // 親に CityBehaviour をアタッチしてメタデータをリンクします。
                var cityBehaviour = parentGameObj.GetComponent<CityBehaviour>();
                if ( cityBehaviour == null)
                {
                    cityBehaviour = parentGameObj.AddComponent<CityBehaviour>();
                }
                cityBehaviour.CityMetaData = metaData;
                
                // 対応する3Dモデルファイルを探します。
                var objInfos = metaData.cityImportConfig.generatedObjFiles;

                for (int currentLod = 0; currentLod <= targetLod; currentLod++) // TODO currentLod を 0からではなくタイプ別最小値から始める
                {
                    string targetObjName = $"LOD{currentLod}_{GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath)}.obj"; // TODO ハードコード
                    var foundObj = SearchObjFile(objInfos, targetObjName);
                    if (foundObj == null)
                    {
                        continue;
                    }
                
                    // ループ 3段目 : 主要地物ごと
                    foreach (var primaryCityObj in primaryCityObjs)
                    {

                        // この LOD で 主要地物が存在するなら、それを配置します。
                        string primaryGameObjName = GameObjName(currentLod, primaryCityObj.ID);
                        var primaryGameObj = PlaceToScene(foundObj, primaryGameObjName, parentGameObj.transform);

                        // この LOD で 主要地物が存在しないなら、シーンに配置済みの低いLODを検索します。
                        if (primaryGameObj == null)
                        {
                            for (int primaryCurrentLod = currentLod; primaryCurrentLod >= 0; primaryCurrentLod--)
                            {
                                string primaryLowerLodName = GameObjName(primaryCurrentLod, primaryCityObj.ID);
                                var primaryLowerLodTrans = FindRecursive(parentGameObj.transform, primaryLowerLodName);
                                if (primaryLowerLodTrans != null)
                                {
                                    primaryGameObj = primaryLowerLodTrans.gameObject;
                                    break;
                                }
                            }
                        }

                        // 検索してもまだ主要地物が存在しないなら、この主要地物はスキップします。
                        if (primaryGameObj == null)
                        {
                            Debug.LogError($"Primary game obj is not found.\ntargetGameObjName = {primaryGameObjName}");
                            continue;
                        }

                        // LOD <= 1 の場合 : 主要地物を配置すれば完了となります。主要でない地物の配置をスキップします。
                        if (currentLod <= 1 ) continue;
                    
                        // LOD >= 2 の場合 : 子の CityObject をそれぞれ配置します。
                        var childCityObjs = primaryCityObj.CityObjectDescendantsDFS;
                    
                        // ループ 4段目 : 主要地物の子ごと
                        foreach (var childCityObj in childCityObjs)
                        {
                            Debug.Log(childCityObj.ID);
                            // TODO 主要でないタイプは配置をスキップする機能を実装
                            // TODO 配置タイプをマスクする機能を実装
                            string childGameObjName = GameObjName(currentLod, childCityObj.ID);
                            var placedChild = PlaceToScene(foundObj, childGameObjName, primaryGameObj.transform);
                        } // ループ 4段目 ここまで

                    } // ループ 3段目 ここまで (主要地物ごと) 
                }
                
            }// ループ 1段目 ここまで (gmlファイルごと)
        }

        private static string GameObjName(int lod, string cityObjId)
        {
            return $"LOD{lod}_{cityObjId}";
        }

        private static GameObject PlaceToScene(ObjInfo objInfo, string objName, Transform parentTransform)
        {
            // 3Dモデルファイル内で、対応するメッシュを探します。
            var gameObjs = AssetDatabase.LoadAllAssetsAtPath(objInfo.assetsPath).OfType<GameObject>().ToArray();
            
            var gameObj = SearchGameObjectAsset(gameObjs, objName);
            if (gameObj == null)
            {
                return null;
            }
            
            // すでに同名のものがある場合は削除します。
            var oldObj = parentTransform.Find(objName);
            if (oldObj != null)
            {
                Object.DestroyImmediate(oldObj.gameObject);
            }
                    
            // メッシュをシーンに配置します。
            // GameObject meshGameObj = new GameObject(gameObj.name, typeof(MeshFilter), typeof(MeshRenderer));
            // meshGameObj.GetComponent<MeshFilter>().sharedMesh = g;
            var newGameObj = Object.Instantiate(gameObj, parentTransform, true);
            newGameObj.name = newGameObj.name.Replace("(Clone)", "");
            // var newGameObj = (GameObject)PrefabUtility.InstantiatePrefab(gameObj, parentTransform);
            return newGameObj;
        }

        private static ObjInfo SearchObjFile(List<ObjInfo> objInfos, string objFileName)
        {
            var foundObj = objInfos
                .FirstOrDefault(info => Path.GetFileName(info.assetsPath) == objFileName);
            return foundObj;
        }
        
        public static GameObject SearchGameObjectAsset(ICollection<GameObject> gameObjs, string gameObjName)
        {
            var foundGo = gameObjs.FirstOrDefault(go => go.name == gameObjName);
            return foundGo;
        }
        
        
        private static Transform FindRecursive(Transform parent, string searchName)
        {
            if (parent.name == searchName) return parent;
            foreach (Transform child in parent)
            {
                var found = FindRecursive(child, searchName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}