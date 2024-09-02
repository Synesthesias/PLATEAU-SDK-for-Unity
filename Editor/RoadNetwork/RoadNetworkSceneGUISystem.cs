using Codice.CM.Common;
using PLATEAU.CityGML;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.RoadNetwork
{
    /// <summary>
    /// SceneGUIまわりの機能を管理するクラス
    /// 記述先のファイルを変更するかも？
    /// </summary>
    public class RoadNetworkSceneGUISystem
    {
        /// <summary>
        /// 
        /// </summary>
        private enum DisplayHndOperation : int
        {
            NoOperation = 0,
            Add = 1,
            Remove = 2,
            Move = 3,
            Select = 4,
            _Num
        }

        private enum DisplayHndOperationMask : int
        {
            NoOperation = 1 << DisplayHndOperation.NoOperation,
            Add = 1 << DisplayHndOperation.Add,
            Remove = 1 << DisplayHndOperation.Remove,
            Move = 1 << DisplayHndOperation.Move,
            Select = 1 << DisplayHndOperation.Select
        }

        /// <summary>
        /// 
        /// </summary>
        private enum DisplayHndType : int
        {
            // Target
            Point = 0,
            Lane = 1,
            Link = 2,
            Node = 3,
            _Num
        }

        /// <summary>
        /// 
        /// </summary>
        public enum DisplayHndMaskSet : int
        {
            Point = DisplayHndOperationMask.NoOperation,                // 
            PointAdd = Point | DisplayHndOperationMask.Add,                 // 
            PointRemove = Point | DisplayHndOperationMask.Remove,           // 
            PointMove = Point | DisplayHndOperationMask.Move,               // 
            PointSelect = Point | DisplayHndOperationMask.Select,           // 
            PointZeroMask = ~(PointAdd | PointRemove | PointMove | PointSelect),

            _LaneShiftOffset = DisplayHndType.Lane * DisplayHndOperation._Num,
            Lane = DisplayHndOperationMask.NoOperation << _LaneShiftOffset,     // 
            LaneAdd = Lane | DisplayHndOperationMask.Add << _LaneShiftOffset,
            LaneRemove = Lane | DisplayHndOperationMask.Remove << _LaneShiftOffset,
            //LaneMove = Lane | DisplayHndOperationMask.Move << _LaneShiftOffset,
            LaneSelect = Lane | DisplayHndOperationMask.Select << _LaneShiftOffset,
            LaneZeroMask = ~(LaneAdd | LaneRemove | LaneSelect),

            _LinkShiftOffset = DisplayHndType.Link * DisplayHndOperation._Num,
            Link = DisplayHndOperationMask.NoOperation << _LinkShiftOffset,     // 
            LinkAdd = Link | DisplayHndOperationMask.Add << _LinkShiftOffset,
            LinkRemove = Link | DisplayHndOperationMask.Remove << _LinkShiftOffset,
            //LinkMove = Road | DisplayHndOperationMask.Move << _LinkShiftOffset,
            LinkSelect = Link | DisplayHndOperationMask.Select << _LinkShiftOffset,
            LinkZeroMask = ~(LinkAdd | LinkRemove | LinkSelect),

            _NodeShiftOffset = DisplayHndType.Node * DisplayHndOperation._Num,
            Node = DisplayHndOperationMask.NoOperation << _NodeShiftOffset,     // 
            //NodeAdd = Node | DisplayHndOperationMask.Add << _NodeShiftOffset,
            //NodeRemove = Node | DisplayHndOperationMask.Remove << _NodeShiftOffset,
            //NodeMove = Node | DisplayHndOperationMask.Move << _NodeShiftOffset,
            NodeSelect = Node | DisplayHndOperationMask.Select << _NodeShiftOffset,
            NodeZeroMask = ~(NodeSelect),

            ZeroMask = PointZeroMask & LaneZeroMask & LinkZeroMask & NodeZeroMask
        }

        private readonly int defaultDisplayHandleMask = (int)(
            DisplayHndMaskSet.PointMove |
            DisplayHndMaskSet.LaneSelect |
            DisplayHndMaskSet.LinkSelect |
            DisplayHndMaskSet.NodeSelect);

        private int displayHandleMask = 0;
        private Vector3 handleLockPos;
        private RnLane bIsHandleLock = null;


        //string nodeTexPath = "Assets/PlateauUnitySDK/Editor/RoadNetwork/Textures/Node.png";
        string laneTexPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/Icon_lane.png";
        string nodeTexPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/Icon_node.png";
        string trafficLightControllerPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/Icon_trafficLightController.png";
        string trafficLight_blueTexPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/trafficLight_blue.png";
        Texture2D laneTex;
        Texture2D nodeTex;
        Texture2D trafficLightControllerTex;
        Texture2D trafficLight_blueTex;

        public RoadNetworkSceneGUISystem(IRoadNetworkEditingSystem editorSystem)
        {
            Assert.IsNotNull(editorSystem);
            this.editorSystem = editorSystem;

            displayHandleMask = defaultDisplayHandleMask;
            editorSystem.OnChangedOperationMode += (object _, EventArgs _) =>
            {
                var mode = editorSystem.OperationMode;
                switch (mode)
                {
                    case "no-op":
                        SetDisplayHandleMask(defaultDisplayHandleMask, DisplayHndMaskSet.ZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.AddPoint):
                        SetDisplayHandleMask(DisplayHndMaskSet.PointAdd, DisplayHndMaskSet.PointZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.RemovePoint):
                        SetDisplayHandleMask(DisplayHndMaskSet.PointRemove, DisplayHndMaskSet.PointZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.AddMainLane):
                        SetDisplayHandleMask(DisplayHndMaskSet.LaneAdd, DisplayHndMaskSet.LaneZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.RemoveMainLane):
                        SetDisplayHandleMask(DisplayHndMaskSet.LaneRemove, DisplayHndMaskSet.LaneZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.AddLink):
                        SetDisplayHandleMask(defaultDisplayHandleMask, DisplayHndMaskSet.ZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.RemoveLink):
                        SetDisplayHandleMask(defaultDisplayHandleMask, DisplayHndMaskSet.ZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.AddNode):
                        SetDisplayHandleMask(defaultDisplayHandleMask, DisplayHndMaskSet.ZeroMask);
                        break;
                    case nameof(IRoadNetworkEditOperation.RemoveNode):
                        SetDisplayHandleMask(defaultDisplayHandleMask, DisplayHndMaskSet.ZeroMask);
                        break;
                    default:
                        displayHandleMask = defaultDisplayHandleMask;
                        break;
                }
            };
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

        private void SetDisplayHandleMask(DisplayHndMaskSet mask, DisplayHndMaskSet clearMask)
        {
            SetDisplayHandleMask((int)mask, clearMask);
        }
        private void SetDisplayHandleMask(int mask, DisplayHndMaskSet clearMask)
        {
            displayHandleMask = (displayHandleMask & (int)clearMask) | (mask & ~(int)clearMask);
        }

        public void OnSceneGUI(UnityEngine.Object target)
        {
            SetRoadNetworkObject2System(target);
            var network = GetRoadNetwork();
            if (network == null)
                return;

            if (EnableLimitSceneViewDefaultContorl)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Debug.Log("disenable defautl contorl");
            }

            if (nodeTex == null)
            {
                var isSuc = LoadTexture();
                if (isSuc == false)
                    return;
            }

            // 仮　簡易編集機能モード時は旧ハンドル描画、管理システムは利用しない
            if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditRoadStructure)
            {
                OnSceneGUISimpleEdit();
                return;
            }


            // ステイトの初期化
            SceneGUIState state;
            systemState.Init(out state);

            var currentCamera = SceneView.currentDrawingSceneView.camera;
            state.currentCamera = currentCamera;

            // ハンドルの配置、要素数を変化させない値変更、遅延実行用のコマンド生成を行う
            // 遅延実行用のコマンドは1フレームにつき一つまで実行できるとする(要素削除順の管理などが面倒なため)
            Update3DHandle(network, ref state);


            int toolbarInt = 0;
            string[] toolbarStrings = { "Toolbar1", "Toolbar2", "Toolbar3", "キャンセル" };
            //RnWay way;
            //way.SegmentIntersectionXz(Vector3.zero, Vector3.one, out Vector3 p1, out Vector3 p2);

            // 編集モードの状態表示
            // 2D GUI
            var sceneViewPixelRect = currentCamera.pixelRect;
            var guiLayoutRect = new Rect(sceneViewPixelRect.position + sceneViewPixelRect.center, sceneViewPixelRect.size / 2.0f);
            Handles.BeginGUI();
            GUILayout.BeginArea(guiLayoutRect);
            GUILayout.Box("道路ネットワーク編集モード");
            GUILayout.HorizontalScrollbar(0.5f, 5, 0.001f, 2.0f);
            //GUILayout.Toggle("道路ネットワーク編集モード");
            //toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

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

        private bool LoadTexture()
        {
            var isSuc = true;
            nodeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(nodeTexPath);
            if (nodeTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + nodeTexPath);
                isSuc = false;
            }

            laneTex = AssetDatabase.LoadAssetAtPath<Texture2D>(laneTexPath);
            if (laneTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + laneTexPath);
                isSuc = false;
            }

            trafficLightControllerTex = AssetDatabase.LoadAssetAtPath<Texture2D>(trafficLightControllerPath);
            if (trafficLightControllerTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + trafficLightControllerPath);
                isSuc = false;
            }

            trafficLight_blueTex = AssetDatabase.LoadAssetAtPath<Texture2D>(trafficLight_blueTexPath);
            if (trafficLight_blueTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + trafficLight_blueTexPath);
                isSuc = false;
            }

            return true;
        }

        public IReadOnlyCollection<EditorData<RnRoadGroup>> connections = new EditorData<RnRoadGroup>[0];
        public Color connectionColor = Color.blue;

        public ICollection<EditorData<RnIntersection>> intersections = new EditorData<RnIntersection>[0];
        //public List<Vector3> intersections = new List<Vector3>();
        public Color intersectionColor = Color.green;
        public float intersectionRadius = 30.0f;

        public float btnSize = 10.0f;

        public List<RnRoadGroup> SimLanes;

        public bool EnableLimitSceneViewDefaultContorl { get; set; }

        private void OnSceneGUISimpleEdit()
        {
            var nodeIconPosOffset = Vector3.up * 0;
            var roadIconPosOffset = Vector3.up * 0;

            IReadOnlyCollection<LinkGroupEditorData> cns = connections.Select(c => c.GetSubData<LinkGroupEditorData>()).ToList();

            var camera = SceneView.currentDrawingSceneView.camera;

            foreach (var item in connections)
            {
                // 選択済みのオブジェクト
                if (item == editorSystem.SelectedRoadNetworkElement) 
                    continue;

                var subData = item.GetSubData<LinkGroupEditorData>();

                var p1 = subData.A.RefGameObject.transform.position;
                var p2 = subData.B.RefGameObject.transform.position;
                var btnP = (p1 + p2) / 2.0f;


                Vector3 pos2d_dis = Vector3.zero;
                pos2d_dis = camera.WorldToScreenPoint(btnP + roadIconPosOffset);
                var isEditable = IsVisibleToCamera(camera, pos2d_dis);
                if (isEditable)
                {
                    // レーンの選択ボタンの表示
                    var laneSelectBtnSize = HandleUtility.GetHandleSize(btnP) * laneHndScaleFactor;
                    var isClicked = Button2DOn3D(camera, pos2d_dis, laneTex);
                    if (isClicked)
                    {
                        Debug.Log(subData.A.RefGameObject.name + "-" + subData.B.RefGameObject.name);
                        editorSystem.SelectedRoadNetworkElement = item;
                        return;
                    }
                }
            }


            // ノードが重複して描画されるので nodeEitorDataで走査
            HashSet<NodeEditorData> nodeEitorData = new HashSet<NodeEditorData>(connections.Count * 2);
            foreach (var item in cns)
            {
                nodeEitorData.Add(item.A);
                nodeEitorData.Add(item.B);
            }

            //foreach (var item in nodeEitorData)
            //{
            //    // 選択済みのオブジェクト
            //    if (item == editorSystem.SelectedRoadNetworkElement)
            //        continue;

            //    Color pre = GUI.color;
            //    var p1 = item.RefGameObject.transform.position;

            //    Vector3 pos2d_dis = Vector3.zero;
            //    pos2d_dis = camera.WorldToScreenPoint(p1 + nodeIconPosOffset);
            //    var isEditable = IsVisibleToCamera(camera, pos2d_dis);
            //    if (isEditable)
            //    {
            //        // レーンの選択ボタンの表示
            //        var laneSelectBtnSize = HandleUtility.GetHandleSize(p1) * laneHndScaleFactor;
            //        var isClicked = Button2DOn3D(camera, pos2d_dis, nodeTex);
            //        if (isClicked)
            //        {
            //            Debug.Log(item.RefGameObject.name);
            //            editorSystem.SelectedRoadNetworkElement = item;
            //            return;
            //        }
            //    }

            //    GUI.color = Color.red;
            //    var offset = Vector3.up * intersectionRadius * 1.1f;
            //    Handles.Label(p1 + offset, item.RefGameObject.name);
            //    GUI.color = pre;
            //}


            foreach (var intersection in intersections)
            {
                // 選択済みのオブジェクト
                if (intersection == editorSystem.SelectedRoadNetworkElement)
                    continue;

                Color pre = GUI.color;
                var p1 = intersection.Ref.GetCenterPoint();

                Vector3 pos2d_dis = Vector3.zero;
                pos2d_dis = camera.WorldToScreenPoint(p1 + nodeIconPosOffset);
                var isEditable = IsVisibleToCamera(camera, pos2d_dis);
                if (isEditable)
                {
                    // レーンの選択ボタンの表示
                    var laneSelectBtnSize = HandleUtility.GetHandleSize(p1) * laneHndScaleFactor;
                    var isClicked = Button2DOn3D(camera, pos2d_dis, nodeTex);
                    if (isClicked)
                    {
                        editorSystem.SelectedRoadNetworkElement = intersection;
                        return;
                    }
                }

                //GUI.color = Color.red;
                //var offset = Vector3.up * intersectionRadius * 1.1f;
                //Handles.Label(p1 + offset, intersection);
                //GUI.color = pre;

            }


            // 選択している道路がある場合
            var selectedConnection = editorSystem.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (selectedConnection != null)
            {
                var roadGroupEditorData = selectedConnection;



                // 簡易モードで表示
                if (editorSystem.RoadNetworkSimpleEditModule.IsDetailMode() == false)
                {
                    // 各車線、歩道、張横分離帯の幅を調整するためのハンドル描画

                    var nLeftLane = roadGroupEditorData.Ref.GetLeftLaneCount();
                    var nRIghtLane = roadGroupEditorData.Ref.GetRightLaneCount();
                    var nLane = nLeftLane + nRIghtLane;
                    HashSet<RnWay> ways = new HashSet<RnWay>(nLane * 2);
                    List<RnWay> unionWay = new List<RnWay>(nLane * 2 - 2);  // 端の2つは覗く
                    var lanes = roadGroupEditorData.Ref.Roads[0].MainLanes;
                    foreach (var lane in lanes)
                    {
                        foreach (var way in lane.BothWays)
                        {
                            // 共有されているwayか？
                            if (ways.Add(way) == false)
                            {
                                //unionWay.Add(way);
                            }
                        }
                    }
                    unionWay = ways.ToList();

                    // 仕様上動かしてはいけないwayを除外する
                    //// レーンが一つの場合は必ず歩道に接するので
                    //if (lanes.Count == 1)
                    //{
                    //    unionWay.Clear();
                    //}
                    //else if (lanes.Count > 0)
                    //{
                    //    unionWay.Remove(lanes[0].LeftWay);
                    //    var otherLane = lanes[lanes.Count - 1];
                    //    unionWay.Remove(otherLane.RightWay);
                    //}

                    var wayEditorDataList = roadGroupEditorData.GetSubData<List<WayEditorData>>();
                    //var nSlider = unionWay.Count; // 左右の
                    if (wayEditorDataList == null)
                    {
                        //wayEditorDataList = new List<WayEditorData>(nSlider);
                        //foreach (var editingTarget in unionWay)
                        //{
                        //    wayEditorDataList.Add(new WayEditorData(editingTarget));
                        //}
                        //roadGroupEditorData.TryAdd(wayEditorDataList);
                    }

                    Assert.IsNotNull(wayEditorDataList);
                    //if (wayEditorDataList.Count != nSlider)
                    //{
                    //    wayEditorDataList.Clear();
                    //    wayEditorDataList.Capacity = nSlider;
                    //    foreach (var editingTarget in unionWay)
                    //    {
                    //        wayEditorDataList.Add(new WayEditorData(editingTarget));
                    //    }
                    //}
                    Handles.BeginGUI();
                    GUILayout.BeginArea(new Rect(100, 100, 200, 300));
                    //GUILayout.BeginVertical();
                    //GUILayout.Label("Ways");
                    //foreach (var wayEditorData in wayEditorDataList)
                    //{
                    //    //// textフィールド版
                    //    //var s = GUILayout.TextField(wayEditorData.SliderVarVals.ToString());
                    //    //var v = wayEditorData.SliderVarVals;
                    //    //if (float.TryParse(s, out v))
                    //    //{
                    //    //    wayEditorData.SliderVarVals = v;
                    //    //}

                    //    // slider版
                    //    const float minScaleLimit = -5.0f;
                    //    const float maxScaleLimit = 5.0f;
                    //    wayEditorData.SliderVarVals =
                    //        GUILayout.HorizontalSlider(wayEditorData.SliderVarVals, minScaleLimit, maxScaleLimit);

                    //    GUILayout.Space(10);
                    //}

                    GUILayout.EndArea();
                    Handles.EndGUI();

                    // 変更あったものに対してのみ差分を適用する
                    foreach (var wayEditorData in wayEditorDataList)
                    {
                        if (wayEditorData.IsChanged == false)
                        {
                            continue;
                        }

                        var target = wayEditorData.Ref;

                        // デフォルトの状態に戻す
                        var baseWay = wayEditorData.BaseWay;
                        for (int i = 0; i < baseWay.Count; i++)
                        {
                            var p = baseWay[i];
                            var p2 = target.GetPoint(i);
                            p2.Vertex = p;
                        }
                        var offset = wayEditorData.SliderVarVals;
                        target.MoveAlongNormal(offset);
                        Debug.Log($"way.MoveAlongNormal({offset})");
                    }
                }
                else // 詳細モードでのみ表示
                {
                    SceneGUIState state = new SceneGUIState();
                    systemState.Init(out state);

                    var currentCamera = SceneView.currentDrawingSceneView.camera;
                    state.currentCamera = currentCamera;


                    foreach (var road in roadGroupEditorData.Ref.Roads)
                    {
                        if (state.isDirtyTarget)
                        {
                            break;
                        }

                        foreach (var sideWalk in road.SideWalks)
                        {
                            foreach (var point in sideWalk.Way.Points)
                            {
                                if (state.isDirtyTarget)
                                {
                                    break;
                                }

                                var parent = sideWalk.Way;
                                var isEditable = false;
                                isEditable = true;

                                // 表示しない（実在はする）
                                if (IsContains(DisplayHndMaskSet.Point) == false)
                                {
                                    isEditable = false;
                                }

                                var networkOperator = editorSystem.EditOperation;
                                var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;


                                if (isEditable && IsSame(DisplayHndMaskSet.PointMove))
                                {
                                    DeployPointMoveHandle(point, state, networkOperator, size);
                                    continue;
                                }

                                if (isEditable && IsSame(DisplayHndMaskSet.PointAdd))
                                {
                                    var isClicked = Handles.Button(point, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                                    if (isClicked)
                                    {
                                        // parent.Pointsからpointを検索してインデックスを取得
                                        var idx = parent.Points.ToList().IndexOf(point);
                                        if (idx == -1)
                                            continue;
                                        state.delayCommand += () =>
                                        {
                                            parent.LineString.Points.Insert(idx, point);
                                        };
                                        state.isDirtyTarget = true;
                                    }
                                    continue;
                                }

                                if (isEditable && IsSame(DisplayHndMaskSet.PointRemove))
                                {
                                    var isClicked = Handles.Button(point, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                                    if (isClicked)
                                    {
                                        state.delayCommand += () =>
                                        {
                                            parent.LineString.Points.Remove(point);
                                        };
                                        state.isDirtyTarget = true;
                                    }
                                    continue;
                                }
                            }
                        }

                        foreach (var lane in road.MainLanes)
                        {
                            if (state.isDirtyTarget)
                            {
                                break;
                            }

                            HashSet<RnWay> ways = new HashSet<RnWay>(lane.BothWays);
                            foreach (var way in lane.BothWays)
                            {
                                ways.Add(way);
                            }
                            foreach (var way in ways)
                            {
                                if (state.isDirtyTarget)
                                {
                                    break;
                                }

                                foreach (var point in way.Points)
                                {
                                    if (state.isDirtyTarget)
                                    {
                                        break;
                                    }

                                    var parent = way;
                                    var isEditable = false;
                                    isEditable = true;

                                    // 表示しない（実在はする）
                                    if (IsContains(DisplayHndMaskSet.Point) == false)
                                    {
                                        isEditable = false;
                                    }

                                    var networkOperator = editorSystem.EditOperation;
                                    var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;


                                    if (isEditable && IsSame(DisplayHndMaskSet.PointMove))
                                    {
                                        DeployPointMoveHandle(point, state, networkOperator, size);
                                        continue;
                                    }

                                    if (isEditable && IsSame(DisplayHndMaskSet.PointAdd))
                                    {
                                        var isClicked = Handles.Button(point, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                                        if (isClicked)
                                        {
                                            // parent.Pointsからpointを検索してインデックスを取得
                                            var idx = parent.Points.ToList().IndexOf(point);
                                            if (idx == -1)
                                                continue;
                                            state.delayCommand += () =>
                                            {
                                                networkOperator.AddPoint(parent, idx, new RnPoint(point.Vertex + Vector3.up));
                                                Debug.Log("ポイント追加ボタンが押された");
                                            };
                                            state.isDirtyTarget = true;
                                        }
                                        continue;
                                    }

                                    if (isEditable && IsSame(DisplayHndMaskSet.PointRemove))
                                    {
                                        var isClicked = Handles.Button(point, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                                        if (isClicked)
                                        {
                                            state.delayCommand += () =>
                                            {
                                                networkOperator.RemovePoint(parent, point);
                                                Debug.Log("ポイント削除ボタンが押された");
                                            };
                                            state.isDirtyTarget = true;
                                        }
                                        continue;
                                    }
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
                        editorSystem.NotifyChangedRoadNetworkObject2Editor();
                    }

                    systemState.Apply(state);

                }
            }

        }

        private static void DeployPointMoveHandle(RnPoint point, SceneGUIState state, IRoadNetworkEditOperation networkOperator, float size)
        {
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

        private SceneGUIState Update3DHandle(RnModel network, ref SceneGUIState state)
        {

            // Node
            foreach (var node in network.Intersections)
            {
                state.ResetLoopOperationFlags();
                ForeachNode(editorSystem, network.Intersections, node, ref state);
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

                var offset = Vector3.up * 2.0f;
                var cnt = 0;
                foreach (var neighbor in node.Neighbors)
                {
                    state.ResetLoopOperationFlags();
                    //...
                    if (state.isBreak) break;
                    if (state.isContinue) continue;

                    var link = neighbor.Road as RnRoad;
                    var way = neighbor.Border;
                    foreach (var point in way.Points)
                    {
                        state.ResetLoopOperationFlags();
                        ForeachAllBorderPoints(editorSystem, link, way, point, ref state, offset * cnt);
                        if (state.isBreak) break;
                        if (state.isContinue) continue;
                    }
                    cnt++;
                }
            }

            HashSet<RnRoad> drawLink = new HashSet<RnRoad>();
            // link
            foreach (var link in network.Roads)
            {
                state.ResetLoopOperationFlags();
                ForeachLink(editorSystem, network.Roads, link, ref state);
                if (state.isBreak) break;
                if (state.isContinue) continue;

                state.linkPos = CalcLinkPos(link);

                // lane
                foreach (var lane in link.MainLanes)
                {
                    state.ResetLoopOperationFlags();
                    ForeachLane(editorSystem, link, link.MainLanes, lane, ref state);
                    //ForeachLanes(editorSystem, link.MainLanes, lane, ref state);
                    if (state.isBreak) break;
                    if (state.isContinue) continue;

                    // bothway
                    foreach (var way in lane.BothWays)
                    {
                        state.ResetLoopOperationFlags();

                        // 仮
                        drawLink.Add(link);

                        if (state.isBreak) break;
                        if (state.isContinue) continue;

                        // point
                        foreach (var point in way.Points)
                        {
                            state.ResetLoopOperationFlags();
                            ForeachBothWayPoints(editorSystem, lane, way, point, ref state);
                            if (state.isBreak) break;
                            if (state.isContinue) continue;
                        }
                    }

                }
            }

            foreach (var link in drawLink)
            {
                // lane
                foreach (var lane in link.MainLanes)
                {
                    // bothway
                    foreach (var way in lane.BothWays)
                    {
                        // 仮　laneのbothwayを描画する
                        //var pre = Gizmos.color;
                        //Gizmos.color = Color.green;
                        //Gizmos.DrawLineStrip(way.Vertices.ToArray(), false);
                        //Gizmos.color = pre;
                        DebugEx.DrawLines(way.Vertices.ToArray(), false, Color.green);
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

        private RnModel GetRoadNetwork()
        {
            return editorSystem.RoadNetwork;
        }

        private void ForeachLane(IRoadNetworkEditingSystem editorSystem, RnRoad parent, IReadOnlyList<RnLane> mainLanes, RnLane lane, ref SceneGUIState state)
        {
            state.lanePos = CalcLanePos(lane);

            bool isEditable = false;
            if (lane != editorSystem.SelectedRoadNetworkElement as RnLane)
            {
                isEditable = true;
            }

            // 表示されているなら子の要素を扱わない
            if (isEditable)
            {
                state.isContinue = true;
            }

            // 表示しない（実在はする）
            if (IsContains(DisplayHndMaskSet.Lane) == false)
            {
                isEditable = false;
            }

            if (isEditable == false)
            {
                return;
            }


            Vector3 pos2d_dis = Vector3.zero;
            if (isEditable)
            {
                pos2d_dis = state.currentCamera.WorldToScreenPoint(state.lanePos);
                isEditable = IsVisibleToCamera(state.currentCamera, pos2d_dis);
            }


            if (IsSame(DisplayHndMaskSet.LaneSelect) == true)
            {
                // レーンの選択ボタンの表示
                var lanePos = state.lanePos + selectBtnPosOffset;
                var laneSelectBtnSize = HandleUtility.GetHandleSize(lanePos) * laneHndScaleFactor;
                var isClicked = Button2DOn3D(state, pos2d_dis, laneTex);
                //var isClicked = Handles.Button(lanePos, Quaternion.identity, laneSelectBtnSize, laneSelectBtnSize, RoadNetworkLaneHandleCap);
                if (isClicked)
                {
                    editorSystem.SelectedRoadNetworkElement = lane;
                    Debug.Log("select lane");
                }
            }

            if (IsSame(DisplayHndMaskSet.LaneAdd) == true)
            {
                // レーンの追加ボタンの表示
                var lanePos = state.lanePos + selectBtnPosOffset;
                var laneSelectBtnSize = HandleUtility.GetHandleSize(lanePos) * laneHndScaleFactor;
                var isClicked = Handles.Button(lanePos, Quaternion.identity, laneSelectBtnSize, laneSelectBtnSize, RoadNetworkLaneHandleCap);
                if (isClicked)
                {
                    state.delayCommand += () =>
                    {
                        var newLane = new RnLane();
                        editorSystem.EditOperation.AddMainLane(parent, newLane);
                    };
                    state.isDirtyTarget = true;
                }
            }

            // レーンの構造変更機能が有効である
            if (editorSystem.CurrentEditMode == RoadNetworkEditMode._EditLaneStructure)
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
                        // IRoadNetworkEditOperationを通してないので通知が通らないので注意 修正する
                        var newLanes = lane.SplitLaneSelf(2);   // 元のレーンを含めてLaneが３つになる
                        if (newLanes == null)
                            return;
                        parent.RemoveLane(lane);
                        foreach (var newLane in newLanes)
                        {
                            parent.AddMainLane(newLane);
                        }
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
                        editorSystem.EditOperation.RemoveMainLane(parent, lane);
                        Debug.Log("車線数を減らすボタンが押された");
                    };
                    state.isDirtyTarget = true;
                }

            }

            if (editorSystem.CurrentEditMode == RoadNetworkEditMode._EditLaneShape)
            {
                //if (bIsHandleLock == null || bIsHandleLock == lane)
                //{
                //    // １つのレーンの幅員を増やす
                //    if (lane.LeftWay.Count > 0)
                //    {
                //        var leftCenterIdx = lane.LeftWay.Count / 2;
                //        var scaleHandlePos = lane.LeftWay[leftCenterIdx];
                //        var dir = Vector3.up;
                //        if (lane.LeftWay.Count >= 2)
                //        {
                //            dir = lane.LeftWay.GetVertexNormal(leftCenterIdx - 1);
                //            dir.Normalize();
                //        }

                //        var size = HandleUtility.GetHandleSize(scaleHandlePos);
                //        EditorGUI.BeginChangeCheck();
                //        var scale = Handles.ScaleHandle(Vector3.one, scaleHandlePos, Quaternion.identity, size);
                //        if (EditorGUI.EndChangeCheck())
                //        {
                //            bIsHandleLock = lane;
                //            foreach (var way in lane.BothWays)
                //            {
                //                int i = 0;
                //                foreach (var point in way.Points)
                //                {
                //                    var vertNorm = way.GetVertexNormal(i++);
                //                    point.Vertex = point + (scale - 1) * 0.1f * vertNorm;
                //                    state.isDirtyTarget = true;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            bIsHandleLock = null;
                //        }
                //    }
                //}
            }

        }

        private bool IsContains(DisplayHndMaskSet mask)
        {
            return (displayHandleMask & (int)mask) != 0;
        }

        private bool IsSame(DisplayHndMaskSet mask)
        {
            return (displayHandleMask & (int)mask) == (int)mask;
        }

        private void ForeachLink(IRoadNetworkEditingSystem editorSystem, IReadOnlyList<RnRoad> links, RnRoad link, ref SceneGUIState state)
        {
            state.linkPos = CalcLinkPos(link);

            if (IsVisibleDistance(state.currentCamera, state.linkPos, 500.0f) == false)
            {
                state.isContinue = true;
                return;
            }

            bool isEditable = false;
            // 自身が選択されていない
            if (link != editorSystem.SelectedRoadNetworkElement as RnRoad)
            {
                //子の要素が選択されていない
                var lane = editorSystem.SelectedRoadNetworkElement as RnLane;
                if (lane?.Parent != link)
                {
                    isEditable = true;
                }
            }

            // 表示する場合は子の要素を扱わない
            if (isEditable)
            {
                state.isContinue = true;
            }

            // 表示しない（実在はする）
            if (IsContains(DisplayHndMaskSet.Link) == false)
            {
                isEditable = false;
            }

            if (isEditable)
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

        private void ForeachBothWayPoints(IRoadNetworkEditingSystem sys, RnLane lane, RnWay parent, RnPoint point, ref SceneGUIState state)
        {
            var isEditable = false;
            isEditable = true;

            // 表示しない（実在はする）
            if (IsContains(DisplayHndMaskSet.Point) == false)
            {
                isEditable = false;
            }

            var networkOperator = sys.EditOperation;
            var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;

            //if (isEditable && IsSame(DisplayHndMaskSet.PointSelect))
            //if (isEditable)
            //{
            //    // Lane追加モードの処理
            //    if (sys.CurrentEditMode == RoadNetworkEditMode.AddLane)
            //    {

            //        var isClicked = Handles.Button(point, Quaternion.identity, size, size, Handles.SphereHandleCap);
            //        if (isClicked)
            //        {
            //            sys.RoadNetworkSimpleLaneGenerateModule.AddBorder(link, parent, point);
            //            if (sys.RoadNetworkSimpleLaneGenerateModule.CanBuild())
            //            {
            //                state.delayCommand += () =>
            //                {
            //                    Debug.Log("Laneが追加された");
            //                    sys.RoadNetworkSimpleLaneGenerateModule.BuildLane(lane.Parent);
            //                };

            //            }
            //            state.isDirtyTarget = true;
            //        }
            //        return;
            //    }
            //}

            if (sys.CurrentEditMode == RoadNetworkEditMode._EditLaneShape)
            {

                if (isEditable && IsSame(DisplayHndMaskSet.PointMove))
                {
                    EditorGUI.BeginChangeCheck();
                    //var vertPos = DeployTranslateHandle(point);
                    var vertPos = DeployFreeMoveHandle(point, size, snap: Vector3.zero);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var res = networkOperator.MovePoint(point, vertPos);
                        state.isDirtyTarget = true;
                        Debug.Assert(res.IsSuccess);
                    }
                    return;
                }

                if (isEditable && IsSame(DisplayHndMaskSet.PointAdd))
                {
                    var isClicked = Handles.Button(point, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                    if (isClicked)
                    {
                        // parent.Pointsからpointを検索してインデックスを取得
                        var idx = parent.Points.ToList().IndexOf(point);
                        if (idx == -1)
                            return;
                        state.delayCommand += () =>
                        {
                            networkOperator.AddPoint(parent, idx, new RnPoint(point.Vertex + Vector3.up));
                            Debug.Log("ポイント追加ボタンが押された");
                        };
                        state.isDirtyTarget = true;
                    }
                    return;
                }

                if (isEditable && IsSame(DisplayHndMaskSet.PointRemove))
                {
                    var isClicked = Handles.Button(point, Quaternion.identity, size, size, RoadNetworkSplitLaneButtonHandleCap);
                    if (isClicked)
                    {
                        state.delayCommand += () =>
                        {
                            networkOperator.RemovePoint(parent, point);
                            Debug.Log("ポイント削除ボタンが押された");
                        };
                        state.isDirtyTarget = true;
                    }
                    return;
                }
            }
        }
        private void ForeachAllBorderPoints(IRoadNetworkEditingSystem sys, RnRoad link, RnWay parent, RnPoint point, ref SceneGUIState state, Vector3 offset)
        {
            var isEditable = false;
            isEditable = true;

            // 表示しない（実在はする）
            if (IsContains(DisplayHndMaskSet.Point) == false)
            {
                isEditable = false;
            }

            var networkOperator = sys.EditOperation;
            var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;

            //if (isEditable && IsSame(DisplayHndMaskSet.PointSelect))
            if (isEditable)
            {
                // Lane追加モードの処理
                if (sys.CurrentEditMode == RoadNetworkEditMode._AddLane)
                {
                    var isClicked = Handles.Button(point + offset, Quaternion.identity, size, size, Handles.SphereHandleCap);
                    if (isClicked)
                    {
                        sys.RoadNetworkSimpleLaneGenerateModule.AddBorder(link, parent, point);
                        if (sys.RoadNetworkSimpleLaneGenerateModule.CanBuild())
                        {
                            state.delayCommand += () =>
                            {
                                Debug.Log("Laneが追加された");
                                sys.RoadNetworkSimpleLaneGenerateModule.Build();
                            };

                        }
                        state.isDirtyTarget = true;
                    }
                    return;
                }

                if (sys.CurrentEditMode == RoadNetworkEditMode._AddLink)
                {
                    var isClicked = Handles.Button(point + offset, Quaternion.identity, size, size, Handles.SphereHandleCap);
                    if (isClicked)
                    {
                        sys.RoadNetworkSimpleLinkGenerateModule.AddPoint(link.ParentModel, point);
                        if (sys.RoadNetworkSimpleLinkGenerateModule.CanBuild())
                        {
                            state.delayCommand += () =>
                            {
                                Debug.Log("Linkが追加された");
                                sys.RoadNetworkSimpleLinkGenerateModule.Build();
                            };

                        }
                        state.isDirtyTarget = true;
                    }
                    return;
                }
            }
        }

        private void ForeachSignalLight(IRoadNetworkEditingSystem editorSystem, List<TrafficSignalLight> signalLights, TrafficSignalLight signalLight, ref SceneGUIState state)
        {
            state.signalLightPos = signalLight.position;

            Vector3 pos2d_dis = Vector3.zero;
            var isDisplayNode = true;
            if (isDisplayNode)
            {
                pos2d_dis = state.currentCamera.WorldToScreenPoint(state.signalLightPos);
                isDisplayNode = IsVisibleToCamera(state.currentCamera, pos2d_dis);
            }

            if (editorSystem.CurrentEditMode == RoadNetworkEditMode.EditTrafficRegulation)
            {
                var size = HandleUtility.GetHandleSize(signalLight.position) * signalLightHndScaleFactor;
                //bool isClicked = Handles.Button(signalLight.position, Quaternion.identity, size, size, RoadNetworkTrafficSignalLightCap);
                var isClicked = Button2DOn3D(state, pos2d_dis, trafficLight_blueTex);
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

            Vector3 pos2d_dis = Vector3.zero;
            if (isDisplay)
            {
                pos2d_dis = state.currentCamera.WorldToScreenPoint(state.signalControllerPos);
                isDisplay = IsVisibleToCamera(state.currentCamera, pos2d_dis);
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
                    //bool isClicked = Handles.Button(signalController.Position, Quaternion.identity, size, size, RoadNetworkTrafficSignalLightCap);
                    var isClicked = Button2DOn3D(state, pos2d_dis, trafficLightControllerTex);
                    if (isClicked)
                    {
                        editorSystem.SelectedRoadNetworkElement = signalController;
                        Debug.Log(signalController.SelfId);
                    }
                }
            }
        }

        private bool IsVisibleDistance(Camera camera, Vector3 pos, float distance)
        {
            var sqrMag = Vector3.SqrMagnitude(camera.transform.position - pos);
            return sqrMag < distance * distance;
        }

        private void ForeachNode(IRoadNetworkEditingSystem editorSystem, IReadOnlyList<RnIntersection> nodes, RnIntersection node, ref SceneGUIState state)
        {
            state.nodePos = node.GetCenterPoint();

            if (IsVisibleDistance(state.currentCamera, state.nodePos, 500.0f) == false)
            {
                state.isContinue = true;
                return;
            }

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

            // 表示しない（実在はする）
            if (IsContains(DisplayHndMaskSet.Node) == false)
            {
                isDisplayNode = false;
            }

            Vector3 pos2d_dis = Vector3.zero;
            if (isDisplayNode)
            {
                pos2d_dis = state.currentCamera.WorldToScreenPoint(state.nodePos);
                isDisplayNode = IsVisibleToCamera(state.currentCamera, pos2d_dis);
            }

            if (isDisplayNode)
            {
                var selectbtnPos = state.nodePos /*+ selectBtnPosOffset*/;
                var selectBtnHandleDefaultSize = linkHndScaleFactor;
                var size = HandleUtility.GetHandleSize(selectbtnPos) * selectBtnHandleDefaultSize;
                var pickSize = size;

                var isClicked = Button2DOn3D(state, pos2d_dis, nodeTex);

                //var isClicked = GUI.Button(buttonRect, "Node"+pos2d);
                //var isClicked = Handles.Button(
                //selectbtnPos, state.currentCamera.transform.rotation, size, pickSize, RoadNetworkNodeHandleCap);
                if (isClicked)
                {
                    editorSystem.SelectedRoadNetworkElement = node;
                    Debug.Log("select" + editorSystem.SelectedRoadNetworkElement.ToString());
                }

            }


        }

        private bool Button2DOn3D(SceneGUIState state, Vector3 pos2d_dis, Texture2D texture)
        {
            return Button2DOn3D(state.currentCamera, pos2d_dis, texture);
        }

        private bool Button2DOn3D(Camera camera, Vector3 pos2d_dis, Texture2D texture)
        {
            var size2d = Vector2.one * 50;
            var pos2d = new Vector2(pos2d_dis.x, camera.pixelHeight - pos2d_dis.y);
            var buttonRect = new Rect(pos2d, size2d);
            var style = new GUIStyle();
            style.normal.background = null;
            Handles.BeginGUI();
            var isClicked = GUI.Button(buttonRect, texture, style);
            Handles.EndGUI();
            return isClicked;
        }

        public bool IsVisibleToCamera(Camera camera, Vector3 screenPoint)
        {
            // Check if the viewportPoint is inside the camera's viewport
            bool isVisible = screenPoint.x >= 0 && screenPoint.x <= camera.pixelWidth &&
                             screenPoint.y >= 0 && screenPoint.y <= camera.pixelHeight &&
                             screenPoint.z > 0; // Also check if the point is in front of the camera

            return isVisible;
        }

        private static Vector3 CalcLinkPos(RnRoad link)
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

        private static Vector3 CalcLanePos(RnLane centerLane)
        {
            return centerLane.GetCenter();
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
