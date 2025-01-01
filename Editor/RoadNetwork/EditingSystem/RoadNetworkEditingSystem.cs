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
        
        public static RoadNetworkEditingSystem SingletonInstance;
        
        /// <summary> シーンビュー上に描画するGUI </summary>
        public RoadNetworkEditSceneViewGui editSceneViewGui;

        /// <summary> シーンビュー上で、編集対象の道路または交差点を選択するボタンを表示する </summary>
        public RoadNetworkEditTargetSelectButton EditTargetSelectButton { get; set; }
        

        // 選択している道路ネットワークを所持したオブジェクト
        public UnityEngine.Object roadNetworkObject;
        // 選択している道路ネットワーク
        public RnModel roadNetworkModel;

        // 選択中の道路ネットワーク要素 Road,Lane,Block...etc
        public System.Object selectedRoadNetworkElement;

        // 選択中の信号制御器のパターン
        public TrafficSignalControllerPattern selectedSignalPattern;
        // 選択中の信号制御器のパターンのフェーズ
        public TrafficSignalControllerPhase selectedSignalPhase;

        // 内部システム同士が連携する時や共通データにアクセスする際に利用する
        public readonly EditingSystem system;

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

            // 仮
            RoadNetworkEditingSystem.SingletonInstance = null;
            return;
        }


        public RoadNetworkEditingSystem()
        {
            system = new EditingSystem(this);
            TryInitialize();

            SingletonInstance = this;
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
            // 初期化の必要性チェック
            bool needInitGUISystem = EditTargetSelectButton == null;
            bool needInitGameObj = roadNetworkEditingSystemObjRoot == null;
            

            if (needInitGUISystem)
            {
                EditTargetSelectButton = new RoadNetworkEditTargetSelectButton(system);
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
            if (roadNetwork != null)
            {
                // 自動設定機能
                this.roadNetworkObject = roadNetworkObj;
                Selection.activeGameObject = roadNetworkObj;

                system.RoadNetworkObject = roadNetworkObj;

                // 道路ネットワークを地形にスナップします
                // var lineE = roadNetwork.CollectAllWays().GetEnumerator();
                // var snapper = new RoadNetworkEditLandSnapper();
                // while (lineE.MoveNext())
                // {
                //     var way = lineE.Current;
                //     snapper.SnapPointsToDemAndTran(way.Points);
                // }


                editSceneViewGui = new RoadNetworkEditSceneViewGui(roadNetworkEditingSystemObjRoot, roadNetwork, system);
                //simpleEditSysModule.Init();

            }

            return true;
        }

        
        
    }
}