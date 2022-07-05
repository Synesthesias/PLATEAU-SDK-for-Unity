using System;
using System.Collections.Generic;
using PLATEAU.CityMeta;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// インポートした objファイルをシーンに配置することに関する設定GUIを描画します。
    /// <see cref="CityImportGUI"/> がこのクラスを保持します。
    /// </summary>
    internal class ScenePlacementGUI
    {
        public void Draw(CityMetaData metaData)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var importConfig = metaData.cityImportConfig;
                var placementConfig = importConfig.scenePlacementConfig;
                var gmlTypes = placementConfig.AllGmlTypes();
                var availableObjs = importConfig.generatedObjFiles;
                var typeLodDict = ObjInfo.AvailableLodInObjs(availableObjs);
                foreach (var gmlType in gmlTypes)
                {
                    var possibleLodRange = typeLodDict[gmlType];
                    if (possibleLodRange == null) continue; // その地物タイプが存在しない時
                    
                    EditorGUILayout.LabelField(gmlType.ToDisplay());
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        var typeConf = placementConfig.GetPerTypeConfig(gmlType);
                        DrawPerTypeConfGUI(typeConf, gmlType, possibleLodRange); 
                    }
                               
                }
                if (PlateauEditorStyle.MainButton("シーンにモデルを再配置"))
                {
                    // 再配置
                    string rootDirName = importConfig.rootDirName;
                    CityMeshPlacerToScene.Place(placementConfig, availableObjs, rootDirName, metaData);
                }
            }
            
        }

        /// <summary>
        /// タイプ別のシーン配置設定GUIです。
        /// </summary>
        private void DrawPerTypeConfGUI(ScenePlacementConfigPerType typeConf, GmlType gmlType, MinMax<int> possibleLodRange)
        {
            var placeMethod = typeConf.placeMethod;
            placeMethod = (ScenePlacementConfig.PlaceMethod)
                EditorGUILayout.Popup("シーン配置方法", (int)placeMethod, ScenePlacementConfig.PlaceMethodDisplay);
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
        }

        
    }
}