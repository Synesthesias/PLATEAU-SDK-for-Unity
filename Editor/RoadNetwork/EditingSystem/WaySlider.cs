using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路のレーンをドラッグして編集する機能です。
    /// </summary>
    internal class WaySlider
    {
        SlideState currentState = SlideState.Default;
        public WayCalcData WaySlideCalcCache { get; private set; }
        private bool isMouseDownHold;
        private Vector2 mousePosDragStart;
        
        /// <summary> 誤操作防止のため、線のドラッグ操作で動いたマウスのピクセル数がこの数未満であればドラッグをキャンセルします。 </summary>
        private const float MinMouseDragPixels = 4f; 
        

        /// <summary>
        /// ドラッグしてスライド中の<see cref="RnWay"/>を返します。
        /// スライド中のものがなければnullを返します。
        /// </summary>
        public RnWay Draw(RoadNetworkEditSceneViewGui editSceneViewGui, RoadNetworkEditTarget editTarget, SceneView sceneView, out bool isRoadChanged)
        {
            isRoadChanged = false;
            var mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (editTarget.SelectedRoadNetworkElement is EditorData<RnRoadGroup> roadGroupEditorData)
            {
                var wayEditorDataList = roadGroupEditorData.ReqSubData<WayEditorDataList>();

                bool isSelectable = !editSceneViewGui.SplineEditorMod.IsEnabled;
                wayEditorDataList.SetSelectable(isSelectable);
                
                switch (currentState)
                {
                    case SlideState.Default:
                        WaySlideCalcCache = null;
                        SelectWay(ray, wayEditorDataList);
                        break;
                    case SlideState.SlidingWay:
                        {
                            var dis = LineUtil.FindClosestPoint(WaySlideCalcCache.ClosestLine, ray, out var closestPointOnWay,
                                out var closestPointOnRay);
                            WaySlideCalcCache.Set(dis, closestPointOnWay, closestPointOnRay);
                            break;
                        }
                }
            }

            // dummyのwayを表示する。ここでいうdummyとは、Wayをスライドさせる際に表示する仮のWayのこと
            RnWay slidingWay = null;

            var evt = Event.current;
            var mouseDown = LineUtil.IsMouseDown();
            var mouseUp = evt.type == EventType.MouseUp && evt.button == 0;
            if (mouseDown) isMouseDownHold = true;
            if (mouseUp) isMouseDownHold = false;

            if (isMouseDownHold)
            {
                switch (currentState)
                {
                    // Wayを押下した瞬間。DefaultからWaitForDragに遷移。
                    case SlideState.Default:
                        if (WaySlideCalcCache != null)
                        {
                            currentState = SlideState.WaitForDrag;
                            mousePosDragStart = mousePos;
                        }
                        break;
                    // 線のドラッグ待ち。
                    case SlideState.WaitForDrag:
                        if (Vector2.Distance(mousePos, mousePosDragStart) >= MinMouseDragPixels)
                        {
                            currentState = SlideState.SlidingWay;
                        }
                        break;
                    
                    // WayをSlide中
                    case SlideState.SlidingWay:
                        if (WaySlideCalcCache != null)
                        {
                            slidingWay = OnSlidingWay(sceneView);
                        }

                        break;
                }
            }
            else // マウスが押されていないとき
            {
                switch (currentState)
                {
                    // Wayのスライドの終了。SlidingWayからDefaultに遷移。
                    case SlideState.SlidingWay:
                        slidingWay = OnEndSlidingWay(sceneView);
                        isRoadChanged = true;
                        break;
                    // 線はドラッグされなかった。Defaultに遷移。
                    case SlideState.WaitForDrag:
                        currentState = SlideState.Default;
                        isRoadChanged = false;
                        break;
                        
                }
            }

            return slidingWay;
        }

        private RnWay OnSlidingWay(SceneView sceneView)
        {
            if (WaySlideCalcCache.ClosestDis <= 0) return null;
            var slidingWay = CalcSlidingWay(sceneView);

            return slidingWay;
        }

        private RnWay OnEndSlidingWay(SceneView sceneView)
        {
            Assert.IsNotNull(WaySlideCalcCache);
            RnWay slidingWay = null;
            if (WaySlideCalcCache.ClosestDis > 0)
            {
                // Wayを動かした結果
                slidingWay = CalcSlidingWay(sceneView);
                

                //元のwayに適用
                var points = WaySlideCalcCache.ClosestWay.Ref.Points;
                points = WaySlideCalcCache.ClosestWay.Ref.IsReversed ? points.Reverse() : points;
                var distWayPoints = points.ToArray();
                var eRevDumWay = slidingWay.ToArray();
                for (int i = 0; i < distWayPoints.Length; i++)
                {
                    var current = distWayPoints[i];
                    current.Vertex = eRevDumWay[i];
                }
            }

            WaySlideCalcCache = null;
            currentState = SlideState.Default;
            return slidingWay;
        }

        private RnWay CalcSlidingWay(SceneView sceneView)
        {
            // カメラ視点からwayに対して外積を取る　これによってwayの右側、左側を定義する
            // 最近傍2点way->rayでwayのどの方向に延びているかを算出
            // 内積を取ることでベクトルが同じ方向を向いているかを調べる

            var vecCamera2Way = WaySlideCalcCache.ClosestPointOnWay -
                                sceneView.camera.transform.position;
            var line = WaySlideCalcCache.ClosestWay.Ref.IsReversed
                ? WaySlideCalcCache.ClosestLine.VecB2A
                : WaySlideCalcCache.ClosestLine.VecA2B;
            var wayRightVec = Vector3.Cross(vecCamera2Way, line);

            var vecWay2Ray = WaySlideCalcCache.ClosestPointOnRay - WaySlideCalcCache.ClosestPointOnWay;
            var isRayOnRightSide = Vector3.Dot(wayRightVec, vecWay2Ray) > 0;

            var slidingWay = new RnWay(WaySlideCalcCache.ClosestWay.Ref.LineString.Clone(true));
            var dirFactor = isRayOnRightSide ? 1f : -1f;
            
            slidingWay.MoveAlongNormal(WaySlideCalcCache.ClosestDis * dirFactor);
            return slidingWay;
        }
        
        private void SelectWay(Ray ray, WayEditorDataList wayEditorDataList)
        {
            if (wayEditorDataList == null)
            {
                return;
            }

            var dataList = wayEditorDataList.Raw;

            const float radius = 2.0f;
            foreach (var wayEditorData in dataList)
            {
                if (wayEditorData.IsSelectable == false)
                    continue;

                if (wayEditorData.IsSelectable == false)
                {
                    continue;
                }

                var eVert = wayEditorData.Ref.Vertices.GetEnumerator();
                eVert.MoveNext();
                var p0 = eVert.Current;
                while (eVert.MoveNext())
                {
                    var p1 = eVert.Current;
                    var line = new LineUtil.Line(p0, p1);
                    var distance = LineUtil.CheckHit(line, radius, ray,
                        out var closestPoint, out var closestPoint2);
                    //var distance = LineUtil.CheckDistance(line, radius, ray);
                    if (distance >= 0.0f)
                    {
                        if (WaySlideCalcCache == null)
                            WaySlideCalcCache = new WayCalcData();
                        if (WaySlideCalcCache.ClosestDis > distance)
                        {
                            WaySlideCalcCache.Set(
                                wayEditorData, distance, line, closestPoint, closestPoint2);
                        }
                    }

                    p0 = p1;
                }
            }
        }


        private enum SlideState
        {
            Default, // 通常の状態
            WaitForDrag, // 誤操作防止のため、マウスドラッグがなければ線を動かさない。ドラッグ待ち状態。ドラッグがなければDefaultに戻り、ドラッグがあればSlidingWayに遷移。
            SlidingWay, // Wayをスライド中
        }
        
        internal class WayCalcData
        {
            
            public WayEditorData ClosestWay { get; private set; } = null;
            public float ClosestDis { get; private set; } = float.MaxValue;
            public LineUtil.Line ClosestLine { get; private set; }
            public Vector3 ClosestPointOnWay { get; private set; } // 
            public Vector3 ClosestPointOnRay { get; private set; } // 
            

            /// <summary>
            /// すべての値を同時に設定する
            /// </summary>
            /// <param name="closestWay"></param>
            /// <param name="closestDis"></param>
            /// <param name="closestLine"></param>
            /// <param name="closestPointOnWay"></param>
            /// <param name="closestPointOnRay"></param>
            public void Set(
                WayEditorData closestWay,
                float closestDis,
                LineUtil.Line closestLine,
                Vector3 closestPointOnWay, // 
                Vector3 closestPointOnRay)
            {
                this.ClosestWay = closestWay;
                this.ClosestDis = closestDis;
                this.ClosestLine = closestLine;
                this.ClosestPointOnWay = closestPointOnWay;
                this.ClosestPointOnRay = closestPointOnRay;
            }

            public void Set(
                float closestDis,
                Vector3 closestPointOnWay, // 
                Vector3 closestPointOnRay)
            {
                this.ClosestDis = closestDis;
                this.ClosestPointOnWay = closestPointOnWay;
                this.ClosestPointOnRay = closestPointOnRay;
            }
            
        }
    }
}