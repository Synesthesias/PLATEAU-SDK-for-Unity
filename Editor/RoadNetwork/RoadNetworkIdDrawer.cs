using PLATEAU.RoadNetwork.Data;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{
    public class RoadNetworkIdDrawer<T> : PropertyDrawer where T : IPrimitiveData
    {
        private static readonly string typeName = typeof(T).Name.Replace("RoadNetworkData", "");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var id = property.FindPropertyRelative(RnID<T>.IdFieldName);
            var val = id.intValue;
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
    public class RnTrackIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataTrack>
    {

    }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataNode>))]
    public class RnNodeIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataNode>
    {

    }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataLink>))]
    public class RnLinkIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataLink>
    {

    }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataLineString>))]
    public class RnLineStringIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataLineString>
    {

    }

    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataPoint>))]
    public class RnPointIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataPoint>
    {

    }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataBlock>))]
    public class RnBlockIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataBlock>
    {

    }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataWay>))]
    public class RnWayIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataWay>
    {

    }


    [CustomPropertyDrawer(typeof(RnID<RoadNetworkDataLane>))]
    public class RnLaneIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataLane>
    {

    }
}