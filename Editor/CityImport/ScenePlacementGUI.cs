using PLATEAU.CityMeta;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    internal class ScenePlacementGUI
    {
        public void Draw(ScenePlacementConfig placementConf)
        {
            var gmlTypes = placementConf.perTypeConfigs.Keys;
            foreach (var gmlType in gmlTypes)
            {
                var typeConf = placementConf.perTypeConfigs[gmlType];
                DrawPerTypeConfGUI(gmlType, typeConf);                
            }
        }

        private void DrawPerTypeConfGUI(GmlType gmlType, ScenePlacementConfigPerType typeConf)
        {
            typeConf.placeMethod = (ScenePlacementConfig.PlaceMethod)
                EditorGUILayout.EnumPopup("シーン配置方法", typeConf.placeMethod);
            typeConf.selectedLod = EditorGUILayout.IntSlider("配置LOD", typeConf.selectedLod, 0, 3);
        }
    }
}