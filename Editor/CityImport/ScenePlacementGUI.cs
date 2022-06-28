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
                var gmlTypes = placementConf.perTypeConfigs.Keys;
                foreach (var gmlType in gmlTypes)
                {
                    EditorGUILayout.LabelField(GmlTypeConvert.ToDisplay(gmlType));
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        var typeConf = placementConf.perTypeConfigs[gmlType];
                        DrawPerTypeConfGUI(gmlType, typeConf); 
                    }
                               
                }
            }
            
        }

        private void DrawPerTypeConfGUI(GmlType gmlType, ScenePlacementConfigPerType typeConf)
        {
            var placeMethod = typeConf.placeMethod;
            placeMethod = (ScenePlacementConfig.PlaceMethod)
                EditorGUILayout.EnumPopup("シーン配置方法", placeMethod);
            typeConf.placeMethod = placeMethod;
            if (ScenePlacementConfig.DoUseSelectedLod(placeMethod))
            {
                typeConf.selectedLod = EditorGUILayout.IntSlider("配置LOD", typeConf.selectedLod, 0, 3);
            }
        }
    }
}