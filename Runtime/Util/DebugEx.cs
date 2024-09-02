using PLATEAU.Native;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Util
{
    public static class DebugEx
    {
        /// <summary>
        /// selfをVector3に変換するときにxzに射影するかxyに射影するか
        /// </summary>
        /// <param name="self"></param>
        /// <param name="showXz"></param>
        /// <returns></returns>
        private static Vector3 ToVec3(this Vector2 self, bool showXz)
        {
            return showXz ? self.Xay() : self.Xya();
        }

        /// <summary>
        /// num段階に分けたうちi番目の色を返す
        /// </summary>
        /// <param name="i"></param>
        /// <param name="num"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color GetDebugColor(int i, int num = 8, float a = 1f)
        {
            var h = 1f * (i % num) / num;
            var ret = Color.HSVToRGB(h, 1f, 1f);
            ret.a = a;
            return ret;
        }

        /// <summary>
        /// Debug.DrawLineのラッパー. デバッグ描画系をここに集約するため
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool depthTest = true)
        {
            Debug.DrawLine(start, end, color ?? Color.white, duration, depthTest);
        }

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

            DrawLine(start, end, bodyColorImpl, duration, depthTest);
            up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;

            var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
            var a2 = Quaternion.AngleAxis(-90, up) * a1;

            a1 = a1.normalized;
            a2 = a2.normalized;
            var arrowColorImpl = arrowColor ?? bodyColorImpl;
            DrawLine(end + a1 * arrowSize, end, arrowColorImpl, duration);
            DrawLine(end + a2 * arrowSize, end, arrowColorImpl, duration);
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
                DrawLine(e.Item1, e.Item2, color ?? Color.white, duration, depthTest);
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
        /// デバッグ文字列を3D空間上に表示する(Editor限定)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="worldPos"></param>
        /// <param name="oX"></param>
        /// <param name="oY"></param>
        /// <param name="color"></param>
        /// <param name="fontSize"></param>
        [Conditional("UNITY_EDITOR")]
        public static void DrawString(string text, Vector3 worldPos, Vector2? screenOffset = null, Color? color = null, int? fontSize = null)
        {
            // https://discussions.unity.com/t/how-to-draw-debug-text-into-scene/14023/6
#if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (color.HasValue) GUI.color = color.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            if (!view || !view.camera)
                return;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }
            var style = new GUIStyle(GUI.skin.label);
            if (fontSize != null)
                style.fontSize = fontSize.Value;
            UnityEditor.Handles.Label(TransformByPixel(worldPos, screenOffset ?? Vector2.zero), text, style);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
#endif
        }

        /// <summary>
        /// 線分のデバッグ描画
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="showXz"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawLineSegment2D(LineSegment2D segment, bool showXz = false, Color? color = null, float duration = 0f,
            bool depthTest = true)
        {
            var start = segment.Start.ToVec3(showXz);
            var end = segment.End.ToVec3(showXz);
            DebugEx.DrawArrow(start, end, bodyColor: color ?? Color.white, arrowUp: showXz ? Vector3.up : Vector3.forward, duration: duration, depthTest: depthTest);
        }

        /// <summary>
        /// 放物線のデバッグ描画
        /// </summary>
        /// <param name="parabola"></param>
        /// <param name="beginX"></param>
        /// <param name="endX"></param>
        /// <param name="splitX"></param>
        /// <param name="showXz"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawParabola2D(Parabola2D parabola, float beginX, float endX, float splitX = 0.1f,
            bool showXz = false, Color? color = null, float duration = 0f,
            bool depthTest = true)
        {
            var n = Mathf.CeilToInt((endX - beginX) / splitX);
            var d = (endX - beginX) / n;
            for (var i = 0; i < n; ++i)
            {
                var x0 = beginX + d * i;
                var x1 = x0 + d;

                var p0 = parabola.GetPoint(x0);
                var p1 = parabola.GetPoint(x1);
                DrawLine(p0.ToVec3(showXz), p1.ToVec3(showXz), color ?? Color.white, duration, depthTest);
            }
        }

        public static void DrawParabolaSegment2D(ParabolaSegment2D parabola, float deltaT = 0.1f, Color? color = null, bool showXz = false, float duration = 0f,
            bool depthTest = true)
        {
            var n = Mathf.CeilToInt((parabola.MaxT - parabola.MinT) / deltaT);
            var d = 1f * (parabola.MaxT - parabola.MinT) / n;
            for (var i = 0; i < n; ++i)
            {
                var x0 = parabola.MinT + d * i;
                var x1 = x0 + d;

                var p0 = parabola.GetPoint(x0);
                var p1 = parabola.GetPoint(x1);
                DrawLine(p0.ToVec3(showXz), p1.ToVec3(showXz), color ?? Color.white, duration, depthTest);
            }
        }

        public static void DrawBorderParabola2D(GeoGraph2D.BorderParabola2D parabola, float beginX, float endX,
            float splitX = 0.1f,
            bool showXz = false, Color? color = null, float duration = 0f,
            bool depthTest = true)
        {
            if (parabola.RangeX.HasValue)
            {
                beginX = Mathf.Clamp(beginX, -parabola.RangeX.Value, parabola.RangeX.Value);
                endX = Mathf.Clamp(endX, -parabola.RangeX.Value, parabola.RangeX.Value);
            }

            var n = Mathf.CeilToInt((endX - beginX) / splitX);
            var d = (endX - beginX) / n;
            for (var i = 0; i < n; ++i)
            {
                var x0 = beginX + d * i;
                var x1 = x0 + d;

                var p0 = parabola.GetPoint(x0);
                var p1 = parabola.GetPoint(x1);
                DrawLine(p0.ToVec3(showXz), p1.ToVec3(showXz), color ?? Color.white, duration, depthTest);
            }
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

        public static void DrawMesh(Mesh mesh, Color? color = null, float duration = 0f, bool depthTest = true)
        {
            Dictionary<int, HashSet<int>> refers = new Dictionary<int, HashSet<int>>();
            for (var i = 0; i < mesh.triangles.Length; i += 3)
            {
                var v = new[]
                {
                    mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]],
                    mesh.vertices[mesh.triangles[i + 2]]
                };
                DebugEx.DrawLines(v, true, color, duration, depthTest);
            }
        }

        public static void DrawMesh(Mesh mesh, Matrix4x4 mat, Color? color = null, float duration = 0f, bool depthTest = true)
        {
            Dictionary<int, HashSet<int>> refers = new Dictionary<int, HashSet<int>>();
            for (var i = 0; i < mesh.triangles.Length; i += 3)
            {
                var v = new[]
                {
                    mat * mesh.vertices[mesh.triangles[i]],
                    mat * mesh.vertices[mesh.triangles[i + 1]],
                    mat *mesh.vertices[mesh.triangles[i + 2]]
                }.Select(x => new Vector3(x.x, x.y, x.z));
                DebugEx.DrawLines(v, true, color, duration, depthTest);
            }
        }

        public static void DrawPlateauMesh(PolygonMesh.Mesh mesh, Matrix4x4 mat, Color? color = null,
            float duration = 0f, bool depthTest = true)
        {
            for (var i = 0; i < mesh.SubMeshCount; ++i)
            {
                var subMesh = mesh.GetSubMeshAt(i);
                for (var j = subMesh.StartIndex; j <= subMesh.EndIndex; j += 3)
                {
                    var v0 = mesh.GetVertexAt(mesh.GetIndiceAt(j));
                    var v1 = mesh.GetVertexAt(mesh.GetIndiceAt(j + 1));
                    var v2 = mesh.GetVertexAt(mesh.GetIndiceAt(j + 2));

                    var v = new[]
                    {
                        mat * v0.ToUnityVector(),
                        mat * v1.ToUnityVector(),
                        mat * v2.ToUnityVector()
                    }.Select(x => new Vector3(x.x, x.y, x.z));
                    DebugEx.DrawLines(v, true, color, duration, depthTest);
                }
            }

        }

        public static void DrawPlateauPolygonMeshNode(Matrix4x4 parentMatrix, PolygonMesh.Node node, Color? color = null,
            float duration = 0f, bool depthTest = true)
        {
            if (node == null)
                return;
            var pos = node.LocalPosition.ToUnityVector();
            var rot = node.LocalRotation.ToUnityQuaternion();
            var scale = node.LocalScale.ToUnityVector();
            var mat = parentMatrix * Matrix4x4.TRS(pos, rot, scale);
            DrawPlateauMesh(node.Mesh, mat, color);
            for (var i = 0; i < node.ChildCount; ++i)
            {
                DrawPlateauPolygonMeshNode(mat, node.GetChildAt(i), color, duration, depthTest);
            }
        }

        public static void DrawPlateauPolygonMeshModel(PLATEAU.PolygonMesh.Model model, Color? color = null, float duration = 0f, bool depthTest = true)
        {
            for (var i = 0; i < model.RootNodesCount; ++i)
            {
                var node = model.GetRootNodeAt(i);
                DrawPlateauPolygonMeshNode(Matrix4x4.identity, node, color);
            }
        }

        // https://github.com/Unity-Technologies/Graphics/pull/2287/files#diff-cc2ed84f51a3297faff7fd239fe421ca1ca75b9643a22f7808d3a274ff3252e9
        // Sphere with radius of 1
        private static readonly Vector4[] s_unitSphere = MakeUnitSphere(16);
        private static Vector4[] MakeUnitSphere(int len)
        {
            Debug.Assert(len > 2);
            var v = new Vector4[len * 3];
            for (int i = 0; i < len; i++)
            {
                var f = i / (float)len;
                float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
                v[0 * len + i] = new Vector4(c, s, 0, 1);
                v[1 * len + i] = new Vector4(0, c, s, 1);
                v[2 * len + i] = new Vector4(s, 0, c, 1);
            }
            return v;
        }
        public static void DrawSphere(Vector3 pos, float radius, Color? color = null, float duration = 0f, bool depthTest = true)
        {
            Vector4[] v = s_unitSphere;
            int len = s_unitSphere.Length / 3;
            var col = color ?? Color.white;
            Vector4 p = pos;
            p.w = 1f;
            for (int i = 0; i < len; i++)
            {
                var sX = p + radius * v[0 * len + i];
                var eX = p + radius * v[0 * len + (i + 1) % len];
                var sY = p + radius * v[1 * len + i];
                var eY = p + radius * v[1 * len + (i + 1) % len];
                var sZ = p + radius * v[2 * len + i];
                var eZ = p + radius * v[2 * len + (i + 1) % len];
                DrawLine(sX, eX, col, duration, depthTest);
                DrawLine(sY, eY, col, duration, depthTest);
                DrawLine(sZ, eZ, col, duration, depthTest);
            }
        }

        /// <summary>
        /// デバッグで破線を描画する
        /// </summary>
        /// <param name="st"></param>
        /// <param name="en"></param>
        /// <param name="color"></param>
        /// <param name="lineLength"></param>
        /// <param name="spaceLength"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawDashedLine(Vector3 st, Vector3 en, Color? color = null, float lineLength = 1f, float spaceLength = 0.2f, float duration = 0f,
            bool depthTest = true)
        {
            var len = (en - st).magnitude;

            var n = len / (lineLength + spaceLength);
            if (n <= 0f)
                return;

            var offset = 1f / n;
            var s = offset * lineLength / (lineLength + spaceLength);

            for (var t = 0f; t < 1f; t += offset)
            {
                var p0 = Vector3.Lerp(st, en, t);
                var p1 = Vector3.Lerp(st, en, Mathf.Min(1f, t + s));
                DebugEx.DrawLine(p0, p1, color, duration, depthTest);
            }
        }

        /// <summary>
        /// デバッグで破線を描画する
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isLoop"></param>
        /// <param name="color"></param>
        /// <param name="lineLength"></param>
        /// <param name="spaceLength"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawDashedLines(IEnumerable<Vector3> vertices, bool isLoop = false, Color? color = null, float lineLength = 3f, float spaceLength = 1f, float duration = 0f,
            bool depthTest = true)
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                DrawDashedLine(e.Item1, e.Item2, color, lineLength, spaceLength, duration, depthTest);
        }
    }
}