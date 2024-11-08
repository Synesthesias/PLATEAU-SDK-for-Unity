using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

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
            // FIXME: UIで外だし
            const float medianWidth = 3f;
            const float laneWidth = 5f;

            var firstRoad = edittingTarget.Ref.Roads[0];
            var totalWidth = firstRoad.AllLanes.Count() * laneWidth;
            if (firstRoad.MedianLane != null)
            {
                totalWidth += medianWidth;
            }

            foreach (var road in edittingTarget.Ref.Roads)
            {
                var offset = totalWidth / 2;
                List<RnPoint> points;
                foreach (var leftLane in road.GetLeftLanes())
                {
                    points = leftLane.LeftWay.LineString.Points;
                    ConvertSplineToLineStringPoints(spline, ref points, offset, true);
                    Debug.Log($"Left Lane Left Way, Offset: {offset}");
                    offset -= laneWidth;

                    points = leftLane.RightWay.LineString.Points;
                    ConvertSplineToLineStringPoints(spline, ref points, offset, true);
                    Debug.Log($"Left Lane Right Way, Offset: {offset}");
                }

                if (road.MedianLane != null)
                {
                    offset -= medianWidth;
                }

                foreach (var rightLane in road.GetRightLanes())
                {
                    points = rightLane.RightWay.LineString.Points;
                    ConvertSplineToLineStringPoints(spline, ref points, offset, true);
                    Debug.Log($"Right Lane Right Way, Offset: {offset}");

                    offset -= laneWidth;

                    points = rightLane.LeftWay.LineString.Points;
                    ConvertSplineToLineStringPoints(spline, ref points, offset, false);
                    Debug.Log($"Right Lane Left Way, Offset: {offset}");
                }
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
            roadGroup.TryCreateSpline(out var spline, out var width);
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


        private void ConvertSplineToLineStringPoints(Spline spline, ref List<RnPoint> destPoints, float offset, bool isLeft)
        {
            var firstPoint = destPoints.First();
            var lastPoint = destPoints.Last();
            destPoints.Clear();

            // 始点に頂点を追加
            float t = isLeft ? 0f : 1f;
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            firstPoint.Vertex = GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset);
            destPoints.Add(firstPoint);

            while (isLeft ? t < 1f : t > 0f)
            {
                // 1m毎にスプライン上の点を取ってきて、10m以上離れているか20度以上角度が異なる場合に頂点として追加
                spline.GetPointAtLinearDistance(t, isLeft ? 1f : -1f, out float newT);
                var newPoint = spline.EvaluatePosition(newT);
                var newTangent = spline.EvaluateTangent(newT);

                if (Vector3.Distance(prevPoint, newPoint) > 30 || Vector3.Angle(prevTangent, newTangent) > 20)
                {
                    destPoints.Add(new RnPoint(GetOffsetPointToNormalDirection(newPoint, newTangent, offset)));
                    prevPoint = newPoint;
                    prevTangent = newTangent;
                }

                t = newT;

                if (isLeft ? t >= 1f : t <= 0f)
                {
                    // 終点に頂点を追加
                    lastPoint.Vertex = GetOffsetPointToNormalDirection(
                        spline.EvaluatePosition(isLeft ? 1f : 0f),
                        spline.EvaluateTangent(isLeft ? 1f : 0f),
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
    }
}
