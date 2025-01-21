using PLATEAU.RoadNetwork.Tester;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Tester
{
    [CustomEditor(typeof(PLATEAURnTesterLineString))]
    public class PLATEAURnTesterLineStringEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAURnTesterLineString;
            if (!cog)
                return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Reverse Vertices"))
                cog.ReverseChildrenSibling();
        }
    }
}