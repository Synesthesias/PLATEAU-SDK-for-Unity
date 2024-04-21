using PLATEAU.RoadNetwork;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{
    [CustomEditor(typeof(PLATEAURoadNetworkFactorySample))]
    public class PLATEAURoadNetworkFactorySampleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAURoadNetworkFactorySample;
            if (!cog)
                return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Create"))
                cog.CreateNetwork();
        }
    }
}