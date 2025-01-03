using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    internal class RnRoadAddSystem : RnSplineSystemBase
    {
        public bool IsActive { get; private set; } = false;

        /// <summary>
        /// 道路追加後のアクション
        /// </summary>
        public Action<RnRoadGroup> OnRoadAdded { get; set; }

        private SplineCreateHandles splineCreateHandles;

        // 前フレームで作図モード中かどうかのフラグ (作図完了判定に使用)
        private bool wasCreatingSpline = false;

        // クリックした頂点に紐づく「更新対象の道路」を記憶しておく
        private RnRoadGroup selectedRoad;

        private RoadNetworkAddSystemContext context;

        public RnRoadAddSystem(RoadNetworkAddSystemContext context)
        {
            this.context = context;
            splineCreateHandles = new SplineCreateHandles(splineEditorCore);
        }

        public void Activate()
        {
            IsActive = true;
            splineCreateHandles = new SplineCreateHandles(splineEditorCore);
        }

        public void Deactivate()
        {
            IsActive = false;
            splineEditorCore.Reset();
            splineCreateHandles = null;
        }

        public override void HandleSceneGUI(UnityEngine.Object target)
        {
            if (!IsActive)
                return;

            var skeletonData = context.SkeletonData;

            // すべての RoadGroupEditorData からペアをまとめて取得
            // TODO: 重い処理なので、必要な時だけ取得するようにする
            var vertexRoadPairs = GetVertexRoadPairs(skeletonData);

            // 1. 頂点をSphereで描画＆クリックしたらスプライン作図開始
            HandleVertexPicking(vertexRoadPairs);

            // 2. SplineCreateHandles でノット追加＆移動を処理
            splineCreateHandles.HandleSceneGUI();

            // 3. 作図完了を検知
            DetectSplineCreationCompletion();
        }

        private List<(Vector3 position, RnRoadGroup road)> GetVertexRoadPairs(RoadNetworkSkeletonData skeletonData)
        {
            var vertexRoadPairs = new List<(Vector3 position, RnRoadGroup road)>();

            foreach (var roadSkeleton in skeletonData.Roads)
            {
                var road = roadSkeleton.Road;
                var spline = roadSkeleton.Spline;

                if (spline.Knots.Count() == 0)
                {
                    Debug.LogWarning($"スプラインが存在しない道路があります: ID{road.Roads[0].DebugMyId}");
                    continue;
                }

                if (road.PrevIntersection == null)
                    vertexRoadPairs.Add((spline.Knots.First().Position, road));
                if (road.NextIntersection == null)
                    vertexRoadPairs.Add((spline.Knots.Last().Position, road));
            }

            return vertexRoadPairs;
        }

        /// <summary>
        /// 頂点の描画 & クリック判定
        /// 頂点をクリックしたら、その頂点が属する道路を記憶してスプライン作成を開始
        /// </summary>
        private void HandleVertexPicking(List<(Vector3 position, RnRoadGroup road)> vertexRoadPairs)
        {
            Event e = Event.current;
            // デフォルトハンドルの制御を奪う (マウス操作を受け付ける)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            for (int i = 0; i < vertexRoadPairs.Count; i++)
            {
                var (pos, road) = vertexRoadPairs[i];
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
                        // まだスプライン作図中でなければ開始
                        if (!splineCreateHandles.IsCreatingSpline)
                        {
                            // クリックした頂点に紐づく道路を記憶
                            selectedRoad = road;
                            // スプラインを開始（第2引数に null => 新規 SplineContainer を生成）
                            splineCreateHandles.BeginCreateSpline(pos, null);
                        }

                        // イベント消費してループ脱出
                        e.Use();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// スプライン作図が完了したタイミングで呼ばれる
        /// → クリック時に記憶しておいた `selectedRoad` を用いて、
        ///    その道路をスプラインに反映する
        /// </summary>
        private void OnSplineCreationFinished()
        {
            Spline newSpline = splineEditorCore.Spline;

            if (selectedRoad == null)
            {
                Debug.LogWarning("どの道路に反映するかが未選択のためスキップ");
                return;
            }

            ExtendRoadAlongSpline(selectedRoad, newSpline);

            splineEditorCore.Reset();
            OnRoadAdded?.Invoke(selectedRoad);
        }

        /// <summary>
        /// Splineに沿って道路を拡張する。
        /// </summary>
        /// <param name="targetRoad"></param>
        /// <param name="spline"></param>
        private void ExtendRoadAlongSpline(RnRoadGroup targetRoad, Spline spline)
        {
            var road = targetRoad.Roads[0];

            foreach (var way in GetAllWaysAlongRoad(road))
            {
                ExtendPointsAlongSpline(way.Item2.LineString.Points, spline, !way.Item1);
            }
        }

        /// <summary>
        /// Splineに沿ってLineStringを拡張する
        /// </summary>
        /// <param name="points"></param>
        /// <param name="spline"></param>
        /// <param name="isReversed">trueの場合末尾ではなく先頭から拡張する</param>
        private void ExtendPointsAlongSpline(List<RnPoint> points, Spline spline, bool isReversed)
        {
            // 末端の点を取得
            var lastPoint = isReversed ? points.First() : points.Last();
            if (lastPoint == null)
            {
                Debug.LogWarning("末端の点が取得できませんでした");
                return;
            }

            // スプラインからLineStringを生成する際のオフセット値を推定
            var offset = EstimateOffset(lastPoint.Vertex, spline, 0f);

            var newPoints = ConvertSplineToLineStringPoints(spline, offset, false);
            var newLastPoint = newPoints.Last();
            newPoints.Remove(newLastPoint);

            // 一度末端の点を削除
            points.Remove(lastPoint);

            // 新しい点を追加
            foreach (var point in newPoints)
            {
                if (isReversed)
                    points.Insert(0, new RnPoint(point));
                else
                    points.Add(new RnPoint(point));
            }

            // 末端の点を追加
            lastPoint.Vertex = newLastPoint;
            if (isReversed)
                points.Insert(0, lastPoint);
            else
                points.Add(lastPoint);
        }

        private static float EstimateOffset(Vector3 point, Spline spline, float t)
        {
            var nearestPoint = GetNearestPointToSplineNormal(point, spline, t);
            // スプラインの法線方向を正としてオフセットを算出
            var tangent = spline.EvaluateTangent(t);
            Vector3 origin = spline.EvaluatePosition(t);
            var distance = Vector3.Distance(origin, nearestPoint);
            var normal = Vector3.Cross(tangent, Vector3.up).normalized;
            return Vector3.Dot(nearestPoint - origin, normal) > 0 ? distance : -distance;
        }

        /// <summary>
        /// pointから最も近いスプラインの法線直線上の点を取得
        /// </summary>
        /// <param name="point"></param>
        /// <param name="spline"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Vector3 GetNearestPointToSplineNormal(Vector3 point, Spline spline, float t)
        {
            var tangent = spline.EvaluateTangent(t);
            var normal = Vector3.Cross(tangent, Vector3.up).normalized;
            Vector3 origin = spline.EvaluatePosition(t);
            point.y = origin.y;
            return origin + normal * Vector3.Dot(point - origin, normal);
        }

        private static List<(bool, RnWay)> GetAllWaysAlongRoad(RnRoad road)
        {
            var scannedLineStrings = new HashSet<RnLineString>();

            var ways = new List<(bool, RnWay)>();

            foreach (var sideWalk in road.SideWalks)
            {
                if (scannedLineStrings.Contains(sideWalk.InsideWay.LineString) == false)
                {
                    ways.Add((sideWalk.InsideWay.IsReversed, sideWalk.InsideWay));
                    scannedLineStrings.Add(sideWalk.InsideWay.LineString);
                }

                if (scannedLineStrings.Contains(sideWalk.OutsideWay.LineString) == false)
                {
                    ways.Add((sideWalk.OutsideWay.IsReversed, sideWalk.OutsideWay));
                    scannedLineStrings.Add(sideWalk.OutsideWay.LineString);
                }
            }

            foreach (var lane in road.AllLanes)
            {
                if (scannedLineStrings.Contains(lane.LeftWay.LineString) == false)
                {
                    ways.Add((lane.IsReverse ^ lane.LeftWay.IsReversed, lane.LeftWay));
                    scannedLineStrings.Add(lane.LeftWay.LineString);
                }
                if (scannedLineStrings.Contains(lane.RightWay.LineString) == false)
                {
                    ways.Add((lane.IsReverse ^ lane.RightWay.IsReversed, lane.RightWay));
                    scannedLineStrings.Add(lane.RightWay.LineString);
                }
            }

            return ways;
        }

        private void DetectSplineCreationCompletion()
        {
            bool isCreatingNow = splineCreateHandles.IsCreatingSpline;
            if (!isCreatingNow && wasCreatingSpline)
            {
                // 直前まで作図モードだった → 今フレームで終了した
                OnSplineCreationFinished();
            }
            wasCreatingSpline = isCreatingNow;
        }
    }
}