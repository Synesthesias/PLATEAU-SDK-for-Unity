using CodiceApp.EventTracking.Plastic;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{
    public static class RnEditorUtil
    {
        // https://qiita.com/Gok/items/96e8747269bf4a2a9cc5
        /// <summary>
        /// インデントレベル設定を考慮した仕切り線.
        /// </summary>
        public static void Separator()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public static long CheckAddTarget<T>(HashSet<T> targets, long addTargetId, out bool isAdded)
            where T : ARnParts<T>
        {
            return CheckAddTarget<T, T>(targets, addTargetId, out isAdded);
        }

        public static long CheckAddTarget<T, U>(HashSet<T> targets, long addTargetId, out bool isAdded) where T : ARnParts<U>
        {
            isAdded = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                addTargetId = EditorGUILayout.LongField("AddTarget", addTargetId);
                using (new EditorGUI.DisabledScope(targets.Any(x => (long)x.DebugMyId == addTargetId)))
                {
                    isAdded = GUILayout.Button("+");
                }

                return addTargetId;
            }
        }
        public static void TargetToggle<T>(string label, HashSet<T> targets, T obj)
        {
            var isNowAdd = targets.Contains(obj);
            var isNextAdd = EditorGUILayout.Toggle(label, isNowAdd);
            if (isNowAdd == isNextAdd)
                return;

            if (isNextAdd)
                targets.Add(obj);
            else
                targets.Remove(obj);
        }
    }
}