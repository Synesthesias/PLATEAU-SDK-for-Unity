using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class SplineToLineStringTest : MonoBehaviour
{
    SplineInstantiate splineInstantiate;
    SplineContainer splineContainer;
    Spline spline = new Spline();

#if UNITY_EDITOR
    internal SplineInstantiate SplineInstantiate => splineInstantiate;
    internal SplineContainer SplineContainer => splineContainer;
#endif

    void Awake()
    {
        // SplineContainerを動的生成するとたまにスプライン編集モードにならないので、Awakeで生成して常に保持しておく
        splineContainer = gameObject.GetComponent<SplineContainer>();
        splineContainer ??= gameObject.AddComponent<SplineContainer>();
        splineInstantiate = gameObject.GetComponent<SplineInstantiate>();
        splineInstantiate ??= gameObject.AddComponent<SplineInstantiate>();
        SplineInstantiate.Container = SplineContainer;
    }

    void OnDestroy()
    {
        if (!gameObject.TryGetComponent(out splineInstantiate))
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(splineInstantiate);
        }
    }

    public void CreateSpline(RnRoadGroup roadGroup)
    {
        roadGroup.TryCreateSpline(out var spline, out var width);
        List<BezierKnot> knots = new List<BezierKnot>();
        BezierKnot prevKnot = new BezierKnot();
        foreach (var knot in spline.Knots)
        {
            // 同じ座標のノットは追加しない
            if (prevKnot.Position.x == knot.Position.x &&
                prevKnot.Position.y == knot.Position.y &&
                prevKnot.Position.z == knot.Position.z)
                continue;
            knots.Add(knot);

            prevKnot = knot;
        }

        spline.Knots = knots.ToArray();

        // 一番編集しやすそうなのでモードはAutoSmoothにする。
        for (int i = 0; i < spline.Knots.Count(); i++)
        {
            spline.SetTangentMode(i, TangentMode.AutoSmooth);
        }

        SplineContainer.Splines = new Spline[] { spline };
        this.spline = spline;
    }

    public void ResetSpline()
    {
        SplineContainer.Splines = new List<Spline>();
    }

    void OnDrawGizmos()
    {
        var lineString = ConvertSplineToLineString(spline);
        for (int i = 0; i < lineString.Length - 1; i++)
        {
            Gizmos.DrawLine(lineString[i] + Vector3.right, lineString[i + 1] + Vector3.right);
            Gizmos.DrawSphere(lineString[i] + Vector3.right, 1f);
        }
    }

    public Vector3[] ConvertSplineToLineString()
    {
        return ConvertSplineToLineString(spline);
    }

    private Vector3[] ConvertSplineToLineString(Spline spline)
    {
        List<Vector3> lineString = new List<Vector3>();

        // 始点に頂点を追加
        float t = 0f;
        Vector3 prevPoint = spline.EvaluatePosition(0f);
        Vector3 prevTangent = spline.EvaluateTangent(0f);
        lineString.Add(prevPoint);

        while (t < 1f)
        {
            // 1m毎にスプライン上の点を取ってきて、10m以上離れているか20度以上角度が異なる場合に頂点として追加
            spline.GetPointAtLinearDistance(t, 1f, out float newT);
            var newPoint = spline.EvaluatePosition(newT);
            var newTangent = spline.EvaluateTangent(newT);

            if (Vector3.Distance(prevPoint, newPoint) > 30 || Vector3.Angle(prevTangent, newTangent) > 20)
            {
                lineString.Add(newPoint);
                prevPoint = newPoint;
                prevTangent = newTangent;
            }

            t = newT;

            if (t >= 1f)
            {
                // 終点に頂点を追加
                lineString.Add(spline.EvaluatePosition(1f));
            }
        }
        var knots = spline.Knots.ToArray();

        return lineString.ToArray();
    }
}
