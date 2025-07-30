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
        private Spline spline;

        // 始点ノット用制約
        private bool startConstrainToLineSegment = false;
        private Vector3 startLineStart;
        private Vector3 startLineEnd;

        // 終点ノット用制約
        private bool endConstrainToLineSegment = false;
        private Vector3 endLineStart;
        private Vector3 endLineEnd;

        /// <summary> 始点・終点について、制約時のtangentの長さ </summary>
        private const float ConstraintTangentLength = 1f;

        public Spline Spline { get => spline; set => spline = value; }

        public SplineEditorCore(Spline spline)
        {
            this.Spline = spline;
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
            return Spline != null ? Spline.Count : 0;
        }

        public Vector3 GetKnotPosition(int index)
        {
            if (Spline == null || index < 0 || index >= Spline.Count)
                return Vector3.zero;
            return Spline[index].Position;
        }

        public void RemoveKnot(int index)
        {
            if (Spline == null) return;
            if (index < 0 || index >= Spline.Count) return;
            Spline.RemoveAt(index);
            UpdateTangentModes();
        }

        public void AddKnotAtT(Vector3 position, float t)
        {
            if (Spline == null) return;

            int count = Spline.Count;
            if (count == 0)
            {
                var newKnot = new BezierKnot(position, Vector3.forward, -Vector3.forward, Quaternion.identity);
                Spline.Add(newKnot);
                UpdateTangentModes();
                return;
            }

            int segmentCount = Mathf.Max(1, count - 1);
            float segmentFloat = t * segmentCount;
            int segmentIndex = Mathf.FloorToInt(segmentFloat);
            int insertIndex = Mathf.Clamp(segmentIndex + 1, 1, count);

            var newKnot2 = new BezierKnot(position, Vector3.forward, -Vector3.forward, Quaternion.identity);
            Spline.Insert(insertIndex, newKnot2);
            UpdateTangentModes();
        }

        public void MoveKnot(int index, Vector3 newPosition)
        {
            if (Spline == null) return;
            if (index < 0 || index >= Spline.Count) return;

            int knotCount = Spline.Count;

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

            var knot = Spline[index];
            knot.Position = newPosition;
            Spline.SetKnot(index, knot);
            UpdateTangentModes();
        }

        public void Reset()
        {
            Spline.Clear();
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
            if (Spline == null) return;

            int count = Spline.Count;
            if (count == 0) return;

            // 全てをAutoSmoothにしてから必要なノットを修正
            for (int i = 0; i < count; i++)
            {
                Spline.SetTangentMode(i, TangentMode.AutoSmooth);
            }

            // 始点ノット
            if (count > 0 && startConstrainToLineSegment)
            {
                Spline.SetTangentMode(0, TangentMode.Broken);
                SetKnotPerpendicularTangents(0, startLineStart, startLineEnd, ConstraintTangentLength);
            }

            // 終点ノット
            if (count > 1 && endConstrainToLineSegment)
            {
                int lastIndex = count - 1;
                Spline.SetTangentMode(lastIndex, TangentMode.Broken);
                SetKnotPerpendicularTangents(lastIndex, endLineStart, endLineEnd, ConstraintTangentLength);
            }
        }

        /// <summary>
        /// 指定ノットのタンジェントを、指定線分に対して垂直な方向に設定
        /// </summary>
        private void SetKnotPerpendicularTangents(int index, Vector3 lineStart, Vector3 lineEnd, float tangentLength)
        {
            Vector3 lineDir = (lineEnd - lineStart).normalized;
            Vector3 perp = GetPerpendicular(lineDir);
            var knot = Spline[index];
            var knotRotation = Quaternion.LookRotation(perp, Vector3.up);
            knot.Rotation.value.x = knotRotation.x;
            knot.Rotation.value.y = knotRotation.y;
            knot.Rotation.value.z = knotRotation.z;
            knot.Rotation.value.w = knotRotation.w;
            knot.TangentIn = new float3(0f, 0f, -tangentLength);
            knot.TangentOut = new float3(0f, 0f, tangentLength);
            Spline.SetKnot(index, knot);
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

        public Vector3 EvaluateSplineAtT(float t)
        {
            if (Spline == null) return Vector3.zero;
            Spline.Evaluate(t, out var position, out var _, out var _);
            return position;
        }
    }
}
