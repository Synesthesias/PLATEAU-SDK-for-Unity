using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 線に沿ったメッシュを生成します。
    /// サブクラスによって実線か破線かを使い分けます。
    /// </summary>
    internal interface ILineMeshGenerator
    {
        public RoadMarkingInstance GenerateMesh(IReadOnlyList<Vector3> points);
    }
    
    
    /// <summary> 実線の道路線メッシュを作ります。 </summary>
    internal class SolidLineMeshGenerator : ILineMeshGenerator
    {
        private const float HeightOffset = 0.05f;
        private RoadMarkingMaterial materialType;
        private float lineWidth;

        public SolidLineMeshGenerator(RoadMarkingMaterial materialType, float lineWidth)
        {
            this.materialType = materialType;
            this.lineWidth = lineWidth;
        }
        
        public RoadMarkingInstance GenerateMesh(IReadOnlyList<Vector3> points)
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

                vertices[i * 2] = points[i] + right * (lineWidth * 0.5f) + Vector3.up * HeightOffset;
                vertices[i * 2 + 1] = points[i] - right * (lineWidth * 0.5f) + Vector3.up * HeightOffset;
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
            return new RoadMarkingInstance(mesh, materialType);
        }
    }

    /// <summary>
    /// 破線の道路線メッシュを作ります。
    /// </summary>
    internal class DashedLineMeshGenerator : ILineMeshGenerator
    {
        /// <summary> 破線内の1つの線の長さ </summary>
        private const float DashLength = 5f;
        private bool direction;
        private RoadMarkingMaterial materialType;
        private float lineWidth;

        public DashedLineMeshGenerator(RoadMarkingMaterial materialType, bool direction, float lineWidth)
        {
            this.materialType = materialType;
            this.direction = direction;
            this.lineWidth = lineWidth;
        }

        public RoadMarkingInstance GenerateMesh(IReadOnlyList<Vector3> srcPointsArg)
        {
            // 破線の基点を揃えるために、方向によっては逆順にします。
            Vector3[] srcPoints = direction ? srcPointsArg.ToArray() : srcPointsArg.Reverse().ToArray();
            if(srcPoints.Length <= 1) return new RoadMarkingInstance(new Mesh(), materialType);

            // 始点から点iまでの距離を計算します。
            var lengths = new float[srcPoints.Length];
            lengths[0] = 0;
            for(int i=1; i<lengths.Length; i++)
            {
                lengths[i] = lengths[i-1] + Vector3.Distance(srcPoints[i-1], srcPoints[i]);
            }
            
            float lenImComplete = 0;
            float lenLineStart = 0;
            var combines = new List<CombineInstance>();
            var gen = new SolidLineMeshGenerator(materialType, lineWidth);
            Queue<Vector3> drawQue = new Queue<Vector3>(); // これから描きたい実線部の線
            drawQue.Enqueue(srcPoints[0]);
            bool isBlank = false;
            for (int i = 1; i < srcPoints.Length; i++)
            {
                float lenDiff = Vector3.Distance(srcPoints[i], srcPoints[i - 1]);
                if (lenDiff <= 0.0000001) continue;
                lenImComplete += lenDiff;
                while (lenImComplete > DashLength) // 2点の間に、DashLengthが複数入る場合に対応するためのwhileです。
                {
                    float lenLineEnd = lenLineStart + DashLength;
                    float t = (lenLineEnd - lengths[i - 1]) / lenDiff;
                    var lineEndPos = Vector3.Lerp(srcPoints[i - 1], srcPoints[i], t);
                    if (isBlank)
                    {
                        // 空白部から実線部への切り替え時です。実線の最初の点を追加します。
                        drawQue.Enqueue(lineEndPos);
                    }
                    else
                    {
                        // 実線部から空白部への切り替え時です。実線を生成し、描画キューを空にします。
                        drawQue.Enqueue(lineEndPos);
                        var combine = gen.GenerateMesh(drawQue.ToArray()).CombineInstance;
                        combines.Add(combine);
                        drawQue.Clear();
                    }

                    isBlank = !isBlank;
                    lenLineStart = lenLineEnd;
                    lenImComplete -= DashLength;
                }

                if (!isBlank)
                {
                    drawQue.Enqueue(srcPoints[i]);
                }
                
            }
            
            var dstMesh = new Mesh();
            dstMesh.CombineMeshes(combines.ToArray());
            return new RoadMarkingInstance(dstMesh, materialType);
        }
    }

    internal class EmptyLineMeshGenerator : ILineMeshGenerator
    {
        public RoadMarkingInstance GenerateMesh(IReadOnlyList<Vector3> points)
        {
            return new RoadMarkingInstance(new Mesh(), RoadMarkingMaterial.White);
        }
    }
}