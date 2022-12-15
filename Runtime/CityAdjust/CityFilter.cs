

// TODO あとで消す

// using System;
// using System.Threading.Tasks;
// using PLATEAU.CityInfo;
// using UnityEngine;
//
// namespace PLATEAU.CityAdjust
// {
//     [Obsolete]
//     public static class CityFilter
//     {
//         public static async Task<PLATEAUInstancedCityModel> FilterByFeatureTypeAsync(this PLATEAUInstancedCityModel cityModel)
//         {
//             var gmlTransforms = cityModel.GmlTransforms;
//             for (int i = 0; i < gmlTransforms.Length; i++)
//             {
//                 var gmlTrans = gmlTransforms[i];
//                 var gmlModel = await cityModel.LoadGmlAsync(gmlTrans);
//                 if (gmlModel == null)
//                 {
//                     Debug.LogError($"GMLファイルのロードに失敗しました: {gmlTrans.name}");
//                     return null;
//                 }
//                 var lods = PLATEAUInstancedCityModel.GetLods(gmlTrans);
//                 foreach (int lod in lods)
//                 {
//                     var cityObjTransforms = PLATEAUInstancedCityModel.GetCityObjects(gmlTrans, lod);
//                     foreach (var cityObjTrans in cityObjTransforms)
//                     {
//                         var cityObj = gmlModel.GetCityObjectById(cityObjTrans.name);
//                         Debug.Log(cityObj.Type);
//                     }
//                 }
//             }
//
//             return cityModel;
//         }
//     }
// }
