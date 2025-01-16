using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路ネットワーク編集システムのインスタンス
    /// 管理元
    /// 出来るだけサブシステム同士で連携を取らないようにする
    /// </summary>
    internal class RoadNetworkEditingSystem
    {
        
        /// <summary> シーンビュー上で道路を変更するGUI </summary>
        public RoadNetworkEditSceneViewGui roadEditSceneViewGui;

        /// <summary> シーンビュー上で交差点を編集するGUI </summary>
        public IntersectionEditSceneViewGui intersectionEditSceneViewGui;

        /// <summary> シーンビュー上で、編集対象の道路または交差点を選択するボタンを表示する </summary>
        public RoadNetworkEditTargetSelectButton EditTargetSelectButton { get; set; }
        
        /// <summary> 道路ネットワークの編集対象です。 </summary>
        public RoadNetworkEditTarget roadNetworkEditTarget;

        /// <summary> 信号情報の編集。現在は使われていません。 </summary>
        public TrafficSignalEditor trafficSignalEditor;
        
        

        private const string roadNetworkEditingSystemObjName = "_RoadNetworkEditingSystemRoot";
        private GameObject roadNetworkEditingSystemObjRoot;
        private PLATEAURnStructureModel structureModel;

        /// <summary>
        /// シーンビュー上に描画します
        /// </summary>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (structureModel == null) return;
            var guiSystem = EditTargetSelectButton;
            guiSystem.OnSceneGUI(structureModel);

            if (roadEditSceneViewGui == null)
            {
                Debug.Log("editSceneViewGui is null.");
                return;
            }
            roadEditSceneViewGui.Update(sceneView);
            intersectionEditSceneViewGui.Update();
            
            var splineEditSystem = roadEditSceneViewGui.SplineEditorMod;
            splineEditSystem.OnSceneGUI(structureModel);
        }
        

        public static void TryTerminate(
            RoadNetworkEditingSystem oldSystem, VisualElement root)
        {
            if (root == null)
            {
                Debug.LogError("Root is null.");
                return;
            }

            if (oldSystem != null)
            {
                oldSystem.Terminate();
            }
            
        }


        public RoadNetworkEditingSystem()
        {
            TryInitialize();
        }

        

        public void Terminate()
        {
            roadEditSceneViewGui?.Terminate();
            intersectionEditSceneViewGui?.Terminate();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// 初期化を試みる
        /// 多重初期化はしないので複数回呼び出しても問題ない
        /// </summary>
        private bool TryInitialize()
        {
            trafficSignalEditor = new TrafficSignalEditor();
            
            
            // 初期化の必要性チェック
            bool needInitGUISystem = EditTargetSelectButton == null;
            bool needInitGameObj = roadNetworkEditingSystemObjRoot == null;

            roadNetworkEditTarget = new RoadNetworkEditTarget();
            if (needInitGUISystem)
            {
                EditTargetSelectButton = new RoadNetworkEditTargetSelectButton(roadEditSceneViewGui, roadNetworkEditTarget);
            }

            if (needInitGameObj)
            {
                // 編集オブジェクトを選択中の場合は選択を解除する
                var activeObj = Selection.activeGameObject;
                while (activeObj != null)
                {
                    if (activeObj.name == roadNetworkEditingSystemObjName)
                    {
                        Selection.activeGameObject = null;
                        break;
                    }

                    activeObj = activeObj.transform.parent?.gameObject;
                }


                // 既存の編集オブジェクトを削除
                roadNetworkEditingSystemObjRoot = GameObject.Find(roadNetworkEditingSystemObjName);
                if (roadNetworkEditingSystemObjRoot != null)
                {
                    GameObject.DestroyImmediate(roadNetworkEditingSystemObjRoot);
                }

                // 新規の編集オブジェクトを作成 
                roadNetworkEditingSystemObjRoot = new GameObject(roadNetworkEditingSystemObjName, typeof(RoadNetworkEditorGizmos));

                SceneView.duringSceneGui += OnSceneGUI;
            }

            // 道路ネットワークの取得を試みる
            var r = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (r == null)
            {
                Debug.Log("Can't find PLATEAURnStructureModel");
                return false;
            }

            structureModel = r;
            var roadNetwork = r.RoadNetwork;
            if (roadNetwork == null)
            {
                Debug.Log("RoadNetwork is null.");
                return false;
            }

            var roadNetworkObj = r.gameObject;

            // その他 初期化
            if (roadNetworkObj == null)
            {
                Debug.Log("roadNetworkObj is null.");
                return false;
            }

            // 自動設定機能
            Selection.activeGameObject = roadNetworkObj;
            roadNetworkEditTarget.RoadNetworkComponent = structureModel;

            // 道路ネットワークを地形にスナップします
            // var lineE = roadNetwork.CollectAllWays().GetEnumerator();
            // var snapper = new RoadNetworkEditLandSnapper();
            // while (lineE.MoveNext())
            // {
            //     var way = lineE.Current;
            //     snapper.SnapPointsToDemAndTran(way.Points);
            // }


            roadEditSceneViewGui = new RoadNetworkEditSceneViewGui(roadNetworkEditingSystemObjRoot, roadNetwork, EditTargetSelectButton,
                roadNetworkEditTarget);
            intersectionEditSceneViewGui = new IntersectionEditSceneViewGui(roadNetworkEditTarget);
            //simpleEditSysModule.Init();

            return true;
        }

        /// <summary> 「詳細編集モード」のチェックボックスが変わった時 </summary>
        public void ChangeDetailEditMode(bool newValue)
        {
            roadEditSceneViewGui.SetDetailMode(newValue);
        }
        
    }
}