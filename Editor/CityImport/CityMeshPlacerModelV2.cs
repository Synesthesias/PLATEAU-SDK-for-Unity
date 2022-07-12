using System;
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
    internal static class CityMeshPlacerModelV2
    {
        public static void Place(CityMeshPlacerConfig placeConfig, CityMetadata metadata)
        {
            // Plateau元データのルートフォルダと同名の ルートGame Objectを作ります。 
            string rootDirName = metadata.cityImportConfig.rootDirName;
            var rootGameObj = GameObjectUtil.AssureGameObject(rootDirName);
            
            // ルートGameObjectに CityBehaviour をアタッチしてメタデータをリンクします。
            var cityBehaviour = GameObjectUtil.AssureComponent<CityBehaviour>(rootGameObj);
            cityBehaviour.CityMetadata = metadata;
            
            string[] gmlRelativePaths = metadata.gmlRelativePaths;
            
            // ループ 1段目 : gmlファイルごと
            foreach (var gmlRelativePath in gmlRelativePaths)
            {
                var cityModel = ParseGml(metadata, gmlRelativePath);
                if (cityModel == null) continue;
                
                // var placeConfig = metaData.cityImportConfig.scenePlacementConfig;
                var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
                var placeMethod = placeConfig.GetPerTypeConfig(gmlType).placeMethod;
                
                if (placeMethod == CityMeshPlacerConfig.PlaceMethod.DoNotPlace) continue;
                
                // gmlファイル名と同名のGameObjectをルート直下に作ります。
                var gmlGameObj =
                    GameObjectUtil.AssureGameObject(GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath));
                gmlGameObj.transform.parent = rootGameObj.transform;

                var primaryCityObjs = cityModel.GetCityObjectsByType(PrimaryCityObjectTypes.PrimaryTypeMask);

                
                // 3Dモデルファイルへの変換でのLOD範囲
                var lodRange = metadata.cityImportConfig.objConvertTypesConfig.TypeLodDict[gmlType];

                // LODを数値指定する設定なら、その指定LODを最大LODとします。
                if (placeMethod.DoUseSelectedLod())
                {
                    int selectedLod = placeConfig.GetPerTypeConfig(gmlType).selectedLod;
                    int min = lodRange.Min;
                    // int max = Math.Min(lodRange.Max, selectedLod);
                    int max = selectedLod;
                    lodRange.SetMinMax(min, max);
                }

                // 1つのLODのみを探索する設定なら、範囲を1つに狭めます。
                if (placeMethod.DoSearchOnlyOneLod())
                {
                    int max = lodRange.Max;
                    lodRange.SetMinMax(max, max);
                }


                // ループ 2段目 : LODごと
                for (int currentLod = lodRange.Max; currentLod >= lodRange.Min; currentLod--) 
                {
                    // 対応する3Dモデルファイルを探します。
                    var foundObj = FindObjFile(metadata, currentLod, gmlRelativePath);
                    if (foundObj == null) continue;

                    bool anyModelExist = false;

                    // ループ 3段目 : 主要地物ごと
                    foreach (var primaryCityObj in primaryCityObjs)
                    {
                        Transform gmlTrans = gmlGameObj.transform;
                        // この LOD で 主要地物モデルが存在するなら、それを配置します。
                        string primaryGameObjName = GameObjNameParser.ComposeName(currentLod, primaryCityObj.ID);
                        var primaryGameObj = PlaceToScene(foundObj, primaryGameObjName, gmlTrans);
                        

                        // このLODで主要地物が存在しないなら、空のGameObjectのみ用意します。
                        if (primaryGameObj == null)
                        {
                            string primaryObjName = GameObjNameParser.ComposeName(currentLod, primaryCityObj.ID);
                            // 古いGameObjectがあれば削除します。
                            var oldPrimaryTrans = GameObjectUtil.FindRecursive(gmlTrans, primaryObjName);
                            if(oldPrimaryTrans != null) Object.DestroyImmediate(oldPrimaryTrans.gameObject);
                            // 空のGameObjectを作ります。
                            primaryGameObj = new GameObject(primaryObjName);
                            primaryGameObj.transform.parent = gmlTrans;
                        }
                        else
                        {
                            anyModelExist = true;
                        }

                        // LOD <= 1 の場合 : 主要地物を配置すれば完了となります。主要でない地物の配置をスキップします。
                        // （メッシュの結合単位に関わらず、 LOD <= 1 では主要地物より細かいものは出てきません。）
                        if (currentLod <= 1 ) continue;
                    
                        // LOD >= 2 の場合 : 子の CityObject をそれぞれ配置します。
                        // （メッシュの結合単位が最小地物の場合に出てくる細かいモデルです。）
                        var childCityObjs = primaryCityObj.CityObjectDescendantsDFS;
                    
                        // 主要地物の子をすべて配置します。
                        anyModelExist |= PlaceCityObjs(foundObj, childCityObjs.ToArray(), primaryGameObj.transform);

                    } // ループ 3段目 ここまで (主要地物ごと) 
                    // ループ 2段目 (LODごと)
                    
                    if (anyModelExist && !placeMethod.DoesAllowMultipleLodPlaced())
                    {
                        // 今のLODですでにモデルが配置されており、かつ複数LODを同時に配置しない設定ならば、
                        // これ以下のLODの配置はスキップします。
                        break;
                    }
                }// ループ 2段目 ここまで (LODごと)
                
            }// ループ 1段目 ここまで (gmlファイルごと)
        }

        private static CityModel ParseGml(CityMetadata metadata, string gmlRelativePath)
        {
            string gmlFullPath = metadata.cityImportConfig.sourcePath.UdxRelativeToFullPath(gmlRelativePath);
            // tessellate を false にすることで、3Dモデルができない代わりにパースが高速になります。3Dモデルはインポート時のものを使います。
            var gmlParserParams = new CitygmlParserParams(true, false);
            var cityModel = CityGml.Load(gmlFullPath, gmlParserParams, DllLogCallback.UnityLogCallbacks);
            if (cityModel == null)
            {
                Debug.LogError($"failed to load city model.\ngmlFullPath = {gmlFullPath}");
            }
            return cityModel;
        }

        /// <summary>
        /// LOD, gml に対応する3Dモデルファイルを探します。
        /// </summary>
        private static ObjInfo FindObjFile(CityMetadata metadata, int lod, string gmlRelativePath)
        {
            var objInfos = metadata.cityImportConfig.generatedObjFiles;
            string targetObjName = $"LOD{lod}_{GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath)}.obj"; // TODO ハードコード
            return objInfos.FirstOrDefault(info => Path.GetFileName(info.assetsPath) == targetObjName);
        }
        

        /// <summary>
        /// <see cref="PlaceToScene"/> の複数版です。
        /// </summary>
        /// <returns>1つでも3Dモデルを配置したら true、そうでなければ false を返します。</returns>
        private static bool PlaceCityObjs(ObjInfo objInfo, ICollection<CityObject> cityObjs, Transform parent)
        {
            bool anyModelPlaced = false;
            foreach (var cityObj in cityObjs)
            {
                string gameObjName = GameObjNameParser.ComposeName(objInfo.lod, cityObj.ID);
                var placed = PlaceToScene(objInfo, gameObjName, parent);
                if (placed != null)
                {
                    anyModelPlaced = true;
                }
            }

            return anyModelPlaced;
        }

        /// <summary>
        /// <paramref name="objInfo"/> の3Dモデルのうち、名前が <paramref name="objName"/> であるものを探します。
        /// あればシーンに配置して、それを返します。配置のとき、すでに同名のものがある場合は削除してから配置します。
        /// なければ null を返します。
        /// </summary>
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