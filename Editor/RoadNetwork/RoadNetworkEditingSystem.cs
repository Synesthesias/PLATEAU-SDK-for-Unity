using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Plastic.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

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
        RoadNetworkEditingResult AddPoint(RnWay parent, int idx, RnPoint point);
        RoadNetworkEditingResult RemovePoint(RnWay parent, RnPoint point);
        RoadNetworkEditingResult MovePoint(RnPoint point, in Vector3 newPos);

        /// <summary>
        /// 幅員のスケーリング
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        RoadNetworkEditingResult ScaleRoadWidth(RnLane lane, float factor);
        RoadNetworkEditingResult ScaleRoadWidth(RnRoad link, float factor);

        /// <summary>
        /// 車線を増やす、減らす
        /// </summary>
        /// <param name="link"></param>
        /// <param name="idx"></param>
        /// <param name="newLane"></param>
        /// <returns></returns>
        RoadNetworkEditingResult AddMainLane(RnRoad parent, RnLane newLane);
        RoadNetworkEditingResult RemoveMainLane(RnRoad parent, RnLane lane);

        /// <summary>
        /// リンクを追加する削除する
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="idx"></param>
        /// <param name="newLink"></param>
        /// <returns></returns>
        RoadNetworkEditingResult AddLink(RnModel parent, RnRoad newLink);
        RoadNetworkEditingResult RemoveLink(RnModel parent, RnRoad link);

        // ノードを追加する削除する
        RoadNetworkEditingResult AddNode(RnModel parent, int idx, RnIntersection newNode);
        RoadNetworkEditingResult RemoveNode(RnModel parent, RnIntersection node);

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
        RoadNetworkEditingResult RegisterRegulation(RnRoad link, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RnLane lane, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RnBlock block, RoadNetworkRegulationElemet newRegulation);
    }

    /// <summary>
    /// 道路ネットワークの編集モード
    /// </summary>
    public enum RoadNetworkEditMode
    {
        _EditLaneShape,
        _EditLaneStructure,
        EditTrafficRegulation,// 交通規制編集
        _AddLane,    // debugOperationの機能を個々に移してもいいかも
        _AddLink,    // debugOperationの機能を個々に移してもいいかも
        _AddNode,    // debugOperationの機能を個々に移してもいいかも
        _EditLaneWidth,    // debugOperationの機能を個々に移してもいいかも
        EditRoadStructure,    // 道路構造編集
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
        private RnModel roadNetworkModel;
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

        private RoadNetworkSimpleEditSysModule simpleEditSysModule;

        // 道路ネットワーク関係のアセットを管理するクラス
        private RoadNetworkEditorAssets assets;

        private string debugOperationMode = "undef";

        private const string roadNetworkEditingSystemObjName = "_RoadNetworkEditingSystemRoot";
        private GameObject roadNetworkEditingSystemObjRoot;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="_ListElement"></typeparam>
        public class EditorDataList<_ListElement> : List<_ListElement>
        {
            /// <summary>
            /// 頻繁に呼ばないように注意する
            /// </summary>
            /// <typeparam name="_Type"></typeparam>
            /// <returns></returns>
            public bool TryGetCache<_ListElementVal>(string key, out IEnumerable<_ListElementVal> o)
            {
                if (cacheDataMap.TryGetValue(key, out var data) == false)
                {
                    o = default;
                    return false;
                }

                var castedData = data as CacheData<_ListElementVal>;
                var d = castedData.data as IEnumerable<_ListElementVal>;
                if (d == null)
                {
                    o = default;
                    return false;
                }

                o = d;
                return true;
            }

            /// <summary>
            /// 追加する
            /// 同型で用途が異なるデータにも対応するため keyを必要とする   例　右車線コレクション、左車線コレクション　どちらもList<Lane>
            /// </summary>
            /// <typeparam name="_Type"></typeparam>
            /// <param name="data"></param>
            /// <returns></returns>
            public bool AddCache<_ListElementVal>(string key, Func<_ListElement, _ListElementVal> selectMethod)
            {
                Assert.IsNotNull(key);
                Assert.IsNotNull(selectMethod);
                if (cacheDataMap.ContainsKey(key))
                {
                    Debug.LogError("追加済みのキャッシュデータキーを使用しようとした");
                    return false;
                }

                var e = this.Select(selectMethod);
                cacheDataMap.Add(key, new CacheData<_ListElementVal>() { selecter = selectMethod, data = e });
                return true;
            }

            public bool Recache<_ListElementVal>(string key)
            {
                Assert.IsNotNull(key);
                if (cacheDataMap.TryGetValue(key, out var cacheData) == false)
                {
                    return false;
                }

                var d = cacheData as CacheData<_ListElementVal>;
                Assert.IsNotNull(d);

                d.data = this.Select(d.selecter);
                return true;
            }

            private class CacheData<_Val>
            {
                public Func<_ListElement, _Val> selecter;
                public IEnumerable<_Val> data;
            }
            Dictionary<string, object> cacheDataMap = new Dictionary<string, object>();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="_BaseData"></typeparam>
        public class EditorData<_BaseData>
            where _BaseData : class
        {
            public EditorData(_BaseData point)
            {
                Assert.IsNotNull(point);
                this.Ref = point;
                this.userData = new List<object>();
            }

            /// <summary>
            /// 頻繁に呼ばないように注意する
            /// </summary>
            /// <typeparam name="_Type"></typeparam>
            /// <returns></returns>
            public _Type GetSubData<_Type>()
                where _Type : class
            {
                foreach (var item in userData)
                {
                    if (item is _Type)
                        return (_Type)item;
                }
                return null;
            }

            /// <summary>
            /// 追加を試みる
            /// 既に同じ型が登録されている場合は不可
            /// </summary>
            /// <typeparam name="_Type"></typeparam>
            /// <param name="data"></param>
            /// <returns></returns>
            public bool TryAdd<_Type>(_Type data)
                where _Type : class
            {
                var isNull = GetSubData<_Type>() == null;
                if (isNull)
                {
                    userData.Add(data);
                    return true;
                }
                return false;
            }

            public _BaseData Ref { get; private set; }
            List<object> userData;

        }

        public class PointEditorData
        {
            public PointEditorData(RnPoint point)
            {
                Assert.IsNotNull(point);
                refPoint = point;
                UpdateBasePosition();
            }

            /// <summary>
            /// 基準位置の更新
            /// </summary>
            public void UpdateBasePosition()
            {
                basePosition = refPoint.Vertex;
            }

            /// <summary>
            /// キャッシュに変更を適用する
            /// </summary>
            /// <param name="translate"></param>
            public void Translate(in Vector3 translate)
            {
                cacheTranslate += translate;
            }

            //public void Scale(in Vector3 scale)
            //{
            //    cacheScale += scale;
            //}

            public void Apply()
            {
                refPoint.Vertex = basePosition + cacheTranslate;
            }

            // 参照中のポイント
            private RnPoint refPoint;
            // 基準の座標
            private Vector3 basePosition;

            private Vector3 cacheTranslate;
            //private Vector3 cacheScale;
        }

        public class NodeEditorData
        {
            public NodeEditorData(GameObject gameObject)
            {
                Assert.IsNotNull(gameObject);
                this.RefGameObject = gameObject;
                UpdateBaseTransform();
                PointWithWeight.Clear();
            }

            public void UpdateBaseTransform()
            {
                baseTransform = new CacheTransform(RefGameObject.transform);
            }

            public bool AddPoint(EditorData<RnPoint> point, float weight)
            {
                Assert.IsTrue(0 <= weight && weight <= 1.0f, "weight " + weight.ToString());
                Assert.IsNotNull(point);
                var subData = point.GetSubData<PointEditorData>();
                Assert.IsNotNull(subData);
                return PointWithWeight.TryAdd(point, (subData, weight));
            }

            public void RemovePoint(EditorData<RnPoint> point)
            {
                PointWithWeight.Remove(point);
            }

            public void SetPointWeight(EditorData<RnPoint> point, float weight)
            {
                var isSuc = PointWithWeight.TryGetValue(point, out var w);
                if (isSuc)
                {
                    w.weight = weight;
                }
            }

            public void ApplyTranslate()
            {
                var translate = RefGameObject.transform.position - baseTransform.position;
                foreach (var item in PointWithWeight)
                {
                    var ptData = item.Value.data;
                    ptData.Translate(translate * item.Value.weight);
                }
            }

            private struct CacheTransform
            {
                public CacheTransform(Transform transform)
                {
                    position = transform.position;
                }

                public Vector3 position;
            }
            public GameObject RefGameObject;
            private CacheTransform baseTransform;

            private Dictionary<EditorData<RnPoint>, (PointEditorData data, float weight)> PointWithWeight { get; set; } =
                new Dictionary<EditorData<RnPoint>, (PointEditorData, float)>();

            public List<LinkGroupEditorData> Connections { get; private set; } = new List<LinkGroupEditorData>();

            public bool IsIntersection { get => Connections.Count >= 3; }

        }
        public class LinkGroupEditorData
        {
            public LinkGroupEditorData(NodeEditorData a, NodeEditorData b, IReadOnlyCollection<RnRoad> links, RnRoadGroup linkGroup)
            {
                Assert.IsNotNull(linkGroup);
                LinkGroup = linkGroup;

                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                if (a.GetHashCode() > b.GetHashCode())
                {
                    A = a;
                    B = b;
                }
                else
                {
                    A = b;
                    B = a;
                }

                Assert.IsNotNull(links);
                ConnectionLinks = links;
                // LinksがNode A,Bに接続されているかチェックするデバッグ機能
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() + B.GetHashCode();
            }

            /// <summary>
            /// レーン数を統一する
            /// </summary>
            public void AlignNumLane()
            {
                var nR = LinkGroup.GetRightLaneCount();
                var nL = LinkGroup.GetLeftLaneCount();
                LinkGroup.SetLaneCount(nL, nR);
            }



            public NodeEditorData A { get; private set; }
            public NodeEditorData B { get; private set; }

            public IReadOnlyCollection<RnRoad> ConnectionLinks { get; private set; }

            public RnRoadGroup LinkGroup { get; private set; }

            public List<Vector3> CacheRoadPosList { get; set; }

            public IReadOnlyList<IEnumerable<RnLane>> RigthLanesGroup
            {
                get
                {
                    // Link間でレーン数が違う場合に処理が出来ない　合わせる処理が必要？
                    throw new NotImplementedException("仕様が未定のため未実装");
                    //return null;
                }
            }

            public IEnumerable<RnLane> RightLanes
            {
                get
                {
                    return LinkGroup.GetRightLanes();
                }
            }
            public IEnumerable<RnLane> LeftLanes
            {
                get
                {
                    return LinkGroup.GetLeftLanes();
                }
            }


            // Person オブジェクトの比較をカスタマイズするクラス
            public class Comparer : IEqualityComparer<LinkGroupEditorData>
            {
                public bool Equals(LinkGroupEditorData x, LinkGroupEditorData y)
                {
                    // 名前が同じ場合は同一とみなす
                    return x.A == y.A && x.B == y.B;
                }

                public int GetHashCode(LinkGroupEditorData obj)
                {
                    // 名前のハッシュコードを返す
                    return obj.GetHashCode();
                }
            }



        }

        // refresh時にClaer()必要
        public class LaneEditCache
        {
            public float Scale { get; set; }
            public RnLane BaseLane { get; set; }
        }

        private Dictionary<RnLane, LaneEditCache> keyValuePairs = new Dictionary<RnLane, LaneEditCache>();

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
            bool needInitGameObj = roadNetworkEditingSystemObjRoot == null;

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
            var roadNetworkObj = GameObject.Find(defaultRoadNetworkObjectName);
            var r = roadNetworkObj.GetComponent<PLATEAURoadNetworkTester>();
            var roadNetwork = r.RoadNetwork;
            if (roadNetwork == null)
            {
                Debug.Log("RoadNetwork is null.");
                return false;
            }

            // その他 初期化
            if (roadNetwork != null)
            {
                // 自動設定機能
                this.roadNetworkObject = roadNetworkObj;
                Selection.activeGameObject = roadNetworkObj;

                system.RoadNetworkObject = roadNetworkObj;

                // 仮ポイントを　地形にスワップする
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


                simpleEditSysModule = new RoadNetworkSimpleEditSysModule(roadNetworkEditingSystemObjRoot, roadNetwork, system);
                //simpleEditSysModule.Init();

                simpleLaneGenerateModule = new RoadNetworkSimpleLaneGenerateModule();
                simpleLinkGenerateModule = new RoadNetworkSimpleLinkGenerateModule();
                simpleNodeGenerateModule = new RoadNetworkSimpleNodeGenerateModule();
            }

            return true;
        }

        /// <summary>
        /// 内部システムが利用するインターフェイス
        /// 内部システム同士が連携する時や共通データにアクセスする際に利用する
        /// </summary>
        public interface IRoadNetworkEditingSystem
        {

            // 仮
            RoadNetworkSceneGUISystem SceneGUISystem { get; }

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
            RnModel RoadNetwork { get; }

            //HashSet<LinkGroupEditorData> Connections { get; }

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
            RoadNetworkSimpleEditSysModule RoadNetworkSimpleEditModule { get; }

            RnLane GetBase(RnLane keyLane);
            float GetScale(RnLane keyLane);
            void RegisterBase(RnLane keyLane, RnLane baseLane, float scale, RnLane oldKeyLane);
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

            public RnModel RoadNetwork
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

            public List<EditorData<RnRoadGroup>> Connections => system.simpleEditSysModule?.Connections;

            public RoadNetworkSimpleEditSysModule RoadNetworkSimpleEditModule => system.simpleEditSysModule;

            public RoadNetworkSceneGUISystem SceneGUISystem => system.SceneGUISystem;

            public void NotifyChangedRoadNetworkObject2Editor()
            {
                if (system.roadNetworkObject == null)
                    return;
                EditorUtility.SetDirty(system.roadNetworkObject);

            }

            public RnLane GetBase(RnLane keyLane)
            {
                var isSuc = system.keyValuePairs.TryGetValue(keyLane, out var editCache);
                if (isSuc)
                    return editCache.BaseLane;
                return null;
            }
            public float GetScale(RnLane keyLane)
            {
                var isSuc = system.keyValuePairs.TryGetValue(keyLane, out var editCache);
                if (isSuc)
                    return editCache.Scale;
                return 1.0f;
            }
            public void RegisterBase(RnLane keyLane, RnLane baseLane, float scale, RnLane oldKeyLane)
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
            public RoadNetworkEditingResult AddPoint(RnWay way, int idx, RnPoint point)
            {
                //var v = new RoadNetworkPoint(new Vector3());
                way.LineString.Points.Insert(idx, point);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RemovePoint(RnWay way, RnPoint point)
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

            public RoadNetworkEditingResult MovePoint(RnPoint point, in Vector3 newPos)
            {
                if (point.Vertex == newPos)
                    return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);

                point.Vertex = newPos;
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult ScaleRoadWidth(RnLane lane, float factor)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult ScaleRoadWidth(RnRoad link, float factor)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult AddMainLane(RnRoad link, RnLane newLane)
            {
                //var v = new RoadNetworkLane(leftWay:, rightWay:, startBorder:, endBorder:);
                link.AddMainLane(newLane);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult AddLink(RnModel parent, RnRoad newLink)
            {
                //var v = new RoadNetworkLink(targetTran:);
                parent.AddLink(newLink);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RemoveLink(RnModel parent, RnRoad link)
            {
                parent.RemoveLink(link);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult AddNode(RnModel parent, int idx, RnIntersection newNode)
            {
                //var v = new RoadNetworkNode(targetTran:);
                parent.AddNode(newNode);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RemoveMainLane(RnRoad link, RnLane lane)
            {
                link.RemoveLane(lane);
                return new RoadNetworkEditingResult(RoadNetworkEditingResultType.Success);
            }

            public RoadNetworkEditingResult RegisterRegulation(RnRoad link, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RnLane lane, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RegisterRegulation(RnBlock block, RoadNetworkRegulationElemet newRegulation)
            {
                throw new NotImplementedException();
            }

            public RoadNetworkEditingResult RemoveNode(RnModel parent, RnIntersection node)
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

            private RnWay baseStartWay;
            private RnWay baseEndWay;

            private List<RnPoint> startPoints = new List<RnPoint>(2);
            private List<RnPoint> endPoints = new List<RnPoint>(2);

            private RnRoad link;


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
            public bool AddBorder(RnRoad link, RnWay way, RnPoint point)
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

                RnWay baseWay;
                List<RnPoint> points;
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

            private bool SelectBaseWay(RnWay way, RnPoint point, ref RnWay baseWay)
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

            public RnLane Build()
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

                var leftLine = new RnLineString();
                var rightLine = new RnLineString();

                // 時計回りで startPoint,endPointが設定されていることが前提
                // TODO　交差していたら参照順を変える処理を追加する
                var leftWay = new RnWay(RnLineString.Create(new RnPoint[] { startPoints[0], endPoints[1] }));
                var rightWay = new RnWay(RnLineString.Create(new RnPoint[] { startPoints[1], endPoints[0] }));

                var lane = new RnLane(leftWay, rightWay, start, end);
                link.AddMainLane(lane);
                return lane;
            }

            private static RnWay CreateBorderWay(RnWay baseWay, List<RnPoint> points)
            {
                var idx0 = baseWay.FindPoint(points[0]);
                var idx1 = baseWay.FindPoint(points[1]);
                return CreateBorderWay(baseWay, ref idx0, ref idx1);
            }

            private static RnWay CreateBorderWay(RnWay baseWay, ref int idx0, ref int idx1)
            {
                bool wasReversed = false;
                if (idx0 > idx1)
                {
                    var tmp = idx0;
                    idx0 = idx1;
                    idx1 = tmp;
                    wasReversed = true;
                }

                List<RnPoint> wayPoints = null;

                var n = idx1 - idx0 + 1;
                wayPoints = new List<RnPoint>(n);
                for (int i = idx0; i <= idx1; i++)
                {
                    wayPoints.Add(baseWay.LineString.Points[i]);
                }
                if (wasReversed)
                {
                    wayPoints.Reverse();
                }
                var way = new RnWay(RnLineString.Create(wayPoints));
                return way;
            }
        }

        public class RoadNetworkMedianStripGenerateModule
        {
            public RoadNetworkMedianStripGenerateModule()
            {
            }

            // 隣り合ったレーン　LineStringsを共有していることが条件
            private List<RnLane> neighborLanes = new List<RnLane>(2);

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

            public RnIntersection Build()
            {
                var ways = new List<RnWay>(4)
                {
                    neighborLanes[0].LeftWay,
                    neighborLanes[0].RightWay,
                    neighborLanes[1].LeftWay,
                    neighborLanes[1].RightWay
                };

                // 共有するLineStringsを探す
                RnLineString unionLineStrins = null;
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
                var newLineStrings = RnLineString.Create(unionLineStrins);
                var lane = neighborLanes[i / 2];
                RnLane newLane = null;
                if (i % 2 == 0)
                {
                    newLane = new RnLane(new RnWay(newLineStrings), lane.RightWay, lane.PrevBorder, lane.NextBorder);
                    //lane.LeftWay = new RoadNetworkWay(newLineStrings);
                }
                else
                {
                    newLane = new RnLane(lane.LeftWay, new RnWay(newLineStrings), lane.PrevBorder, lane.NextBorder);
                    //lane.RightWay = new RoadNetworkWay(newLineStrings);
                }
                var parentLink = lane.Parent as RnRoad;
                parentLink?.ReplaceLane(lane, newLane);
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

            public RnIntersection Build()
            {
                var node = new RnIntersection(tranObj);
                return null;
            }
        }

        public class RoadNetworkSimpleLinkGenerateModule
        {
            public RoadNetworkSimpleLinkGenerateModule()
            {
            }

            private PLATEAU.CityInfo.PLATEAUCityObjectGroup tranObj;
            private RnModel parent;
            private List<RnPoint> points = new List<RnPoint>();

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

            public bool AddPoint(RnModel parent, RnPoint point)
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

            public RnRoad Build()
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

                var startBorder = new RnWay(RnLineString.Create(points.GetRange(0, 2)));
                var leftWay = new RnWay(RnLineString.Create(points.GetRange(1, 2)));
                var endBorder = new RnWay(RnLineString.Create(points.GetRange(2, 2)));
                var rightWay = new RnWay(RnLineString.Create(new RnPoint[] { points[3], points[0] }));
                var lane = new RnLane(leftWay, rightWay, startBorder, endBorder);
                var link = RnRoad.CreateOneLaneLink(tranObj, lane);
                parent.AddLink(link);
                return link;
            }

        }

        public class RoadNetworkLaneWidthEditModule
        {
            public RoadNetworkLaneWidthEditModule()
            {
            }

            private RnLane lane;
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

            public bool SetLane(RnLane lane, float scale)
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

            public RnRoad Edit()
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

        public class RoadNetworkSimpleEditSysModule
        {
            public List<EditorData<RnRoadGroup>> Connections { get => linkGroupEditorData; }
            public RoadNetworkSimpleEditSysModule(GameObject root, RnModel rnModel, IRoadNetworkEditingSystem system)
            {
                ReConstruct(root, rnModel, system);
            }

            private GameObject roadNetworkEditingSystemObjRoot;
            private RnModel roadNetwork;
            private IRoadNetworkEditingSystem system;

            private Dictionary<RnRoadBase, NodeEditorData> nodeEditorData = new Dictionary<RnRoadBase, NodeEditorData>();
            private EditorDataList<EditorData<RnRoadGroup>> linkGroupEditorData = new EditorDataList<EditorData<RnRoadGroup>>();
            private Dictionary<RnPoint, EditorData<RnPoint>> ptEditorData = new Dictionary<RnPoint, EditorData<RnPoint>>();

            //private HashSet<LinkGroupEditorData> linkGroupEditorData = new HashSet<LinkGroupEditorData>();
            /// <summary>
            /// 計算や処理に必要な要素を初期化する
            /// </summary>
            /// <param name="root"></param>
            /// <param name="rnModel"></param>
            /// <param name="system"></param>
            public void ReConstruct(GameObject root, RnModel rnModel, IRoadNetworkEditingSystem system)
            {
                Assert.IsNotNull(root);
                Assert.IsNotNull(rnModel);
                Assert.IsNotNull(system);
                roadNetworkEditingSystemObjRoot = root;
                roadNetwork = rnModel;
                this.system = system;

                EditorApplication.update -= Update;
                ClearCache();
            }

            /// <summary>
            /// 計算や処理を行う初期化
            /// それらに必要な要素は初期化済みとする
            /// </summary>
            public void Init()
            {
                ClearCache();

                nodeEditorData = new Dictionary<RnRoadBase, NodeEditorData>(roadNetwork.Nodes.Count);
                // ノードに紐づくオブジェクトを作成 editor用のデータを作成
                var nodePrefabPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/RoadNetwork/Node.prefab";
                // プレハブをResourcesフォルダからロード
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(nodePrefabPath);
                Assert.IsNotNull(prefab);
                var id = 0;
                StringBuilder sb = new StringBuilder("Node".Length + "XXX".Length);
                foreach (var node in roadNetwork.Nodes)
                {
                    var name = sb.AppendFormat("Node{0}", id).ToString();
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //var obj = new GameObject(name, typeof(MeshFilter));
                    obj.transform.position = node.GetCenterPoint();
                    obj.transform.SetParent(roadNetworkEditingSystemObjRoot.transform, false);
                    //var obj = GameObject.InstantiateGameObjects(prefab,);
                    //var obj = GameObject.Instantiate(
                    //    prefab, node.GetCenterPoint(), Quaternion.Euler(90.0f,0.0f,0.0f), roadNetworkEditingSystemObjRoot.transform);
                    obj.name = name;
                    obj.transform.hasChanged = false;   // transformの変更を検知するためfalseを設定
                    var subData = new NodeEditorData(obj);
                    nodeEditorData.Add(node, subData);

                    id++;
                    sb.Clear();
                }

                // pointのeditor用のデータを作成
                //var lineE = roadNetwork.CollectAllLineStrings();
                //lineE.Reset();
                var lineE = roadNetwork.CollectAllLineStrings().GetEnumerator();
                while (lineE.MoveNext())
                {
                    var line = lineE.Current;
                    //Ray ray;
                    foreach (var point in line.Points)
                    {
                        var ptData = new EditorData<RnPoint>(point);
                        var isSuc = ptEditorData.TryAdd(point, ptData);
                        if (isSuc)
                        {
                            isSuc = ptData.TryAdd(new PointEditorData(point));
                            Assert.IsTrue(isSuc);
                        }
                    }
                }

                // Nodeとその近辺のPointを紐づける
                HashSet<RnPoint> points = new HashSet<RnPoint>(500);  // capacityは適当
                var numNode = roadNetwork.Nodes.Count;
                if (numNode == 0)
                    return;

                HashSet<LinkGroupEditorData> linkGroups = new HashSet<LinkGroupEditorData>(numNode * (numNode - 1));  // node同士の繋がりを表現するコレクション prev+next名で表現する
                HashSet<RnNeighbor> calcedNeighbor = new HashSet<RnNeighbor>(numNode * (numNode - 1));  // 計算済みのNeighborを保持する
                foreach (var node in roadNetwork.Nodes)
                {
                    foreach (var neighbor in node.Neighbors)
                    {
                        // この接続元は計算済み
                        if (calcedNeighbor.Contains(neighbor))
                        {
                            continue;
                        }
                        var link = neighbor.Link;
                        if (link == null)
                        {
                            continue;
                        }
                        var linkGroup = link.CreateLinkGroup();
                        if (linkGroup == null)
                        {
                            continue;
                        }

                        // 接続先にNodeが無い場合はスキップ　仮
                        if (linkGroup.PrevNode == null || linkGroup.NextNode == null)
                        {
                            continue;
                        }

                        var node0 = linkGroup.PrevNode;
                        var node1 = linkGroup.NextNode;
                        var cn = new LinkGroupEditorData(nodeEditorData[node0], nodeEditorData[node1], linkGroup.Links, linkGroup);
                        if (linkGroups.Add(cn) == true)
                        {
                            nodeEditorData[node0].Connections.Add(cn);
                            nodeEditorData[node1].Connections.Add(cn);
                        }

                        calcedNeighbor.Add(neighbor);
                        var otherNode = node == node0 ? node1 : node0;
                        var otherLink = link == linkGroup.Links.First() ? linkGroup.Links.Last() : linkGroup.Links.First();
                        foreach (var otherNeighbor in otherNode.Neighbors)
                        {
                            if (otherLink == otherNeighbor.Link)
                            {
                                calcedNeighbor.Add(otherNeighbor);
                            }
                        }
                    }
                }

                linkGroupEditorData.Capacity = linkGroups.Count;
                foreach (var item in linkGroups)
                {
                    var editorData = new EditorData<RnRoadGroup>(item.LinkGroup);
                    editorData.TryAdd(item);
                    linkGroupEditorData.Add(editorData);
                }

                // Transform変更を検知する
                EditorApplication.update -= Update;
                EditorApplication.update += Update;


                // キャッシュの生成
                linkGroupEditorData.AddCache("linkGroup", (d) => d.GetSubData<LinkGroupEditorData>());
                //linkGroupEditorData.Select((d) => d.GetSubData<LinkGroupEditorData>()).ToList();

                return;
                var linkLinstCap = roadNetwork.Links.Count / numNode * 2;
                foreach (var node in roadNetwork.Nodes)
                {
                    // Nodeと繋がりのあるレーンすべてのPointを取得(現在はLink同士、Node同士が繋がっていた場合には2番目以降のLinkのPointは参照しない　もしくはNode間の境目は参照しない)

                    bool bIsNext;
                    // NodeとつながりのあるNodeを探す
                    foreach (var neighbor in node.Neighbors)
                    {
                        // 計算済みに追加 計算済みのNeighborはスキップ
                        if (calcedNeighbor.Add(neighbor) == false)
                            continue;

                        RnIntersection connectedNode = null;
                        var link = neighbor.Link;

                        List<RnRoad> linkList = new List<RnRoad>(linkLinstCap);    // capは適当
                        while (link != null)
                        {
                            //// Nodeと繋がりのあるレーンすべてのPointを取得(現在はLink同士が繋がっていた場合には2番目以降のLinkのPointは参照しない)
                            //foreach (var lane in link.AllLanes)
                            //{
                            //    foreach (var point in lane.Points)
                            //    {
                            //        points.Add(point);  // HashSetなので重複は無視される
                            //    }
                            //}

                            // 走査方向を決定する Next or Prev
                            bIsNext = link.Next != node;

                            // 現在のノードから離れる接続先
                            var neighborBase = bIsNext ? link.Next : link.Prev;

                            // Linkだった場合、次のLinkを取得
                            if (neighborBase is RnRoad neighborLink)
                            {
                                link = neighborLink;
                                linkList.Add(neighborLink);
                                continue;   // 走査を続ける
                            }
                            // Node 無い場合、走査の終了
                            else if (neighborBase is RnIntersection neighborNode)
                            {
                                // Nodeと他のNodeを挟まずに繋がりのあるNodeを設定  〇Node->connectedNode　〇Node->Link->connectedNode ×Node->OtherNode->connectedNode
                                connectedNode = neighborNode;

                                // 自身のノードに戻ってきた
                                if (node == connectedNode)
                                {
                                    link = null;
                                    continue;
                                }

                                // 辿ってきたLinkと紐づいたNeighborを探索
                                bool isAddCalced = false;
                                foreach (var item in connectedNode.Neighbors)
                                {
                                    if (link == item.Link)
                                    {
                                        var cn = new LinkGroupEditorData(nodeEditorData[node], nodeEditorData[connectedNode], linkList, null);
                                        if (linkGroups.Add(cn) == true)
                                        {
                                            nodeEditorData[node].Connections.Add(cn);
                                            nodeEditorData[connectedNode].Connections.Add(cn);
                                        }

                                        isAddCalced = calcedNeighbor.Add(item);
                                        if (isAddCalced)
                                            break;
                                    }
                                }
                                // 何故かflaseになる場合がある(原因不明 一時的にAssert回避
                                //Assert.IsTrue(isAddCalced); // 最初のforeachで計算済みはスキップしているので必ず追加されるはず
                            }
                            else
                            {
                                // 続きが無い場合、走査の終了
                            }
                            link = null;

                        }

                        //// 仮weight付け　本来はNode間を繋ぐ線にも紐づくべき
                        //foreach (var pt in points)
                        //{
                        //    var d0 = nodeEditorData[node];
                        //    var d1 = connectedNode != null ? nodeEditorData[connectedNode] : null;
                        //    float w0 = 1.0f;
                        //    float w1 = 0.0f;
                        //    if (d1 != null)
                        //    {
                        //        var dist0 = Vector3.Distance(d0.RefGameObject.transform.position, pt.Vertex);
                        //        var dist1 = Vector3.Distance(d1.RefGameObject.transform.position, pt.Vertex);
                        //        w0 = dist0 / (dist0 + dist1);
                        //        w1 = dist1 / (dist0 + dist1);
                        //    }
                        //    var ptData = ptEditorData[pt];
                        //    d0.AddPoint(ptData, w0);
                        //    d1?.AddPoint(ptData, w1);
                        //}

                        // 直接つながっているものはweight1にする

                        // 例外としてNodeと直接つながっているものはweight0.5にする


                        // 同じRnPointを利用したneighborがある場合、Pointに対して再度計算を行うことになる 修正が必要
                        // コレクションを初期化
                        points.Clear();
                    }
                }

                // Transform変更を検知する
                EditorApplication.update -= Update;
                EditorApplication.update += Update;
            }

            private void ClearCache()
            {
                nodeEditorData.Clear();
                linkGroupEditorData.Clear();
                ptEditorData.Clear();

            }

            private void Update()
            {
                if (roadNetworkEditingSystemObjRoot == null)
                    return;
                bool isChanged = false;
                //foreach (var data in nodeEditorData)
                //{
                //    var val = data.Value;
                //    var transform = val.RefGameObject.transform;
                //    if (transform.hasChanged)
                //    {
                //        // 平行移動を適用する
                //        Debug.Log("has changed " + val.RefGameObject.name);
                //        val.ApplyTranslate();

                //        // 検知状態をリセット
                //        transform.hasChanged = false;

                //        isChanged = true;
                //    }
                //}

                if (isChanged)
                {
                    this.system.NotifyChangedRoadNetworkObject2Editor();
                }

                // billboardの更新
                bool isNeedUpdateBillboard = false;
                var camera = SceneView.currentDrawingSceneView?.camera; // おそらく描画時にのみインスタンスが設定される
                if (camera?.transform.hasChanged == true)
                    isNeedUpdateBillboard = true;

                foreach (var data in nodeEditorData)
                {
                    if (isNeedUpdateBillboard == false)
                        break;

                    var cameraPos = camera.transform.position;
                    var val = data.Value;
                    var transform = val.RefGameObject.transform;
                    transform.LookAt(cameraPos, Vector3.up);
                }
                if (camera)
                {
                    camera.transform.hasChanged = false;
                }

                // 検知状態のリセット
                //foreach (var data in nodeEditorData)
                //{
                //    var val = data.Value;
                //    var transform = val.RefGameObject.transform;
                //    // 検知状態をリセット
                //    transform.hasChanged = false;
                //}

                // gizmos描画の更新
                var gizmos = roadNetworkEditingSystemObjRoot.GetComponent<RoadNetworkEditorGizmos>();
                var guisys = system.SceneGUISystem;
                if (guisys != null)
                {
                    // 仮　専用ノード間の繋がりを描画
                    if (linkGroupEditorData.TryGetCache("linkGroup", out IEnumerable<LinkGroupEditorData> eConn) == false)
                    {
                        Assert.IsTrue(false);
                        return;
                    }
                    List<LinkGroupEditorData> connections = eConn.ToList();

                    gizmos.intersectionConnectionLinePairs.Clear();
                    var pts = gizmos.intersectionConnectionLinePairs;
                    pts.Capacity = (connections.Count + 3) * 2;
                    foreach (var item in connections)
                    {
                        if (item.CacheRoadPosList == null)
                        {
                            item.CacheRoadPosList = new List<Vector3>(item.ConnectionLinks.Count * 2);

                            foreach (var link in item.ConnectionLinks)
                            {
                                var allLanes = link.AllLanes.GetEnumerator();
                                Vector3 prevBorderPos = Vector3.zero;
                                Vector3 nextBorderPos = Vector3.zero;

                                if (allLanes.MoveNext())
                                {
                                    var lane = allLanes.Current;
                                    var prevpoints = lane.PrevBorder.Points;
                                    var nextpoints = lane.NextBorder.Points;
                                    if (CalcBorderPos(prevpoints, out prevBorderPos) &&
                                        CalcBorderPos(nextpoints, out nextBorderPos))
                                    {
                                        item.CacheRoadPosList.Add(prevBorderPos);
                                        item.CacheRoadPosList.Add(nextBorderPos);
                                    }
                                }
                            }
                        }
                        foreach (var p in item.CacheRoadPosList)
                        {
                            pts.Add(p);
                        }

                    }

                    //gizmos.intersectionConnectionLinePairs2.Clear();
                    //var pts = gizmos.intersectionConnectionLinePairs2;
                    //pts.Capacity = (connections.Count + 3) * 2;
                    //foreach (var item in connections)
                    //{
                    //    foreach (var link in item.ConnectionLinks)
                    //    {
                    //        foreach (var lane in link.AllLanes)
                    //        {
                    //            lane.NextBorder.LineString.Points
                    //        }
                    //        pts.Add();

                    //    }
                    //    //Debug.Log(item.A.RefGameObject.name + "." + item.B.RefGameObject.name);
                    //}

                    gizmos.intersectionConnectionLinePairs = pts;
                    guisys.connections = linkGroupEditorData;
                    //if (connections.Count > 0)
                    //{
                    //    Handles.DrawLines(pts);
                    //    //Gizmos.DrawLineList(pts);
                    //}
                    guisys.intersections.Clear();
                    var intersectionsPoss = guisys.intersections;
                    intersectionsPoss.Capacity = nodeEditorData.Count;
                    foreach (var item in nodeEditorData.Values)
                    {
                        if (item.IsIntersection == false)
                            continue;
                        intersectionsPoss.Add(item.RefGameObject.transform.position);
                    }
                }
            }

            private static bool CalcBorderPos(IEnumerable<RnPoint> points, out UnityEngine.Vector3 v)
            {
                var nP = points.Count();
                if (nP <= 0)
                {
                    v = Vector3.zero;
                    return false;
                }
                var sum = Vector3.zero;
                //lane.NextBorder.HalfLineIntersectionXz
                foreach (var p in points)
                {
                    sum += p.Vertex;
                }
                var borderPos = sum / nP;
                v = borderPos;
                return true;
            }
        }
    }

}
