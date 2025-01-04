using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
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
        
        /// <summary> シーンビュー上に描画するGUI </summary>
        public RoadNetworkEditSceneViewGui editSceneViewGui;

        /// <summary> シーンビュー上で、編集対象の道路または交差点を選択するボタンを表示する </summary>
        public RoadNetworkEditTargetSelectButton EditTargetSelectButton { get; set; }
        
        /// <summary> 道路ネットワークの編集対象です。 </summary>
        public RoadNetworkEditTarget roadNetworkEditTarget;

        /// <summary> 信号情報の編集。現在は使われていません。 </summary>
        public TrafficSignalEditor trafficSignalEditor;
        

        private const string roadNetworkEditingSystemObjName = "_RoadNetworkEditingSystemRoot";
        private GameObject roadNetworkEditingSystemObjRoot;
        private PLATEAURnStructureModel structureModel;


        public static RoadNetworkEditingSystem TryInitalize(
            RoadNetworkEditingSystem oldSystem)
        {

            var newSystem = oldSystem;
            if (newSystem != null)
            {
                newSystem.Terminate();
            }

            newSystem =
                new RoadNetworkEditingSystem();
            
            
            return newSystem;
        }

        /// <summary>
        /// シーンビュー上に描画します
        /// </summary>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (structureModel == null) return;
            var guiSystem = EditTargetSelectButton;
            guiSystem.OnSceneGUI(structureModel);

            if (editSceneViewGui == null)
            {
                Debug.Log("editSceneViewGui is null.");
                return;
            }
            editSceneViewGui.Update(sceneView);
            
            var splineEditSystem = editSceneViewGui.SplineEditorMod;
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

        

        private void Terminate()
        {
            editSceneViewGui?.Terminate();
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
                EditTargetSelectButton = new RoadNetworkEditTargetSelectButton(editSceneViewGui, roadNetworkEditTarget);
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


            editSceneViewGui = new RoadNetworkEditSceneViewGui(roadNetworkEditingSystemObjRoot, roadNetwork, EditTargetSelectButton,
                roadNetworkEditTarget);
            //simpleEditSysModule.Init();

            return true;
        }

        
        
    }
}