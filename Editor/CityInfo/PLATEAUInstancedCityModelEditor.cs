using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Geometries;
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
                
                // TODO 3Dモデルの原点は、東京湾海水面基準で 0m になると思っていましたが、何か違う可能性があります。
                //      SDK利用者の声で「高さが合わない。この高さでは AR Toolkit と合わない。」という声が多発のため、とりあえず高さは不明として非表示にします。
                //      参考: https://qiita.com/MR_IdTe/items/93fe776b9be0127e9c47
                //      補足: LOD1 の道路には高さ情報がないので、必ず高さは 0m となります。 LOD2 の道路は高さが反映されます。
                EditorGUILayout.LabelField($"高さ: {geoCoord.Height} (東京湾海水面基準)"); // 通常 高さ : 0m と表示されます。
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
