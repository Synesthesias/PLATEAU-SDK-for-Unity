using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプラインによる道路編集機能を提供する
    /// </summary>
    public class RnSplineEditing
    {
        private SplineInstantiate splineInstantiate;
        private SplineContainer splineContainer;
        private Spline spline = new Spline();

        public bool IsInitialized { get; private set; } = false;


        public RnSplineEditing()
        {
        }

        /// <summary>
        /// 処理フローのサンプル
        /// 想定している処理フローは以下の通り
        /// </summary>
        private static void SampleFlow()
        {
            // インスタンス生成
            var instance = new RnSplineEditing();

            // 初期化
            instance.Initialize();

            // 道路を選択
            var selectRoadGroup = (EditorData<RnRoadGroup>)null;
            var parameter = (IScriptableRoadMdl)null;

            // スプライン機能を起動
            instance.Enable(parameter, selectRoadGroup);

            // 別の道路を選択
            var newSelectRoadGroup = (EditorData<RnRoadGroup>)null;
            var newParameter = (IScriptableRoadMdl)null;

            // 現在のスプラインを終了
            instance.Disable();

            // 新しいスプラインを開始
            instance.Enable(newParameter, newSelectRoadGroup);

            // スプラインの編集結果を道路に適用
            instance.Apply();

            // スプライン機能を完全に終了 
            instance.Terminate();
        }

        /// <summary>
        /// 道路の編集パラメータ
        /// スプラインのパラメータもここに含む
        /// </summary>
        private IScriptableRoadMdl parameter;

        /// <summary>
        /// 編集対象の道路グループ
        /// </summary>
        private EditorData<RnRoadGroup> edittingTarget;
        private RnRoadGroup RoadGroup => edittingTarget.Ref;

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

            Debug.Log("Initialize()");
            Assert.IsFalse(IsInitialized, "既に初期化されています。");
            IsInitialized = true;

            parameter = null;
            edittingTarget = null;

            InitializeComponents();
        }


        /// <summary>
        /// 終了時に呼び出す
        /// </summary>
        public void Terminate()
        {
            Debug.Log("Terminate()");
            Assert.IsTrue(IsInitialized, "初期化されていない");
            IsInitialized = false;
        }

        /// <summary>
        /// スプライン編集機能を有効化
        /// </summary>
        public void Enable(IScriptableRoadMdl parameter, EditorData<RnRoadGroup> edittingTarget)
        {
            Debug.Log("Enable()");
            Assert.IsTrue(IsInitialized, "初期化されていない");

            this.parameter = parameter;
            this.edittingTarget = edittingTarget;

            CreateSpline(edittingTarget.Ref);

            // スプライン編集モードにする。(参考：EditorSplineUtility.SetKnotPlacementTool())
            EditorApplication.delayCall += ToolManager.SetActiveContext<SplineToolContext>;
        }

        /// <summary>
        /// スプライン編集機能を無効化
        /// </summary>
        public void Disable()
        {
            Debug.Log("Disable()");
            Assert.IsTrue(IsInitialized, "初期化されていない");

            spline.Clear();
            EditorApplication.delayCall += ToolManager.SetActiveContext<GameObjectToolContext>;
        }

        /// <summary>
        /// スプラインの編集結果を道路に適用
        /// </summary>
        public void Apply()
        {
            Debug.Log("Apply()");
            Assert.IsTrue(IsInitialized, "初期化されていない");

            // RoadGroup.Roads.Clear();
            // CreateLanes(parameter.NumLeftLane, paremeter.NumRightLane);

            if (spline.Knots.Any() && edittingTarget != null)
            {
                ApplySpline();
            }
            spline.Clear();
        }

        private void ApplySpline()
        {
            // FIXME: UIから取得する
            const float medianWidth = 2f;
            const float laneWidth = 3f;

            var road = edittingTarget.Ref.Roads[0];
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
                ConvertSplineToLineStringPoints(spline, ref points, offset, !leftLane.LeftWay.IsReversed);
                Debug.Log($"Left Lane Left Way, Offset: {offset}");
                offset -= laneWidth;

                points = leftLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, !leftLane.RightWay.IsReversed);
                Debug.Log($"Left Lane Right Way, Offset: {offset}");
            }

            if (road.MedianLane != null)
            {
                offset -= medianWidth;
            }

            foreach (var rightLane in road.GetRightLanes())
            {
                points = rightLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, rightLane.RightWay.IsReversed);
                Debug.Log($"Right Lane Right Way, Offset: {offset}");

                offset -= laneWidth;

                points = rightLane.LeftWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, rightLane.LeftWay.IsReversed);
                Debug.Log($"Right Lane Left Way, Offset: {offset}");
            }
        }

        private void InitializeComponents()
        {
            // FIXME: 複数ある場合の対応が必要
            var roadNetworkObject = GameObject.FindObjectOfType<PLATEAURnStructureModel>().gameObject;

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
                // 同じ座標のノットは追加しない
                if (prevKnot.Position.x == knot.Position.x &&
                    prevKnot.Position.y == knot.Position.y &&
                    prevKnot.Position.z == knot.Position.z)
                    continue;
                knots.Add(knot);

                prevKnot = knot;
            }

            spline.Knots = knots.ToArray();

            // 一番編集しやすそうなのでモードはAutoSmoothにする。
            for (int i = 0; i < spline.Knots.Count(); i++)
            {
                spline.SetTangentMode(i, TangentMode.AutoSmooth);
            }

            splineContainer.Splines = new Spline[] { spline };
            this.spline = spline;
        }


        private void ConvertSplineToLineStringPoints(Spline spline, ref List<RnPoint> destPoints, float offset, bool isForward)
        {
            var firstPoint = destPoints.First();
            var lastPoint = destPoints.Last();
            destPoints.Clear();

            // 始点に頂点を追加
            float t = isForward ? 0f : 1f;
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            firstPoint.Vertex = GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset);
            destPoints.Add(firstPoint);

            while (isForward ? t < 1f : t > 0f)
            {
                // 1m毎にスプライン上の点を取ってきて、10m以上離れているか20度以上角度が異なる場合に頂点として追加
                spline.GetPointAtLinearDistance(t, isForward ? 1f : -1f, out float newT);
                var newPoint = spline.EvaluatePosition(newT);
                var newTangent = spline.EvaluateTangent(newT);

                if (Vector3.Distance(prevPoint, newPoint) > 30 || Vector3.Angle(prevTangent, newTangent) > 20)
                {
                    destPoints.Add(new RnPoint(GetOffsetPointToNormalDirection(newPoint, newTangent, offset)));
                    prevPoint = newPoint;
                    prevTangent = newTangent;
                }

                t = newT;

                if (isForward ? t >= 1f : t <= 0f)
                {
                    // 終点に頂点を追加
                    lastPoint.Vertex = GetOffsetPointToNormalDirection(
                        spline.EvaluatePosition(isForward ? 1f : 0f),
                        spline.EvaluateTangent(isForward ? 1f : 0f),
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

        private static void MergeSeparatedSideWalks(RnRoad road, out RnSideWalk leftSideWalk, out RnSideWalk  rightSideWalk)
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

            Debug.Log($"left:{leftSideWalkAndSharedPoints.Count} right:{rightSideWalkAndSharedPoints.Count}");
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
                } else if (isLastWay)
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
}
