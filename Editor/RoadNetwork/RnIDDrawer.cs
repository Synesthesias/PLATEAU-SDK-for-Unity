﻿using PLATEAU.RoadNetwork.Data;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{
#if true
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

    [CustomPropertyDrawer(typeof(RnID<RnDataRoadBase>))]
    public class RnRoadBaseIDDrawer : RnIDDrawer<RnDataRoadBase> { }

    [CustomPropertyDrawer(typeof(RnID<RnDataLineString>))]
    public class RnLineStringIDDrawer : RnIDDrawer<RnDataLineString> { }

    [CustomPropertyDrawer(typeof(RnID<RnDataPoint>))]
    public class RnPointIDDrawer : RnIDDrawer<RnDataPoint> { }


    [CustomPropertyDrawer(typeof(RnID<RnDataWay>))]
    public class RnWayIDDrawer : RnIDDrawer<RnDataWay> { }


    [CustomPropertyDrawer(typeof(RnID<RnDataLane>))]
    public class RnLaneIDDrawer : RnIDDrawer<RnDataLane> { }

    [CustomPropertyDrawer(typeof(RnID<RnDataSideWalk>))]
    public class RnSideWalkIDDrawer : RnIDDrawer<RnDataSideWalk> { }
#endif
}