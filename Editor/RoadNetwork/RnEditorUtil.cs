using System.Collections.Generic;
using UnityEditor;

namespace PLATEAU.Editor.RoadNetwork
{
    public static class RnEditorUtil
    {
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