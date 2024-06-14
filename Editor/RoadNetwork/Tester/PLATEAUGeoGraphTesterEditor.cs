using PLATEAU.RoadNetwork;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
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