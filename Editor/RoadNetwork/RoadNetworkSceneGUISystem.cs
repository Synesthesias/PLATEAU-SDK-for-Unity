using System;
using PLATEAU.RoadNetwork;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Linq;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.RoadNetwork
{
    /// <summary>
    /// SceneGUIまわりの機能を管理するクラス
    /// 記述先のファイルを変更するかも？
    /// </summary>
    public class RoadNetworkSceneGUISystem
    {
        public RoadNetworkSceneGUISystem(IRoadNetworkEditingSystem editorSystem)
        {
            Assert.IsNotNull(editorSystem);
            this.editorSystem = editorSystem;
        }

        private const float pointHndScaleFactor = 0.1f;
        private const float laneHndScaleFactor = 0.4f;
        private const float linkHndScaleFactor = 0.5f;
        private const float signalLightHndScaleFactor = 0.2f;
        private const float signalControllerScaleFactor = 0.3f;
        private readonly Vector3 selectBtnPosOffset = Vector3.up * 10.0f;

        private IRoadNetworkEditingSystem editorSystem;

        /// <summary>
        /// OnSceneGUI()内での状態
        /// </summary>
        private struct SceneGUIState
        {
            public bool isDirtyTarget;      // ターゲットに変更があったか
            public Action delayCommand;     // 遅延コマンド　要素の追加や削除を行う際に利用する foreach外で利用する 

            // cache
            public Vector3 linkPos;
            public Vector3 lanePos;

            public Vector3 nodePos;
            public Vector3 signalControllerPos;
            public Vector3 signalLightPos;

            // loop operation
            public bool isContinue;
            public bool isBreak;
            internal Camera currentCamera;

            public void ResetLoopOperationFlags()
            {
                isContinue = false;
                isBreak = false;
            }
        };

        /// <summary>
        /// クラス内の状態
        /// </summary>
        private struct SceneGUISystemState
        {
            public void Init(out SceneGUIState state)
            {
                state = new SceneGUIState
                {
                    isDirtyTarget = false,
                    delayCommand = null,
                };
            }
            public void Apply(in SceneGUIState state)
            {

            }
        };
        private SceneGUISystemState systemState;

        public void OnSceneGUI(UnityEngine.Object target)
        {
            SetRoadNetworkObject2System(target);
            var network = GetRoadNetwork();
            if (network == null)
                return;

            // ステイトの初期化
            SceneGUIState state;
            systemState.Init(out state);

            var currentCamera = SceneView.currentDrawingSceneView.camera;
            state.currentCamera = currentCamera;

            // ハンドルの配置、要素数を変化させない値変更、遅延実行用のコマンド生成を行う
            // 遅延実行用のコマンドは1フレームにつき一つまで実行できるとする(要素削除順の管理などが面倒なため)
            Update3DHandle(network, ref state);

            // 編集モードの状態表示
            // 2D GUI
            var sceneViewPixelRect = currentCamera.pixelRect;
            var guiLayoutRect = new Rect(sceneViewPixelRect.position + sceneViewPixelRect.center, sceneViewPixelRect.size / 2.0f);
            Handles.BeginGUI();
            GUILayout.BeginArea(guiLayoutRect);
            GUILayout.Box("道路ネットワーク編集モード");
            GUILayout.EndArea();
            Handles.EndGUI();

            // 遅延実行 コレクションの要素数などを変化させる
            if (state.delayCommand != null)
                state.delayCommand.Invoke();

            // 変更を通知する
            if (state.isDirtyTarget)
            {
                editorSystem.NotifyChangedRoadNetworkObject2Editor();
            }

            systemState.Apply(state);
        }


        private SceneGUIState Update3DHandle(RoadNetworkModel network, ref SceneGUIState state)
        {

            // Node
            foreach (var node in network.Nodes)
            {
                state.ResetLoopOperationFlags();
                ForeachNode(editorSystem, network.Nodes, node, ref state);
                if (state.isBreak) break;
                if (state.isContinue) continue;

                // SignalController
                foreach (var signalController in new TrafficSignalLightController[1] { node.SignalController })
                {
                    state.ResetLoopOperationFlags();
                    ForeachSignalController(editorSystem, signalController, ref state);
                    if (state.isBreak) break;
                    if (state.isContinue) continue;

                    // signalLight
                    foreach (var signalLight in signalController.SignalLights)
                    {
                        state.ResetLoopOperationFlags();
                        ForeachSignalLight(editorSystem, signalController.SignalLights, signalLight, ref state);

                        if (state.isBreak) break;
                        if (state.isContinue) continue;

                    }
                }
            }

            // link
            foreach (var link in network.Links)
            {
                state.ResetLoopOperationFlags();
                ForeachLink(editorSystem, network.Links, link, ref state);
                if (state.isBreak) break;
                if (state.isContinue) continue;

                state.linkPos = CalcLinkPos(link);

                // lane
                foreach (var lane in link.MainLanes)
                {
                    state.ResetLoopOperationFlags();
                    ForeachLane(editorSystem, link.MainLanes, lane, ref state);
                    //ForeachLanes(editorSystem, link.MainLanes, lane, ref state);
                    if (state.isBreak) break;
                    if (state.isContinue) continue;

                    // way
                    foreach (var way in lane.BothWays)
                    {
                        state.ResetLoopOperationFlags();
                        if (state.isBreak) break;
                        if (state.isContinue) continue;

                        // point
                        foreach (var point in way.Points)
                        {
                            state.ResetLoopOperationFlags();
                            ForeachPoints(editorSystem, point, ref state);
                            if (state.isBreak) break;
                            if (state.isContinue) continue;
                        }
                    }
                }
            }

            return state;
        }

        private bool SetRoadNetworkObject2System(UnityEngine.Object target)
        {
            editorSystem.RoadNetworkObject = target;
            return editorSystem.RoadNetworkObject != null;
        }

        private RoadNetworkModel GetRoadNetwork()
        {
            return editorSystem.RoadNetwork;
        }

        private void ForeachLane(IRoadNetworkEditingSystem editorSystem, IReadOnlyList<RoadNetworkLane> mainLanes, RoadNetworkLane lane, ref SceneGUIState state)
        {
            state.lanePos = CalcLanePos(lane);

            bool isDisplay = false;
            if (lane != editorSystem.SelectedRoadNetworkElement as RoadNetworkLane)
            {
                isDisplay = true;
            }

            if (isDisplay)
            {
                state.isContinue = true;
            }

            if (isDisplay)
            {
                // レーンの選択ボタンの表示
                var lanePos = state.lanePos + selectBtnPosOffset;
                var laneSelectBtnSize = HandleUtility.GetHandleSize(lanePos) * laneHndScaleFactor;
                var isClicked = Handles.Button(lanePos, Quaternion.identity, laneSelectBtnSize, laneSelectBtnSize, RoadNetworkLaneHandleCap);
                if (isClicked)
                {
                    editorSystem.SelectedRoadNetworkElement = lane;
                }

                // レーンの構造変更機能が有効である
                if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditLaneStructure)
                {
                    var offset = Vector3.up * signalLightHndScaleFactor;
                    var scaleHandlePos = state.lanePos + offset;

                    var sizeOffset = laneHndScaleFactor;
                    var size = HandleUtility.GetHandleSize(scaleHandlePos) * sizeOffset;
                    var isClickedSplit = Handles.Button(scaleHandlePos, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                    if (isClickedSplit)
                    {
                        // 車線数を増やす
                        state.delayCommand += () =>
                        {
                            //var newLanes = lane.SplitLane(2);   // Laneが３つになる
                            //if (newLanes == null)
                            //    return;
                            //lanes.AddRange(newLanes);
                            Debug.Log("車線数を増やすボタンが押された");

                        };
                        state.isDirtyTarget = true;
                    }

                    // 仮　車線数を減らす　ParentLinkがnullであるためレーンを選択できないので適当なレーンを削除する
                    var isClickedRemove = Handles.Button(scaleHandlePos + Vector3.right * size * 1.5f, Quaternion.identity, size, size, RoadNetworkRemoveLaneButtonHandleCap);
                    if (isClickedRemove)
                    {
                        state.delayCommand += () =>
                        {
                            //lanes.Remove(lane); // Link,他のLaneなどとの繋がりを切る処理が必要
                            Debug.Log("車線数を減らすボタンが押された");
                        };
                        state.isDirtyTarget = true;
                    }
                }

                if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditLaneShape)
                {
                    // １つのレーンの幅員を増やす
                    //if (lane.LeftWay.Count > 0)
                    //{
                    //    var leftCenterIdx = lane.LeftWay.Count / 2;
                    //    var scaleHandlePos = lane.LeftWay[leftCenterIdx];
                    //    var dir = Vector3.up;
                    //    if (lane.LeftWay.Count >= 2)
                    //    {
                    //        dir = lane.LeftWay.GetVertexNormal(leftCenterIdx - 1);
                    //        dir.Normalize();
                    //    }

                    //    var size = HandleUtility.GetHandleSize(scaleHandlePos);
                    //    EditorGUI.BeginChangeCheck();
                    //    var scale = Deploy1DScaleHandle(1.0f, scaleHandlePos, dir, Quaternion.identity, size);
                    //    if (EditorGUI.EndChangeCheck())
                    //    {
                    //        foreach (var way in lane.BothWays)
                    //        {
                    //            int i = 0;
                    //            foreach (var point in way.Points)
                    //            {
                    //                var vertNorm = way.GetVertexNormal(i++);
                    //                point.Vertex = point + (scale - 1) * 0.1f * vertNorm;
                    //                state.isDirtyTarget = true;
                    //            }
                    //        }
                    //    }
                    //}
                }

            }
        }

        private void ForeachLink(IRoadNetworkEditingSystem editorSystem, IReadOnlyList<RoadNetworkLink> links, RoadNetworkLink link, ref SceneGUIState state)
        {
            state.linkPos = CalcLinkPos(link);

            bool isDisplay = false;
            // 自身が選択されていない
            if (link != editorSystem.SelectedRoadNetworkElement as RoadNetworkLink)
            {
                //子の要素が選択されていない
                var lane = editorSystem.SelectedRoadNetworkElement as RoadNetworkLane;
                if (lane?.ParentLink != link)
                {
                    isDisplay = true;
                }
            }

            if (isDisplay)
            {
                state.isContinue = true;
            }

            if (isDisplay)
            {
                // 処理負荷軽減のため適当なレーンを選択して中心位置を計算
                var linkSelectbtnPos = state.linkPos + selectBtnPosOffset;
                var linkSelectBtnHandleDefaultSize = linkHndScaleFactor;
                var size = HandleUtility.GetHandleSize(linkSelectbtnPos) * linkSelectBtnHandleDefaultSize;
                var pickSize = size;
                var isClicked = Handles.Button(
                linkSelectbtnPos, Quaternion.identity, size, pickSize, RoadNetworkLinkHandleCap);
                if (isClicked)
                {
                    editorSystem.SelectedRoadNetworkElement = link;
                }

            }
        }

        private void ForeachPoints(IRoadNetworkEditingSystem sys, RoadNetworkPoint point, ref SceneGUIState state)
        {
            if (sys.CurrentEditMode != RoadNetworkEditMode.EditLaneShape)
                return;

            var networkOperator = sys.EditOperation;
            var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;
            EditorGUI.BeginChangeCheck();
            //var vertPos = DeployTranslateHandle(point);
            var vertPos = DeployFreeMoveHandle(point, size, snap: Vector3.zero);

            if (EditorGUI.EndChangeCheck())
            {
                var res = networkOperator.MovePoint(point, vertPos);
                state.isDirtyTarget = true;
                Debug.Assert(res.IsSuccess);
            }

        }

        private void ForeachSignalLight(IRoadNetworkEditingSystem editorSystem, List<TrafficSignalLight> signalLights, TrafficSignalLight signalLight, ref SceneGUIState state)
        {
            state.signalLightPos = signalLight.position;
            if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditTrafficRegulation)
            {
                var size = HandleUtility.GetHandleSize(signalLight.position) * signalLightHndScaleFactor;
                bool isClicked = Handles.Button(signalLight.position, Quaternion.identity, size, size, RoadNetworkTrafficSignalLightCap);
                if (isClicked)
                {
                    //editorSystem.SelectedRoadNetworkElement = signalLight;
                    Debug.Log("SignalLight");
                }
            }
        }

        private void ForeachSignalController(IRoadNetworkEditingSystem editorSystem, TrafficSignalLightController signalController, ref SceneGUIState state)
        {
            // 存在しないなら飛ばす
            if (signalController == null)
            {
                state.isContinue = true;
                return;
            }

            state.signalControllerPos = signalController.Position;

            bool isDisplay = false;
            if (signalController != editorSystem.SelectedRoadNetworkElement)
            {
                isDisplay = true;
            }
            // 表示されているなら子の要素を表示しない
            if (isDisplay)
            {
                state.isContinue = true;
            }

            // ハンドルを表示する
            if (isDisplay)
            {
                if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditTrafficRegulation)
                {
                    var size = HandleUtility.GetHandleSize(signalController.Position) * signalControllerScaleFactor;
                    bool isClicked = Handles.Button(signalController.Position, Quaternion.identity, size, size, RoadNetworkTrafficSignalLightCap);
                    if (isClicked)
                    {
                        editorSystem.SelectedRoadNetworkElement = signalController;
                        Debug.Log(signalController.SelfId);
                    }
                }
            }
        }

        private void ForeachNode(IRoadNetworkEditingSystem editorSystem, IReadOnlyList<RoadNetworkNode> nodes, RoadNetworkNode node, ref SceneGUIState state)
        {
            state.nodePos = node.GetCenterPoint();

            bool isDisplayNode = false;
            // 自身が選択されていない
            if (editorSystem.SelectedRoadNetworkElement != node)
            {
                // 子の要素が選択されていない
                var trafficLightController = editorSystem.SelectedRoadNetworkElement as TrafficSignalLightController;
                if (trafficLightController?.CorrespondingNode != node)
                {
                    isDisplayNode = true;
                }
            }

            // 要素を表示するなら子の要素を表示しない
            if (isDisplayNode)
            {
                state.isContinue = true;
            }

            if (isDisplayNode)
            {
                var selectbtnPos = state.nodePos + selectBtnPosOffset;
                var selectBtnHandleDefaultSize = linkHndScaleFactor;
                var size = HandleUtility.GetHandleSize(selectbtnPos) * selectBtnHandleDefaultSize;
                var pickSize = size;
                var isClicked = Handles.Button(
                selectbtnPos, state.currentCamera.transform.rotation, size, pickSize, RoadNetworkNodeHandleCap);
                if (isClicked)
                {
                    editorSystem.SelectedRoadNetworkElement = node;
                    Debug.Log("sele" + editorSystem.SelectedRoadNetworkElement.ToString());
                }

            }


        }

        private static Vector3 CalcLinkPos(RoadNetworkLink link)
        {
            var midIdx = link.AllLanes.Count() / 2;
            // midIdxのLaneを取得する
            var lanesEnum = link.AllLanes.GetEnumerator();
            var cnt = 0;
            while (lanesEnum.MoveNext())
            {
                if (cnt++ == midIdx)
                    break;
            }
            var centerLane = lanesEnum.Current;

            var avePos = CalcLanePos(centerLane);
            return avePos;
        }

        private static Vector3 CalcLanePos(RoadNetworkLane centerLane)
        {
            var numVert = centerLane.Vertices.Count();
            var sumVert = Vector3.zero;
            foreach (var vert in centerLane.Vertices)
            {
                sumVert += vert;
            }
            var avePos = sumVert / (float)numVert;
            return avePos;
        }



        private static void RoadNetworkTrafficSignalLightCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
                    break;
                case EventType.Repaint:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
                    break;
            }
        }
        private static void RoadNetworkNodeHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
                    break;
                case EventType.Repaint:
                    Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
                    break;
            }

        }

        static void RoadNetworkLinkHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    //Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
                    Handles.DrawWireCube(position, new Vector3(size, size, size));
                    var subCubeSize = size * signalControllerScaleFactor;
                    Handles.DrawWireCube(position + Vector3.right * subCubeSize, new Vector3(subCubeSize, subCubeSize, subCubeSize));
                    Handles.DrawWireCube(position + Vector3.left * subCubeSize, new Vector3(subCubeSize, subCubeSize, subCubeSize));
                    Handles.DrawLine(position + Vector3.right * size * linkHndScaleFactor, position + Vector3.left * size * linkHndScaleFactor);
                    break;
            }

        }

        static void RoadNetworkLaneHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireCube(position, new Vector3(size, size, size));
                    Handles.DrawWireCube(position, new Vector3(size, size * pointHndScaleFactor, size * signalControllerScaleFactor));
                    break;
            }

        }

        static void RoadNetworkSplitLaneButtonHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * linkHndScaleFactor);
                    Handles.DrawWireCube(position + Vector3.forward * 0.07f, new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    Handles.DrawWireCube(position + Vector3.back * 0.07f, new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    break;
            }
        }

        static void RoadNetworkRemoveLaneButtonHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * linkHndScaleFactor);
                    Handles.DrawWireCube(position + Vector3.forward * 0.07f, new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    break;
            }
        }

        private static Vector3 DeployFreeMoveHandle(in Vector3 pos, float size, in Vector3 snap)
        {
            return Handles.FreeMoveHandle(pos, size, snap, Handles.SphereHandleCap);
        }

        //Vector3 static  DeployTranslateHandle(in Vector3 pos)
        //{
        //    return Handles.PositionHandle(pos, Quaternion.identity);
        //}

        //float static Deploy1DScaleHandle(float scale, in Vector3 pos, in Vector3 dir, in Quaternion rot, float size, float snap = 0.01f)
        //{
        //    return Handles.ScaleSlider(scale, pos, dir, rot, size, snap);
        //}

    }
}
