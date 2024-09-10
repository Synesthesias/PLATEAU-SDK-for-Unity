using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 線に沿ったメッシュを生成します。
    /// サブクラスによって実線か破線かを使い分けます。
    /// </summary>
    public interface ILineMeshGenerator
    {
        public Mesh GenerateMesh(IReadOnlyList<Vector3> points);
    }
    
    
    /// <summary> 実線の道路線メッシュを作ります。 </summary>
    public class SolidLineMeshGenerator : ILineMeshGenerator
    {
        private const float LineWidth = 0.15f;
        private const float HeightOffset = 0.05f;
        
        public Mesh GenerateMesh(IReadOnlyList<Vector3> points)
        {
            
            if (points.Count < 2)
            {
                Debug.LogWarning("Not enough points to generate mesh.");
                return null;
            }

            var mesh = new Mesh();
            var vertices = new Vector3[points.Count * 2];
            int[] triangles = new int[(points.Count - 1) * 6];
            for (int i = 0; i < points.Count; i++)
            {
                var forward = Vector3.zero;
                if (i < points.Count - 1) forward += points[i + 1] - points[i];
                if (i > 0) forward += points[i] - points[i - 1];
                forward.Normalize();
                var right = Vector3.Cross(forward, Vector3.up).normalized;

                vertices[i * 2] = points[i] + right * LineWidth * 0.5f + Vector3.up * HeightOffset;
                vertices[i * 2 + 1] = points[i] - right * LineWidth * 0.5f + Vector3.up * HeightOffset;
                if (i < points.Count - 1)
                {
                    int baseIndex = i * 6;
                    int vertexIndex = i * 2;
                    triangles[baseIndex + 0] = vertexIndex + 0;
                    triangles[baseIndex + 1] = vertexIndex + 2;
                    triangles[baseIndex + 2] = vertexIndex + 1;
                    triangles[baseIndex + 3] = vertexIndex + 2;
                    triangles[baseIndex + 4] = vertexIndex + 3;
                    triangles[baseIndex + 5] = vertexIndex + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    /// <summary>
    /// 破線の道路線メッシュを作ります。
    /// </summary>
    public class DashedLineMeshGenerator : ILineMeshGenerator
    {
        /// <summary> 破線内の1つの線の長さ </summary>
        private const float DashLength = 5f;

        public Mesh GenerateMesh(IReadOnlyList<Vector3> srcPoints)
        {
            float length = 0f;
            bool isBlank = false;
            var gen = new SolidLineMeshGenerator();
            Queue<Vector3> drawQue = new Queue<Vector3>(); // これから描きたい実線部の線
            var combines = new List<CombineInstance>();
            for (int i = 0; i < srcPoints.Count; i++)
            {
                if (i <= 0) continue;

                float lengthIM1 = length; // srcPoints[i-1]までの長さ, I Minus 1 の略
                float srcLen = Vector3.Distance(srcPoints[i - 1], srcPoints[i]);
                float lengthI = lengthIM1 + srcLen;
                if(!isBlank) drawQue.Enqueue(srcPoints[i-1]);
                // srcPointsからなる線をDashLengthの長さで区切ります。
                do
                {
                    length += DashLength;
                    float t = (length - lengthIM1) / srcLen;
                    if (t >= 1) break;
                    var lerpPos = Vector3.Lerp(srcPoints[i - 1], srcPoints[i], t);
                    if (isBlank)
                    {
                        drawQue.Enqueue(lerpPos); // 空白部の最後 = 次の実線部の最初として追加します
                    }
                    if (!isBlank)
                    {
                        // キューから線を描画します
                        drawQue.Enqueue(lerpPos);
                        var mesh = gen.GenerateMesh(drawQue.ToArray());
                        var combine = new CombineInstance { mesh = mesh, transform = Matrix4x4.identity };
                        combines.Add(combine);
                        drawQue.Clear();
                    }

                    isBlank = !isBlank;
                } while (length < lengthI); 
                length = lengthI;
            }

            var dstMesh = new Mesh();
            dstMesh.CombineMeshes(combines.ToArray());
            return dstMesh;
        }
    }
}