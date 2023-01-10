using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;
using UnityEditor;

namespace PLATEAU.Editor.CityInfo
{
    [CustomEditor(typeof(PLATEAUInstancedCityModel))]
    public class PLATEAUInstancedCityModelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var icm = target as PLATEAUInstancedCityModel;
            if (icm == null) return;
            using var geoRef = icm.GeoReference;
            var geoCoord = geoRef.Unproject(new PlateauVector3d(0, 0, 0));
            
            PlateauEditorStyle.SubTitle("PLATEAU 都市モデル");
            
            PlateauEditorStyle.Heading("位置", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField($"緯度: {geoCoord.Latitude}");
                EditorGUILayout.LabelField($"経度: {geoCoord.Longitude}");
                EditorGUILayout.LabelField($"高さ: {geoCoord.Height} (東京湾海水面基準)");
            }
            
            PlateauEditorStyle.Heading("座標", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField($"座標系番号: {GeoReference.ZoneIdExplanation[geoRef.ZoneID-1]}");
                EditorGUILayout.LabelField($"基準点(メートル): {geoRef.ReferencePoint}");
            }
            
            PlateauEditorStyle.Heading("デバッグ用情報", null);
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}
