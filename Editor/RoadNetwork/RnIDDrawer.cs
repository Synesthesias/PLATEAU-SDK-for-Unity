using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
#if false
    public class RnIDDrawer<T> : PropertyDrawer where T : IPrimitiveData
    {
        private static readonly string typeName = typeof(T).Name.Replace("RoadNetworkData", "");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var id = property.FindPropertyRelative(RnID<T>.IdFieldName);
            var val = id?.intValue ?? -1;
            // 編集不可にする
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.IntField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"{label.text}[{typeName}]", val);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataTrack>))]
    public class RnTrackIDDrawer : RnIDDrawer<RoadNetworkDataTrack> { }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataNode>))]
    public class RnNodeIDDrawer : RnIDDrawer<RoadNetworkDataNode> { }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataLink>))]
    public class RnLinkIDDrawer : RnIDDrawer<RoadNetworkDataLink> { }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataLineString>))]
    public class RnLineStringIDDrawer : RnIDDrawer<RoadNetworkDataLineString> { }

    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataPoint>))]
    public class RnPointIDDrawer : RnIDDrawer<RoadNetworkDataPoint> { }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataBlock>))]
    public class RnBlockIDDrawer : RnIDDrawer<RoadNetworkDataBlock> { }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataWay>))]
    public class RnWayIDDrawer : RnIDDrawer<RoadNetworkDataWay> { }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataLane>))]
    public class RnLaneIDDrawer : RnIDDrawer<RoadNetworkDataLane> { }

#endif
}