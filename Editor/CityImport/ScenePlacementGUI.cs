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
        public void Draw(CityImportConfig importConfig, CityMetaData metaData)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var placementConfig = importConfig.scenePlacementConfig;
                var gmlTypes = placementConfig.AllGmlTypes();
                foreach (var gmlType in gmlTypes)
                {
                    EditorGUILayout.LabelField(gmlType.ToDisplay());
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        var typeConf = placementConfig.GetPerTypeConfig(gmlType);
                        DrawPerTypeConfGUI(typeConf, gmlType); 
                    }
                               
                }
                if (PlateauEditorStyle.MainButton("シーンにモデルを再配置"))
                {
                    var availableObjs = importConfig.generatedObjFiles;
                    string rootDirName = importConfig.rootDirName;
                    CityMeshPlacerToScene.Place(placementConfig, availableObjs, rootDirName, metaData);
                }
            }
            
        }

        private void DrawPerTypeConfGUI(ScenePlacementConfigPerType typeConf, GmlType gmlType)
        {
            var placeMethod = typeConf.placeMethod;
            placeMethod = (ScenePlacementConfig.PlaceMethod)
                EditorGUILayout.Popup("シーン配置方法", (int)placeMethod, ScenePlacementConfig.PlaceMethodDisplay);
            typeConf.placeMethod = placeMethod;
            if (placeMethod.DoUseSelectedLod())
            {
                var possibleLodRange = gmlType.PossibleLodRange();
                typeConf.selectedLod = EditorGUILayout.IntSlider("配置LOD", typeConf.selectedLod, possibleLodRange.Min, possibleLodRange.Max);
            }
        }
    }
}