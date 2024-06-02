using NUnit.Framework;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    // https://docs.unity.cn/ja/2022.3/ScriptReference/EditorWindow.html

    /// <summary>
    /// 内部システムが利用するインターフェイス
    /// </summary>
    public interface IRoadNetworkEditingSystemInterface
    {
        public RoadNetworkUIDoc Editor { get; }
        public IRoadNetworkEditOperation NetworkOperator { get; }
        public RoadNetworkSceneGUISystem SceneGUISystem { get; }
    }

    /// <summary>
    /// 道路ネットワーク編集システムのインスタンス
    /// 管理元
    /// 出来るだけサブシステム同士で連携を取らないようにする
    /// </summary>
    public class RoadNetworkEditingSystem : IRoadNetworkEditingSystemInterface
    {
        public interface IEditorInstance
        {
            void RequestReinitialize();
        }

        public RoadNetworkEditingSystem(IEditorInstance editorInstance, VisualElement rootVisualElement)
        {
            Assert.IsNotNull(editorInstance);
            this.editorInstance = new EditorInstance(this, editorInstance);

            Assert.IsNotNull(rootVisualElement);
            this.rootVisualElement = rootVisualElement;
            system = new EditingSystem(this);
            TryInitialize(rootVisualElement);
        }


        public RoadNetworkUIDoc Editor => editor;
        public IRoadNetworkEditOperation NetworkOperator => editOperation;
        public RoadNetworkSceneGUISystem SceneGUISystem => sceneGUISystem;

        private readonly IEditorInstance editorInstance;
        private readonly VisualElement rootVisualElement;
        private readonly string defaultRoadNetworkObjectName = "RoadNetworkTester";

        // 選択している道路ネットワークを所持したオブジェクト
        private UnityEngine.Object roadNetworkObject;
        // 選択している道路ネットワーク
        private RoadNetworkModel roadNetworkModel;
        // 現在の編集モード
        private RoadNetworkEditMode editingMode;

        // 選択中の道路ネットワーク要素 Link,Lane,Block...etc
        private System.Object selectedRoadNetworkElement;

        private bool TryInitialize(VisualElement rootVisualElement)
        {
            // 初期化の必要性チェック
            bool needIniteditOperation = editOperation == null;
            bool needInitEditor = editor == null;
            bool needInitGUISystem = sceneGUISystem == null;


            // 初期化 Initlaize()
            if (needIniteditOperation)
            {
                editOperation = new RoadNetworkEditorOperation();
            }

            if (needInitEditor)
            {
                assets = new RoadNetworkEditorAssets();

                var visualTree = assets.GetAsset(RoadNetworkEditorAssets.EditorAssetName);
                var inst = visualTree.Instantiate();
                rootVisualElement.Add(inst);
                //visualTree.CloneTree(rootVisualElement);

                editor = new RoadNetworkUIDoc(system, rootVisualElement, assets);
                editor.Initialize();

            }

            if (needInitGUISystem)
            {
                sceneGUISystem = new RoadNetworkSceneGUISystem(system);
            }

            // 他のシステム連携が必要な初期化 PostInitliaze()
            if (needInitEditor)
            {
                editor.PostInitialize();
            }

            // その他 初期化

            // 道路ネットワークの取得を試みる　自動設定機能
            var roadNetworkObj = GameObject.Find(defaultRoadNetworkObjectName);
            if (roadNetworkObj != null)
            {
                this.roadNetworkObject = roadNetworkObj;
                Selection.activeGameObject = roadNetworkObj;
            }
            return true;
        }

        private readonly IRoadNetworkEditingSystem system;

        private IRoadNetworkEditOperation editOperation;
        private RoadNetworkUIDoc editor;
        private RoadNetworkEditorAssets assets;
        private RoadNetworkSceneGUISystem sceneGUISystem;

        /// <summary>
        /// 内部システムが利用するインターフェイス
        /// 内部システム同士が連携する時や共通データにアクセスする際に利用する
        /// </summary>
        public interface IRoadNetworkEditingSystem
        {
            IEditorInstance EditorInstance { get; }

            UnityEngine.Object RoadNetworkObject { get; set; }
            event EventHandler OnChangedRoadNetworkObject;

            RoadNetworkModel RoadNetwork { get; }
            RoadNetworkEditMode CurrentEditMode { get; set; }
            event EventHandler OnChangedEditMode;
            IRoadNetworkEditOperation EditOperation { get; }

            System.Object SelectedRoadNetworkElement { get; set; }
            event EventHandler OnChangedSelectRoadNetworkElement;
            void NotifyChangedRoadNetworkObject2Editor();

        }

        /// <summary>
        /// 内部システムが利用するインスタンス
        /// 内部システム同士が連携する時や共通データにアクセスする際に利用する
        /// </summary>
        private class EditingSystem : IRoadNetworkEditingSystem
        {
            public EditingSystem(RoadNetworkEditingSystem system)
            {
                Assert.IsNotNull(system);
                this.system = system;
            }


            private readonly RoadNetworkEditingSystem system;


            public UnityEngine.Object RoadNetworkObject 
            { 
                get => system.roadNetworkObject;
                set 
                {
                    if (system.roadNetworkObject == value)
                        return;

                    var roadNetworkObj = value as IRoadNetworkObject;
                    Assert.IsNotNull(roadNetworkObj);
                    if (roadNetworkObj == null)
                        return;
                    var roadNetwork = roadNetworkObj.RoadNetwork;
                    if (roadNetwork == null)
                        return;

                    system.roadNetworkObject = value;
                    system.roadNetworkModel = roadNetwork;
                    OnChangedRoadNetworkObject?.Invoke(this, EventArgs.Empty);
                }

            }
            public event EventHandler OnChangedRoadNetworkObject;

            public RoadNetworkModel RoadNetwork 
            { 
                get => system.roadNetworkModel; 
            }

            public RoadNetworkEditMode CurrentEditMode 
            { 
                get => system.editingMode;
                set 
                {
                    if (system.editingMode == value)
                        return;
                    system.editingMode = value;
                    OnChangedEditMode?.Invoke(this, EventArgs.Empty);
                }
            }
            public event EventHandler OnChangedEditMode;
            public event EventHandler OnChangedSelectRoadNetworkElement;

            public IRoadNetworkEditOperation EditOperation => system.editOperation;

            public object SelectedRoadNetworkElement 
            { 
                get => system.selectedRoadNetworkElement; 
                set 
                {
                    if (system.selectedRoadNetworkElement == value)
                        return;
                    system.selectedRoadNetworkElement = value;
                    OnChangedSelectRoadNetworkElement?.Invoke(this, EventArgs.Empty);
                } 
            }

            public IEditorInstance EditorInstance => system.editorInstance;

            public void NotifyChangedRoadNetworkObject2Editor()
            {
                if (system.roadNetworkObject == null)
                    return;
                EditorUtility.SetDirty(system.roadNetworkObject);

            }
        }

        /// <summary>
        /// RoadNetwork編集機能のデータ操作部分の機能を提供するクラス
        /// EditingSystemのサブクラスに移動するかも
        /// </summary>
        public class RoadNetworkEditorOperation : IRoadNetworkEditOperation
        {
            public RoadNetworkEditingResult AddPoint(RoadNetworkWay way, int idx, RoadNetworkPoint point)
            {
                way.LineString.Points.Insert(idx, point);
                
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RemovePoint(RoadNetworkWay way, RoadNetworkPoint point)
            {
                var isSuc = way.LineString.Points.Remove(point);
                if (isSuc)
                {
                    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
                }
                else
                {
                    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.InvalidArgs, "Can't found point.");
                }
            }

            public RoadNetworkEditingResult MovePoint(RoadNetworkPoint point, in Vector3 newPos)
            {
                if (point.Vertex == newPos)
                    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);

                point.Vertex = newPos;
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult ScaleRoadWidth(RoadNetworkLane lane, float factor)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult ScaleRoadWidth(RoadNetworkLink link, float factor)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult AddMainLane(RoadNetworkLink link, int idx, RoadNetworkLane newLane)
            {
                throw new NotImplementedException();
                //link.MainLanes.Insert(idx, newLane);
                //return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }
            public RoadNetworkEditingResult RemoveLane(RoadNetworkLink link, RoadNetworkLane lane)
            {
                throw new NotImplementedException();

                //// MainLane以外も削除出来るようにする

                //bool isSuc = link.MainLanes.Remove(lane);
                //if (isSuc)
                //{
                //    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
                //}
                //else
                //{
                //    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.InvalidArgs, "Can't found lane.");
                //}
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkLink link, _RoadNetworkRegulation newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkLane lane, _RoadNetworkRegulation newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkBlock block, _RoadNetworkRegulation newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkTrack track, _RoadNetworkRegulation newRegulation)
            {
                throw new NotImplementedException();
            }

        }

        public class EditorInstance : IEditorInstance
        {            
            public EditorInstance(RoadNetworkEditingSystem system, IEditorInstance editorInstance)
            {
                this.system = system;
                this.editorInstance = editorInstance;
            }

            private RoadNetworkEditingSystem system;
            private IEditorInstance editorInstance;

            public void RequestReinitialize()
            {
                //system.rootVisualElement.RemoveFromHierarchy();
                //editorInstance.RequestReinitialize();
                //return;
                var children = system.rootVisualElement.Children().ToArray();
                foreach (var item in children)
                {
                    item.RemoveFromHierarchy();
                }
                editorInstance.RequestReinitialize();

            }
        }
    }


}
