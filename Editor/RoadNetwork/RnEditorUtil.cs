using PLATEAU.RoadNetwork;
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

        /// <summary>
        /// 選択/非選択トグル
        /// selectedObjectsに入っていると選択/そうじゃない場合は非選択
        /// </summary>
        /// <param name="label"></param>
        /// <param name="selectedObjects"></param>
        /// <param name="obj"></param>
        public static bool SelectToggle(string label, HashSet<object> selectedObjects, object obj)
        {
            var isNowAdd = selectedObjects.Contains(obj);
            var isNextAdd = EditorGUILayout.Toggle(label, isNowAdd);
            if (isNowAdd == isNextAdd)
                return isNextAdd;

            if (isNextAdd)
                selectedObjects.Add(obj);
            else
                selectedObjects.Remove(obj);
            return isNextAdd;
        }

        /// <summary>
        /// 表示/非表示トグル
        /// inVisibleObjectsに入っていると非表示/そうじゃない場合は表示
        /// </summary>
        /// <param name="inVisibleObjects"></param>
        /// <param name="obj"></param>
        /// <param name="label"></param>
        public static bool VisibleToggle(HashSet<object> inVisibleObjects, object obj, string label = "visible")
        {
            var isNowVisible = inVisibleObjects.Contains(obj) == false;
            var isNextVisible = EditorGUILayout.Toggle(label, isNowVisible);
            if (isNowVisible == isNextVisible)
                return isNextVisible;
            if (isNextVisible)
                inVisibleObjects.Remove(obj);
            else
                inVisibleObjects.Add(obj);
            return isNextVisible;
        }

        /// <summary>
        /// foldoutObjectsに入っていると展開/そうじゃない場合は折りたたみ.
        /// obj = nullの場合はlabelをobjとして扱う
        /// </summary>
        /// <param name="label"></param>
        /// <param name="foldoutObjects"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Foldout(string label, HashSet<object> foldoutObjects, object obj = null)
        {
            // objがnullの場合はlabelをobjとして扱う
            obj ??= label;
            var isNowVisible = foldoutObjects.Contains(obj);
            var isNextVisible = EditorGUILayout.Foldout(isNowVisible, label);
            if (isNowVisible == isNextVisible)
                return isNextVisible;
            if (isNextVisible)
                foldoutObjects.Add(obj);
            else
                foldoutObjects.Remove(obj);
            return isNextVisible;
        }
    }
}