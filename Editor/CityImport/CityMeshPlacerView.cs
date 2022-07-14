using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.CommonDataStructure;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Util;
using PLATEAU.Util.CityObjectTypeExtensions;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// インポートした objファイルをシーンに配置することに関する設定GUIを描画します。
    /// <see cref="CityImporterView"/> がこのクラスを保持します。
    /// </summary>
    internal static class CityMeshPlacerView
    {
        public static void Draw(CityMeshPlacerPresenter presenter, CityMeshPlacerConfig placerConfig, IReadOnlyCollection<ObjInfo> availableObjs)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var gmlTypes = placerConfig.AllGmlTypes();
                var typeLodDict = ObjInfo.AvailableLodInObjs(availableObjs);
                foreach (var gmlType in gmlTypes)
                {
                    var possibleLodRange = typeLodDict[gmlType];
                    if (possibleLodRange == null) continue; // その地物タイプが存在しない時
                    
                    EditorGUILayout.LabelField(gmlType.ToDisplay());
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        var typeConf = placerConfig.GetPerTypeConfig(gmlType);
                        DrawPerTypeConfGUI(typeConf, possibleLodRange, gmlType); 
                    }
                               
                }
                if (PlateauEditorStyle.MainButton("シーンにモデルを再配置"))
                {
                    // 再配置
                    presenter.Place();
                }
                
            }
            
        }

        /// <summary>
        /// タイプ別のシーン配置設定GUIです。
        /// </summary>
        private static void DrawPerTypeConfGUI(ScenePlacementConfigPerType typeConf, MinMax<int> possibleLodRange, GmlType gmlType)
        {
            var placeMethod = typeConf.placeMethod;
            placeMethod = (CityMeshPlacerConfig.PlaceMethod)
                EditorGUILayout.Popup("シーン配置方法", (int)placeMethod, CityMeshPlacerConfig.PlaceMethodDisplay);
            typeConf.placeMethod = placeMethod;
            if (placeMethod.DoUseSelectedLod())
            {
                int minLod = possibleLodRange.Min;
                int maxLod = possibleLodRange.Max;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"存在するLOD");
                    EditorGUILayout.LabelField($"最小 {minLod} , 最大 {maxLod}");
                }
                
                int rangeDiff = maxLod - minLod;
                if (rangeDiff >= 1)
                {
                    // LODが複数存在するなら、スライダーによる選択GUIを表示します。
                    typeConf.selectedLod = EditorGUILayout.IntSlider("配置LOD選択", typeConf.selectedLod, minLod, maxLod);
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // 存在するobjファイルのLODが1つのみであれば、その値を表示します。
                        typeConf.selectedLod = maxLod;
                        EditorGUILayout.LabelField($"配置LOD");
                        EditorGUILayout.LabelField($"{typeConf.selectedLod}");
                    }
                    
                }
                
            }
            
            // 都市オブジェクトの種類別の配置設定
            var availableCityObjTypes = CityObjectTypeClassification.GetFlags(gmlType, possibleLodRange);
            if (availableCityObjTypes > 0)
            {
                typeConf.cityObjectTypeFlags = 
                    (ulong)((CityObjectType)availableCityObjTypes).FlagField("配置都市オブジェクトの種類", typeConf.cityObjectTypeFlags);
            }
            
            
        }

        
    }
}