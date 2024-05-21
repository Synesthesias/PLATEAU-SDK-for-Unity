using System;
using System.Collections;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
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

        private IRoadNetworkEditingSystem editorSystem;
        private UnityEngine.Object target;

        /// <summary>
        /// OnSceneGUI()内での状態
        /// </summary>
        private struct SceneGUIState
        {
            public bool isDirtyTarget;      // ターゲットに変更があったか
            public Action delayCommand;     // 遅延コマンド　要素の追加や削除を行う際に利用する foreach外で利用する 
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

            // 編集モードの状態表示
            // 2D GUI
            var sceneViewPixelRect = SceneView.currentDrawingSceneView.camera.pixelRect;
            var guiLayoutRect = new Rect(sceneViewPixelRect.position + sceneViewPixelRect.center, sceneViewPixelRect.size / 2.0f);
            Handles.BeginGUI();
            GUILayout.BeginArea(guiLayoutRect);
            GUILayout.Box("道路ネットワーク編集モード");
            GUILayout.EndArea();
            Handles.EndGUI();

            // 編集モードの状態表示
            //var currentMouse2DPos = Event.current.mousePosition;
            //// guicontext,guistyle
            //var mouse3DPos = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(currentMouse2DPos);
            //Handles.Label(mouse3DPos)

            // ハンドルの配置、要素数を変化させない値変更、遅延実行用のコマンド生成を行う
            // 遅延実行用のコマンドは1フレームにつき一つまで実行できるとする(要素削除順の管理などが面倒なため)
            foreach (var link in network.Links)
            {
                // リンク内のレーンの幅員を増やす
                //...

                //foreach (var lane in link.AllLanes)
                foreach (var lane in link.MainLanes)
                {
                    ForeachMainLanes(editorSystem, link.MainLanes, lane, ref state);

                    foreach (var way in lane.BothWays)
                    {
                        foreach (var point in way.Points)
                        {
                            ForeachPoints(editorSystem, point, ref state);
                        }
                    }
                }
            }

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
            void ForeachMainLanes(IRoadNetworkEditingSystem sys, List<RoadNetworkLane> lanes, RoadNetworkLane lane, ref SceneGUIState state)
            {
                if (sys.CurrentEditMode == RoadNetworkEditMode.EditLaneShape)
                {
                    // １つのレーンの幅員を増やす
                    if (lane.LeftWay.Count > 0)
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

                if (sys.CurrentEditMode == RoadNetworkEditMode.EditLaneStructure)
                {
                    // 射線を増やす

                    // １つのレーンの幅員を増やす
                    if (lane.LeftWay.Count > 0 && lane.IsValidWay)
                    {
                        var leftCenterIdx = lane.LeftWay.Count / 2;
                        var scaleHandlePos = lane.LeftWay[leftCenterIdx];
                        var dir = Vector3.up;
                        var size = HandleUtility.GetHandleSize(scaleHandlePos);
                        EditorGUI.BeginChangeCheck();
                        var scale = Deploy1DScaleHandle(1.0f, scaleHandlePos, dir, Quaternion.identity, size, 1.0f);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (scale > 1)
                            {
                                state.delayCommand += () =>
                                {
                                    var newLanes = lane.SplitLane(2);
                                    if (newLanes == null)
                                        return;
                                    lanes.AddRange(newLanes);

                                };
                                state.isDirtyTarget = true;
                            }
                        }
                    }

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

            // end local method ======================
        }
    }
}
