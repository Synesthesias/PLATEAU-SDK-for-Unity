using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PLATEAU.RoadNetwork.Util
{
    public struct CombSet2<T>
    {
        public T A { get; }

        public T B { get; }

        public CombSet2(T a, T b)
        {
            A = a;
            B = b;
            if (A.GetHashCode() > B.GetHashCode())
                (A, B) = (B, A);
        }
    }

    public struct CombSet3<T>
    {
        public T A { get; }

        public T B { get; }

        public T C { get; }

        public CombSet3(T a, T b, T c)
        {
            A = a;
            B = b;
            C = c;
            if (A.GetHashCode() > B.GetHashCode())
                (A, B) = (B, A);
            if (B.GetHashCode() > C.GetHashCode())
                (B, C) = (C, B);
            if (A.GetHashCode() > B.GetHashCode())
                (A, B) = (B, A);
        }
    }


    /// <summary>
    /// LineSegmentとLineStringとの交点チェック結果
    /// </summary>
    public class LineCrossPointResult
    {
        public class TargetLineInfo
        {
            /// <summary>
            /// 対象線分
            /// </summary>
            public RnLineString LineString { get; set; }

            /// <summary>
            /// 交点情報.
            /// index : LineString上の配列インデックス位置. 線分の途中の点の場合は小数になる
            ///     v : 交点座標
            /// </summary>
            public List<(float index, Vector3 v)> Intersections { get; set; } = new();
        }

        /// <summary>
        /// 交点チェック対象のLineString情報
        /// </summary>
        public List<TargetLineInfo> TargetLines { get; set; } = new();

        /// <summary>
        /// TargetLinesのうち、交差しているもの
        /// </summary>
        public IEnumerable<TargetLineInfo> CrossingLines => TargetLines.Where(t => t.Intersections.Count > 0);

        /// <summary>
        /// 対象の線分
        /// </summary>
        public LineSegment3D LineSegment { get; set; }
    }


    public static class RnEx
    {
        /// <summary>
        /// EditorのScene上で選択されているPLATEAUCityObjectGroupを取得する(Editorのみ)
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PLATEAUCityObjectGroup> GetSceneSelectedCityObjectGroups()
        {
#if UNITY_EDITOR
            return Selection.gameObjects.Select(go => go.GetComponent<PLATEAUCityObjectGroup>()).Where(cog => cog != null);
#else
            return Enumerable.Empty<PLATEAUCityObjectGroup>();
#endif
        }
        public static CombSet2<T> CombSet<T>(T a, T b)
        {
            return new CombSet2<T>(a, b);
        }

        public static CombSet3<T> CombSet<T>(T a, T b, T c)
        {
            return new CombSet3<T>(a, b, c);
        }

        public static void Replace<T>(IList<T> self, T before, T after) where T : class
        {
            for (var i = 0; i < self.Count; i++)
            {
                if (self[i] == before)
                    self[i] = after;
            }
        }

        public static void ReplaceLane(IList<RnLane> self, RnLane before, RnLane after)
        {
            Replace(self, before, after);
        }

        /// <summary>
        /// ModelのRootNodeを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        private static IEnumerable<Node> GetRootNodes(this Model self)
        {
            for (var i = 0; i < self.RootNodesCount; ++i)
                yield return self.GetRootNodeAt(i);
        }

        /// <summary>
        /// Sceneエディタ上で選択されているかどうか
        /// </summary>
        /// <param name="cog"></param>
        /// <returns></returns>
        public static bool IsEditorSceneSelected(PLATEAUCityObjectGroup cog)
        {
#if UNITY_EDITOR
            if (!cog)
                return false;
            return UnityEditor.Selection.gameObjects.Contains(cog.gameObject);
#else
            return false;
#endif
        }

        /// <summary>
        /// leftVerticesとrightVerticesの間をtで補間した点列を生成する.
        /// start/endはそれぞれの端点.
        /// startBorder/endBorderはそれぞれの端点のボーダーでstartBorder/LeftVertices/endBorder/RightVerticesで囲まれた範囲の外に出ないようにチェックするためのもの
        /// </summary>
        /// <param name="leftVertices"></param>
        /// <param name="rightVertices"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="startBorder"></param>
        /// <param name="endBorder"></param>
        /// <param name="t"></param>
        /// <param name="pointSkipDistance"></param>
        /// <returns></returns>
        public static RnLineString CreateInnerLerpLineString(IReadOnlyList<Vector3> leftVertices, IReadOnlyList<Vector3> rightVertices, RnPoint start, RnPoint end, RnWay startBorder, RnWay endBorder, float t, float pointSkipDistance = 1e-3f)
        {
            // 左右がどちらも直線もしくは点以下の場合 -> start/endを直接つなぐ
            if (leftVertices.Count <= 2 && rightVertices.Count <= 2)
            {
                return new RnLineString(new List<RnPoint> { start, end });
            }

            var line = new RnLineString();
            void AddPoint(RnPoint p)
            {
                if (p == null)
                    return;
                line.AddPointOrSkip(p, pointSkipDistance);
            }

            AddPoint(start);
            var segments = GeoGraphEx.GetInnerLerpSegments(leftVertices, rightVertices, RnModel.Plane, t);
            // 1つ目の点はボーダーと重複するのでスキップ
            // #TODO : 実際はボーダーよりも外側にあるのはすべてスキップすべき
            foreach (var s in segments.Skip(1))
                AddPoint(new RnPoint(s));
            AddPoint(end);
            // 自己交差があれば削除する
            var plane = RnModel.Plane;
            GeoGraph2D.RemoveSelfCrossing(line.Points
                , t => t.Vertex.GetTangent(plane)
                , (p1, p2, p3, p4, inter, f1, f2) => new RnPoint(Vector3.Lerp(p1, p2, f1)));

            return line;
        }

        /// <summary>
        /// lineSegmentとwaysの交点を取得する. ただし、2Dでの交点
        /// </summary>
        /// <param name="lineSegment"></param>
        /// <param name="ways"></param>
        /// <returns></returns>
        public static LineCrossPointResult GetLineIntersections(LineSegment3D lineSegment, IEnumerable<RnWay> ways)
        {
            var ret = new LineCrossPointResult { LineSegment = lineSegment };

            // 全てのwayのLineStringを取得
            var targetLines = ways
                .Select(w => w.LineString)
                .ToHashSet();

            foreach (var way in targetLines)
            {
                var elem = new LineCrossPointResult.TargetLineInfo { LineString = way };

                foreach (var r in way.GetIntersectionBy2D(lineSegment, RnModel.Plane))
                {
                    elem.Intersections.Add((r.index, r.v));
                }
                ret.TargetLines.Add(elem);
            }

            return ret;
        }
    }
}
