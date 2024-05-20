using NUnit.Framework;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using System;
using UnityEditor;
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
        public RoadNetworkEditingSystem(VisualElement rootVisualElement)
        {
            system = new EditingSystem(this);
            TryInitialize(rootVisualElement);
        }


        public RoadNetworkUIDoc Editor => editor;
        public IRoadNetworkEditOperation NetworkOperator => editOperation;
        public RoadNetworkSceneGUISystem SceneGUISystem => sceneGUISystem;

        private UnityEngine.Object roadNetworkObject;
        private RoadNetworkModel roadNetworkModel;
        private RoadNetworkEditMode editingMode;

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
                var root = rootVisualElement;
                visualTree.CloneTree(root);


                editor = new RoadNetworkUIDoc(system, root, assets);
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
            UnityEngine.Object RoadNetworkObject { get; set; }
            RoadNetworkModel RoadNetwork { get; }
            RoadNetworkEditMode CurrentEditMode { get; set; }
            event EventHandler OnChangeEditMode;
            IRoadNetworkEditOperation EditOperation { get; }
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
                    var roadNetworkObj = value as IRoadNetworkObject;
                    Assert.IsNotNull(roadNetworkObj);
                    if (roadNetworkObj == null)
                        return;
                    var roadNetwork = roadNetworkObj.RoadNetwork;
                    if (roadNetwork == null)
                        return;

                    system.roadNetworkObject = value;
                    system.roadNetworkModel = roadNetwork;
                }

            }

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
                    OnChangeEditMode.Invoke(this, EventArgs.Empty);
                }
            }
            public event EventHandler OnChangeEditMode;

            public IRoadNetworkEditOperation EditOperation => system.editOperation;
        }

        /// <summary>
        /// RoadNetwork編集機能のデータ操作部分の機能を提供するクラス
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
                link.MainLanes.Insert(idx, newLane);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }
            public RoadNetworkEditingResult RemoveLane(RoadNetworkLink link, RoadNetworkLane lane)
            {
                // MainLane以外も削除出来るようにする

                bool isSuc = link.MainLanes.Remove(lane);
                if (isSuc)
                {
                    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
                }
                else
                {
                    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.InvalidArgs, "Can't found lane.");
                }
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
    }


}
