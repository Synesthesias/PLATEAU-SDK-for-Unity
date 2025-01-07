using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    
    
    /// <summary>
    /// 道路のレーン編集（交差点編集時の線も含む）のために、線1つをシーン上に描画します。
    /// 線の種類をサブクラスで書き分けます。
    /// </summary>
    public interface ILaneLineDrawer
    {
        protected const float HeightOffset = 0.2f; // 線が、道路標示のポリゴンなどにめり込まない程度の高さです。
        void Draw();
    }
    
    /// <summary>
    /// <see cref="ILaneLineDrawer"/>の実線版です。
    /// </summary>
    internal class LaneLineDrawerSolid : ILaneLineDrawer
    {
        private List<Vector3> line;
        private Color color;
        private LaneLineDrawMethod method;

        public LaneLineDrawerSolid(List<Vector3> line, Color color, LaneLineDrawMethod method)
        {
            this.line = line;
            this.color = color;
            this.method = method;
        }

        public void Draw()
        {
#if UNITY_EDITOR
            if (line == null) return;
            if (line.Count < 2) return;
            var prevColor = method switch
            {
                LaneLineDrawMethod.Gizmos => Gizmos.color,
                LaneLineDrawMethod.Handles => Handles.color,
                _ => throw new ArgumentOutOfRangeException()
            };
            var positions = line.Select(p => p + Vector3.up * ILaneLineDrawer.HeightOffset).ToArray();

            switch (method)
            {
                case LaneLineDrawMethod.Gizmos:
                    Gizmos.color = color;
                    Gizmos.DrawLineStrip(positions,false);
                    Gizmos.color = prevColor;
                    break;
                case LaneLineDrawMethod.Handles:
                    Handles.color = color;
                    Handles.DrawPolyLine(positions);
                    Handles.color = prevColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
#endif
        }
    }

    
    /// <summary>
    /// <see cref="ILaneLineDrawer"/>の矢印がリピートする版です。
    /// </summary>
    internal class LaneLineDrawerArrow : ILaneLineDrawer
    {
        private RnWay way;
        private Color color;
        private LaneLineDrawMethod method;
        private float oneSpaceLength;
        public LaneLineDrawerArrow(RnWay way, Color color, LaneLineDrawMethod method, float oneSpaceLength = 1f)
        {
            this.way = way;
            this.color = color;
            this.method = method;
            this.oneSpaceLength = oneSpaceLength;
        }

        public void Draw()
        {
#if UNITY_EDITOR
            const float OneArrowLength = 3f;
            var prevColor = method switch
            {
                LaneLineDrawMethod.Gizmos => Gizmos.color,
                LaneLineDrawMethod.Handles => Handles.color
            };
            
            switch (method)
            {
                case LaneLineDrawMethod.Gizmos:
                    Gizmos.color = color;
                    break;
                case LaneLineDrawMethod.Handles:
                    Handles.color = color;
                    break;
            }
            DrawDashedArrows(way.Select(v => v.PutY(v.y + ILaneLineDrawer.HeightOffset)), method, false, OneArrowLength, oneSpaceLength);
            switch (method)
            {
                case LaneLineDrawMethod.Gizmos:
                    Gizmos.color = prevColor;
                    break;
                case LaneLineDrawMethod.Handles:
                    Handles.color = prevColor;
                    break;
            }


#endif
        }

        private static void DrawDashedArrows(IEnumerable<Vector3> vertices, LaneLineDrawMethod method, bool isLoop = false, float lineLength = 3f, float spaceLength = 1f)
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                DrawDashedArrow(e.Item1, e.Item2, method, lineLength, spaceLength);
        }


        private static void DrawArrow(
              Vector3 start
            , Vector3 end
            , LaneLineDrawMethod method
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null)
        {
            Vector3 up = arrowUp ?? Vector3.up;

            DrawLine(start, end, method);
            if (arrowSize > 0f)
            {
                up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;
                var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
                var a2 = Quaternion.AngleAxis(-90, up) * a1;
                a1 = a1.normalized;
                a2 = a2.normalized;
                DrawLine(end + a1 * arrowSize, end, method);
                DrawLine(end + a2 * arrowSize, end, method);
            }
        }
        

        /// <summary>
        /// Debug.DrawLineのラッパー. デバッグ描画系をここに集約するため
        /// </summary>
        private static void DrawLine(Vector3 start, Vector3 end, LaneLineDrawMethod method)
        {
            switch (method)
            {
                case LaneLineDrawMethod.Gizmos:
                    Gizmos.DrawLine(start, end);
                    break;
                case LaneLineDrawMethod.Handles:
                #if UNITY_EDITOR
                    Handles.DrawLine(start, end);
                #endif
                    break;
            }
        }


        private static void DrawDashedArrow(Vector3 st, Vector3 en, LaneLineDrawMethod method, float lineLength = 1f, float spaceLength = 0.2f, float arrowSize = 0.5f, Vector3? arrowUp = null)
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
                DrawArrow(p0, p1, method, arrowSize, arrowUp);
            }
        }
    }

    internal enum LaneLineDrawMethod
    {
        Gizmos, Handles
    }
    
}