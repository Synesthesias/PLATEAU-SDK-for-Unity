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
        public void Draw(ScenePlacementConfig placementConf)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var gmlTypes = placementConf.PerTypeConfigs.Keys;
                foreach (var gmlType in gmlTypes)
                {
                    EditorGUILayout.LabelField(gmlType.ToDisplay());
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        var typeConf = placementConf.PerTypeConfigs[gmlType];
                        DrawPerTypeConfGUI(typeConf, gmlType); 
                    }
                               
                }
            }
            
        }

        private void DrawPerTypeConfGUI(ScenePlacementConfigPerType typeConf, GmlType gmlType)
        {
            var placeMethod = typeConf.placeMethod;
            placeMethod = (ScenePlacementConfig.PlaceMethod)
                EditorGUILayout.EnumPopup("シーン配置方法", placeMethod);
            typeConf.placeMethod = placeMethod;
            if (placeMethod.DoUseSelectedLod())
            {
                var possibleLodRange = gmlType.PossibleLodRange();
                typeConf.selectedLod = EditorGUILayout.IntSlider("配置LOD", typeConf.selectedLod, possibleLodRange.Min, possibleLodRange.Max);
            }
        }
    }
}