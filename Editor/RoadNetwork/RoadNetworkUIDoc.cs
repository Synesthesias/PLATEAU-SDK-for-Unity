using NUnit.Framework;
using PLATEAU.RoadNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;
using GenerateParameterFunc =
    System.Action<PLATEAU.Editor.RoadNetwork.RoadNetworkUIDoc,
        PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem.IRoadNetworkEditingSystem,
        PLATEAU.Editor.RoadNetwork.RoadNetworkEditorAssets,
        UnityEngine.UIElements.VisualElement>;


namespace PLATEAU.Editor.RoadNetwork
{
    public enum RoadNetworkEditingResultType
    {
        _Undefind = 0xfffffff,
        Success = 0x0000000,
        InvalidNewValue = 1 << 0,      // 適切な変更値ではない
        InvalidTarget = 1 << 1,      // 適切な変更対象ではない(主に第1引数のLink,Laneなどについて)         
        CantApplyEditing = 1 << 2,      // 適用できない編集。リンクの幅員を小さくしすぎて一部のレーンの幅員が0になるなど
        InvalidArgs = 1 << 3,      // 適切な引数ではない。(主に第2引数以降　Wayのポイントを削除する際に渡したポイントが存在しないなど)
        _UndefindError = 1 << 256,
        //Faild // 失敗原因を明確にするために未使用
    }

    public struct RoadNetworkEditingResult
    {
        public readonly RoadNetworkEditingResultType Result;
        public readonly string Msg;

        public bool IsSuccess { get => Result == RoadNetworkEditingResultType.Success; }
        public RoadNetworkEditingResult(RoadNetworkEditingResultType result, string msg = "")
        {
            this.Result = result;
            this.Msg = msg;
        }
    }

    /// <summary>
    /// 道路ネットワークの交通規制を登録するための構造体ラッパ
    /// 削除予定　インターフェイスで引数で利用しているがそれぞれのクラスの引数を用意する
    /// </summary>
    public struct RoadNetworkRegulationElemet
    {
        // 信号制御器
        // 信号灯器
        // 一時停止線
        // 一時停止標識
        // 工事、事故、その他の規制
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
        RoadNetworkEditingResult AddPoint(RoadNetworkWay way, int idx, RoadNetworkPoint point);
        RoadNetworkEditingResult RemovePoint(RoadNetworkWay way, RoadNetworkPoint point);
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
        /// 射線を増やす、減らす
        /// </summary>
        /// <param name="link"></param>
        /// <param name="idx"></param>
        /// <param name="newLane"></param>
        /// <returns></returns>
        RoadNetworkEditingResult AddMainLane(RoadNetworkLink link, int idx, RoadNetworkLane newLane);
        RoadNetworkEditingResult RemoveLane(RoadNetworkLink link, RoadNetworkLane lane);

        /// <summary>
        /// 交通規制情報の登録
        /// </summary>
        /// <param name="link"></param>
        /// <param name="newRegulation"></param>
        /// <returns></returns>
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkLink link, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkLane lane, RoadNetworkRegulationElemet newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkBlock block, RoadNetworkRegulationElemet newRegulation);
    }

    /// <summary>
    /// 道路ネットワークの編集モード
    /// </summary>
    public enum RoadNetworkEditMode
    {
        EditLaneShape,
        EditLaneStructure,
        EditTrafficRegulation,
    }

    /// <summary>
    /// 道路ネットワークエディタのUIDocumentを扱うクラス
    /// インスペクター、エディタ、ランタイムでも利用できるように汎用化する
    /// (エディタ専用のUIを使用してるため現状はランタイム使用不可)
    /// 
    /// 未完成のクラス　大きく改装予定　役割は変えない
    /// </summary>
    public class RoadNetworkUIDoc
    {
        // RoadNetworkUIDoc内に隠したい, 依存部分をこのクラスの下層に配置する？
        //public RoadNetworkEditMode CurrentMode { get => (RoadNetworkEditMode)modeSelector.value; }
        public RoadNetworkEditMode CurrentMode
        {
            get => system.CurrentEditMode;
            set => system.CurrentEditMode = value;
        }

        private Button refreshButton;
        private EnumField modeSelector;
        private ScrollView parameterView;
        private ObjectField objSelecter;
        private TrafficSignalLightControllerUIDoc trafficSignalLightControllerUIDoc;

        private readonly IRoadNetworkEditingSystem system;
        private readonly RoadNetworkEditorAssets assets;

        private readonly Dictionary<RoadNetworkEditMode, GenerateParameterFunc> ParamterLayoutSet =
            new Dictionary<RoadNetworkEditMode, GenerateParameterFunc> {
            { RoadNetworkEditMode.EditLaneShape, CreateEditLaneShapeLayout },
            { RoadNetworkEditMode.EditLaneStructure, CreateEditLaneStructureLayout },
            { RoadNetworkEditMode.EditTrafficRegulation, CreateTrafficRegulationLayout },
        };

        private static void CreateDebugNodeLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            var element = assets.GetAsset(RoadNetworkEditorAssets.Vec3Field).Instantiate();
            element.Q<Vector3Field>().label = "座標";
            root.Add(element);
        }

        private static void CreateDebugLinkLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

            Func<VisualElement> createDataIDField = () =>
            {
                var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDField).Instantiate().Q<IntegerField>();
                dataID.label = "";
                dataID.value = -1;
                return dataID;
            };
            CreateParamterBox(assets, root, "本線レーン", createDataIDField);
            CreateParamterBox(assets, root, "右折レーン", createDataIDField);
            CreateParamterBox(assets, root, "左折レーン", createDataIDField);

            var floatFieldAsset = assets.GetAsset(RoadNetworkEditorAssets.FloatFieldAsset);
            if (floatFieldAsset.Instantiate() is var linkCap)
            {
                var slider = linkCap.Q<Slider>();
                {
                    slider.label = "リンク容量(pcu/時/車線)";
                    slider.highValue = 100.0f;
                    slider.value = 0.0f;
                }
                root.Add(linkCap);
            }

            if (floatFieldAsset.Instantiate() is var freeSpd)
            {
                var slider = freeSpd.Q<Slider>();
                {
                    slider.label = "自由流速度(km/時)";
                    slider.highValue = 300.0f;
                    slider.value = 60.0f;
                }
                root.Add(freeSpd);
            }
            if (floatFieldAsset.Instantiate() is var jamDens)
            {
                var slider = jamDens.Q<Slider>();
                {
                    slider.label = "ジャム密度(pcu/km/車線)";
                    slider.highValue = 100.0f;
                    slider.value = 0.0f;
                }
                root.Add(jamDens);
            }
        }

        private static void CreateDebugLaneLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            Func<VisualElement> createVec3Field = () =>
            {
                var vecField = assets.GetAsset(RoadNetworkEditorAssets.Vec3Field).Instantiate().Q<Vector3Field>();
                vecField.label = "";
                return vecField;
            };
            CreateParamterBox(assets, root, "道形状の設定(左側Way)", createVec3Field);
            CreateParamterBox(assets, root, "道形状の設定(右側Way)", createVec3Field);

            Func<VisualElement> createDataIDField = () =>
            {
                var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDField).Instantiate().Q<IntegerField>();
                dataID.label = "";
                dataID.value = -1;
                return dataID;
            };
            CreateParamterBox(assets, root, "連結元レーン(上流側)", createDataIDField);
            CreateParamterBox(assets, root, "連結先レーン(下流側)", createDataIDField);
        }


        private static void CreateParamterBox(RoadNetworkEditorAssets assets, VisualElement root, string labelText, Func<VisualElement> createElemetFunc)
        {
            var parameterBoxAsset = assets.GetAsset(RoadNetworkEditorAssets.ParameterBoxAssetName);

            var parameterBox = parameterBoxAsset.Instantiate();

            var label = parameterBox.Q<Label>("GroupBoxLabel");
            var addBtn = parameterBox.Q<Button>("AddElementBtn");
            var removeBtn = parameterBox.Q<Button>("RemoveElementBtn");
            var elementFolder = parameterBox.Q<ScrollView>("ElementFolder");

            label.text = labelText;

            addBtn.clicked += () =>
            {
                elementFolder.Add(createElemetFunc());
            };
            removeBtn.clicked += () =>
            {
                if (elementFolder.childCount <= 0)
                    return;
                elementFolder.RemoveAt(elementFolder.childCount - 1);
            };

            root.Add(parameterBox);
        }

        private static void CreateEditLaneShapeLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

        }

        private static void CreateEditLaneStructureLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

        }

        private static void CreateTrafficRegulationLayout(RoadNetworkUIDoc roadNetworkUIDoc, IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            roadNetworkUIDoc.trafficSignalLightControllerUIDoc = new TrafficSignalLightControllerUIDoc(system, assets, root);
            roadNetworkUIDoc.trafficSignalLightControllerUIDoc.CreateTrafficRegulationLayout();

            //var testVec3Field = new Vector3Field("controller position");
            //var testObj = new TrafficSignalLightController("test", null, Vector3.zero);
            //var acc = UIDocBind.Helper.A(testObj).GetOrCreate(testObj.Position, nameof(testObj.Position));
            //UIDocBind.Helper.Bind(testVec3Field, acc, testObj);
            //root.Add(testVec3Field);

            return;
        }

        public RoadNetworkUIDoc(IRoadNetworkEditingSystem system, VisualElement editorRoot, in RoadNetworkEditorAssets assets)
        {
            // 正当性チェック 最低限の初期化
            Assert.IsNotNull(system);
            Assert.IsNotNull(editorRoot);
            Assert.IsNotNull(assets);

            this.system = system;
            this.assets = assets;

            refreshButton = editorRoot.Q<Button>("RefreshBtn");
            Assert.IsNotNull(refreshButton);
            modeSelector = editorRoot.Q<EnumField>("ModeSelector");
            Assert.IsNotNull(modeSelector);

            objSelecter = editorRoot.Q<ObjectField>("EditingTargetSelecter");
            Assert.IsNotNull(objSelecter);

            parameterView = editorRoot.Q<ScrollView>("ParameterView");
            Assert.IsNotNull(parameterView);


        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            //　parameterの変更は必ずシステムを介して行う

            refreshButton.RegisterCallback<MouseUpEvent>((evt) =>
            {
                system.EditorInstance.RequestReinitialize();
            });

            system.OnChangedRoadNetworkObject += SelectRoadNetwrokObject;
            system.OnChangedEditMode += UpdateMode;

            modeSelector.RegisterCallback<ChangeEvent<Enum>>((evt) =>
            {
                var mode = (RoadNetworkEditMode)evt.newValue;
                CurrentMode = (RoadNetworkEditMode)modeSelector.value;
            });

            system.OnChangedSelectRoadNetworkElement -= SelectRoadNetwrokElement;
            system.OnChangedSelectRoadNetworkElement += SelectRoadNetwrokElement;
        }

        public void PostInitialize()
        {
            CurrentMode = (RoadNetworkEditMode)modeSelector.value;
        }

        /// <summary>
        /// 終了
        /// </summary>
        public void Terminate()
        {

        }

        private void UpdateMode(object _, EventArgs _1)
        {
            var mode = system.CurrentEditMode;
            parameterView.Clear();
            ParamterLayoutSet[mode](this, system, assets, parameterView);
        }

        private void SelectRoadNetwrokObject(object _, EventArgs _1)
        {
            objSelecter.value = system.RoadNetworkObject;
            UpdateMode(null, null);
        }

        private void SelectRoadNetwrokElement(object _, EventArgs _1)
        {
            if (system.CurrentEditMode == RoadNetworkEditMode.EditTrafficRegulation)
            {
                var node = system.SelectedRoadNetworkElement as RoadNetworkNode;
                if (node != null)
                {
                    if (node.SignalController == null)
                    {
                        var trafficController = new TrafficSignalLightController("SignalController" + node.DebugMyId, node, node.GetCenterPoint());
                        node.SignalController = trafficController;
                        foreach (var item in node.Neighbors)
                        {
                            var n = item.Border.Count();
                            for (int i = 0; i < n - 1; i++)
                            {
                                var pos = (item.Border[i] + item.Border[i + 1]) / 2.0f;
                                var signalLight = new TrafficSignalLight(trafficController, pos);
                                trafficController.SignalLights.Add(signalLight);
                            }
                        }
                    }
                }
            }
        }

    }

}
