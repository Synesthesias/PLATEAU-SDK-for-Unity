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
    internal class RoadNetworkEditingSystem : IRoadNetworkEditingSystemInterface
    {
        
        public static RoadNetworkEditingSystem SingletonInstance;

        /// <summary>
        /// 編集機能を提供するインターフェイス
        /// </summary>
        public IRoadNetworkEditOperation NetworkOperator => editOperation;

        /// <summary>
        /// シーンのGUIのシステムを提供する
        /// UnityEditor.Editorを継承するクラスでのみ使用する
        /// 呼び出す箇所は一か所にする
        /// </summary>
        public RoadNetworkSceneGUISystem SceneGUISystem => sceneGUISystem;

        public readonly ISystemInstance systemInstance;

        // 選択している道路ネットワークを所持したオブジェクト
        public UnityEngine.Object roadNetworkObject;
        // 選択している道路ネットワーク
        public RnModel roadNetworkModel;
        // 現在の編集モード
        public RoadNetworkEditMode editingMode;

        // 選択中の道路ネットワーク要素 Road,Lane,Block...etc
        public System.Object selectedRoadNetworkElement;

        // 選択中の信号制御器のパターン
        public TrafficSignalControllerPattern selectedSignalPattern;
        // 選択中の信号制御器のパターンのフェーズ
        public TrafficSignalControllerPhase selectedSignalPhase;

        // 内部システム同士が連携する時や共通データにアクセスする際に利用する
        public readonly IRoadNetworkEditingSystem system;

        public IRoadNetworkEditOperation editOperation;
        public RoadNetworkSceneGUISystem sceneGUISystem;

        // Laneの生成機能を提供するモジュール
        public RoadNetworkEditSceneViewGui editSceneViewGui;

        private const string roadNetworkEditingSystemObjName = "_RoadNetworkEditingSystemRoot";
        private GameObject roadNetworkEditingSystemObjRoot;
        private const float SnapHeightOffset = 0.1f; // ポイントスナップ時の高低差のオフセット（0だとポイント間を繋ぐ線がめり込むことがあるため）
        
        /// <summary>
        /// システムのインスタンスを管理する機能を提供するインターフェイス
        /// </summary>
        public interface ISystemInstance
        {
            void RequestReinitialize();
            void ReInitialize();
        }

        public static RoadNetworkEditingSystem TryInitalize(
            RoadNetworkEditingSystem oldSystem, VisualElement root, ISystemInstance instance)
        {
            if (root == null)
            {
                Debug.LogError("Root is null.");
                return oldSystem;
            }

            var newSystem = oldSystem;
            if (newSystem != null)
            {
                newSystem.system.Instance.ReInitialize();
                newSystem.Terminate();
            }

            newSystem =
                new RoadNetworkEditingSystem(instance, root);
            
            return newSystem;
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
                oldSystem.system.Instance.ReInitialize();
                oldSystem.Terminate();
            }

            // 仮
            RoadNetworkEditingSystem.SingletonInstance = null;
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorInstance"></param>
        /// <param name="rootVisualElement"></param>
        public RoadNetworkEditingSystem(ISystemInstance editorInstance, VisualElement rootVisualElement)
        {
            Assert.IsNotNull(editorInstance);
            this.systemInstance = editorInstance;

            Assert.IsNotNull(rootVisualElement);
            system = new EditingSystem(this);
            TryInitialize(rootVisualElement);

            SingletonInstance = this;
        }

        

        private void Terminate()
        {
            editSceneViewGui?.Terminate();
        }

        /// <summary>
        /// 初期化を試みる
        /// 多重初期化はしないので複数回呼び出しても問題ない
        /// </summary>
        /// <param name="rootVisualElement"></param>
        /// <returns></returns>
        private bool TryInitialize(VisualElement rootVisualElement)
        {
            // 初期化の必要性チェック
            bool needIniteditOperation = editOperation == null;
            bool needInitGUISystem = sceneGUISystem == null;
            bool needInitGameObj = roadNetworkEditingSystemObjRoot == null;

            // 初期化 Initlaize()
            if (needIniteditOperation)
            {
                editOperation = new RoadNetworkEditorOperation();
            }

            if (needInitGUISystem)
            {
                sceneGUISystem = new RoadNetworkSceneGUISystem(system);
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
            }

            // 道路ネットワークの取得を試みる　
            var r = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (r == null)
            {
                Debug.Log("Can't find PLATEAURnStructureModel");
                return false;
            }
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

                // 仮ポイントを　地形にスワップする
                // var lineE = roadNetwork.CollectAllWays().GetEnumerator();
                // while (lineE.MoveNext())
                // {
                //     var way = lineE.Current;
                //     SnapPointsToDemAndTran(way.Points);
                // }


                editSceneViewGui = new RoadNetworkEditSceneViewGui(roadNetworkEditingSystemObjRoot, roadNetwork, system);
                //simpleEditSysModule.Init();

            }

            return true;
        }

        public static void SnapPointsToDemAndTran(IEnumerable<RnPoint> items)
        {
            foreach (var item in items)
            {
                SnapPointToDemAndTran(item);
            }
        }

        public static void SnapPointToDemAndTran(RnPoint item)
        {
            Ray ray;
            const float rayDis = 1000.0f;
            const float maxRayDistance = rayDis * 2.0f;
            ray = new Ray(item.Vertex + Vector3.up * rayDis, Vector3.down);
            SnapPointToObj(item, ray, maxRayDistance, "dem_", "tran_");
        }

        public static void SnapPointToObj(RnPoint item, in Ray ray, float maxDistance, params string[] filter)
        {
            var hits = Physics.RaycastAll(ray, maxDistance);    // 地形メッシュが埋まっていてもスナップ出来るように

            var isTarget = false;
            var closestDist = float.MaxValue;
            Vector3 targetPos = Vector3.zero;
            foreach (RaycastHit hit in hits)
            {
                foreach (var f in filter)
                {
                    if (hit.collider.name.Contains(f))
                    {
                        var dis = Vector3.Distance(hit.point, ray.origin);
                        if (dis < closestDist)
                        {
                            closestDist = dis;
                            targetPos = hit.point;
                        }
                        isTarget = true;
                        continue;
                    }
                }
            }

            if (isTarget)
            {
                item.Vertex = targetPos + Vector3.up * SnapHeightOffset;
                return;
            }

        }
        
    }
}