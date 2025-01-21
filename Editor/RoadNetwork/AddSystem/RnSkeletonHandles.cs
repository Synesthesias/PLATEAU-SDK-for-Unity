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
    internal class RnSkeletonHandles
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

        public RnSkeletonHandles(RoadNetworkAddSystemContext context)
        {
            this.context = context;
        }

        public void HandleSceneGUI(UnityEngine.Object target, bool canPickRoad, bool canPickIntersection)
        {
            var skeletonData = context.SkeletonData;

            HandleVertexPicking(canPickRoad, canPickIntersection);
        }

        /// <summary>
        /// 頂点の描画 & クリック判定
        /// 頂点をクリックしたら、その頂点が属する道路を記憶してスプライン作成を開始
        /// </summary>
        private void HandleVertexPicking(bool canPickRoad, bool canPickIntersection)
        {
            Event e = Event.current;
            // デフォルトハンドルの制御を奪う (マウス操作を受け付ける)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (canPickRoad)
                PickRoadEdge(context.SkeletonData.Roads, e);
            if (canPickIntersection)
                PickIntersectionEdge(context.SkeletonData.Intersections, e);
        }

        private void PickIntersectionEdge(List<RnIntersectionSkeleton> intersectionSkeletons, Event e)
        {
            foreach (var intersectionSkeleton in intersectionSkeletons)
            {
                var edges = intersectionSkeleton.ExtensibleEdges;

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
        }

        private void PickRoadEdge(List<RnRoadSkeleton> roadSkeletons, Event e)
        {
            foreach (var roadSleketon in roadSkeletons)
            {
                foreach (var edge in roadSleketon.ExtensibleEdges)
                {
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
        }
    }
}