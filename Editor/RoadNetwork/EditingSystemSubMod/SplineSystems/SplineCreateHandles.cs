using NUnit.Framework;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    internal struct SplineEndPoint
    {
        public Vector3 position;
        public Vector3 tangent;
    }

    internal class SplineCreateHandles
    {
        public bool IsCreatingSpline { get; private set; }
        public bool IsEndPointCreated { get; private set; } = false;

        private SplineEditorCore currentCore;
        private float fixedY = 0f;
        private ICreatedSplineReceiver finishReceiver;
        private List<SplineEndPoint> endPoints = new List<SplineEndPoint>();

        public SplineCreateHandles(SplineEditorCore core, ICreatedSplineReceiver finishReceiver)
        {
            IsCreatingSpline = false;
            currentCore = core;
            this.finishReceiver = finishReceiver;
        }

        public void SetEndPoints(List<SplineEndPoint> endPoints)
        {
            this.endPoints = endPoints;
        }

        public void BeginCreateSpline(Vector3 startPoint, Vector3 startTangent)
        {
            if (IsCreatingSpline) return;

            IsEndPointCreated = false;

            IsCreatingSpline = true;
            fixedY = startPoint.y;

            currentCore.AddKnotAtT(startPoint, 0f);
            currentCore.SetStartPointConstraint(true, startPoint, startPoint + Vector3.Cross(Vector3.up, startTangent).normalized * 0.01f);
        }

        public void HandleSceneGUI()
        {
            if (!IsCreatingSpline || currentCore == null || currentCore.Spline == null)
                return;

            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return)
                {
                    EndCreateSpline();
                    e.Use();
                    return;
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    CancelCreateSpline();
                    e.Use();
                    return;
                }
            }

            HandleKnotMovement();

            // エンドポイントのスフィア表示とクリック検出
            HandleEndPointSelection();

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
            }

            DrawPreviewLines();
        }

        public void SetEndPoint(Vector3 pos, Vector3 tangent)
        {
            AddKnot(pos);
            currentCore.SetEndPointConstraint(true, pos, pos + Vector3.Cross(Vector3.up, tangent).normalized * 0.01f);
            IsEndPointCreated = true;
        }

        public void EndCreateSpline()
        {
            IsCreatingSpline = false;
            finishReceiver.OnSplineCreated(currentCore.Spline);
        }

        public void CancelCreateSpline()
        {
            IsCreatingSpline = false;
            finishReceiver.OnSplineCreatCanceled();
        }

        private void AddKnot(Vector3 pos)
        {
            if (IsEndPointCreated) return;

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

        private void HandleEndPointSelection()
        {
            if (endPoints == null || endPoints.Count == 0)
                return;

            foreach (var ep in endPoints)
            {
                float handleSize = HandleUtility.GetHandleSize(ep.position) * 0.1f;
                if (Handles.Button(ep.position, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                {
                    SetEndPoint(ep.position, ep.tangent);
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

    /// <summary> <see cref="SplineCreateHandles"/>でスプラインの生成が完了した通知を受け取ります。 </summary>
    internal interface ICreatedSplineReceiver
    {
        void OnSplineCreatCanceled();
        void OnSplineCreated(Spline createdSpline);
    }
}