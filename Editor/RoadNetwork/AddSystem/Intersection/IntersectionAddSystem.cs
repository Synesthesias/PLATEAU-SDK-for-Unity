using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.AddSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.AddSystem
{
    /// <summary>
    /// 交差点タイプ
    /// </summary>
    enum IntersectionType
    {
        T = 0,
        Cross = 1,
    }

    /// <summary>
    /// T字路のときの切り欠き方向
    /// </summary>
    enum TIntersectionType
    {
        Left = 0,
        Right = 1,
        Front = 2,
        None = 3,
    }

    /// <summary>
    /// 交差点作成に必要なジオメトリ情報をまとめた構造体
    /// </summary>
    internal struct IntersectionGeometryData
    {
        public RnRoad Road;
        public Vector3 Forward;
        public Vector3 Right;
        public RnPoint FirstPoint;
        public RnPoint LastPoint;
        public float BorderLength;
        public float LeftSideWalkEdgeLength;
        public float RightSideWalkEdgeLength;
        public float LongerSideWalkEdgeLength;
        public RnPoint LeftSideWalkOuterPoint;
        public RnPoint RightSideWalkOuterPoint;
        public bool IsRoadPrev;
    }

    internal class IntersectionAddSystem
    {
        public bool IsActive { get; private set; } = false;
        public IntersectionType IntersectionType { get; private set; } = IntersectionType.Cross;
        public bool IsCreating { get; private set; } = false;

        private RoadEdgeInfo selectedEdgeInfo;
        private TIntersectionType tIntersectionType = TIntersectionType.Front;

        /// <summary>
        /// 交差点追加後のアクション (交差点と、交差点に接続する道路のペアを返す)
        /// </summary>
        public Action<RnIntersection, RnRoad> OnIntersectionAdded { get; set; }

        private RnSkeletonHandles roadSelectionHandles;
        private RoadNetworkAddSystemContext context;

        public IntersectionAddSystem(RoadNetworkAddSystemContext context)
        {
            this.context = context;
            this.roadSelectionHandles = new RnSkeletonHandles(context);
            this.roadSelectionHandles.OnRoadSelected += HandlePointPicked;
        }

        /// <summary>
        /// 交差点追加モードを有効化
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// 交差点追加モードを無効化
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            IsCreating = false;
        }

        /// <summary>
        /// SceneView上でのGUIハンドリング
        /// </summary>
        public void HandleSceneGUI(SceneView sceneView)
        {
            if (!IsActive) return;

            // まだ道路を選択していない場合は、ハンドルで選択処理
            if (!IsCreating)
            {
                roadSelectionHandles.HandleSceneGUI(sceneView, true, false);
                return;
            }

            // RキーでT字路の切り欠き方向をローテーション
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
            {
                RotateTIntersectionType();
            }
            // エンターキーで交差点を確定
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                AddIntersection();
                IsCreating = false;
            }
            // Escキーでキャンセル
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                IsCreating = false;
            }

            // プレビューを描画
            DrawPreviewExterior();
        }

        /// <summary>
        /// T字路の切り欠き方向を順番に切り替える
        /// </summary>
        private void RotateTIntersectionType()
        {
            switch (tIntersectionType)
            {
                case TIntersectionType.Left:
                    tIntersectionType = TIntersectionType.Right;
                    break;
                case TIntersectionType.Right:
                    tIntersectionType = TIntersectionType.Front;
                    break;
                case TIntersectionType.Front:
                    tIntersectionType = TIntersectionType.Left;
                    break;
            }
        }

        /// <summary>
        /// 選択された道路エッジから情報を取得して交差点作成モードに入る
        /// </summary>
        private void HandlePointPicked(ExtensibleRoadEdge edge)
        {
            if (edge.road == null) return;

            var edgeMaker = new RnRoadEdgeMaker(edge.road.Roads[0]);
            selectedEdgeInfo = edgeMaker.Execute(edge);
            IsCreating = true;
        }

        /// <summary>
        /// 交差点プレビューを描画
        /// </summary>
        private void DrawPreviewExterior()
        {
            var data = CreateIntersectionGeometryData(selectedEdgeInfo);

            // 交差点形状を計算 (T字路ならば切り欠き方向を指定)
            var intersectionType = IntersectionType == IntersectionType.Cross
                ? TIntersectionType.None
                : tIntersectionType;

            // 外枠の座標を取得
            var exteriorPoints = CalcExteriorPoints(
                data.Forward,
                data.Right,
                data.FirstPoint,
                data.LastPoint,
                data.BorderLength,
                data.LeftSideWalkEdgeLength,
                data.RightSideWalkEdgeLength,
                data.LongerSideWalkEdgeLength,
                data.LeftSideWalkOuterPoint,
                data.RightSideWalkOuterPoint,
                intersectionType
            );

            Handles.color = Color.green;
            Handles.DrawAAPolyLine(exteriorPoints.Select(p => p.Vertex).ToArray());
        }

        /// <summary>
        /// 交差点を実際に作成し、RoadNetworkに追加する
        /// </summary>
        private void AddIntersection()
        {
            var data = CreateIntersectionGeometryData(selectedEdgeInfo);

            // 交差点本体の作成
            var intersection = ConstructIntersection(data);

            // 交差点をネットワークに紐付け
            AttachIntersectionToRoadNetwork(intersection, data);

            // OnIntersectionAddedのコールバック実行
            OnIntersectionAdded?.Invoke(intersection, data.Road);
        }

        /// <summary>
        /// 交差点作成に必要なジオメトリデータをまとめて取得
        /// </summary>
        private IntersectionGeometryData CreateIntersectionGeometryData(RoadEdgeInfo edgeInfo)
        {
            var roadGroup = edgeInfo.Edge.road;
            var road = roadGroup.Roads[0];

            // 全体的な方向情報
            var isRoadPrev = edgeInfo.Edge.isPrev;
            var forward = edgeInfo.Edge.forward.normalized;
            var right = Quaternion.AngleAxis(90f, Vector3.up) * forward;

            // 道路の左右端点を取得
            var firstPoint = GetEdgePoint(road, useLeftWay: true, isRoadPrev);
            var lastPoint = GetEdgePoint(road, useLeftWay: false, isRoadPrev);

            // 向きが逆なら順番を入れ替える
            if (!isRoadPrev) (lastPoint, firstPoint) = (firstPoint, lastPoint);

            // 歩道のエッジとその長さを取得
            var leftSideWalkEdge = edgeInfo.LeftSideWalkEdge.Edge;
            var rightSideWalkEdge = edgeInfo.RightSideWalkEdge.Edge;
            float leftSideWalkEdgeLength = leftSideWalkEdge == null ? 0f : leftSideWalkEdge.CalcLength();
            float rightSideWalkEdgeLength = rightSideWalkEdge == null ? 0f : rightSideWalkEdge.CalcLength();
            float borderLength = Vector3.Distance(firstPoint.Vertex, lastPoint.Vertex);

            // より長い歩道エッジ長
            float longerSideWalkEdgeLength = Math.Max(leftSideWalkEdgeLength, rightSideWalkEdgeLength);

            // 歩道の外側点を取得
            var leftSideWalkOuterPoint = edgeInfo.LeftSideWalkEdge.SideWalk == null
                ? new RnPoint(firstPoint.Vertex + right * leftSideWalkEdgeLength)
                : (edgeInfo.LeftSideWalkEdge.IsOutsidePrev
                    ? edgeInfo.LeftSideWalkEdge.SideWalk.OutsideWay.LineString.Points.First()
                    : edgeInfo.LeftSideWalkEdge.SideWalk.OutsideWay.LineString.Points.Last());

            var rightSideWalkOuterPoint = edgeInfo.RightSideWalkEdge.SideWalk == null
                ? new RnPoint(lastPoint.Vertex - right * rightSideWalkEdgeLength)
                : (edgeInfo.RightSideWalkEdge.IsOutsidePrev
                    ? edgeInfo.RightSideWalkEdge.SideWalk.OutsideWay.LineString.Points.First()
                    : edgeInfo.RightSideWalkEdge.SideWalk.OutsideWay.LineString.Points.Last());

            // 逆向きの場合は再度スワップ
            if (isRoadPrev)
            {
                (leftSideWalkOuterPoint, rightSideWalkOuterPoint) = (rightSideWalkOuterPoint, leftSideWalkOuterPoint);
                (leftSideWalkEdgeLength, rightSideWalkEdgeLength) = (rightSideWalkEdgeLength, leftSideWalkEdgeLength);
            }

            return new IntersectionGeometryData
            {
                Road = road,
                Forward = forward,
                Right = right,
                FirstPoint = firstPoint,
                LastPoint = lastPoint,
                BorderLength = borderLength,
                LeftSideWalkEdgeLength = leftSideWalkEdgeLength,
                RightSideWalkEdgeLength = rightSideWalkEdgeLength,
                LongerSideWalkEdgeLength = longerSideWalkEdgeLength,
                LeftSideWalkOuterPoint = leftSideWalkOuterPoint,
                RightSideWalkOuterPoint = rightSideWalkOuterPoint,
                IsRoadPrev = isRoadPrev,
            };
        }

        /// <summary>
        /// 交差点を構築する (Cross/Tを切り替え)
        /// </summary>
        private RnIntersection ConstructIntersection(IntersectionGeometryData data)
        {
            RnIntersection intersection;
            if (IntersectionType == IntersectionType.Cross)
            {
                intersection = CreateCrossIntersection(data);
            }
            else
            {
                intersection = CreateTIntersection(data, tIntersectionType);
            }

            // 隣接道路がある輪郭を追加
            var isRoadPrev = data.IsRoadPrev;
            foreach (var roadBorder in data.Road.MainLanes.Select(l => isRoadPrev ^ l.IsReverse ? l.PrevBorder : l.NextBorder))
            {
                intersection.AddEdge(data.Road, roadBorder);
            }

            // 座標合わせ
            intersection.Align();

            // ネットワークに追加
            context.RoadNetwork.AddIntersection(intersection);
            return intersection;
        }

        /// <summary>
        /// 交差点を道路ネットワークに接続し、切り分け・歩道等を調整
        /// </summary>
        private void AttachIntersectionToRoadNetwork(RnIntersection intersection, IntersectionGeometryData data)
        {
            var road = data.Road;
            bool isRoadPrev = data.IsRoadPrev;

            // 隣接情報を更新
            if (isRoadPrev)
                road.SetPrevNext(intersection, road.Next);
            else
                road.SetPrevNext(road.Prev, intersection);

            // 逆側は一時的に切り離して拡張処理を行う
            var oppositeIntersection = isRoadPrev ? road.Next : road.Prev;
            if (isRoadPrev)
                road.SetPrevNext(intersection, null);
            else
                road.SetPrevNext(null, intersection);

            // 横断歩道を追加するために道路側の拡張を試みる
            var option = new RnModelEx.CalibrateIntersectionBorderOption();
            bool sliceResult = road.ParentModel.TrySliceRoadHorizontalNearByBorder(
                road,
                option,
                out var prevRoad,
                out var centerRoad,
                out var nextRoad
            );

            if (sliceResult && (prevRoad != null || nextRoad != null))
            {
                if (isRoadPrev)
                {
                    prevRoad?.TryMerge2NeighborIntersection(RnLaneBorderType.Prev);
                }
                else
                {
                    nextRoad?.TryMerge2NeighborIntersection(RnLaneBorderType.Next);
                }
            }

            // 隣接関係を復元
            if (isRoadPrev)
                road.SetPrevNext(intersection, oppositeIntersection);
            else
                road.SetPrevNext(oppositeIntersection, intersection);

            intersection.TargetTrans.Clear();
        }

        /// <summary>
        /// Cross 交差点の形状を生成
        /// </summary>
        private RnIntersection CreateCrossIntersection(IntersectionGeometryData data)
        {
            var intersection = new RnIntersection();
            var exteriorPoints = CalcExteriorPoints(
                data.Forward,
                data.Right,
                data.FirstPoint,
                data.LastPoint,
                data.BorderLength,
                data.LeftSideWalkEdgeLength,
                data.RightSideWalkEdgeLength,
                data.LongerSideWalkEdgeLength,
                data.LeftSideWalkOuterPoint,
                data.RightSideWalkOuterPoint,
                TIntersectionType.None
            );

            // 4辺分、各4点ずつ歩道生成 (合計16点)
            for (int i = 0; i < 15; i += 4)
            {
                var startWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i], exteriorPoints[i + 1] }));
                var endWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 2], exteriorPoints[i + 3] }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 3], exteriorPoints[i] }));
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 1], exteriorPoints[i + 2] }));

                var sideWalk = RnSideWalk.Create(intersection, outsideWay, insideWay, startWay, endWay);
                intersection.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);
            }

            // 輪郭作成
            for (int i = 0; i < 15; i += 4)
            {
                var borderWay = new RnWay(
                    new RnLineString(new List<RnPoint>()
                        { exteriorPoints[i + 3], exteriorPoints[(i + 4) % 16] }));
                var nonBorderWay = new RnWay(
                    new RnLineString(new List<RnPoint>()
                        { exteriorPoints[i], exteriorPoints[i + 3] }));

                // 隣接道路がある輪郭は別途 (i=12) で追加するので除外
                if (i != 12)
                    intersection.AddEdge(null, borderWay);

                intersection.AddEdge(null, nonBorderWay);
            }

            return intersection;
        }

        /// <summary>
        /// T字路の形状を生成
        /// </summary>
        private RnIntersection CreateTIntersection(IntersectionGeometryData data, TIntersectionType intersectionType)
        {
            var intersection = new RnIntersection();
            var exteriorPoints = CalcExteriorPoints(
                data.Forward,
                data.Right,
                data.FirstPoint,
                data.LastPoint,
                data.BorderLength,
                data.LeftSideWalkEdgeLength,
                data.RightSideWalkEdgeLength,
                data.LongerSideWalkEdgeLength,
                data.LeftSideWalkOuterPoint,
                data.RightSideWalkOuterPoint,
                intersectionType
            );

            // 3辺分、各4点ずつ歩道生成 (合計12点)
            for (int i = 0; i < 11; i += 4)
            {
                var startWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i], exteriorPoints[i + 1] }));
                var endWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 2], exteriorPoints[i + 3] }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 3], exteriorPoints[i] }));
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 1], exteriorPoints[i + 2] }));

                var sideWalk = RnSideWalk.Create(intersection, outsideWay, insideWay, startWay, endWay);
                intersection.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);
            }

            // 輪郭作成
            for (int i = 0; i < 11; i += 4)
            {
                var borderWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i + 3], exteriorPoints[(i + 4) % 12] }));
                var nonBorderWay = new RnWay(new RnLineString(new List<RnPoint>()
                    { exteriorPoints[i], exteriorPoints[i + 3] }));

                // 隣接道路がある輪郭は別途 (i=8) で追加するので除外
                if (i != 8)
                    intersection.AddEdge(null, borderWay);

                intersection.AddEdge(null, nonBorderWay);
            }

            return intersection;
        }

        /// <summary>
        /// 道路の左右どちらかのWayから、先端となるRnPointを取得
        /// </summary>
        private RnPoint GetEdgePoint(RnRoad road, bool useLeftWay, bool isRoadPrev)
        {
            var way = useLeftWay ? road.GetLeftWayOfLanes() : road.GetRightWayOfLanes();
            return isRoadPrev ^ (useLeftWay ? road.MainLanes[0].IsReverse : road.MainLanes.Last().IsReverse) ^ way.IsReversed
                ? way.LineString.Points.First()
                : way.LineString.Points.Last();
        }

        /// <summary>
        /// 交差点周りの外輪郭を構成する座標列を取得する
        /// (T字路の場合、指定された方向を切り欠く)
        /// </summary>
        /// <param name="forward">道路の進行方向</param>
        /// <param name="right">道路進行方向に対して垂直右方向ベクトル</param>
        /// <param name="firstPoint">左端の始点</param>
        /// <param name="lastPoint">右端の終点</param>
        /// <param name="borderLength">道路の境界長</param>
        /// <param name="leftSideWalkEdgeLength">左歩道の幅</param>
        /// <param name="rightSideWalkEdgeLength">右歩道の幅</param>
        /// <param name="longerSideWalkEdgeLength">左右どちらか長い方の歩道幅</param>
        /// <param name="leftSideWalkOuterPoint">左歩道外側の始端点</param>
        /// <param name="rightSideWalkOuterPoint">右歩道外側の終端点</param>
        /// <param name="intersectionType">T字路の切り欠き方向 (Crossの場合はNone)</param>
        /// <returns></returns>
        private static List<RnPoint> CalcExteriorPoints(
            Vector3 forward,
            Vector3 right,
            RnPoint firstPoint,
            RnPoint lastPoint,
            float borderLength,
            float leftSideWalkEdgeLength,
            float rightSideWalkEdgeLength,
            float longerSideWalkEdgeLength,
            RnPoint leftSideWalkOuterPoint,
            RnPoint rightSideWalkOuterPoint,
            TIntersectionType intersectionType
        )
        {
            var exteriorPoints = new List<RnPoint>();

            // 1. Start (firstPoint)
            exteriorPoints.Add(firstPoint);
            exteriorPoints.Add(rightSideWalkOuterPoint);

            // 2. 前方/右側に歩道分・交差点分進んでいく (交互に座標を追加)
            var pos = rightSideWalkOuterPoint.Vertex;
            pos += (forward + right) * 3f;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(pos));

            pos += forward * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(pos));

            pos += forward * borderLength;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(pos));

            pos += forward * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(pos));

            // 3. 前方から左へ折れる
            pos += (forward - right) * 3f;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(pos));

            pos -= right * leftSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(pos));

            pos -= right * borderLength;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(pos));

            pos -= right * rightSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(pos));

            // 4. 後方から左へ折れる
            pos += (-forward - right) * 3f;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(pos));

            pos += (-forward) * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(pos));

            pos += (-forward) * borderLength;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(pos));

            pos += (-forward) * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(pos));

            // 5. 最後に左歩道外側とLastPointを追加
            exteriorPoints.Add(leftSideWalkOuterPoint);
            exteriorPoints.Add(lastPoint);

            return exteriorPoints;
        }

        /// <summary>
        /// 任意の中心、および任意方向・大きさのベクトル(axisX, axisY)を指定し、
        /// 4分の1楕円（0°～90°）上の 6点 (0°,18°,36°,54°,72°,90°) を取得する。
        ///
        ///   P(θ) = center + axisX * cos(θ) + axisY * sin(θ)
        /// </summary>
        public static Vector3[] GetQuarterEllipsePoints(Vector3 center, Vector3 axisX, Vector3 axisY)
        {
            float[] anglesDeg = { 0f, 18f, 36f, 54f, 72f, 90f };
            Vector3[] result = new Vector3[anglesDeg.Length];

            for (int i = 0; i < anglesDeg.Length; i++)
            {
                float deg = anglesDeg[i];
                float rad = deg * Mathf.Deg2Rad;
                float cosVal = Mathf.Cos(rad);
                float sinVal = Mathf.Sin(rad);

                Vector3 pos = center + axisX * cosVal + axisY * sinVal;
                result[i] = pos;
            }
            return result;
        }

        /// <summary>
        /// 交差点の種類を設定
        /// </summary>
        internal void SetIntersectionType(IntersectionType t)
        {
            IntersectionType = t;
        }
    }
}