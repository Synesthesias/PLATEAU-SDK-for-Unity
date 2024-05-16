using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Util
{
    public static class PLATEAUDebugUtil
    {
        /// <summary>
        /// start -> endの方向に矢印を描画する
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="arrowSize"></param>
        /// <param name="arrowUp"></param>
        /// <param name="bodyColor"></param>
        /// <param name="arrowColor">矢印部分だけの色. nullの場合はcolorと同じ</param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawArrow(
            Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? bodyColor = null
            , Color? arrowColor = null
            , float duration = 0f
            , bool depthTest = true)
        {
            Vector3 up = arrowUp ?? Vector3.up;

            var bodyColorImpl = bodyColor ?? Color.white;

            Debug.DrawLine(start, end, bodyColorImpl, duration, depthTest);
            up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;

            var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
            var a2 = Quaternion.AngleAxis(-90, up) * a1;

            a1 = a1.normalized;
            a2 = a2.normalized;
            var arrowColorImpl = arrowColor ?? bodyColorImpl;
            Debug.DrawLine(end + a1 * arrowSize, end, arrowColorImpl);
            Debug.DrawLine(end + a2 * arrowSize, end, arrowColorImpl);
        }

        /// <summary>
        /// verticesの要素を結ぶように矢印描画を行う
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isLoop">要素の最後 -> 最初の頂点も矢印で結ぶ</param>
        /// <param name="arrowSize"></param>
        /// <param name="arrowUp"></param>
        /// <param name="color"></param>
        /// <param name="arrowColor">矢印部分だけの色. nullの場合はcolorと同じ</param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawArrows(
            IEnumerable<Vector3> vertices
            , bool isLoop = false
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , Color? arrowColor = null
            , float duration = 0f
            , bool depthTest = true
            )
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                DrawArrow(e.Item1, e.Item2, arrowSize, arrowUp, color, arrowColor, duration, depthTest);
        }

        public static void DrawLines(
            IEnumerable<Vector3> vertices
            , bool isLoop = false
            , Color? color = null
            , float duration = 0f
            , bool depthTest = true)
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                Debug.DrawLine(e.Item1, e.Item2, color ?? Color.white, duration, depthTest);
        }



        public static void DrawCenters(IEnumerable<Vector3> vertices
            , bool isLoop = false
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , float duration = 0f
            , bool depthTest = true)
        {
            var polygon = vertices.ToList();

            for (var i = 0; i < polygon.Count; i++)
            {
                var v1 = polygon[i];
                var v2 = polygon[(i + 1) % polygon.Count];

                var dir = v2 - v1;

            }
        }

        /// <summary>
        /// LineSegmentをデバッグ表示
        /// </summary>
        /// <param name="self"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        /// <param name="showXz">xz平面に書くかどうか, trueだとxy平面</param>
        /// <param name="offset"></param>
        public static void DrawLineSegment2D(LineSegment2D self, Color? color = null, float duration = 0f, bool depthTest = true, bool showXz = true, float offset = 0f)
        {
            var start = showXz ? self.Start.Xay(offset) : self.Start.Xya(offset);
            var end = showXz ? self.End.Xay(offset) : self.End.Xya(offset);
            Debug.DrawLine(start, end, color ?? Color.white, duration, depthTest);
        }

        /// <summary>
        /// デバッグ文字列を3D空間上に表示する(Editor限定)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="worldPos"></param>
        /// <param name="oX"></param>
        /// <param name="oY"></param>
        /// <param name="color"></param>
        [Conditional("UNITY_EDITOR")]
        public static void DrawString(string text, Vector3 worldPos, Vector2? screenOffset = null, Color? color = null)
        {
            // https://discussions.unity.com/t/how-to-draw-debug-text-into-scene/14023/6
#if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (color.HasValue) GUI.color = color.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }

            UnityEditor.Handles.Label(TransformByPixel(worldPos, screenOffset ?? Vector2.zero), text);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
#endif
        }

        private static Vector3 TransformByPixel(Vector3 position, Vector2 screenOffset)
        {
            return TransformByPixel(position, new Vector3(screenOffset.x, screenOffset.y));
        }

        private static Vector3 TransformByPixel(Vector3 position, Vector3 screenOffset)
        {
#if UNITY_EDITOR
            Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
            if (cam)
                return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + screenOffset);
            else
                return position;
#else
            return position;
#endif
        }
    }
}