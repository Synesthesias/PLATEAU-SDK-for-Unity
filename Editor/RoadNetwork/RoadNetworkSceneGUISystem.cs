using System;
using System.Collections;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Linq;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

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

        private IRoadNetworkEditingSystem editorSystem;
        private UnityEngine.Object target;

        /// <summary>
        /// OnSceneGUI()内での状態
        /// </summary>
        private struct SceneGUIState
        {
            public bool isDirtyTarget;      // ターゲットに変更があったか
            public Action delayCommand;     // 遅延コマンド　要素の追加や削除を行う際に利用する foreach外で利用する 

            public Vector3 linkPos;
            public Vector3 lanePos;
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
        SceneGUISystemState systemState;

        public void SetEditingTarget(UnityEngine.Object mdl)
        {
            Assert.IsNotNull(mdl);
            Assert.IsTrue(mdl is IRoadNetworkObject);
            target = mdl;
        }

        public void OnSceneGUI()
        {
            SetRoadNetworkObject2System();
            var network = GetRoadNetwork();
            if (network == null)
                return;

            // ステイトの初期化
            SceneGUIState state;
            systemState.Init(out state);

            var currentCamera = SceneView.currentDrawingSceneView.camera;

            // 編集モードの状態表示
            //var currentMouse2DPos = Event.current.mousePosition;
            //// guicontext,guistyle
            //var mouse3DPos = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(currentMouse2DPos);
            //Handles.Label(mouse3DPos)

            // ハンドルの配置、要素数を変化させない値変更、遅延実行用のコマンド生成を行う
            // 遅延実行用のコマンドは1フレームにつき一つまで実行できるとする(要素削除順の管理などが面倒なため)
            foreach (var link in network.Links)
            {
                ForeachLinks(editorSystem, network.Links, link, ref state);

                ////foreach (var lane in link.AllLanes)
                //foreach (var lane in link.MainLanes)
                //{
                //    ForeachMainLanes(editorSystem, link.MainLanes, lane, ref state);

                //    foreach (var way in lane.BothWays)
                //    {
                //        foreach (var point in way.Points)
                //        {
                //            ForeachPoints(editorSystem, point, ref state);
                //        }
                //    }
                //}
            }

            if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditTrafficRegulation) {
                foreach (var signalController in network.SignalControllers)
                {
                    //var size = HandleUtility.GetHandleSize(signalController.position);
                    EditorGUI.BeginChangeCheck();
                    var pos = Handles.PositionHandle(signalController.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        signalController.position = pos;
                        state.isDirtyTarget = true;
                    }
                    
                }

                foreach (var signalLight in network.SignalLihgts)
                {
                    //var size = HandleUtility.GetHandleSize(signalController.position);
                    EditorGUI.BeginChangeCheck();
                    var pos = Handles.PositionHandle(signalLight.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        signalLight.position = pos;
                        state.isDirtyTarget = true;
                    }
                }
            }

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
                SetDirty2NetworkModel();
            }

            systemState.Apply(state);

            // local method ======================
            void ForeachLinks(IRoadNetworkEditingSystem editorSystem, List<RoadNetworkLink> links, RoadNetworkLink link, ref SceneGUIState state)
            {
                state.linkPos = CalcLinkPos(link);

                // 選択対象を取得する
                var selectedLink = editorSystem.SelectedRoadNetworkElement as RoadNetworkLink;
                if (selectedLink == null) 
                {
                    var lane = editorSystem.SelectedRoadNetworkElement as RoadNetworkLane;
                    if (lane != null)
                    {
                        if (lane.ParentLink == null)
                        {
                            //Assert.IsNotNull(lane.ParentLink);  // nullだった　未実装？

                        }
                        else
                        {
                            selectedLink = lane.ParentLink;
                        }
                    }
                }

                // レーンが選択されていないならレーンを選択するボタンを表示する
                if (selectedLink != link)
                {
                    // 処理負荷軽減のため適当なレーンを選択して中心位置を計算
                    var numLane = link.AllLanes.Count();
                    if (numLane == 0)
                    {

                    }
                    else
                    {
                        var linkSelectBtnHandleDefaultPosOffset = Vector3.up * 3.0f;
                        var linkSelectbtnPos = state.linkPos + linkSelectBtnHandleDefaultPosOffset;
                        var linkSelectBtnHandleDefaultSize = 0.5f;
                        var size = HandleUtility.GetHandleSize(linkSelectbtnPos) * linkSelectBtnHandleDefaultSize;
                        var pickSize = size;
                        var isClicked = Handles.Button(
                        linkSelectbtnPos, currentCamera.transform.rotation, size, pickSize, RoadNetworkLinkHandleCap);
                        if (isClicked)
                        {
                            editorSystem.SelectedRoadNetworkElement = link;
                        }
                         
                    }

                }
                else
                {
                    if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditLaneStructure)
                    {
                        if (link.MainLanes.Count() > 0)
                        {
                            var lanes = link.MainLanes;
                            var lane = link.MainLanes.First();

                            if (lane.LeftWay.Count > 0 && lane.IsValidWay)
                            {
                                var leftCenterIdx = lane.LeftWay.Count / 2;
                                var offset = Vector3.up * 0.2f;
                                var scaleHandlePos = lane.LeftWay[leftCenterIdx] + offset;
                                var dir = Vector3.up;
                                var sizeOffset = 0.4f;
                                var size = HandleUtility.GetHandleSize(scaleHandlePos) * sizeOffset;
                                var isClickedSplit = Handles.Button(scaleHandlePos, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                                if (isClickedSplit)
                                {
                                    // 車線数を増やす
                                    state.delayCommand += () =>
                                    {
                                        var newLanes = lane.SplitLane(2);   // Laneが３つになる
                                        if (newLanes == null)
                                            return;
                                        lanes.AddRange(newLanes);

                                    };
                                    state.isDirtyTarget = true;
                                }

                                // 仮　車線数を減らす　ParentLinkがnullであるためレーンを選択できないので適当なレーンを削除する
                                var isClickedRemove = Handles.Button(scaleHandlePos + Vector3.up * size * 1.5f, Quaternion.identity, size, size, RoadNetworkRemoveLaneButtonHandleCap);
                                if (isClickedRemove)
                                {
                                    state.delayCommand += () =>
                                    {
                                        lanes.Remove(lane); // Link,他のLaneなどとの繋がりを切る処理が必要
                                    };
                                    state.isDirtyTarget = true;
                                }

                            }
                        }
                    }

                    // レーンの走査を行う
                    //foreach (var lane in link.AllLanes)
                    foreach (var lane in link.MainLanes)
                    {
                        ForeachLanes(editorSystem, link.MainLanes, lane, ref state);

                        foreach (var way in lane.BothWays)
                        {
                            foreach (var point in way.Points)
                            {
                                ForeachPoints(editorSystem, point, ref state);
                            }
                        }
                    }
                }
            }

            void ForeachLanes(IRoadNetworkEditingSystem sys, List<RoadNetworkLane> lanes, RoadNetworkLane lane, ref SceneGUIState state)
            {
                state.lanePos = CalcLanePos(lane);
                if (sys.CurrentEditMode == RoadNetworkEditMode.EditLaneShape)
                {
                    // １つのレーンの幅員を増やす
                    if (false)
                    //if (lane.LeftWay.Count > 0)
                    {
                        var leftCenterIdx = lane.LeftWay.Count / 2;
                        var scaleHandlePos = lane.LeftWay[leftCenterIdx];
                        var dir = Vector3.up;
                        if (lane.LeftWay.Count >= 2)
                        {
                            dir = lane.LeftWay.GetVertexNormal(leftCenterIdx - 1);
                            dir.Normalize();
                        }

                        var size = HandleUtility.GetHandleSize(scaleHandlePos);
                        EditorGUI.BeginChangeCheck();
                        var scale = Deploy1DScaleHandle(1.0f, scaleHandlePos, dir, Quaternion.identity, size);
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach (var way in lane.BothWays)
                            {
                                int i = 0;
                                foreach (var point in way.Points)
                                {
                                    var vertNorm = way.GetVertexNormal(i++);
                                    point.Vertex = point + (scale - 1) * 0.1f * vertNorm;
                                    state.isDirtyTarget = true;
                                }
                            }
                        }
                    }
                }

                var linkSelectBtnHandleDefaultPosOffset = Vector3.up * 2.0f;
                var lanePos = state.lanePos + linkSelectBtnHandleDefaultPosOffset;
                var linkSelectBtnHandleDefaultSize = 0.4f;
                var laneSelectBtnSize = HandleUtility.GetHandleSize(lanePos) * linkSelectBtnHandleDefaultSize;
                var isClicked = Handles.Button(lanePos, currentCamera.transform.rotation, laneSelectBtnSize, laneSelectBtnSize, RoadNetworkLaneHandleCap);
                if (isClicked)
                {
                    editorSystem.SelectedRoadNetworkElement = lane;
                }
            }


            void ForeachPoints(IRoadNetworkEditingSystem sys, RoadNetworkPoint point, ref SceneGUIState state)
            {
                if (sys.CurrentEditMode != RoadNetworkEditMode.EditLaneShape)
                    return;

                var networkOperator = sys.EditOperation;
                var size = HandleUtility.GetHandleSize(point) * 0.1f;
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

            Vector3 DeployFreeMoveHandle(in Vector3 pos, float size, in Vector3 snap)
            {
                return Handles.FreeMoveHandle(pos, size, snap, Handles.SphereHandleCap);
            }

            Vector3 DeployTranslateHandle(in Vector3 pos)
            {
                return Handles.PositionHandle(pos, Quaternion.identity);
            }

            float Deploy1DScaleHandle(float scale, in Vector3 pos, in Vector3 dir, in Quaternion rot, float size, float snap = 0.01f)
            {
                return Handles.ScaleSlider(scale, pos, dir, rot, size, snap);
            }

            bool SetRoadNetworkObject2System()
            {
                editorSystem.RoadNetworkObject = target;
                return editorSystem.RoadNetworkObject != null;
            }

            RoadNetworkModel GetRoadNetwork()
            {
                return editorSystem.RoadNetwork;
            }

            void SetDirty2NetworkModel()
            {
                var _mdl = target;
                if (_mdl == null)
                    return;
                EditorUtility.SetDirty(target);
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
                        var subCubeSize = size * 0.3f;
                        Handles.DrawWireCube(position + Vector3.right * subCubeSize, new Vector3(subCubeSize, subCubeSize, subCubeSize));
                        Handles.DrawWireCube(position + Vector3.left * subCubeSize, new Vector3(subCubeSize, subCubeSize, subCubeSize));
                        Handles.DrawLine(position + Vector3.right * size * 0.5f, position + Vector3.left * size * 0.5f);
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
                        Handles.DrawWireCube(position, new Vector3(size, size * 0.1f, size * 0.3f));
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
                        Handles.DrawWireDisc(position, Vector3.up, size * 0.5f);
                        Handles.DrawWireCube(position + Vector3.forward * 0.07f, new Vector3(size, size * 0.1f, size * 0.15f));
                        Handles.DrawWireCube(position + Vector3.back * 0.07f, new Vector3(size, size * 0.1f, size * 0.15f));
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
                        Handles.DrawWireDisc(position, Vector3.up, size * 0.5f);
                        Handles.DrawWireCube(position + Vector3.forward * 0.07f, new Vector3(size, size * 0.1f, size * 0.15f));
                        break;
                }
            }

            // end local method ======================
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
    }
}
