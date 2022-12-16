



// TODO あとで消す


// using System;
// using System.Collections.Generic;
// using System.Collections.ObjectModel;
// using System.Linq;
// using PLATEAU.Dataset;
// using PLATEAU.Editor.EditorWindow.Common;
// using UnityEditor;
// using UnityEngine;
// using DataT = System.Collections.Generic.Dictionary<PLATEAU.Dataset.PredefinedCityModelPackage, PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts.AdjustPackageLodGUI.LodSliderConfig>;
//
// namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
// {
//     internal class AdjustPackageLodGUI
//     {
//         
//
//         
//         
//         public ReadOnlyDictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)> Result {
//             get
//             {
//                 var ret = new Dictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)>();
//                 foreach (var pair in this.data)
//                 {
//                     var package = pair.Key;
//                     var lodConf = pair.Value;
//                     ret.Add(package, (lodConf.UserMinLod, lodConf.UserMaxLod));
//                 }
//                 return new ReadOnlyDictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)>(ret);
//             }
//         }
//         
//         /// <summary>
//         /// 都市フィルタリング機能のLOD指定のGUIを描画します。
//         /// </summary>
//         /// <param name="packageToLodMinMax">対象の都市モデルの中で存在するパッケージとLODを渡します。</param>
//         public void Draw(CityAdjustGUI.PackageToLodMinMax packageToLodMinMax)
//         {
//             foreach (var pair in this.data.ToArray())
//             {
//                 var package = pair.Key;
//                 var lodConf = pair.Value;
//
//                 if (package == PredefinedCityModelPackage.None) continue;
//                 if (!packageToLodMinMax.Packages.Contains(package)) continue;
//                 using (new EditorGUILayout.HorizontalScope())
//                 {
//                     EditorGUILayout.LabelField(package.ToJapaneseName(), GUILayout.Width(100));
//                     if (lodConf.AvailableMaxLod == lodConf.AvailableMinLod)
//                     {
//                         EditorGUILayout.LabelField($"LOD {lodConf.AvailableMaxLod}");
//                     }
//                     else
//                     {
//                         // TODO lodConf を直接渡すほうがシンプル
//                         PlateauEditorStyle.LODSlider($"LOD {lodConf.UserMinLod}-{lodConf.UserMaxLod}", ref lodConf.UserMinLod, ref lodConf.UserMaxLod, lodConf.AvailableMinLod, lodConf.AvailableMaxLod);
//                     }
//                     
//                 }
//
//                 this.data[package] = lodConf;
//             }
//         }
//         
//         
//     }
// }
