using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static PLATEAU.Editor.RoadNetwork.EditingSystem.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.RoadNetwork
{
    /// <summary>
    /// SceneGUIまわりの機能を管理するクラス
    /// 記述先のファイルを変更するかも？
    /// </summary>
    internal class RoadNetworkSceneGUISystem
    {

        private const string LaneTexPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/Icon_lane.png";
        private const string NodeTexPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/Icon_node.png";

        private const string TrafficLightControllerPath =
            "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/Icon_trafficLightController.png";

        private const string TrafficLightBlueTexPath =
            "Packages/com.synesthesias.plateau-unity-sdk/Resources/Icon/trafficLight_blue.png";

        Texture2D laneTex;
        Texture2D nodeTex;
        Texture2D trafficLightControllerTex;
        Texture2D trafficLight_blueTex;

        public RoadNetworkSceneGUISystem(IRoadNetworkEditingSystem editorSystem)
        {
            Assert.IsNotNull(editorSystem);
            this.editorSystem = editorSystem;
        }

        private const float pointHndScaleFactor = 0.15f;
        private const float laneHndScaleFactor = 0.4f;
        private const float linkHndScaleFactor = 0.5f;
        private const float signalLightHndScaleFactor = 0.2f;
        private const float signalControllerScaleFactor = 0.3f;
        private readonly Vector3 selectBtnPosOffset = Vector3.up * 10.0f;

        private IRoadNetworkEditingSystem editorSystem;

        private SceneGUISystemState systemState;
        
        public IReadOnlyCollection<EditorData<RnRoadGroup>> connections = new EditorData<RnRoadGroup>[0];
        public Color connectionColor = Color.blue;

        public ICollection<EditorData<RnIntersection>> intersections = new EditorData<RnIntersection>[0];
        public Color intersectionColor = Color.green;
        public float intersectionRadius = 30.0f;

        public float btnSize = 10.0f;

        public List<RnRoadGroup> SimLanes;

        public bool EnableLimitSceneViewDefaultContorl { get; set; }

        /// <summary>
        /// 道路編集において、シーンビュー上で編集対象を選択し、シーンビュー上で編集し、結果を適用します。
        /// </summary>
        public void OnSceneGUI(UnityEngine.Object target)
        {
            SetRoadNetworkObject2System(target);
            var network = GetRoadNetwork();
            if (network == null)
                return;

            if (EnableLimitSceneViewDefaultContorl)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
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
                OnSceneGUISimpleEdit(); // ここが描画メイン
                return;
            }
        }

        private bool LoadTexture()
        {
            // var isSuc = true;
            nodeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(NodeTexPath);
            if (nodeTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + NodeTexPath);
                // isSuc = false;
            }

            laneTex = AssetDatabase.LoadAssetAtPath<Texture2D>(LaneTexPath);
            if (laneTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + LaneTexPath);
                // isSuc = false;
            }

            trafficLightControllerTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TrafficLightControllerPath);
            if (trafficLightControllerTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + TrafficLightControllerPath);
                // isSuc = false;
            }

            trafficLight_blueTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TrafficLightBlueTexPath);
            if (trafficLight_blueTex == null)
            {
                Debug.LogError("テクスチャが見つかりませんでした: " + TrafficLightBlueTexPath);
                // isSuc = false;
            }

            return true;
        }

        /// <summary>
        /// 道路編集において、シーンビュー上で編集対象を選択し、シーンビュー上で編集し、結果を適用します。
        /// </summary>
        private void OnSceneGUISimpleEdit()
        {

            var camera = SceneView.currentDrawingSceneView.camera;

            // 対象選択
            DrawTargetSelectButtonRoad(camera);
            DrawTargetSelectButtonIntersection(camera);


            // 選択している道路がある場合、編集画面を表示します
            var selectedConnection = editorSystem.SelectedRoadNetworkElement as EditorData<RnRoadGroup>;
            if (selectedConnection != null)
            {
                DrawSelectedRoadGroup(selectedConnection);
            }

            // 選択している交差点がある場合、編集画面を表示します
            var intersectionEditorData = editorSystem.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            if (intersectionEditorData != null)
            {
                DrawSelectedIntersection(intersectionEditorData);
            }
        }

        /// <summary>
        /// 編集対象を選ぶためのアイコンのうち、道路アイコンをシーンビュー上に表示します。
        /// </summary>
        private void DrawTargetSelectButtonRoad(Camera camera)
        {
            var roadIconPosOffset = Vector3.up * 0;

            foreach (var item in connections)
            {
                // 選択済みのオブジェクト
                if (item == editorSystem.SelectedRoadNetworkElement)
                    continue;

                var subData = item.ReqSubData<RoadGroupEditorData>();

                var btnP = subData.GetCenter();

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
                        //Debug.Log(subData.A.ToString() + "-" + subData.B.ToString()); // デバッグ用
                        editorSystem.SelectedRoadNetworkElement = item;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 編集対象を選ぶためのアイコンのうち、交差点アイコンをシーンビュー上に表示します。
        /// </summary>
        private void DrawTargetSelectButtonIntersection(Camera camera)
        {
            var nodeIconPosOffset = Vector3.up * 0;
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
            }
        }

        private void DrawSelectedRoadGroup(EditorData<RnRoadGroup> selectedConnection)
        {
            var roadGroupEditorData = selectedConnection;


            if (editorSystem.RoadNetworkSimpleEditModule.SplineEditorMod.IsEnabled)
            {
                // スプライン編集モード。何もしない
            }
            else if (editorSystem.RoadNetworkSimpleEditModule.IsDetailMode() == false)
            {
                // 簡易モードで表示
                // 各車線、歩道、張横分離帯の幅を調整するためのハンドル描画

                var nLeftLane = roadGroupEditorData.Ref.GetLeftLaneCount();
                var nRightLane = roadGroupEditorData.Ref.GetRightLaneCount();
                var nLane = nLeftLane + nRightLane;
                HashSet<RnWay> ways = new HashSet<RnWay>(nLane * 2);
                List<RnWay> unionWay = new List<RnWay>(nLane * 2 - 2); // 端の2つは覗く
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

                Handles.BeginGUI();
                Handles.EndGUI();
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
                        foreach (var point in sideWalk.OutsideWay.Points)
                        {
                            if (state.isDirtyTarget)
                            {
                                break;
                            }

                            var parent = sideWalk.OutsideWay;

                            var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;

                            DeployPointMoveHandle(point, state, size);
                        }
                    }

                    foreach (var lane in road.AllLanes)
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

                                var networkOperator = editorSystem.EditOperation;
                                var size = HandleUtility.GetHandleSize(point) * pointHndScaleFactor;


                                // ctrlを押しているか
                                if (Event.current.control == false)
                                {
                                    DeployPointMoveHandle(point, state, size);
                                    continue;
                                }
                                else
                                {
                                    var currentEvent = Event.current;
                                    {
                                        // ポイントの追加
                                        if (currentEvent.shift == false)
                                        {
                                            // ポイントの追加ボタンの表示
                                            var isClicked = Handles.Button(point, Quaternion.identity, size, size,
                                                RoadNetworkAddPointButtonHandleCap);
                                            if (isClicked)
                                            {
                                                // parent.Pointsからpointを検索してインデックスを取得
                                                var idx = parent.Points.ToList().IndexOf(point);
                                                if (idx == -1)
                                                    continue;
                                                state.delayCommand += () =>
                                                {
                                                    networkOperator.AddPoint(parent, idx,
                                                        new RnPoint(point.Vertex + Vector3.up));
                                                    Debug.Log("ポイント追加ボタンが押された");
                                                };
                                                state.isDirtyTarget = true;
                                                continue;
                                            }
                                        }
                                        // ポイントの削除
                                        else
                                        {
                                            // ポイントの削除ボタンの表示
                                            var isClicked = Handles.Button(point, Quaternion.identity, size, size,
                                                RoadNetworkRemovePointButtonHandleCap);
                                            if (isClicked)
                                            {
                                                state.delayCommand += () =>
                                                {
                                                    networkOperator.RemovePoint(parent, point);
                                                    Debug.Log("ポイント削除ボタンが押された");
                                                };
                                                state.isDirtyTarget = true;
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 遅延実行 コレクションの要素数などを変化させる
                if (state.delayCommand != null)
                    state.delayCommand.Invoke();

                // 選択した道路オブジェクトに変更があったとき
                if (state.isDirtyTarget)
                {
                    editorSystem.NotifyChangedRoadNetworkObject2Editor(); // 通知
                }

                systemState.Apply(state);
            }
        }

        private void DrawSelectedIntersection(EditorData<RnIntersection> intersectionData)
        {
            // 簡易モードで表示
            if (editorSystem.RoadNetworkSimpleEditModule.IsDetailMode() == false)
            {
                SceneGUIState state = new SceneGUIState();
                systemState.Init(out state);

                var currentCamera = SceneView.currentDrawingSceneView.camera;
                state.currentCamera = currentCamera;


                var EditingIntersectionMod = editorSystem.RoadNetworkSimpleEditModule.EditingIntersectionMod;

                var buttonSize = 2.0f;

                bool isSelectdEntablePoint = EditingIntersectionMod.IsSelectdEntablePoint;
                if (isSelectdEntablePoint == false)
                {
                    foreach (var item in EditingIntersectionMod.EnterablePoints)
                    {
                        // 流入点の位置にボタンを表示する
                        if (Handles.Button(item.CalcCenter(), Quaternion.identity, buttonSize, buttonSize,
                                RoadNetworkEntarablePointButtonHandleCap))
                        {
                            EditingIntersectionMod.SetEntablePoint(item);
                            // 流入点が選択された
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var item in EditingIntersectionMod.ExitablePoints)
                    {
                        // 流出点の位置にボタンを表示する
                        if (Handles.Button(item.CalcCenter(), Quaternion.identity, buttonSize, buttonSize,
                                RoadNetworkExitablePointButtonHandleCap))
                        {
                            // 流出点が選択された
                            EditingIntersectionMod.SetExitablePoint(item);
                            break;
                        }
                    }
                }

                // Trackの生成、削除に必要な設定が済んで更新できるか？
                if (EditingIntersectionMod.CanTryUpdateTrack)
                {
                    EditingIntersectionMod.UpdateTrack();
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
            else // 詳細モードでのみ表示
            {
            }
        }

        private static void DeployPointMoveHandle(RnPoint point, SceneGUIState state, float size)
        {
            EditorGUI.BeginChangeCheck();
            var vertPos = DeployFreeMoveHandle(point, size, snap: Vector3.zero);
            if (EditorGUI.EndChangeCheck())
            {
                var mousePos = Event.current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(mousePos);
                const float maxRayDistance = 1000.0f;
                RoadNetworkEditingSystem.SnapPointToObj(point, ray, maxRayDistance, "dem_", "tran_");
                state.isDirtyTarget = true;
            }
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

        private bool IsVisibleDistance(Camera camera, Vector3 pos, float distance)
        {
            var sqrMag = Vector3.SqrMagnitude(camera.transform.position - pos);
            return sqrMag < distance * distance;
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
            return centerLane.GetCentralVertex();
        }


        private static void RoadNetworkTrafficSignalLightCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
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

        private static void RoadNetworkNodeHandleCap(int controlID, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
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

        private static void RoadNetworkLinkHandleCap(int controlID, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
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
                    Handles.DrawWireCube(position + Vector3.right * subCubeSize,
                        new Vector3(subCubeSize, subCubeSize, subCubeSize));
                    Handles.DrawWireCube(position + Vector3.left * subCubeSize,
                        new Vector3(subCubeSize, subCubeSize, subCubeSize));
                    Handles.DrawLine(position + Vector3.right * size * linkHndScaleFactor,
                        position + Vector3.left * size * linkHndScaleFactor);
                    break;
            }
        }

        private static void RoadNetworkLaneHandleCap(int controlID, Vector3 position, Quaternion rotation, float size,
            EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireCube(position, new Vector3(size, size, size));
                    Handles.DrawWireCube(position,
                        new Vector3(size, size * pointHndScaleFactor, size * signalControllerScaleFactor));
                    break;
            }
        }

        private static void RoadNetworkSplitLaneButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * linkHndScaleFactor);
                    Handles.DrawWireCube(position + Vector3.forward * 0.07f,
                        new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    Handles.DrawWireCube(position + Vector3.back * 0.07f,
                        new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    break;
            }
        }

        private static void RoadNetworkAddPointButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * linkHndScaleFactor);
                    Handles.DrawWireCube(position, new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    Handles.DrawWireCube(position, new Vector3(size * 0.15f, size * pointHndScaleFactor, size));
                    break;
            }
        }

        private static void RoadNetworkRemovePointButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * linkHndScaleFactor);
                    Handles.DrawWireCube(position, new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    break;
            }
        }

        private static void RoadNetworkEntarablePointButtonHandleCap(int controlID, Vector3 position,
            Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Repaint)
                Handles.color = Color.red;
            Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
        }

        private static void RoadNetworkExitablePointButtonHandleCap(int controlID, Vector3 position,
            Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Repaint)
                Handles.color = Color.blue;
            Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
        }

        private static void RoadNetworkRemoveLaneButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up, size * linkHndScaleFactor);
                    Handles.DrawWireCube(position + Vector3.forward * 0.07f,
                        new Vector3(size, size * pointHndScaleFactor, size * 0.15f));
                    break;
            }
        }

        private static Vector3 DeployFreeMoveHandle(in Vector3 pos, float size, in Vector3 snap)
        {
            return Handles.FreeMoveHandle(pos, size, snap, Handles.SphereHandleCap);
        }
        
        
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
            Point = DisplayHndOperationMask.NoOperation, // 
            PointAdd = Point | DisplayHndOperationMask.Add, // 
            PointRemove = Point | DisplayHndOperationMask.Remove, // 
            PointMove = Point | DisplayHndOperationMask.Move, // 
            PointSelect = Point | DisplayHndOperationMask.Select, // 
            PointZeroMask = ~(PointAdd | PointRemove | PointMove | PointSelect),

            _LaneShiftOffset = DisplayHndType.Lane * DisplayHndOperation._Num,
            Lane = DisplayHndOperationMask.NoOperation << _LaneShiftOffset, // 
            LaneAdd = Lane | DisplayHndOperationMask.Add << _LaneShiftOffset,
            LaneRemove = Lane | DisplayHndOperationMask.Remove << _LaneShiftOffset,

            //LaneMove = Lane | DisplayHndOperationMask.Move << _LaneShiftOffset,
            LaneSelect = Lane | DisplayHndOperationMask.Select << _LaneShiftOffset,
            LaneZeroMask = ~(LaneAdd | LaneRemove | LaneSelect),

            _LinkShiftOffset = DisplayHndType.Link * DisplayHndOperation._Num,
            Link = DisplayHndOperationMask.NoOperation << _LinkShiftOffset, // 
            LinkAdd = Link | DisplayHndOperationMask.Add << _LinkShiftOffset,
            LinkRemove = Link | DisplayHndOperationMask.Remove << _LinkShiftOffset,

            //LinkMove = Road | DisplayHndOperationMask.Move << _LinkShiftOffset,
            LinkSelect = Link | DisplayHndOperationMask.Select << _LinkShiftOffset,
            LinkZeroMask = ~(LinkAdd | LinkRemove | LinkSelect),

            _NodeShiftOffset = DisplayHndType.Node * DisplayHndOperation._Num,
            Node = DisplayHndOperationMask.NoOperation << _NodeShiftOffset, // 

            //NodeAdd = Node | DisplayHndOperationMask.Add << _NodeShiftOffset,
            //NodeRemove = Node | DisplayHndOperationMask.Remove << _NodeShiftOffset,
            //NodeMove = Node | DisplayHndOperationMask.Move << _NodeShiftOffset,
            NodeSelect = Node | DisplayHndOperationMask.Select << _NodeShiftOffset,
            NodeZeroMask = ~(NodeSelect),

            ZeroMask = PointZeroMask & LaneZeroMask & LinkZeroMask & NodeZeroMask
        }
        
        /// <summary>
        /// OnSceneGUI()内での状態
        /// </summary>
        private struct SceneGUIState
        {
            public bool isDirtyTarget; // ターゲットに変更があったか
            public Action delayCommand; // 遅延コマンド　要素の追加や削除を行う際に利用する foreach外で利用する 

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
        }

        /// <summary>
        /// クラス内の状態
        /// </summary>
        private struct SceneGUISystemState
        {
            public void Init(out SceneGUIState state)
            {
                state = new SceneGUIState { isDirtyTarget = false, delayCommand = null, };
            }

            public void Apply(in SceneGUIState state)
            {
            }
        }
    }
}