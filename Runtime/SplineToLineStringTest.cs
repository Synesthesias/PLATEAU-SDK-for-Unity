using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using PLATEAU.RoadNetwork.Structure;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class SplineToLineStringTest : MonoBehaviour
{
    SplineInstantiate m_SplineInstantiate;
    Spline spline = new Spline();

#if UNITY_EDITOR
    internal SplineInstantiate SplineInstantiate => m_SplineInstantiate;
#endif

    void Awake()
    {
        if (!gameObject.TryGetComponent(out m_SplineInstantiate))
        {
            m_SplineInstantiate = gameObject.AddComponent<SplineInstantiate>();
            // m_SplineInstantiate.hideFlags = HideFlags.HideInInspector;
        }
    }

    void OnDestroy()
    {
        if (!gameObject.TryGetComponent(out m_SplineInstantiate))
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(m_SplineInstantiate);
        }
    }

    public void CreateSpline(RnRoadGroup roadGroup)
    {
        var container = GetComponent<SplineContainer>();
        if (container == null)
        {
            container = gameObject.AddComponent<SplineContainer>();
        }
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

        container.Splines = new Spline[] { spline };
        SplineInstantiate.Container = container;
        this.spline = spline;
    }


    void OnDrawGizmos()
    {
        var lineString = ConvertSplineToLineString(spline);
        for (int i = 0; i < lineString.Length - 1; i++)
        {
            Gizmos.DrawLine(lineString[i], lineString[i + 1]);
        }
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
            // 1m毎にスプラインから頂点を取ってきて、10m以上離れているか20度以上角度が異なる場合に頂点を追加
            spline.GetPointAtLinearDistance(t, 1f, out float newT);
            var newPoint = spline.EvaluatePosition(newT);
            var newTangent = spline.EvaluateTangent(newT);

            if (Vector3.Distance(prevPoint, newPoint) > 10 || Vector3.Angle(prevTangent, newTangent) > 20)
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
