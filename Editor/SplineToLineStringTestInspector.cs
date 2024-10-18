using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

[CustomEditor(typeof(SplineToLineStringTest))]
public class SplineToLineStringTestInspector : Editor
{
    private Spline spline = new Spline();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var splineToLineStringTest = target as SplineToLineStringTest;
        if (splineToLineStringTest == null)
        {
            return;
        }

        // テスト用。適当に道路オブジェクトを取って来る
        var rn = FindObjectOfType<PLATEAURnStructureModel>();
        var road = rn.RoadNetwork.Roads.First(roadBase => roadBase.GetType() == typeof(RnRoad) && ((RnRoad)roadBase).IsValid) as RnRoad;
        var roadGroup = road.CreateRoadGroup();

        if (GUILayout.Button("Convert to Spline and Edit"))
        {
            splineToLineStringTest.CreateSpline(roadGroup);

            // スプライン編集モードにする。(参考：EditorSplineUtility.SetKnotPlacementTool())
            EditorApplication.delayCall += ToolManager.SetActiveContext<SplineToolContext>;
        }

        if (GUILayout.Button("End Edit"))
        {
            var container = splineToLineStringTest.GetComponent<SplineContainer>();
            if (container == null)
            {
                return;
            }

            // LineStringに変換
            splineToLineStringTest.ConvertSplineToLineString();

            splineToLineStringTest.ResetSpline();

            // Selection.activeGameObject = null;
        }
    }
}
