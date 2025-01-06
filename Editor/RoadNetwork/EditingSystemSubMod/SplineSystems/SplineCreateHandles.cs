using PLATEAU.Util.GeoGraph;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    internal class SplineCreateHandles
    {
        public bool IsCreatingSpline { get; private set; }

        private SplineEditorCore currentCore;
        private float fixedY = 0f;
        private ICreatedSplineReceiver finishReceiver;
        private KnotAddMethod knotAddMethod;

        public SplineCreateHandles(SplineEditorCore core, KnotAddMethod knotAddMethod, ICreatedSplineReceiver finishReceiver)
        {
            IsCreatingSpline = false;
            currentCore = core;
            this.knotAddMethod = knotAddMethod;
            this.finishReceiver = finishReceiver;
        }

        public void BeginCreateSpline(Vector3 startPoint, SplineContainer targetContainer = null)
        {
            if (IsCreatingSpline) return;

            IsCreatingSpline = true;
            fixedY = startPoint.y;

            currentCore.AddKnotAtT(startPoint, 0f);
        }

        public void HandleSceneGUI()
        {
            if (!IsCreatingSpline || currentCore == null || currentCore.Spline == null)
                return;

            Event e = Event.current;

            if ((e.type == EventType.MouseDown && e.button == 1) ||
                (e.type == EventType.KeyDown &&
                (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return)))
            {
                EndCreateSpline();
                e.Use();
                return;
            }

            HandleKnotMovement();

            if (LineUtil.IsMouseDown())
            {
                // クリックで線に点を追加します。
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                var newKnotPos = Vector3.zero;
                if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
                {
                    newKnotPos = hit.point;
                    newKnotPos.y = fixedY;
                    e.Use();
                }
                else
                {
                    Plane plane = new Plane(Vector3.up, new Vector3(0f, fixedY, 0f));
                    if (plane.Raycast(ray, out float distance))
                    {
                        newKnotPos = ray.GetPoint(distance);
                        newKnotPos.y = fixedY;
                        AddKnot(newKnotPos);
                        e.Use();
                    }
                }

                // 指定した方法で点を追加
                switch (knotAddMethod)
                {
                    case KnotAddMethod.AppendToLast:
                        AddKnot(newKnotPos);
                        break;
                    case KnotAddMethod.InsertClickPos:
                        var line = currentCore.Spline.Knots.Select(k => k.Position).Select(p => new Vector3(p.x, p.y, p.z)).ToArray();
                        float t = LineInsertIndexT(newKnotPos, line);
                        currentCore.AddKnotAtT(newKnotPos, t);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }

            DrawPreviewLines();
        }

        public void EndCreateSpline()
        {
            IsCreatingSpline = false;
            finishReceiver.OnSplineCreated(currentCore.Spline);
        }

        private void AddKnot(Vector3 pos)
        {
            currentCore.AddKnotAtT(pos, 1f);
        }

        private void HandleKnotMovement()
        {
            int knotCount = currentCore.GetKnotCount();
            for (int i = 0; i < knotCount; i++)
            {
                Vector3 knotPos = currentCore.GetKnotPosition(i);
                float size = HandleUtility.GetHandleSize(knotPos) * 0.1f;

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.Slider2D(
                    knotPos,
                    Vector3.up,
                    Vector3.right,
                    Vector3.forward,
                    size,
                    Handles.SphereHandleCap,
                    Vector2.zero
                );
                if (EditorGUI.EndChangeCheck())
                {
                    newPos.y = fixedY;
                    currentCore.MoveKnot(i, newPos);
                }
            }
        }

        private void DrawPreviewLines()
        {
            int knotCount = currentCore.GetKnotCount();
            if (knotCount < 2) return;

            Handles.color = Color.green;

            // スプライン曲線を描画
            Vector3[] points = new Vector3[100];
            for (int i = 0; i < points.Length; i++)
            {
                float t = (float)i / (points.Length - 1);
                points[i] = currentCore.EvaluateSplineAtT(t);
            }

            Handles.DrawAAPolyLine(2f, points);
        }
        
        /// <summary> lineに点pを挿入するとき、もっとも線に近いインデックスに挿入するにはどこにすべきか(0～1)を返します。 </summary>
        private float LineInsertIndexT(Vector3 p, Vector3[] line)
        {
            if (line.Length <= 1) return 0;
            float minDist = float.MaxValue;
            int nearestID = 0;
            for(int i=0; i<line.Length-1; i++)
            {
                var dist = DistanceFromPointToLineSegment(p, line[i], line[i+1]);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestID = i;
                }
            }
            float t = (0.01f + nearestID) / (line.Length - 1);
            t = Mathf.Clamp01(t);
            return t;
        }

        /// <summary> 点pから線分abまでの距離を返します。 </summary>
        private float DistanceFromPointToLineSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            // 参考 : https://qiita.com/deltaMASH/items/e7ffcca78c9b75710d09
            var ap = p - a;
            var ab = b - a;
            var ba = a - b;
            var bp = p - b;
            if (Vector3.Dot(ap, ab) < 0) return ap.magnitude;
            if (Vector3.Dot(bp, ba) < 0) return bp.magnitude;
            var aiNorm = Vector3.Dot(ap, ab) / ab.magnitude;
            var neighbor = a + ab / ab.magnitude * aiNorm;
            var dist = (p - neighbor).magnitude;
            return dist;
        }

        /// <summary> クリックで点を足すときの方法です </summary>
        internal enum KnotAddMethod
        {
            /// <summary> 線の最後に追加します </summary>
            AppendToLast,
            /// <summary> クリック位置に挿入します </summary>
            InsertClickPos
        }
    }

    /// <summary> <see cref="SplineCreateHandles"/>でスプラインの生成が完了した通知を受け取ります。 </summary>
    internal interface ICreatedSplineReceiver
    {
        public void OnSplineCreated(Spline createdSpline);
    }
}