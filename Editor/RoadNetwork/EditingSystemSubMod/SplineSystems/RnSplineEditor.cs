using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプラインによる道路編集機能を提供する
    /// </summary>
    internal class RnSplineEditor
    {
        private Spline spline = new Spline();
        private GameObject roadNetworkObject;
        private SplineEditorHandles splineEditHandles;

        private bool IsInitialized { get; set; } = false;
        public bool IsEnabled { get; private set; } = false;

        public ISplineEditedReceiver splineEditedReceiver;

        public RnSplineEditor(ISplineEditedReceiver splineEditedReceiver)
        {
            this.splineEditedReceiver = splineEditedReceiver;
        }

        /// <summary>
        /// 道路の編集パラメータ
        /// スプラインのパラメータもここに含む
        /// </summary>
        private IScriptableRoadMdl parameter;

        /// <summary>
        /// 編集対象の道路グループ
        /// </summary>
        private EditorData<RnRoadGroup> targetRoad;
        private RnRoadGroup RoadGroup => targetRoad.Ref;

        /// <summary>
        /// 初期化時に呼び出す
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="edittingTarget"></param>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Terminate();
            }

            Assert.IsFalse(IsInitialized, "初期化済み");
            IsInitialized = true;

            parameter = null;
            targetRoad = null;
        }

        /// <summary>
        /// 終了時に呼び出す
        /// </summary>
        public void Terminate()
        {
            Assert.IsTrue(IsInitialized, "初期化されていない");

            IsInitialized = false;
        }

        /// <summary>
        /// スプライン編集機能を有効化
        /// </summary>
        public void Enable(IScriptableRoadMdl parameter, EditorData<RnRoadGroup> target)
        {
            Assert.IsTrue(IsInitialized, "初期化されていない");

            IsEnabled = true;

            this.parameter = parameter;
            this.targetRoad = target;

            CreateSpline(target.Ref);

            var core = new SplineEditorCore(spline);


            // TODO: 端点取得の関数化
            var road = target.Ref.Roads[0];
            Vector3 startLeftPoint, startRightPoint, endLeftPoint, endRightPoint;
            {
                var way = road.GetLeftWayOfLanes();
                startLeftPoint = road.MainLanes[0].IsReversed ^ way.IsReversed ? way.LineString.Points.Last().Vertex : way.LineString.Points.First().Vertex;
                endLeftPoint = road.MainLanes[0].IsReversed ^ way.IsReversed ? way.LineString.Points.First().Vertex : way.LineString.Points.Last().Vertex;
            }
            {
                var way = road.GetRightWayOfLanes();
                startRightPoint = road.MainLanes.Last().IsReversed ^ way.IsReversed ? way.LineString.Points.Last().Vertex : way.LineString.Points.First().Vertex;
                endRightPoint = road.MainLanes.Last().IsReversed ^ way.IsReversed ? way.LineString.Points.First().Vertex : way.LineString.Points.Last().Vertex;
            }

            core.SetStartPointConstraint(true,
                startLeftPoint,
                startRightPoint
                );
            core.SetEndPointConstraint(true,
                endLeftPoint,
                endRightPoint
            );
            splineEditHandles = new SplineEditorHandles(core, FinishSplineEdit, CancelSplineEdit);
        }

        /// <summary>
        /// スプライン編集機能を無効化
        /// </summary>
        public void Disable()
        {
            Assert.IsTrue(IsInitialized, "初期化されていない");

            IsEnabled = false;

            spline.Clear();
        }

        public void FinishSplineEdit()
        {
            Apply();
            Disable();
            splineEditedReceiver.OnSplineEdited();
        }

        private void CancelSplineEdit()
        {
            Disable();
            splineEditedReceiver.OnSplineEditCanceled();
        }

        /// <summary>
        /// スプラインの編集結果を道路に適用
        /// </summary>
        private void Apply()
        {
            Assert.IsTrue(IsInitialized, "初期化されていない");

            IsEnabled = false;

            if (spline.Knots.Any() && targetRoad != null)
            {
                ApplySpline();
            }
            spline.Clear();
        }


        public void OnSceneGUI(Object target)
        {
            if (target != roadNetworkObject && target != null && target is MonoBehaviour monoBehaviour)
            {
                roadNetworkObject = monoBehaviour.gameObject;
            }

            if (!IsEnabled)
                return;

            splineEditHandles.HandleSceneGUI();
        }

        private void ApplySpline()
        {
            // FIXME: UIから変更可能にする
            const float medianWidth = 2f;
            const float laneWidth = 3f;

            var road = targetRoad.Ref.Roads[0];
            var totalWidth = road.MainLanes.Count() * laneWidth;
            if (road.MedianLane != null)
            {
                totalWidth += medianWidth;
            }

            MergeSeparatedSideWalks(road, out var leftSideWalk, out var rightSideWalk);

            var offset = totalWidth / 2;
            foreach (var leftLane in road.GetLeftLanes())
            {
                var leftStartPoint = leftLane.LeftWay.Points.First();
                var leftEndPoint = leftLane.LeftWay.Points.Last();

                var nextLeftWayPoints = ConvertSplineToLineStringPointsNonSmooth(spline, offset, leftLane.IsReversed);
                leftLane.LeftWay.SetPointsUnkeepReference(nextLeftWayPoints);
                offset -= laneWidth;

                // 端点のみ元の参照を保持
                leftStartPoint.Vertex = nextLeftWayPoints.First();
                leftLane.LeftWay.SetPoint(0, leftStartPoint);
                leftEndPoint.Vertex = nextLeftWayPoints.Last();
                leftLane.LeftWay.SetPoint(leftLane.LeftWay.Points.Count() - 1, leftEndPoint);

                var rightStartPoint = leftLane.RightWay.Points.First();
                var rightEndPoint = leftLane.RightWay.Points.Last();

                var nextRightWayPoints = ConvertSplineToLineStringPointsNonSmooth(spline, offset, leftLane.IsReversed);
                leftLane.RightWay.SetPointsUnkeepReference(nextRightWayPoints);

                // 端点のみ元の参照を保持
                rightStartPoint.Vertex = nextRightWayPoints.First();
                leftLane.RightWay.SetPoint(0, rightStartPoint);
                rightEndPoint.Vertex = nextRightWayPoints.Last();
                leftLane.RightWay.SetPoint(leftLane.RightWay.Points.Count() - 1, rightEndPoint);
            }

            if (road.MedianLane != null)
            {
                offset -= medianWidth;
            }

            foreach (var rightLane in road.GetRightLanes())
            {
                var rightStartPoint = rightLane.RightWay.Points.First();
                var rightEndPoint = rightLane.RightWay.Points.Last();

                var nextRightWayPoints = ConvertSplineToLineStringPointsNonSmooth(spline, offset, rightLane.IsReversed);
                rightLane.RightWay.SetPointsUnkeepReference(nextRightWayPoints);

                // 端点のみ元の参照を保持
                rightStartPoint.Vertex = nextRightWayPoints.First();
                rightLane.RightWay.SetPoint(0, rightStartPoint);
                rightEndPoint.Vertex = nextRightWayPoints.Last();
                rightLane.RightWay.SetPoint(rightLane.RightWay.Points.Count() - 1, rightEndPoint);

                offset -= laneWidth;

                var leftStartPoint = rightLane.LeftWay.Points.First();
                var leftEndPoint = rightLane.LeftWay.Points.Last();

                var nextLeftWayPoints = ConvertSplineToLineStringPointsNonSmooth(spline, offset, rightLane.IsReversed);
                rightLane.LeftWay.SetPointsUnkeepReference(nextLeftWayPoints);

                // 端点のみ元の参照を保持
                leftStartPoint.Vertex = nextLeftWayPoints.First();
                rightLane.LeftWay.SetPoint(0, leftStartPoint);
                leftEndPoint.Vertex = nextLeftWayPoints.Last();
                rightLane.LeftWay.SetPoint(rightLane.LeftWay.Points.Count() - 1, leftEndPoint);
            }

            // 境界を設定します。
            foreach (var lane in road.GetLeftLanes().Concat(road.GetRightLanes()))
            {
                SetBorder(lane, lane.LeftWay.Points.ToArray(), lane.RightWay.Points.ToArray());
            }

        }

        private void SetBorder(RnLane lane, IReadOnlyList<RnPoint> leftWayPoints, IReadOnlyList<RnPoint> rightWayPoints)
        {
            // 反転を考慮したインデックスを求めます
            int leftWayCount = leftWayPoints.Count;
            int rightWayCount = rightWayPoints.Count;
            int leftWayStart = 0;
            int leftWayEnd = leftWayCount - 1;
            int rightWayStart = 0;
            int rightWayEnd = rightWayCount - 1;

            var prevBorder = lane.PrevBorder;
            var nextBorder = lane.NextBorder;

            // prevBorderを設定します
            if (prevBorder != null && prevBorder.Count > 1)
            {
                var left = leftWayPoints[leftWayStart].Vertex;
                var right = rightWayPoints[rightWayStart].Vertex;
                var v0 = prevBorder.GetPoint(0);
                var v1 = prevBorder.GetPoint(prevBorder.Count - 1); // Borderの頂点数は2であることを仮定します。

                // Borderの向きがLeft->RightなのかRight->Leftかは元の向きに合わせる
                bool isLeftToRight = Vector3.Distance(left, v0.Vertex) < Vector3.Distance(right, v0.Vertex);
                v0.Vertex = isLeftToRight ? left : right;
                v1.Vertex = isLeftToRight ? right : left;

            }

            // nextBorderを設定します
            if (nextBorder != null && nextBorder.Count > 1)
            {
                var left = leftWayPoints[leftWayEnd].Vertex;
                var right = rightWayPoints[rightWayEnd].Vertex;
                var v0 = nextBorder.GetPoint(0);
                var v1 = nextBorder.GetPoint(nextBorder.Count - 1); // Borderの頂点数は2であることを仮定します。

                // Borderの向きがLeft->RightなのかRight->Leftかは元の向きに合わせる
                bool isLeftToRight = Vector3.Distance(left, v0.Vertex) < Vector3.Distance(right, v0.Vertex);
                v0.Vertex = isLeftToRight ? left : right;
                v1.Vertex = isLeftToRight ? right : left;
            }
        }

        private void CreateSpline(RnRoadGroup roadGroup)
        {
            roadGroup.TryCreateSimpleSpline(out var spline, out var width);
            List<BezierKnot> knots = new List<BezierKnot>();
            BezierKnot prevKnot = new BezierKnot();
            foreach (var knot in spline.Knots)
            {
                // 重複しているノットがあるので削除
                if (prevKnot.Position.x == knot.Position.x &&
                    prevKnot.Position.z == knot.Position.z)
                    continue;

                var newKnot = knot;
                newKnot.Position += new float3(0f, 0f, 0f);
                knots.Add(newKnot);

                prevKnot = knot;
            }

            spline.Knots = knots.ToArray();

            this.spline = spline;
        }

        /// <summary>
        /// SplineからLineStringへの変換について、スムーズ処理を行わずにノットをそのまま点として追加します。
        /// 以前はスムーズ処理をしていましたが、編集するたびに点が増えていくことから取りやめました。
        /// </summary>
        private List<Vector3> ConvertSplineToLineStringPointsNonSmooth(Spline spline, float offset,
            bool isReversed)
        {
            var destPoints = new List<Vector3>();
            float t = isReversed ? 1f : 0f;

            // 始点に頂点を追加
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            var firstPoint = GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset);
            destPoints.Add(firstPoint);

            // 始点と終点を除くループ
            for (int i = 1; i < spline.Count - 1; i++)
            {
                t = (float)i / (float)(spline.Count - 1);
                if (isReversed) t = 1f - t;
                // 30m以上離れているか0.5度以上角度が異なる場合に頂点として追加
                spline.GetPointAtLinearDistance(t, isReversed ? -1f : 1f, out float newT);
                var newPoint = spline.EvaluatePosition(newT);
                var newTangent = spline.EvaluateTangent(newT);

                if (Vector3.Distance(prevPoint, newPoint) > 30 || Vector3.Angle(prevTangent, newTangent) > 0.5)
                {
                    destPoints.Add(GetOffsetPointToNormalDirection(newPoint, newTangent, offset));
                    prevPoint = newPoint;
                    prevTangent = newTangent;
                }
            }

            // 終点を追加
            var lastPoint = new RnPoint()
            {
                Vertex = GetOffsetPointToNormalDirection(
                    spline.EvaluatePosition(isReversed ? 0f : 1f),
                    spline.EvaluateTangent(isReversed ? 0f : 1f),
                    offset)
            };
            destPoints.Add(lastPoint);
            return destPoints;
        }

        private void ConvertSplineToLineStringPointsSmooth(Spline spline, ref List<RnPoint> destPoints, float offset, bool isReversed)
        {
            var firstPoint = destPoints.First();
            var lastPoint = destPoints.Last();
            destPoints.Clear();

            // 始点に頂点を追加
            float t = isReversed ? 1f : 0f;
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            firstPoint.Vertex = GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset);
            destPoints.Add(firstPoint);

            while (isReversed ? t > 0f : t < 1f)
            {
                // 1m毎にスプライン上の点を取ってきて、30m以上離れているか5度以上角度が異なる場合に頂点として追加
                spline.GetPointAtLinearDistance(t, isReversed ? -1f : 1f, out float newT);
                var newPoint = spline.EvaluatePosition(newT);
                var newTangent = spline.EvaluateTangent(newT);

                if (Vector3.Distance(prevPoint, newPoint) > 30 || Vector3.Angle(prevTangent, newTangent) > 5)
                {
                    destPoints.Add(new RnPoint(GetOffsetPointToNormalDirection(newPoint, newTangent, offset)));
                    prevPoint = newPoint;
                    prevTangent = newTangent;
                }

                t = newT;

                if (isReversed ? t <= 0f : t >= 1f)
                {
                    // 終点に頂点を追加
                    lastPoint.Vertex = GetOffsetPointToNormalDirection(
                        spline.EvaluatePosition(isReversed ? 0f : 1f),
                        spline.EvaluateTangent(isReversed ? 0f : 1f),
                        offset);
                    destPoints.Add(lastPoint);
                }
            }
        }

        private Vector3 GetOffsetPointToNormalDirection(Vector3 point, Vector3 tangent, float offset)
        {
            var normal = Vector3.Cross(tangent, Vector3.up).normalized;
            return point + normal * offset;
        }

        private static void MergeSeparatedSideWalks(RnRoad road, out RnSideWalk leftSideWalk, out RnSideWalk rightSideWalk)
        {
            leftSideWalk = null;
            rightSideWalk = null;

            // Insideを左端のWay(LeftLaneのRightWay)と共有しているような歩道を列挙
            var leftWay = road.GetLeftWayOfLanes();
            var rightWay = road.GetRightWayOfLanes();

            var leftSideWalkAndSharedPoints = new List<(RnSideWalk, HashSet<RnPoint>)>();
            var rightSideWalkAndSharedPoints = new List<(RnSideWalk, HashSet<RnPoint>)>();

            foreach (var sidewalk in road.SideWalks)
            {
                var leftSharedPoints = GetSharedPointSet(leftWay, sidewalk.InsideWay);
                if (leftSharedPoints.Count >= 1)
                {
                    leftSideWalkAndSharedPoints.Add((sidewalk, leftSharedPoints));
                    leftSideWalk = sidewalk;
                }

                var rightSharedPoints = GetSharedPointSet(rightWay, sidewalk.InsideWay);
                if (rightSharedPoints.Count >= 1)
                {
                    rightSideWalkAndSharedPoints.Add((sidewalk, rightSharedPoints));
                    rightSideWalk = sidewalk;
                }
            }

            if (leftSideWalkAndSharedPoints.Count > 1)
            {
                MergeSeparatedWalkWays(road, leftSideWalkAndSharedPoints, leftWay, out leftSideWalk);
            }

            if (rightSideWalkAndSharedPoints.Count > 1)
            {
                MergeSeparatedWalkWays(road, rightSideWalkAndSharedPoints, rightWay, out rightSideWalk);
            }
        }

        private static void MergeSeparatedWalkWays(RnRoad road, List<(RnSideWalk, HashSet<RnPoint>)> sideWalkAndSharedPointSets,
            RnWay sideWay, out RnSideWalk newSideWalk)
        {
            // leftWayの点の順番でSideWalkを並べ替える
            sideWalkAndSharedPointSets.Sort((a, b) =>
            {
                var aFirstPoint = a.Item2.First();
                var bFirstPoint = b.Item2.First();
                return sideWay.LineString.Points.IndexOf(aFirstPoint) - sideWay.LineString.Points.IndexOf(bFirstPoint);
            });

            // 並べ替えたWalkWayを結合
            var outsidePoints = new List<RnPoint>();
            int cnt = 0;
            foreach (var (sideWalk, _) in sideWalkAndSharedPointSets)
            {
                var isFirstWay = cnt == 0;
                var isLastWay = cnt == sideWalkAndSharedPointSets.Count - 1;
                cnt++;

                var separatedOutsidePoints = sideWalk.OutsideWay.LineString.Points;
                int firstPointIndex;
                int lastPointIndex;
                if (isFirstWay)
                {
                    firstPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.First())
                        ? int.MinValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.First());
                    lastPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.Last())
                        ? int.MinValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.Last());
                }
                else if (isLastWay)
                {
                    firstPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.First())
                        ? int.MaxValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.First());
                    lastPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.Last())
                        ? int.MaxValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.Last());
                }
                else
                {
                    firstPointIndex = sideWay.LineString.Points.IndexOf(separatedOutsidePoints.First());
                    lastPointIndex = sideWay.LineString.Points.IndexOf(separatedOutsidePoints.Last());
                }

                if (firstPointIndex > lastPointIndex)
                    separatedOutsidePoints.Reverse();

                for (int i = 0; i < separatedOutsidePoints.Count; ++i)
                {
                    // 端の点は接続情報を保持するために参照を保持
                    if (i == 0 && isFirstWay)
                    {
                        outsidePoints.Add(separatedOutsidePoints[i]);
                        continue;
                    }
                    if (i == separatedOutsidePoints.Count - 1 && isLastWay)
                    {
                        outsidePoints.Add(separatedOutsidePoints[i]);
                        continue;
                    }

                    outsidePoints.Add(new RnPoint(separatedOutsidePoints[i]));
                }
                road.RemoveSideWalk(sideWalk);
            }
            var outsideWay = new RnWay(new RnLineString(outsidePoints.AsReadOnly()));
            var insideWay = new RnWay(sideWay.LineString);
            var startEdgeWay = sideWalkAndSharedPointSets.First().Item1.StartEdgeWay;
            var endEdgeWay = sideWalkAndSharedPointSets.Last().Item1.EndEdgeWay;
            newSideWalk = RnSideWalk.Create(road, outsideWay, insideWay, startEdgeWay, endEdgeWay);
            road.AddSideWalk(newSideWalk);
        }

        private static HashSet<RnPoint> GetSharedPointSet(RnWay way1, RnWay way2)
        {
            var way1PointSet = new HashSet<RnPoint>(way1.LineString.Points);
            var way2PointSet = new HashSet<RnPoint>(way2.LineString.Points);

            way1PointSet.IntersectWith(way2PointSet);
            return way1PointSet;
        }

        private static bool TryGetIndexOfPoint(RnWay way, RnPoint point, out int index)
        {
            index = way.LineString.Points.IndexOf(point);
            return index != -1;
        }

        private static bool IsEdgePoint(RnSideWalk sideWalk, RnPoint point)
        {
            if (sideWalk.StartEdgeWay != null && sideWalk.StartEdgeWay.LineString.Points.Contains(point))
                return true;
            if (sideWalk.EndEdgeWay != null && sideWalk.EndEdgeWay.LineString.Points.Contains(point))
                return true;

            return false;
        }
    }

    internal interface ISplineEditedReceiver
    {
        public void OnSplineEdited();
        public void OnSplineEditCanceled();
    }
}