using PLATEAU.RoadNetwork;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
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