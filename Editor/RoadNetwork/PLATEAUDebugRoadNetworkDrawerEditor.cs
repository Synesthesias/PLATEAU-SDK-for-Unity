using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Packages.PlateauUnitySDK.Runtime.RoadNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.Packages.PlateauUnitySDK.Editor.RoadNetwork
{
    [CustomEditor(typeof(PLATEAUDebugRoadNetworkDrawer))]
    public class DebugRoadNetworkDrawerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUDebugRoadNetworkDrawer;
            if (cog == null) return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Create"))
                cog.CreateNetwork();
        }
    }
}