using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;
using static PLATEAU.CityInfo.CityObjectList.Attributes;

namespace PLATEAU.Editor.RoadNetwork
{
    /// <summary>
    /// 内部システムが利用するインターフェイス
    /// </summary>
    public interface IRoadNetworkEditingSystemInterface
    {
        //public RoadNetworkUIDoc UIDocEditor { get; }
        public IRoadNetworkEditOperation NetworkOperator { get; }
        public RoadNetworkSceneGUISystem SceneGUISystem { get; }
    }

    public interface IRoadNetworkEditOperation
    {
        /// <summary>
        /// ポイントの追加、削除、移動
        /// </summary>
        /// <param name="way"></param>
        /// <param name="idx"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        RoadNetworkEditingResult AddPoint(RoadNetworkWay parent, int idx, RoadNetworkPoint point);
        RoadNetworkEditingResult RemovePoint(RoadNetworkWay parent, RoadNetworkPoint point);
        RoadNetworkEditingResult MovePoint(RoadNetworkPoint point, in Vector3 newPos);

        /// <summary>
        /// 幅員のスケーリング
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        RoadNetworkEditingResult ScaleRoadWidth(RoadNetworkLane lane, float factor);
        RoadNetworkEditingResult ScaleRoadWidth(RoadNetworkLink link, float factor);

        /// <summary>
        /// 車線を増やす、減らす
        /// </summary>
        /// <param name="link"></param>
        /// <param name="idx"></param>
        /// <param name="newLane"></param>
        /// <returns></returns>
        RoadNetworkEditingResult AddMainLane(RoadNetworkLink parent, RoadNetworkLane newLane);
        RoadNetworkEditingResult RemoveMainLane(RoadNetworkLink parent, RoadNetworkLane lane);

        /// <summary>
        /// リンクを追加する削除する
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="idx"></param>
        /// <param name="newLink"></param>
        /// <returns></returns>
        RoadNetworkEditingResult AddLink(RoadNetworkModel parent, RoadNetworkLink newLink);
        RoadNetworkEditingResult RemoveLink(RoadNetworkModel parent, RoadNetworkLink link);

        // ノードを追加する削除する
        RoadNetworkEditingResult AddNode(RoadNetworkModel parent, int idx, RoadNetworkNode newNode);
        RoadNetworkEditingResult RemoveNode(RoadNetworkModel parent, RoadNetworkNode node);

        //RoadNetworkEditingResult AddBlock(RoadNetworkLane parentLane, int idx, RoadNetworkBlock newBlock);
        //RoadNetworkEditingResult RemoveBlock(RoadNetworkLane parentLane, RoadNetworkBlock block);

        //RoadNetworkEditingResult AddTrack(RoadNetworkNode parentNode, int idx, RoadNetworkTrack newTrack);
        //RoadNetworkEditingResult RemoveTrack(RoadNetworkNode parentNode, RoadNetworkTrack track);

        /// <summary>
        /// 交通規制情報の登録
        /// </summary>
        /// <param name="link"></param>
        /// <param name="newRegulation"></param>
        /// <returns></returns>
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkLink link, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkLane lane, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkBlock block, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkTrack track, RoadNetworkRegulationElemet newRegulation);
    }

    /// <summary>
    /// 道路ネットワークの編集モード
    /// </summary>
    public enum RoadNetworkEditMode
    {
        EditLaneShape,
        EditLaneStructure,
        EditTrafficRegulation,
        AddLane,    // debugOperationの機能を個々に移してもいいかも
        AddLink,    // debugOperationの機能を個々に移してもいいかも
        AddNode,    // debugOperationの機能を個々に移してもいいかも
        EditLaneWidth,    // debugOperationの機能を個々に移してもいいかも
        AddMedianStrip,    // debugOperationの機能を個々に移してもいいかも
    }

    /// <summary>
    /// 道路ネットワーク編集システムのインスタンス
    /// 管理元
    /// 出来るだけサブシステム同士で連携を取らないようにする
    /// </summary>
    public class RoadNetworkEditingSystem : IRoadNetworkEditingSystemInterface
    {
        /// <summary>
        /// システムのインスタンスを管理する機能を提供するインターフェイス
        /// </summary>
        public interface ISystemInstance
        {
            void RequestReinitialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorInstance"></param>
        /// <param name="rootVisualElement"></param>
        public RoadNetworkEditingSystem(ISystemInstance editorInstance, VisualElement rootVisualElement)
        {
            UnityEngine.Assertions.Assert.IsNotNull(editorInstance);
            this.systemInstance = new EditorInstance(this, editorInstance);

            Assert.IsNotNull(rootVisualElement);
            this.rootVisualElement = rootVisualElement;
            system = new EditingSystem(this);
            TryInitialize(rootVisualElement);
        }

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

        // 道路ネットワークを所持したオブジェクトのデフォルト名
        private readonly string defaultRoadNetworkObjectName = "RoadNetworkTester";

        private readonly ISystemInstance systemInstance;
        private readonly VisualElement rootVisualElement;

        // 選択している道路ネットワークを所持したオブジェクト
        private UnityEngine.Object roadNetworkObject;
        // 選択している道路ネットワーク
        private RoadNetworkModel roadNetworkModel;
        // 現在の編集モード
        private RoadNetworkEditMode editingMode;

        // 選択中の道路ネットワーク要素 Link,Lane,Block...etc
        private System.Object selectedRoadNetworkElement;

        // 選択中の信号制御器のパターン
        private TrafficSignalControllerPattern selectedSignalPattern;
        // 選択中の信号制御器のパターンのフェーズ
        private TrafficSignalControllerPhase selectedSignalPhase;

        // 内部システム同士が連携する時や共通データにアクセスする際に利用する
        private readonly IRoadNetworkEditingSystem system;

        private IRoadNetworkEditOperation editOperation;
        private RoadNetworkUIDoc uiDocEditor;
        private RoadNetworkSceneGUISystem sceneGUISystem;
        
        // Laneの生成機能を提供するモジュール
        private RoadNetworkSimpleLaneGenerateModule simpleLaneGenerateModule;
        private RoadNetworkSimpleLinkGenerateModule simpleLinkGenerateModule;
        private RoadNetworkSimpleNodeGenerateModule simpleNodeGenerateModule;
        private RoadNetworkMedianStripGenerateModule medianStripGenerateModule;

        // 道路ネットワーク関係のアセットを管理するクラス
        private RoadNetworkEditorAssets assets;

        private string debugOperationMode = "undef";

        // refresh時にClaer()必要
        public class LaneEditCache
        {
            public float Scale { get; set; }
            public RoadNetworkLane BaseLane { get; set; }
        }
        private Dictionary<RoadNetworkLane, LaneEditCache> keyValuePairs = new Dictionary<RoadNetworkLane, LaneEditCache>();

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
            bool needInitEditor = uiDocEditor == null;
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

                uiDocEditor = new RoadNetworkUIDoc(system, rootVisualElement, assets);
                uiDocEditor.Initialize();

            }

            if (needInitGUISystem)
            {
                sceneGUISystem = new RoadNetworkSceneGUISystem(system);
            }

            // 他のシステム連携が必要な初期化 PostInitliaze()
            if (needInitEditor)
            {
                uiDocEditor.PostInitialize();
            }

            // その他 初期化

            // 道路ネットワークの取得を試みる　自動設定機能
            var roadNetworkObj = GameObject.Find(defaultRoadNetworkObjectName);
            if (roadNetworkObj != null)
            {
                this.roadNetworkObject = roadNetworkObj;
                Selection.activeGameObject = roadNetworkObj;


                // 仮ポイントを　地形にスワップする
                var r = roadNetworkObj.GetComponent<PLATEAURoadNetworkTester>();
                var roadNetwork = r.RoadNetwork;

                var lineE = roadNetwork.CollectAllLineStrings().GetEnumerator();
                while (lineE.MoveNext())
                {
                    var line = lineE.Current;
                    Ray ray;
                    foreach (var item in line.Points)
                    {
                        const float rayDis = 1000.0f;
                        ray = new Ray(item.Vertex + Vector3.up * rayDis, Vector3.down * rayDis);
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            if (hit.collider.name.Contains("dem_"))
                            item.Vertex = hit.point;
                        }
                    }
                }
            }

            simpleLaneGenerateModule = new RoadNetworkSimpleLaneGenerateModule();
            simpleLinkGenerateModule = new RoadNetworkSimpleLinkGenerateModule();
            simpleNodeGenerateModule = new RoadNetworkSimpleNodeGenerateModule();
            return true;
        }

        /// <summary>
        /// 内部システムが利用するインターフェイス
        /// 内部システム同士が連携する時や共通データにアクセスする際に利用する
        /// </summary>
        public interface IRoadNetworkEditingSystem
        {
            /// <summary>
            /// 編集機能のインスタンス
            /// </summary>
            ISystemInstance EditorInstance { get; }

            /// <summary>
            /// 道路ネットワークを所持したUnityオブジェクト
            /// </summary>
            UnityEngine.Object RoadNetworkObject { get; set; }
            event EventHandler OnChangedRoadNetworkObject;

            /// <summary>
            /// 道路ネットワーク
            /// </summary>
            RoadNetworkModel RoadNetwork { get; }

            /// <summary>
            /// 現在の編集モード
            /// </summary>
            RoadNetworkEditMode CurrentEditMode { get; set; }
            event EventHandler OnChangedEditMode;
            /// <summary>
            /// 編集機能を提供するインターフェイス
            /// </summary>
            IRoadNetworkEditOperation EditOperation { get; }

            /// <summary>
            /// 選択中の道路ネットワーク要素
            /// </summary>
            System.Object SelectedRoadNetworkElement { get; set; }
            event EventHandler OnChangedSelectRoadNetworkElement;

            /// <summary>
            /// 選択中の信号制御器のパターン
            /// </summary>
            TrafficSignalControllerPattern SelectedSignalControllerPattern { get; set; }
            event EventHandler OnChangedSignalControllerPattern;
            /// <summary>
            /// 選択中の信号制御器のパターンのフェーズ
            /// </summary>
            TrafficSignalControllerPhase SelectedSignalPhase { get; set; }
            event EventHandler OnChangedSignalControllerPhase;

            /// <summary>
            /// 道路ネットワークを所持したオブジェクトに変更があったことをUnityEditorに伝える
            /// </summary>
            void NotifyChangedRoadNetworkObject2Editor();

            string OperationMode { get; set; }
            event EventHandler OnChangedOperationMode;

            RoadNetworkSimpleLaneGenerateModule RoadNetworkSimpleLaneGenerateModule { get; }
            RoadNetworkSimpleLinkGenerateModule RoadNetworkSimpleLinkGenerateModule { get; }
            RoadNetworkSimpleNodeGenerateModule RoadNetworkSimpleNodeGenerateModule { get; }

            RoadNetworkLane GetBase(RoadNetworkLane keyLane);
            float GetScale(RoadNetworkLane keyLane);
            void RegisterBase(RoadNetworkLane keyLane, RoadNetworkLane baseLane, float scale, RoadNetworkLane oldKeyLane);
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
            public event EventHandler OnChangedSignalControllerPattern;
            public event EventHandler OnChangedSignalControllerPhase;
            public event EventHandler OnChangedOperationMode;

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

            public ISystemInstance EditorInstance => system.systemInstance;

            public TrafficSignalControllerPattern SelectedSignalControllerPattern 
            { 
                get => system.selectedSignalPattern; 
                set
                {
                    if (system.selectedSignalPattern == value)
                        return;
                    system.selectedSignalPattern = value;
                    OnChangedSignalControllerPattern?.Invoke(this, EventArgs.Empty);

                    system.selectedSignalPhase = null;
                } 
            }

            public TrafficSignalControllerPhase SelectedSignalPhase 
            { 
                get => system.selectedSignalPhase; 
                set 
                {
                    if (system.selectedSignalPhase == value)
                        return;
                    system.selectedSignalPhase = value;
                    OnChangedSignalControllerPhase?.Invoke(this, EventArgs.Empty);
                }
            }

            public string OperationMode
            {
                get => system.debugOperationMode;
                set
                {
                    if (system.debugOperationMode == value)
                        return;
                    system.debugOperationMode = value;
                    OnChangedOperationMode?.Invoke(this, EventArgs.Empty);
                }
            }

            public RoadNetworkSimpleLaneGenerateModule RoadNetworkSimpleLaneGenerateModule => system.simpleLaneGenerateModule;

            public RoadNetworkSimpleLinkGenerateModule RoadNetworkSimpleLinkGenerateModule => system.simpleLinkGenerateModule;

            public RoadNetworkSimpleNodeGenerateModule RoadNetworkSimpleNodeGenerateModule => system.simpleNodeGenerateModule;

            public void NotifyChangedRoadNetworkObject2Editor()
            {
                if (system.roadNetworkObject == null)
                    return;
                EditorUtility.SetDirty(system.roadNetworkObject);

            }

            public RoadNetworkLane GetBase(RoadNetworkLane keyLane)
            {
                var isSuc = system.keyValuePairs.TryGetValue(keyLane, out var editCache);
                if (isSuc)
                    return editCache.BaseLane;
                return null;
            }
            public float GetScale(RoadNetworkLane keyLane)
            {
                var isSuc = system.keyValuePairs.TryGetValue(keyLane, out var editCache);
                if (isSuc)
                    return editCache.Scale;
                return 1.0f;
            }
            public void RegisterBase(RoadNetworkLane keyLane, RoadNetworkLane baseLane, float scale, RoadNetworkLane oldKeyLane)
            {
                var cache = new LaneEditCache() { BaseLane = baseLane, Scale = scale };
                bool isAdded = system.keyValuePairs.TryAdd(keyLane, cache);
                if (isAdded)
                {
                    system.keyValuePairs[keyLane] = cache;
                }
                if (oldKeyLane != null)
                {
                    system.keyValuePairs.Remove(oldKeyLane);
                }
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
                //var v = new RoadNetworkPoint(new Vector3());
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

            public RoadNetworkEditingResult AddMainLane(RoadNetworkLink link, RoadNetworkLane newLane)
            {
                //var v = new RoadNetworkLane(leftWay:, rightWay:, startBorder:, endBorder:);
                link.AddMainLane(newLane);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult AddLink(RoadNetworkModel parent, RoadNetworkLink newLink)
            {
                //var v = new RoadNetworkLink(targetTran:);
                parent.AddLink(newLink);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RemoveLink(RoadNetworkModel parent, RoadNetworkLink link)
            {
                parent.RemoveLink(link);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult AddNode(RoadNetworkModel parent, int idx, RoadNetworkNode newNode)
            {
                //var v = new RoadNetworkNode(targetTran:);
                parent.AddNode(newNode);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RemoveMainLane(RoadNetworkLink link, RoadNetworkLane lane)
            {
                link.RemoveMainLane(lane);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkLink link, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkLane lane, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkBlock block, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RoadNetworkTrack track, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RemoveNode(RoadNetworkModel parent, RoadNetworkNode node)
            {
                parent.RemoveNode(node);
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 編集機能のインスタンスを管理する機能を行っているクラス
        /// </summary>
        public class EditorInstance : ISystemInstance
        {            
            public EditorInstance(RoadNetworkEditingSystem system, ISystemInstance editorInstance)
            {
                this.system = system;
                this.editorInstance = editorInstance;
            }

            private RoadNetworkEditingSystem system;
            private ISystemInstance editorInstance;

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

        /// <summary>
        /// 単純なレーン生成機能を提供するクラス
        /// </summary>
        public class RoadNetworkSimpleLaneGenerateModule
        {
            public RoadNetworkSimpleLaneGenerateModule()
            {
                Init();
            }

            private RoadNetworkWay baseStartWay;
            private RoadNetworkWay baseEndWay;

            private List<RoadNetworkPoint> startPoints = new List<RoadNetworkPoint>(2);
            private List<RoadNetworkPoint> endPoints = new List<RoadNetworkPoint>(2);

            private RoadNetworkLink link;


            public void Init()
            {
                baseStartWay = null;
                baseEndWay = null;
                startPoints.Clear();
                endPoints.Clear();
                link = null;
            }

            public void Reset()
            {
                Debug.Log("設定をリセット");
                Init();
            }

            ///// <summary>
            ///// ポイントを追加する
            ///// ただし前に追加したポイントが以下の条件時は失敗する
            ///// 　追加済みポイント１，２と追加するポイントを結ぶ直線が直角以下であるとき　（綺麗にレーンを構成するwayが重なってしまう可能性があるため）
            ///// （追加済みのポイントが存在しない時はborderが利用される）
            ///// </summary>
            ///// <param name="point"></param>
            ///// <returns></returns>
            //public bool AddPoint(Vector3 point)
            //{
            //    if (laneCenterPoints.Count >= 2)
            //    {

            //        //var p1 = laneCenterPoints[laneCenterPoints.Count - 2];
            //        //var p2 = laneCenterPoints[laneCenterPoints.Count - 1];
            //        //var p3 = point;
            //        //// p1,p2,p3を結ぶ直線が直角以下であるとき
            //        //if (Vector3.Dot(p2 - p1, p3 - p2) < 0)
            //        //{
            //        //    return false;
            //        //}
            //    }

            //    laneCenterPoints.Add(point);
            //    return true;
            //}

            /// <summary>
            /// 時計回りに選択して追加する
            /// 追加出来ないpointは予め非表示にしたい
            /// </summary>
            /// <param name="way"></param>
            /// <returns></returns>
            public bool AddBorder(RoadNetworkLink link, RoadNetworkWay way, RoadNetworkPoint point)
            {
                if (startPoints.Count == 2 && endPoints.Count == 2)
                {
                    Debug.Log("start, endのポイントがすでに設定されている");
                    Reset();
                    return false;
                }

                if (way.Points.Contains(point) == false)
                {
                    Debug.Log("wayに含まれないPointを指定した");
                    Reset();
                    return false;
                }

                if (startPoints.Contains(point) || endPoints.Contains(point))
                {
                    Debug.Log("すでに設定されているPointを指定した");
                    Reset();
                    return false;
                }

                if (this.link != null)
                {
                    if (this.link != link)
                    {
                        Debug.Log("すでに設定されているPointが属さないLinkのPointを選択した");
                        Reset();
                        return false;
                    }
                }


                if (this.link == null)
                {
                    this.link = link;
                }

                // start or end
                bool isStart = true;
                if (startPoints.Count == 2)
                {
                    isStart = false;
                }

                RoadNetworkWay baseWay;
                List<RoadNetworkPoint> points;
                if (isStart)
                {
                    baseWay = baseStartWay;
                    points = startPoints;
                }
                else
                {
                    baseWay = baseEndWay;
                    points = endPoints;
                }

                // baseWayの設定
                var isSuc = SelectBaseWay(way, point, ref baseWay);
                if (isSuc == false)
                {
                    Reset();
                    return false;
                }

                // baseWayの更新
                if (isStart)
                {
                    baseStartWay = baseWay;
                }
                else
                {
                    baseEndWay = baseWay;
                }

                // pointを追加する
                points.Add(point);

                var debugMsg = isStart ? "start" : "end";
                Debug.Log($"{debugMsg} {points.Count}つめのpointを追加しました");

                return true;
            }

            private bool SelectBaseWay(RoadNetworkWay way, RoadNetworkPoint point, ref RoadNetworkWay baseWay)
            {
                // nullなので設定
                if (baseWay == null)
                {
                    baseWay = way;
                    return true;
                }

                // そのまま
                if (baseWay.Points.Contains(point))
                {
                    return true;
                }

                // baseWayに含まれていない場合はbaseWayを新規のwayに更新する
                // ただし新規のwayに追加済みのポイントが含まれていない場合は失敗
                Assert.IsTrue(startPoints.Count > 0);
                if (way.Points.Contains(startPoints[0]) == false)
                {
                    Debug.Log("wayに含まれないPointを指定した");
                    return false;
                }

                baseWay = way;
                return true;
            }

            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="way"></param>
            ///// <returns></returns>
            //public bool SetEndWay(RoadNetworkWay way)
            //{
            //    if (way == null)
            //        return false;
            //    endWay = way;
            //    return true;
            //}

            public bool CanBuild()
            {
                if (startPoints.Count != 2 || endPoints.Count != 2)
                {
                    return false;
                }
                if (link == null)
                {
                    return false;
                }
                return true;
            }

            public RoadNetworkLane Build()
            {
                if (startPoints.Count != 2 || endPoints.Count != 2)
                {
                    Debug.Log("pointの設定が足りない");
                    Reset();
                    return null;
                }

                if (link == null)
                {
                    Debug.Log("linkの設定が足りない");
                    Reset();
                    return null;
                }

                var start = CreateBorderWay(baseStartWay, startPoints);
                var end = CreateBorderWay(baseEndWay, endPoints);

                var leftLine = new RoadNetworkLineString();
                var rightLine = new RoadNetworkLineString();

                // 時計回りで startPoint,endPointが設定されていることが前提
                // TODO　交差していたら参照順を変える処理を追加する
                var leftWay = new RoadNetworkWay(RoadNetworkLineString.Create( new RoadNetworkPoint[]{ startPoints[0], endPoints[1] }));
                var rightWay = new RoadNetworkWay(RoadNetworkLineString.Create(new RoadNetworkPoint[] { startPoints[1], endPoints[0] }));
                
                var lane = new RoadNetworkLane(leftWay, rightWay, start, end);
                link.AddMainLane(lane);
                return lane;
            }

            private static RoadNetworkWay CreateBorderWay(RoadNetworkWay baseWay, List<RoadNetworkPoint> points)
            {
                var idx0 = baseWay.FindPoint(points[0]);
                var idx1 = baseWay.FindPoint(points[1]);
                return CreateBorderWay(baseWay, ref idx0, ref idx1);
            }

            private static RoadNetworkWay CreateBorderWay(RoadNetworkWay baseWay, ref int idx0, ref int idx1)
            {
                bool wasReversed = false;
                if (idx0 > idx1)
                {
                    var tmp = idx0;
                    idx0 = idx1;
                    idx1 = tmp;
                    wasReversed = true;
                }

                List<RoadNetworkPoint> wayPoints = null;

                var n = idx1 - idx0 + 1;
                wayPoints = new List<RoadNetworkPoint>(n);
                for (int i = idx0; i <= idx1; i++)
                {
                    wayPoints.Add(baseWay.LineString.Points[i]);
                }
                if (wasReversed)
                {
                    wayPoints.Reverse();
                }
                var way = new RoadNetworkWay(RoadNetworkLineString.Create(wayPoints));
                return way;
            }
        }

        public class RoadNetworkMedianStripGenerateModule
        {
            public RoadNetworkMedianStripGenerateModule()
            {
            }

            // 隣り合ったレーン　LineStringsを共有していることが条件
            private List<RoadNetworkLane> neighborLanes = new List<RoadNetworkLane>(2);    

            public void Init()
            {
                neighborLanes.Clear();
            }

            public void Reset()
            {
                Debug.Log("再設定");
                Init();
            }

            public bool CanBuild()
            {
                return neighborLanes.Count == 2;
            }

            public RoadNetworkNode Build()
            {
                var ways = new List<RoadNetworkWay>(4)
                {
                    neighborLanes[0].LeftWay,
                    neighborLanes[0].RightWay,
                    neighborLanes[1].LeftWay,
                    neighborLanes[1].RightWay
                };

                // 共有するLineStringsを探す
                RoadNetworkLineString unionLineStrins = null;
                int i = 0;
                for (i = 0; i < ways.Count; i++)
                {
                    for (int j = 0; j < ways.Count; j++)
                    {
                        // 自信は除く
                        // (RoadNetworkWayのインスタンス比較だと実装によっては共有しているかもしれないのでindexで判断)
                        if (i == j)
                            continue;
                        if (ways[i].LineString == ways[j].LineString)
                        {
                            unionLineStrins = ways[i].LineString;
                            break;
                        }
                    }
                }

                if (unionLineStrins == null)
                {
                    Debug.Log("共有しているLineStringsが見つからない");
                    Reset();
                    return null;
                }

                // LineStringsを複製する
                //var
                var newLineStrings = RoadNetworkLineString.Create(unionLineStrins);
                var lane = neighborLanes[i/2];
                RoadNetworkLane newLane = null;
                if (i % 2 == 0)
                {
                    newLane = new RoadNetworkLane(new RoadNetworkWay(newLineStrings), lane.RightWay, lane.PrevBorder, lane.NextBorder);
                    //lane.LeftWay = new RoadNetworkWay(newLineStrings);
                }
                else
                {
                    newLane = new RoadNetworkLane(lane.LeftWay, new RoadNetworkWay(newLineStrings), lane.PrevBorder, lane.NextBorder);
                    //lane.RightWay = new RoadNetworkWay(newLineStrings);
                }
                var parentLink = lane.ParentLink;
                parentLink.ReplaceLane(lane, newLane);
                // 中央分離帯の形状にする 始点と終点だけ共有 （中央分離帯の途中で道が空いている場合は交差点を配置して中央分離帯を2回作成する）


                // 共有するwayがleftWayにあるlane0
                //var newLineStrings = RoadNetworkLineString.Create(newPoints);
                return null;
            }
        }

        public class RoadNetworkSimpleNodeGenerateModule
        {
            public RoadNetworkSimpleNodeGenerateModule()
            {

            }

            private PLATEAU.CityInfo.PLATEAUCityObjectGroup tranObj;

            public void Init()
            {

            }

            public void Reset()
            {

            }

            public bool CanBuild()
            {
                return false;
            }

            public RoadNetworkNode Build()
            {
                var node = new RoadNetworkNode(tranObj);
                return null;
            }
        }

        public class RoadNetworkSimpleLinkGenerateModule
        {
            public RoadNetworkSimpleLinkGenerateModule() 
            { 
            }

            private PLATEAU.CityInfo.PLATEAUCityObjectGroup tranObj;
            private RoadNetworkModel parent;
            private List<RoadNetworkPoint> points = new List<RoadNetworkPoint>();

            public void Init()
            {
                tranObj = null;
                parent = null;
                points.Clear();
            }

            public void Reset()
            {
                Debug.Log("再設定が必要");
                Init();
            }

            public bool AddPoint(RoadNetworkModel parent, RoadNetworkPoint point)
            {
                if (this.parent == null)
                {
                    this.parent = parent;
                }
                else
                {
                    if (this.parent != parent)
                    {
                        Debug.Log("親が異なる");
                        Reset();
                        return false;
                    }
                }

                if (points.Contains(point))
                {
                    Debug.Log("追加済みのポイントを選択した");
                    return false;
                }
                points.Add(point);
                Debug.Log("pointを追加した");
                return true;
            }

            public void SetTranObj(PLATEAU.CityInfo.PLATEAUCityObjectGroup tranObj)
            {
                this.tranObj = tranObj;
            }

            public bool CanBuild()
            {
                if (parent == null)
                {
                    return false;
                }

                if (points.Count < 4)
                {
                    return false;
                }
                return true;
            }

            public RoadNetworkLink Build()
            {
                if (parent == null)
                {
                    Debug.Log("親が設定されていない");
                    return null;
                }

                if (points.Count < 4)
                {
                    Debug.Log("Linkを作成するには4点以上必要");// startBorderから選択して右回り
                    return null;
                }

                var startBorder = new RoadNetworkWay(RoadNetworkLineString.Create(points.GetRange(0, 2)));
                var leftWay = new RoadNetworkWay(RoadNetworkLineString.Create(points.GetRange(1, 2)));
                var endBorder = new RoadNetworkWay(RoadNetworkLineString.Create(points.GetRange(2, 2)));
                var rightWay = new RoadNetworkWay(RoadNetworkLineString.Create(new RoadNetworkPoint[] { points[3], points[0] }));
                var lane = new RoadNetworkLane(leftWay, rightWay, startBorder, endBorder);
                var link = RoadNetworkLink.CreateOneLaneLink(tranObj, lane);
                parent.AddLink(link);
                return link;
            }

        }

        public class RoadNetworkLaneWidthEditModule
        {
            public RoadNetworkLaneWidthEditModule()
            {
            }

            private RoadNetworkLane lane;
            private float currentScale;
            


            public void Init()
            {
                //handleBasePos = Vector3.zero;
                lane = null;
            }

            public void Reset()
            {
                Debug.Log("再設定が必要");
                Init();
            }

            public bool SetLane(RoadNetworkLane lane, float scale)
            {
                this.lane = lane;
                return true;
            }

            public bool CanEdit()
            {
                if (lane == null)
                    return false;
                return true;
            }

            public RoadNetworkLink Edit()
            {
                if (this.lane == null)
                {
                    Debug.Log("親が設定されていない");
                    return null;
                }

                //foreach (var way in lane.BothWays)
                //{
                //    int i = 0;
                //    foreach (var point in way.Points)
                //    {
                //        var vertNorm = way.GetVertexNormal(i++);
                //        point.Vertex = point + (scale - 1) * 0.1f * vertNorm;
                //        state.isDirtyTarget = true;
                //    }
                //}


                return null;
            }

        }

    }

}
