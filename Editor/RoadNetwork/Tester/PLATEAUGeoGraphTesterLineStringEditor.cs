using PLATEAU.RoadNetwork.Tester;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Tester
{
    [CustomEditor(typeof(PLATEAUGeoGraphTesterLineString))]
    public class PLATEAUGeoGraphTesterLineStringEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUGeoGraphTesterLineString;
            if (!cog)
                return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Reverse Vertices"))
                cog.ReverseChildrenSibling();
        }
    }
}