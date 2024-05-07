using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    public class RoadNetworkIdDrawer<T> : PropertyDrawer where T : IPrimitiveData
    {
        private static readonly string typeName = typeof(T).Name.Replace("RoadNetworkData", "");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var id = property.FindPropertyRelative(RnId<IPrimitiveData>.IdFieldName);
            var val = id.intValue;
            EditorGUI.IntField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"{label.text}[{typeName}]", val);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataTrack>))]
    public class RnTrackIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataTrack>
    {

    }


    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataNode>))]
    public class RnNodeIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataNode>
    {

    }


    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataLink>))]
    public class RnLinkIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataLink>
    {

    }


    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataLineString>))]
    public class RnLineStringIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataLineString>
    {

    }

    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataPoint>))]
    public class RnPointIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataPoint>
    {

    }


    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataBlock>))]
    public class RnBlockIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataBlock>
    {

    }


    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataWay>))]
    public class RnWayIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataWay>
    {

    }


    [CustomPropertyDrawer(typeof(RnId<RoadNetworkDataLane>))]
    public class RnLaneIdDrawer : RoadNetworkIdDrawer<RoadNetworkDataLane>
    {

    }
}