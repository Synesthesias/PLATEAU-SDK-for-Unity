using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;


namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプライン編集のハンドル描画部分
    /// </summary>
    public static class SplineEditorHandles
    {
        private static int hoveredKnotIndex = -1;
        private static float hoverDistanceThreshold = 0.5f;

        /// <summary>
        /// ハンドルの描画を行う。OnSceneGUIから呼び出すこと
        /// </summary>
        /// <param name="core"></param>
        public static void HandleSceneGUI(SplineEditorCore core)
        {
            Event e = Event.current;

            // 1. ホバー判定
            DetermineHoveredKnot(core);

            // 2. Ctrl+クリックで削除
            if (HandleDeletion(core))
            {
                // 削除したらここで終了
                return;
            }

            // 3. 移動または追加
            if (hoveredKnotIndex != -1)
            {
                // ノットがホバーされている：このノットのみ移動可能
                HandleMovement(core);
            }
            else
            {
                // ノットホバーなし：ノットは静的表示、スプライン上クリックで追加可能
                DrawAllKnotsStatic(core);
                HandleAddition(core);
            }
        }

        private static void DetermineHoveredKnot(SplineEditorCore core)
        {
            hoveredKnotIndex = -1;
            int knotCount = core.GetKnotCount();
            if (knotCount == 0) return;

            float bestDist = float.MaxValue;
            for (int i = 0; i < knotCount; i++)
            {
                Vector3 pos = core.GetKnotPosition(i);
                float size = HandleUtility.GetHandleSize(pos) * 0.1f;
                float dist = HandleUtility.DistanceToCircle(pos, size);
                if (dist < bestDist && dist < 20f) // 適当な閾値(20fは例、必要に応じて調整)
                {
                    bestDist = dist;
                    hoveredKnotIndex = i;
                }
            }
        }

        private static bool HandleDeletion(SplineEditorCore core)
        {
            Event e = Event.current;
            if (hoveredKnotIndex >= 0 && e.type == EventType.MouseDown && e.button == 0 && e.control)
            {
                core.RemoveKnot(hoveredKnotIndex);
                e.Use();
                return true;
            }
            return false;
        }

        private static void HandleMovement(SplineEditorCore core)
        {
            int i = hoveredKnotIndex; // ホバー中ノットのみ移動
            Vector3 currentPos = core.GetKnotPosition(i);
            float baseSize = HandleUtility.GetHandleSize(currentPos) * 0.1f;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.Slider2D(
                controlID,
                currentPos,
                Vector3.up,
                Vector3.right,
                Vector3.forward,
                baseSize,
                (id, position, rotation, size, eventType) =>
                {
                    // ホバー中なので常に白＆大きく
                    Handles.color = Color.white;
                    Handles.SphereHandleCap(id, position, rotation, baseSize * 2f, eventType);
                },
                Vector2.zero
            );

            if (EditorGUI.EndChangeCheck())
            {
                newPos.y = currentPos.y;
                core.MoveKnot(i, newPos);
            }
        }

        private static void DrawAllKnotsStatic(SplineEditorCore core)
        {
            int knotCount = core.GetKnotCount();
            for (int i = 0; i < knotCount; i++)
            {
                Vector3 pos = core.GetKnotPosition(i);
                float size = HandleUtility.GetHandleSize(pos) * 0.1f;
                Handles.color = Color.cyan;
                Handles.SphereHandleCap(0, pos, Quaternion.identity, size, EventType.Repaint);
            }
        }

        private static void HandleAddition(SplineEditorCore core)
        {
            Event e = Event.current;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            float outT;
            Vector3 nearestPoint = GetNearestPointOnSpline(core, ray, out outT, 50);
            Vector3 closestOnRay = ClosestPointOnRay(ray, nearestPoint);
            float dist = Vector3.Distance(closestOnRay, nearestPoint);

            if (dist < hoverDistanceThreshold)
            {
                Handles.color = new Color(0, 1, 0, 0.3f);
                float size = HandleUtility.GetHandleSize(nearestPoint) * 0.15f;
                Handles.SphereHandleCap(0, nearestPoint, Quaternion.identity, size, EventType.Repaint);

                if (e.type == EventType.MouseDown && e.button == 0 && !e.control)
                {
                    core.AddKnotAtT(nearestPoint, outT);
                    // e.Use()しない → Slider2Dないので問題なし
                }
            }
        }

        private static Vector3 GetNearestPointOnSpline(SplineEditorCore core, Ray ray, out float outT, int sampleCount = 50)
        {
            var spline = core.Spline;

            if (spline == null || spline.Count == 0)
            {
                outT = 0f;
                return ray.origin + ray.direction * 10f;
            }

            float bestDist = float.MaxValue;
            Vector3 bestPoint = Vector3.zero;
            float bestT = 0f;
            for (int i = 0; i <= sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                Vector3 pt = SplineUtility.EvaluatePosition(spline, t);
                Vector3 closestOnRay = ClosestPointOnRay(ray, pt);
                float dist = Vector3.Distance(pt, closestOnRay);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPoint = pt;
                    bestT = t;
                }
            }

            outT = bestT;
            return bestPoint;
        }

        private static Vector3 ClosestPointOnRay(Ray ray, Vector3 point)
        {
            float t = Vector3.Dot(point - ray.origin, ray.direction);
            if (t < 0f) t = 0f;
            return ray.origin + ray.direction * t;
        }
    }
}
