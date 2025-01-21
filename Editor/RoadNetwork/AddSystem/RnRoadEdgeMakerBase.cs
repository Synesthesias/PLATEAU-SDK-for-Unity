using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.AddSystem
{
    /// <summary>
    /// 新しく道路・交差点を接続するためのエッジを作成するクラス
    /// TODO: 継承クラスにおいて各Wayの向きを無理やり推定しているが、そもそもデータ仕様として向きを統一するべきか検討
    /// </summary>
    public abstract class RnRoadEdgeMakerBase<RnRoadBase>
    {
        protected static RnWay GetOrCreateSideWalkEdge(RnSideWalk sideWalk, Vector3 edgeLineOrigin, Vector3 edgeLineDirection, out bool isStartEdge)
        {
            isStartEdge = false;
            var points = sideWalk.OutsideWay.LineString.Points;
            // pointsの終点側でエッジ上に存在する点群を取得
            var pointsOnEdge = new List<RnPoint>();
            foreach (var point in new Stack<RnPoint>(points))
            {
                Debug.Log($"Distance: {GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection)}");
                if (GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection) < 1f)
                {
                    pointsOnEdge.Add(point);
                }
                else
                    break;
            }

            // エッジ上の点がない場合は始点側をチェック
            if (pointsOnEdge.Count == 0)
            {
                foreach (var point in points)
                {
                    Debug.Log($"Distance: {GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection)}");
                    if (GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection) < 1f)
                    {
                        pointsOnEdge.Add(point);
                    }
                    else
                        break;
                }
            }

            if (pointsOnEdge.Count == 0)
            {
                return null;
            }

            // エッジ上の最後の点以外pointsから削除
            foreach (var point in pointsOnEdge.GetRange(0, pointsOnEdge.Count - 1))
            {
                points.Remove(point);
            }

            if (sideWalk.StartEdgeWay == null || sideWalk.StartEdgeWay.LineString.Points.Contains(pointsOnEdge.First()))
            {
                isStartEdge = true;
                if (sideWalk.StartEdgeWay == null)
                {
                    sideWalk.SetStartEdgeWay(new RnWay(new RnLineString(pointsOnEdge)));
                }
                else
                {
                    sideWalk.StartEdgeWay.LineString.Points.AddRange(pointsOnEdge.Skip(1));
                }
                return sideWalk.StartEdgeWay;
            }
            else
            {
                if (sideWalk.EndEdgeWay == null || sideWalk.EndEdgeWay.LineString.Points.Contains(pointsOnEdge.First()))
                {
                    if (sideWalk.EndEdgeWay == null)
                    {
                        sideWalk.SetEndEdgeWay(new RnWay(new RnLineString(pointsOnEdge)));
                    }
                    else
                    {
                        sideWalk.EndEdgeWay.LineString.Points.AddRange(pointsOnEdge.Skip(1));
                    }
                    return sideWalk.EndEdgeWay;
                }
            }
            return null;
        }

        protected static float GetDistanceToLine(Vector3 point, Vector3 origin, Vector3 direction)
        {
            float t = Vector3.Dot(point - origin, direction) / direction.sqrMagnitude;
            Vector3 closest = origin + t * direction;
            return Vector3.Distance(point, closest);
        }

        /// <summary>
        /// Pointsの端点をnewEdgeに合わせる
        /// </summary>
        /// <param name="points"></param>
        /// <param name="oldEdgeCenter"></param>
        /// <param name="oldEdgeDirection"></param>
        /// <param name="newEdgeCenter"></param>
        /// <param name="newEdgeDirection"></param>
        /// <param name="oldEdgePoint"></param>
        /// <param name="newEdgePoint"></param>
        /// <param name="isPrev"></param>
        protected void AlignWayEdge(List<RnPoint> points, Vector3 oldEdgeCenter, Vector3 oldEdgeDirection, Vector3 newEdgeCenter, Vector3 newEdgeDirection, out RnPoint oldEdgePoint, out RnPoint newEdgePoint, bool isPrev = false)
        {
            // isPrevの場合は先頭側を処理する
            if (isPrev)
                points.Reverse();

            oldEdgePoint = points.Last();

            // 1. 旧エッジに沿った点が複数ある場合はそれらを削除する。（歩道のOutsideが中心線に対して直角に閉じている場合があるため）
            // pointsのエッジ上に存在する点群を取得
            var pointsOnEdge = new List<RnPoint>();
            foreach (var point in new Stack<RnPoint>(points))
            {
                Debug.Log($"distance: {GetDistanceToLine(point.Vertex, oldEdgeCenter, oldEdgeDirection)}");
                new GameObject("point").transform.position = point.Vertex;
                if (GetDistanceToLine(point, oldEdgeCenter, oldEdgeDirection) < 1f)
                {
                    pointsOnEdge.Add(point);
                }
                else
                    break;
            }
            // エッジ上の最初の点以外pointsから削除
            if (pointsOnEdge.Count > 1)
            {
                foreach (var point in pointsOnEdge)
                {
                    points.Remove(point);
                }
                points.Add(pointsOnEdge.Last());
            }

            var lastPoint = points.Last();
            points.Remove(lastPoint);

            // 2. 新しいエッジに終点が来るように調整
            var distanceWithSign = GetDistanceToLineWithSign(lastPoint.Vertex, newEdgeDirection, newEdgeCenter, newEdgeDirection);
            if (distanceWithSign < 0f)
            {
                // 伸ばす必要があるケース
                points.Add(new RnPoint(ProjectToLine(lastPoint.Vertex, newEdgeCenter, newEdgeDirection)));
            }
            else
            {
                // 縮める必要があるケース
                // TODO: LineStringの頂点を削除する処理を追加
                points.Add(new RnPoint(ProjectToLine(points.Last().Vertex, newEdgeCenter, newEdgeDirection)));
            }

            newEdgePoint = points.Last();

            if (isPrev)
                points.Reverse();
        }

        protected static Vector3 ProjectToLine(Vector3 point, Vector3 lineOrigin, Vector3 lineDirection)
        {
            var v = point - lineOrigin;
            var t = Vector3.Dot(v, lineDirection) / Vector3.Dot(lineDirection, lineDirection);
            return lineOrigin + t * lineDirection;
        }

        protected static float GetDistanceToLineWithSign(Vector3 point, Vector3 forward, Vector3 lineOrigin, Vector3 lineDirection)
        {
            var projected = ProjectToLine(point, lineOrigin, lineDirection);
            var v = projected - point;
            return Vector3.Dot(v, forward) < 0f ? -v.magnitude : v.magnitude;
        }
    }
}
