using NetTopologySuite.Operation.Valid;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.AddSystem
{
    internal struct ExtensibleRoadEdge
    {
        public RnRoadGroup road;
        public bool isPrev;
        public Vector3 center;
        public Vector3 forward;

        public ExtensibleRoadEdge(RnRoadGroup road, bool isPrev, Vector3 center, Vector3 forward)
        {
            this.road = road;
            this.isPrev = isPrev;
            this.center = center;
            this.forward = forward;
        }
    }

    internal struct ExtensibleIntersectionEdge
    {
        public RnIntersection intersection;
        public RnNeighbor neighbor;
        public int index;
        public Vector3 center;

        public ExtensibleIntersectionEdge(RnIntersection intersection, RnNeighbor neighbor, int index, Vector3 center)
        {
            this.intersection = intersection;
            this.neighbor = neighbor;
            this.index = index;
            this.center = center;
        }
    }

    internal class RnExtensiblePointHandles
    {
        /// <summary>
        /// 道路端点選択後のアクション
        /// </summary>
        public Action<ExtensibleRoadEdge> OnRoadSelected { get; set; }

        /// <summary>
        /// 交差点端点選択後のアクション
        /// </summary>
        public Action<ExtensibleIntersectionEdge> OnIntersectionSelected { get; set; }

        private RoadNetworkAddSystemContext context;

        public RnExtensiblePointHandles(RoadNetworkAddSystemContext context)
        {
            this.context = context;
        }

        public void HandleSceneGUI(UnityEngine.Object target, bool canPickRoad, bool canPickIntersection)
        {
            var skeletonData = context.SkeletonData;

            // すべての RoadGroupEditorData からペアをまとめて取得
            // TODO: 重い処理なので、必要な時だけ取得するようにする
            var vertexRoadPairs = GetVertexRoadPairs(skeletonData);
            var extensibleEdges = new List<ExtensibleIntersectionEdge>();

            foreach (var intersection in context.RoadNetwork.Intersections)
            {
                extensibleEdges.AddRange(FindExtensibleEdges(intersection));
            }

            HandleVertexPicking(vertexRoadPairs, extensibleEdges, canPickRoad, canPickIntersection);
        }

        private List<ExtensibleRoadEdge> GetVertexRoadPairs(RoadNetworkSkeletonData skeletonData)
        {
            var vertexRoadPairs = new List<ExtensibleRoadEdge>();

            foreach (var roadSkeleton in skeletonData.Roads)
            {
                var road = roadSkeleton.Road;
                var spline = roadSkeleton.Spline;

                if (spline.Knots.Count() == 0)
                    continue;

                if (road.PrevIntersection == null)
                    vertexRoadPairs.Add(new ExtensibleRoadEdge(road, true, spline.Knots.First().Position, -spline.EvaluateTangent(0f)));
                if (road.NextIntersection == null)
                    vertexRoadPairs.Add(new ExtensibleRoadEdge(road, false, spline.Knots.Last().Position, spline.EvaluateTangent(1f)));
            }

            return vertexRoadPairs;
        }

        /// <summary>
        /// 頂点の描画 & クリック判定
        /// 頂点をクリックしたら、その頂点が属する道路を記憶してスプライン作成を開始
        /// </summary>
        private void HandleVertexPicking(List<ExtensibleRoadEdge> vertexRoadPairs, List<ExtensibleIntersectionEdge> edges, bool canPickRoad, bool canPickIntersection)
        {
            Event e = Event.current;
            // デフォルトハンドルの制御を奪う (マウス操作を受け付ける)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (canPickRoad)
                PickRoadEdge(vertexRoadPairs, e);
            if (canPickIntersection)
                PickIntersectionEdge(edges, e);
        }

        private void PickIntersectionEdge(List<ExtensibleIntersectionEdge> edges, Event e)
        {
            foreach (var edge in edges)
            {
                float size = HandleUtility.GetHandleSize(edge.center) * 0.15f;

                // Sphere描画
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, edge.center, Quaternion.identity, size, EventType.Repaint);

                // 左クリック判定
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    float screenDist = HandleUtility.DistanceToCircle(edge.center, size);
                    // 適当な閾値: 2f 以下であればクリックとみなす
                    if (screenDist < 2f)
                    {
                        OnIntersectionSelected?.Invoke(edge);
                        // イベント消費してループ脱出
                        e.Use();
                        break;
                    }
                }
            }
        }

        private void PickRoadEdge(List<ExtensibleRoadEdge> vertexRoadPairs, Event e)
        {
            for (int i = 0; i < vertexRoadPairs.Count; i++)
            {
                var edge = vertexRoadPairs[i];
                var pos = edge.center;
                float size = HandleUtility.GetHandleSize(pos) * 0.15f;

                // Sphere描画
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, pos, Quaternion.identity, size, EventType.Repaint);

                // 左クリック判定
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    float screenDist = HandleUtility.DistanceToCircle(pos, size);
                    // 適当な閾値: 2f 以下であればクリックとみなす
                    if (screenDist < 2f)
                    {
                        OnRoadSelected?.Invoke(edge);
                        // イベント消費してループ脱出
                        e.Use();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 道路が追加可能なエッジを探す
        /// </summary>
        /// <param name="intersection"></param>
        /// <returns></returns>
        private static List<ExtensibleIntersectionEdge> FindExtensibleEdges(RnIntersection intersection)
        {
            var extensibleEdges = new List<ExtensibleIntersectionEdge>();
            foreach (var edge in intersection.Edges)
            {
                if (edge.IsBorder) continue;

                var border = edge.Border;
                // 歩道のinsideWayと線分を共有しない線分を探す
                for (var i = 0; i < border.Points.Count() - 1; i++)
                {
                    var point1 = border.Points.ElementAt(i);
                    var point2 = border.Points.ElementAt(i + 1);
                    //if (intersection.SideWalks.Any(sideWalk => sideWalk.InsideWay))
                    //    continue;
                    //return new List<(RnWay, int)> { (edge, i) };
                    extensibleEdges.Add(new ExtensibleIntersectionEdge(intersection, edge, i, (point1.Vertex + point2.Vertex) / 2));
                }
            }
            return extensibleEdges;
        }
    }
}