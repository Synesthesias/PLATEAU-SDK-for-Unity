using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                string targetObjName = $"LOD{targetLod}_{GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath)}.obj"; // TODO ハードコード
                var foundObj = SearchObjFile(objInfos, targetObjName);
                if (foundObj == null)
                {
                    Debug.LogError($"3d model file is not found.\ntargetObjName = {targetObjName}");
                    continue;
                }
                
                // ループ 2段目 : 主要地物ごと
                foreach (var primaryCityObj in primaryCityObjs)
                {

                    string targetGameObjName = GameObjName(targetLod, primaryCityObj.ID);
                    var primaryGameObj = PlaceToScene(foundObj, targetGameObjName, parentGameObj.transform);
                    if (primaryGameObj == null)
                    {
                        continue;
                    }

                    // LOD <= 1 の場合 : 主要地物を配置すれば完了となります。主要でない地物の配置をスキップします。
                    if (targetLod <= 1) continue;
                    
                    // LOD >= 2 の場合 : 子の CityObject をそれぞれ配置します。
                    var childCityObjs = primaryCityObj.CityObjectDescendantsDFS;
                    
                    // ループ 3段目 : 主要地物の子ごと
                    foreach (var childCityObj in childCityObjs)
                    {
                        // TODO 主要でないタイプは配置をスキップする機能を実装
                        // TODO 配置タイプをマスクする機能を実装
                        string childGameObjName = GameObjName(targetLod, childCityObj.ID);
                        PlaceToScene(foundObj, childGameObjName, primaryGameObj.transform);
                    } // ループ 3段目 ここまで
                    // TODO targetLod のジオメトリがなく、選択LODが無い場合に出力する設定の場合、下のLODを検索する機能を実装

                } // ループ 2段目 ここまで (主要地物ごと) 
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
                Debug.LogError($"GameObject in asset is not found.\nobjName = {objName}");
                return null;
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
    }
}