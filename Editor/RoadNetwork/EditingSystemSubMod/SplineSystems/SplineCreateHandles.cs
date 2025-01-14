using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    public class SplineCreateHandles
    {
        public bool IsCreatingSpline { get; private set; }

        private SplineEditorCore currentCore;
        private float fixedY = 0f;

        public SplineCreateHandles(SplineEditorCore core)
        {
            IsCreatingSpline = false;
            currentCore = core;
        }

        public void BeginCreateSpline(Vector3 startPoint, Vector3 startTangent)
        {
            if (IsCreatingSpline) return;

            IsCreatingSpline = true;
            fixedY = startPoint.y;

            currentCore.AddKnotAtT(startPoint, 0f);
            currentCore.SetStartPointConstraint(true, startPoint, startPoint + Vector3.Cross(startTangent, Vector3.up).normalized);
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

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
                {
                    Vector3 newKnotPos = hit.point;
                    newKnotPos.y = fixedY;
                    AddKnot(newKnotPos);
                    e.Use();
                }
                else
                {
                    Plane plane = new Plane(Vector3.up, new Vector3(0f, fixedY, 0f));
                    if (plane.Raycast(ray, out float distance))
                    {
                        Vector3 newKnotPos = ray.GetPoint(distance);
                        newKnotPos.y = fixedY;
                        AddKnot(newKnotPos);
                        e.Use();
                    }
                }
            }

            DrawPreviewLines();
        }

        public void EndCreateSpline()
        {
            IsCreatingSpline = false;
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
    }
}