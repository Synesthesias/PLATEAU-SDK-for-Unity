using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// 道路のレーン編集のために、線1つをシーン上に描画します。
    /// 線の種類をサブクラスで書き分けます。
    /// </summary>
    public interface ILaneLineDrawer
    {
        void Draw();
    }
    
    /// <summary>
    /// <see cref="ILaneLineDrawer"/>の実線版です。
    /// </summary>
    public class LaneLineDrawerSolid : ILaneLineDrawer
    {
        private List<Vector3> line;
        private Color color;

        public LaneLineDrawerSolid(List<Vector3> line, Color color)
        {
            this.line = line;
            this.color = color;
        }

        public static LaneLineDrawerSolid CreateWithEmptyLine(Color color)
        {
            return new LaneLineDrawerSolid(new List<Vector3>(), color);
        }

        public void Draw()
        {
#if UNITY_EDITOR
            if (line == null) return;
            if (line.Count < 2) return;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLineStrip(line.ToArray() ,false);
            Gizmos.color = prevColor;
#endif
        }

        public void SetLine(List<Vector3> lineArg)
        {
            line = lineArg;
        }

        public void ClearLine() => line.Clear();
    }

    
    /// <summary>
    /// <see cref="ILaneLineDrawer"/>の矢印がリピートする版です。
    /// </summary>
    public class LaneLineDrawerArrow : ILaneLineDrawer
    {
        private RnWay way;
        private Color color;
        public LaneLineDrawerArrow(RnWay way, Color color)
        {
            this.way = way;
            this.color = color;
        }

        public void Draw()
        {
#if UNITY_EDITOR
            const float YOffset = 0.5f;
            const float OneArrowLength = 3f;
            const float OneSpaceLength = 1f;
            var prevColor = Gizmos.color;
            Gizmos.color = color;
            DrawDashedArrows(way.Select(v => v.PutY(v.y + YOffset)), false, OneArrowLength, OneSpaceLength);
            Gizmos.color = prevColor;
#endif
        }

        private static void DrawDashedArrows(IEnumerable<Vector3> vertices, bool isLoop = false, float lineLength = 3f, float spaceLength = 1f)
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                DrawDashedArrow(e.Item1, e.Item2, lineLength, spaceLength);
        }


        private static void DrawArrow(
              Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null)
        {
            Vector3 up = arrowUp ?? Vector3.up;

            DrawLine(start, end);
            if (arrowSize > 0f)
            {
                up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;
                var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
                var a2 = Quaternion.AngleAxis(-90, up) * a1;
                a1 = a1.normalized;
                a2 = a2.normalized;
                DrawLine(end + a1 * arrowSize, end);
                DrawLine(end + a2 * arrowSize, end);
            }
        }
        

        /// <summary>
        /// Debug.DrawLineのラッパー. デバッグ描画系をここに集約するため
        /// </summary>
        private static void DrawLine(Vector3 start, Vector3 end)
        {
            Gizmos.DrawLine(start, end);
        }


        private static void DrawDashedArrow(Vector3 st, Vector3 en, float lineLength = 1f, float spaceLength = 0.2f, float arrowSize = 0.5f, Vector3? arrowUp = null)
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
                DrawArrow(p0, p1, arrowSize, arrowUp);
            }
        }
        

    }
    
}