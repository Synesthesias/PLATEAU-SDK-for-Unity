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
                
                var primaryCityObjs = cityModel.GetCityObjectsByType(PrimaryCityObjectTypes.PrimaryTypeMask);
                
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
                int targetLod = metaData.cityImportConfig.scenePlacementConfig.GetPerTypeConfig(gmlType).selectedLod;
                targetLod = 2; // TODO 仮
                
                
                // シーンへの配置先として、親GameObjectを作ります。
                string rootDirName = metaData.cityImportConfig.rootDirName;
                var parentGameObj = GameObjectUtil.AssureGameObject(rootDirName);
                
                // 親に CityBehaviour をアタッチしてメタデータをリンクします。
                var cityBehaviour = GameObjectUtil.AssureComponent<CityBehaviour>(parentGameObj);
                cityBehaviour.CityMetaData = metaData;
                
                // 対応する3Dモデルファイルを探します。
                var objInfos = metaData.cityImportConfig.generatedObjFiles;

                for (int currentLod = 0; currentLod <= targetLod; currentLod++) // TODO currentLod を 0からではなくタイプ別最小値から始める
                {
                    string targetObjName = $"LOD{currentLod}_{GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath)}.obj"; // TODO ハードコード
                    var foundObj = objInfos.FirstOrDefault(info => Path.GetFileName(info.assetsPath) == targetObjName);
                    if (foundObj == null)
                    {
                        continue;
                    }
                
                    // ループ 3段目 : 主要地物ごと
                    foreach (var primaryCityObj in primaryCityObjs)
                    {

                        // この LOD で 主要地物が存在するなら、それを配置します。
                        string primaryGameObjName = GameObjNameParser.ComposeName(currentLod, primaryCityObj.ID);
                        var primaryGameObj = PlaceToScene(foundObj, primaryGameObjName, parentGameObj.transform);

                        // この LOD で 主要地物が存在しないなら、シーンに配置済みの低いLODを検索します。
                        if (primaryGameObj == null)
                        {
                            primaryGameObj =
                                SearchCityObjInScene(primaryCityObj.ID, currentLod, parentGameObj.transform);
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
                    
                        // 主要地物の子をすべて配置します。
                        PlaceCityObjs(foundObj, childCityObjs.ToArray(), primaryGameObj.transform);

                    } // ループ 3段目 ここまで (主要地物ごと) 
                }
                
            }// ループ 1段目 ここまで (gmlファイルごと)
        }

        /// <summary>
        /// シーン中の <see cref="CityObject"/> を検索します。
        /// 検索範囲は <paramref name="parentTransform"/> の子（再帰的）です。
        /// LODが <paramref name="startLod"/> のものから検索し、なければ それより低いLODを検索します。
        /// それでも見つからなければ null を返します。
        /// </summary>
        private static GameObject SearchCityObjInScene(string cityObjId, int startLod, Transform parentTransform)
        {
            GameObject foundGameObj = null;
            for (int searchingLod = startLod; searchingLod >= 0; searchingLod--)
            {
                string gameObjName = GameObjNameParser.ComposeName(searchingLod, cityObjId);
                var trans = GameObjectUtil.FindRecursive(parentTransform, gameObjName);
                if (trans != null)
                {
                    foundGameObj = trans.gameObject;
                    break;
                }
            }

            return foundGameObj;
        }

        /// <summary>
        /// PlaceToSceneの複数版です。
        /// </summary>
        private static void PlaceCityObjs(ObjInfo objInfo, ICollection<CityObject> cityObjs, Transform parent)
        {
            foreach (var cityObj in cityObjs)
            {
                string gameObjName = GameObjNameParser.ComposeName(objInfo.lod, cityObj.ID);
                PlaceToScene(objInfo, gameObjName, parent);
            }
        }

        private static GameObject PlaceToScene(ObjInfo objInfo, string objName, Transform parentTransform)
        {
            // 3Dモデルファイル内で、対応するメッシュを探します。
            var gameObjs = AssetDatabase.LoadAllAssetsAtPath(objInfo.assetsPath).OfType<GameObject>().ToArray();
            
            var gameObj =  gameObjs.FirstOrDefault(go => go.name == objName);
            if (gameObj == null)
            {
                return null;
            }
            
            // すでに同名のものがある場合は削除します。
            GameObjectUtil.DestroyChildOf(parentTransform, objName);
                    
            // メッシュをシーンに配置します。
            var newGameObj = Object.Instantiate(gameObj, parentTransform, true);
            newGameObj.name = newGameObj.name.Replace("(Clone)", "");
            return newGameObj;
        }
        

    }
}