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
        private SplineInstantiate splineInstantiate;
        private SplineContainer splineContainer;
        private Spline spline = new Spline();
        private GameObject roadNetworkObject;
        private SplineEditorHandles splineEditHandles;

        public bool IsInitialized { get; private set; } = false;
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
            core.SetStartPointConstraint(true,
                target.Ref.Roads[0].GetLeftWayOfLanes().LineString.Points[0].Vertex,
                target.Ref.Roads[0].GetRightWayOfLanes().LineString.Points[^1].Vertex
                );
            core.SetEndPointConstraint(true,
                target.Ref.Roads[0].GetLeftWayOfLanes().LineString.Points[^1].Vertex,
                target.Ref.Roads[0].GetRightWayOfLanes().LineString.Points[0].Vertex
            );
            splineEditHandles = new SplineEditorHandles(core, FinishSplineEdit);
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
                InitializeSplineComponents();
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
            var totalWidth = road.AllLanes.Count() * laneWidth;
            if (road.MedianLane != null)
            {
                totalWidth += medianWidth;
            }

            MergeSeparatedSideWalks(road, out var leftSideWalk, out var rightSideWalk);

            var offset = totalWidth / 2;
            List<RnPoint> points;
            foreach (var leftLane in road.GetLeftLanes())
            {
                points = leftLane.LeftWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, leftLane.IsReverse ^ leftLane.LeftWay.IsReversed);
                offset -= laneWidth;

                points = leftLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, leftLane.IsReverse ^ leftLane.RightWay.IsReversed);
            }

            if (road.MedianLane != null)
            {
                offset -= medianWidth;
            }

            foreach (var rightLane in road.GetRightLanes())
            {
                points = rightLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, rightLane.IsReverse ^ rightLane.RightWay.IsReversed);

                offset -= laneWidth;

                points = rightLane.LeftWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, rightLane.IsReverse ^ rightLane.LeftWay.IsReversed);
            }
        }

        private void InitializeSplineComponents()
        {
            splineContainer = roadNetworkObject.GetComponent<SplineContainer>();
            splineContainer ??= roadNetworkObject.AddComponent<SplineContainer>();
            splineInstantiate = roadNetworkObject.GetComponent<SplineInstantiate>();
            splineInstantiate ??= roadNetworkObject.AddComponent<SplineInstantiate>();
            splineInstantiate.Container = splineContainer;
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

            splineContainer.Splines = new Spline[] { spline };
            this.spline = spline;
        }

        private void ConvertSplineToLineStringPoints(Spline spline, ref List<RnPoint> destPoints, float offset, bool isReversed)
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
    }
}