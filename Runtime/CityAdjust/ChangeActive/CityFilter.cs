using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using UnityEngine;

namespace PLATEAU.CityAdjust.ChangeActive
{
    internal static class CityFilter
    {
        public static async Task FilterByCityObjectTypeAsync(this PLATEAUInstancedCityModel cityModel,
            ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> selectionDict)
        {
            var gmlTransforms = cityModel.GmlTransforms;
            // GMLごとのループ
            foreach (var gmlTrans in gmlTransforms)
            {
                var gmlInfo = GmlFile.Create(gmlTrans.name);
                var gmlPackage = gmlInfo.Package;

                var lods = PLATEAUInstancedCityModel.GetLods(gmlTrans);
                // LODごとのループ
                foreach (int lod in lods)
                {
                    var cityObjTransforms = PLATEAUInstancedCityModel.GetCityObjects(gmlTrans, lod);
                    // 都市オブジェクトごとのループ
                    foreach (var cityObjTrans in cityObjTransforms)
                    {
                        CityGML.CityObjectType cityObjType = 0;
                        var cityObjGrp = cityObjTrans.GetComponent<PLATEAUCityObjectGroup>();

                        // PLATEAUCityObjectGroupが存在する場合、属性情報を利用してタイプ判定
                        if (cityObjGrp != null)
                        {
                            List<CityObjectList.CityObject> cityObjList = (List<CityObjectList.CityObject>)cityObjGrp.GetAllCityObjects();
                            // 最小地物のみ処理
                            if (cityObjList.Count == 1)
                                cityObjType = cityObjList.First().CityObjectType;
                        }
                        // PLATEAUCityObjectGroupが存在しない場合、Modelを利用してタイプ判定
                        else
                        {
                            // TODO GMLの中身をロードして CityObjectType を調べなくとも、パッケージ種だけで Activeにすべきか判断できる場合がある。
                            //      建築物と植生以外はパッケージ種でしか分岐しないので、ロードを省いて高速化することができるはず。
                            var gmlModel = await cityModel.LoadGmlAsync(gmlTrans);
                            if (gmlModel == null)
                            {
                                Debug.LogError($"GMLファイルのロードに失敗しました: {gmlTrans.name}");
                                return;
                            }

                            try
                            {
                                var cityObj = gmlModel.GetCityObjectById(cityObjTrans.name);
                                cityObjType = cityObj.Type;
                            }
                            catch (KeyNotFoundException e)
                            {
                                Debug.LogError(e.Message);
                            }
                        }

                        var typeNode = CityObjectTypeHierarchy.GetNodeByPackage(gmlPackage);
                        try
                        {
                            // 都市オブジェクト分類は階層構造のノードになっています。
                            // それを下位から上位へたどり、
                            // 1つでも GUI上で選択されていないものがあればそのオブジェクトは非表示、
                            // そうでなければ表示します。
                            typeNode = CityObjectTypeHierarchy.GetNodeByType(cityObjType)
                                        ?? CityObjectTypeHierarchy.GetNodeByPackage(gmlPackage);
                        }
                        catch(KeyNotFoundException e)
                        {
                            Debug.LogError(e.Message);
                        }

                        bool shouldObjEnabled = true;
                        // 分類の階層構造をたどるループ
                        while (typeNode.Parent != null)
                        {
                            if (selectionDict.TryGetValue(typeNode, out bool isSelected))
                            {
                                if (!isSelected)
                                {
                                    shouldObjEnabled = false;
                                    break;
                                }
                            }
                            else
                            {
                                Debug.LogError($"分類 {typeNode.NodeName} が GUIにありません。");
                                break;
                            }

                            typeNode = typeNode.Parent;
                        } // 分類の階層構造をたどるループ  ここまで
                        
                        cityObjTrans.gameObject.SetActive(shouldObjEnabled);
                        
                    } // 都市オブジェクトごとのループ  ここまで
                } // LODごとのループ  ここまで
                gmlInfo.Dispose();
            } // GMLごとのループ  ここまで
        }

        public static void FilterByLod(this PLATEAUInstancedCityModel cityModel,
            ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> packageToLodRangeDict)
        {
            foreach (var gmlTrans in cityModel.GmlTransforms)
            {
                var gmlInfo = GmlFile.Create(gmlTrans.name);
                var gmlPackage = gmlInfo.Package;
                if (!packageToLodRangeDict.ContainsKey(gmlPackage)) continue; 
                var lods = PLATEAUInstancedCityModel.GetLodTransforms(gmlTrans);
                foreach (var lodTrans in lods)
                {
                    
                    if (PLATEAUInstancedCityModel.TryParseLodGameObjectName(lodTrans.name, out int lodInt))
                    {
                        int min = packageToLodRangeDict[gmlPackage].minLod;
                        int max = packageToLodRangeDict[gmlPackage].maxLod;
                        bool shouldActive = min <= lodInt && lodInt <= max;
                        lodTrans.gameObject.SetActive(shouldActive);
                    }
                }
                gmlInfo.Dispose();
            }
        }
    }
}
