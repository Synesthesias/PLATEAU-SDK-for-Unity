using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプライン編集のコア実装。エディタ非依存の部分のみ分離
    /// </summary>
    public class SplineEditorCore
    {
        private SplineContainer splineContainer;
        private Spline spline;

        // 始点ノット用制約
        private bool startConstrainToLineSegment = false;
        private Vector3 startLineStart;
        private Vector3 startLineEnd;

        // 終点ノット用制約
        private bool endConstrainToLineSegment = false;
        private Vector3 endLineStart;
        private Vector3 endLineEnd;

        public SplineEditorCore(SplineContainer container)
        {
            splineContainer = container;
            spline = container != null ? container.Spline : null;
        }

        /// <summary>
        /// 始点ノットに対する制約を設定
        /// enable=trueで(lineStart～lineEnd)上に投影
        /// </summary>
        public void SetStartPointConstraint(bool enable, Vector3 lineStart, Vector3 lineEnd)
        {
            startConstrainToLineSegment = enable;
            startLineStart = lineStart;
            startLineEnd = lineEnd;
            UpdateTangentModes();
        }

        /// <summary>
        /// 終点ノットに対する制約を設定
        /// enable=trueで(lineStart～lineEnd)上に投影
        /// </summary>
        public void SetEndPointConstraint(bool enable, Vector3 lineStart, Vector3 lineEnd)
        {
            endConstrainToLineSegment = enable;
            endLineStart = lineStart;
            endLineEnd = lineEnd;
            UpdateTangentModes();
        }

        public int GetKnotCount()
        {
            return spline != null ? spline.Count : 0;
        }

        public Vector3 GetKnotPosition(int index)
        {
            if (spline == null || index < 0 || index >= spline.Count)
                return Vector3.zero;
            return spline[index].Position;
        }

        public SplineContainer GetContainer()
        {
            return splineContainer;
        }

        public void RemoveKnot(int index)
        {
            if (spline == null) return;
            if (index < 0 || index >= spline.Count) return;
            spline.RemoveAt(index);
            MarkDirty();
            UpdateTangentModes();
        }

        public void AddKnotAtT(Vector3 position, float t)
        {
            if (spline == null) return;

            int count = spline.Count;
            if (count == 0)
            {
                var newKnot = new BezierKnot(position, Vector3.forward, -Vector3.forward, Quaternion.identity);
                spline.Add(newKnot);
                MarkDirty();
                UpdateTangentModes();
                return;
            }

            int segmentCount = Mathf.Max(1, count - 1);
            float segmentFloat = t * segmentCount;
            int segmentIndex = Mathf.FloorToInt(segmentFloat);
            int insertIndex = Mathf.Clamp(segmentIndex + 1, 1, count);

            var newKnot2 = new BezierKnot(position, Vector3.forward, -Vector3.forward, Quaternion.identity);
            spline.Insert(insertIndex, newKnot2);
            MarkDirty();
            UpdateTangentModes();
        }

        public void MoveKnot(int index, Vector3 newPosition)
        {
            if (spline == null) return;
            if (index < 0 || index >= spline.Count) return;

            int knotCount = spline.Count;

            // 始点制約
            if (index == 0 && startConstrainToLineSegment)
            {
                newPosition = ProjectPointOnSegment(newPosition, startLineStart, startLineEnd);
            }
            // 終点制約
            else if (index == knotCount - 1 && endConstrainToLineSegment)
            {
                newPosition = ProjectPointOnSegment(newPosition, endLineStart, endLineEnd);
            }

            var knot = spline[index];
            knot.Position = newPosition;
            spline.SetKnot(index, knot);
            MarkDirty();
            UpdateTangentModes();
        }

        private Vector3 ProjectPointOnSegment(Vector3 p, Vector3 A, Vector3 B)
        {
            Vector3 AP = p - A;
            Vector3 AB = B - A;
            float magnitudeAB = AB.sqrMagnitude;
            if (magnitudeAB == 0f) return A;
            float u = Vector3.Dot(AP, AB) / magnitudeAB;
            u = Mathf.Clamp01(u);
            return A + AB * u;
        }

        /// <summary>
        /// タンジェントモードを更新する
        /// ・端点ノットに制約が有効な場合、そのノットを線分に垂直なタンジェント
        /// ・中間ノットはAutoSmooth
        /// ・制約がない端点は全てAutoSmooth
        /// </summary>
        private void UpdateTangentModes()
        {
            if (spline == null) return;

            int count = spline.Count;
            if (count == 0) return;

            // 全てをAutoSmoothにしてから必要なノットを修正
            for (int i = 0; i < count; i++)
            {
                spline.SetTangentMode(i, TangentMode.AutoSmooth);
            }

            // 始点ノット
            if (count > 0 && startConstrainToLineSegment)
            {
                spline.SetTangentMode(0, TangentMode.Broken);
                SetKnotPerpendicularTangents(0, startLineStart, startLineEnd);
            }

            // 終点ノット
            if (count > 1 && endConstrainToLineSegment)
            {
                int lastIndex = count - 1;
                spline.SetTangentMode(lastIndex, TangentMode.Broken);
                SetKnotPerpendicularTangents(lastIndex, endLineStart, endLineEnd);
            }

            MarkDirty();
        }

        /// <summary>
        /// 指定ノットのタンジェントを、指定線分に対して垂直な方向に設定
        /// </summary>
        private void SetKnotPerpendicularTangents(int index, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDir = (lineEnd - lineStart).normalized;
            Vector3 perp = GetPerpendicular(lineDir);
            var knot = spline[index];
            var knotRotation = Quaternion.LookRotation(perp, Vector3.up);
            knot.Rotation.value.x = knotRotation.x;
            knot.Rotation.value.y = knotRotation.y;
            knot.Rotation.value.z = knotRotation.z;
            knot.Rotation.value.w = knotRotation.w;
            knot.TangentIn = new float3(0f, 0f, -20f);
            knot.TangentOut = new float3(0f, 0f, 20f);
            spline.SetKnot(index, knot);
        }

        /// <summary>
        /// dirとほぼ平行でない任意のベクトルを使い、垂直ベクトルを求める
        /// </summary>
        private Vector3 GetPerpendicular(Vector3 dir)
        {
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.9f)
                up = Vector3.right;

            Vector3 perp = Vector3.Cross(dir, up).normalized;
            return perp;
        }

        private void MarkDirty()
        {
#if UNITY_EDITOR
            if (splineContainer != null)
            {
                UnityEditor.EditorUtility.SetDirty(splineContainer);
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }
    }
}
