using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork
{
    /// <summary>
    /// 道路の編集モードをオンにしたときにシーンビュー上に表示される、編集対象の道路や交差点を選択するためのボタンです。
    /// </summary>
    internal class RoadNetworkEditTargetSelectButton
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
        private RoadNetworkEditTarget editTarget;
        private RoadNetworkEditSceneViewGui editSceneViewGui;

        public RoadNetworkEditTargetSelectButton(RoadNetworkEditSceneViewGui editSceneViewGui, RoadNetworkEditTarget editTarget)
        {
            this.editTarget = editTarget;
            this.editSceneViewGui = editSceneViewGui;
        }

        public const float PointHndScaleFactor = 0.15f;
        private const float laneHndScaleFactor = 0.4f;
        public const float LinkHndScaleFactor = 0.5f;
        
        private readonly Vector3 selectBtnPosOffset = Vector3.up * 10.0f;
        
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
        public void OnSceneGUI(PLATEAURnStructureModel target)
        {
            SetRoadNetworkObject2System(target);
            var network = editTarget.RoadNetwork;
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

            OnSceneGUISimpleEdit(); // ここが描画メイン

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

            // 道路レーンの幅編集は RoadNetworkEditSceneViewGui.Update で行います。
            // 交差点編集の描画は RoadNetworkEditSceneViewGui.Updateで行います。
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
                if (item == editTarget.SelectedRoadNetworkElement)
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
                        editTarget.SelectedRoadNetworkElement = item;
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
                if (intersection == editTarget.SelectedRoadNetworkElement)
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
                        editTarget.SelectedRoadNetworkElement = intersection;
                        return;
                    }
                }
            }
        }
        
        private bool SetRoadNetworkObject2System(PLATEAURnStructureModel target)
        {
            editTarget.RoadNetworkComponent = target;
            return editTarget.RoadNetworkComponent != null;
        }
        

        private bool IsVisibleDistance(Camera camera, Vector3 pos, float distance)
        {
            var sqrMag = Vector3.SqrMagnitude(camera.transform.position - pos);
            return sqrMag < distance * distance;
        }

        private bool Button2DOn3D(RoadEditSceneGUIState state, Vector3 pos2d_dis, Texture2D texture)
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


        

        

        
        
    }
    
    /// <summary>
    /// OnSceneGUI()内での状態
    /// </summary>
    internal struct RoadEditSceneGUIState
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
}