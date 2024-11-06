using PLATEAU.RoadNetwork.Tester;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Tester
{
    [CustomEditor(typeof(PLATEAUGeoGraphTester))]
    public class PLATEAUGeoGraphTesterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUGeoGraphTester;
            if (!cog)
                return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Convert Tran"))
                cog.ConvertTrans();
        }
    }
}