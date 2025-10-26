using PLATEAU.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.CityObject.Drawer;
using PLATEAU.RoadNetwork.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.CityObject
{
    [CustomEditor(typeof(PLATEAUSubDividedCityObjectGroup))]
    public class PLATEAUSubDividedCityObjectGroupEditor : UnityEditor.Editor
    {
        private class SubDividedCityObjectInstanceHelper : SubDividedCityObjectDebugEditorWindow.IInstanceHelper
        {
            private PLATEAUSubDividedCityObjectGroup target;

            public SubDividedCityObjectInstanceHelper(PLATEAUSubDividedCityObjectGroup target)
            {
                this.target = target;
            }

            public PLATEAUSubDividedCityObjectGroup GetCityObjects()
            {
                return target;
            }

            public bool IsTarget(SubDividedCityObject cityObject)
            {
                return RnEx.IsEditorSceneSelected(cityObject.PrimaryCityObjectGroupKey);
            }

            private PLATEAUSubDividedCityObjectDrawerDebug DebugDrawer =>
                target.GetComponent<PLATEAUSubDividedCityObjectDrawerDebug>();

            public HashSet<SubDividedCityObject> TargetCityObjects
            {
                get
                {
                    if (DebugDrawer)
                        return DebugDrawer.TargetCityObjects;
                    return new HashSet<SubDividedCityObject>();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAUSubDividedCityObjectGroup;
            if (!obj)
                return;

            base.OnInspectorGUI();

            GUILayout.Label($"ConvertedCityObjectVertexCount : {obj.CityObjects?.Sum(c => c.Meshes.Sum(m => m.Vertices.Count)) ?? 0}");

            if (GUILayout.Button("Open Editor"))
                SubDividedCityObjectDebugEditorWindow.OpenWindow(new SubDividedCityObjectInstanceHelper(obj), true);
        }
    }
}