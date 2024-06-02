using Codice.Client.BaseCommands;
using Codice.Client.Common.FsNodeReaders;
using NUnit.Framework;
using PLATEAU.RoadNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;

using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

using GenerateParameterFunc = 
    System.Action<
        PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem.IRoadNetworkEditingSystem, 
        PLATEAU.Editor.RoadNetwork.RoadNetworkEditorAssets, 
        UnityEngine.UIElements.VisualElement>;


namespace PLATEAU.Editor.RoadNetwork
{
    public enum RoadNetworkEditingResultType
    {
        _Undefind        = 0xfffffff,
        Success          = 0x0000000,
        InvalidNewValue  = 1 << 0,      // 適切な変更値ではない
        InvalidTarget    = 1 << 1,      // 適切な変更対象ではない(主に第1引数のLink,Laneなどについて)         
        CantApplyEditing = 1 << 2,      // 適用できない編集。リンクの幅員を小さくしすぎて一部のレーンの幅員が0になるなど
        InvalidArgs      = 1 << 3,      // 適切な引数ではない。(主に第2引数以降　Wayのポイントを削除する際に渡したポイントが存在しないなど)
        _UndefindError   = 1 << 256,
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

    public struct _RoadNetworkRegulation
    {

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
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkLink link, _RoadNetworkRegulation newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkLane lane, _RoadNetworkRegulation newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkBlock block, _RoadNetworkRegulation newRegulation);
        RoadNetworkEditingResult RegisterRegulation(RoadNetworkTrack track, _RoadNetworkRegulation newRegulation);
    }

    /// <summary>
    /// 道路ネットワークの編集モード
    /// </summary>
    public enum RoadNetworkEditMode
    {
        EditLaneShape,
        EditLaneStructure,
        EditTrafficRegulation,
        //DebugNone,
        //DebugNode,
        //DebugLink,
        //DebugLane,
        //DebugBlock,
        //DebugTrack,
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
        public RoadNetworkEditMode CurrentMode { 
            get => system.CurrentEditMode; 
            set => system.CurrentEditMode = value;
        }

        private Button refreshButton;
        private EnumField modeSelector;
        private ScrollView parameterView;
        private ObjectField objSelecter;

        private readonly IRoadNetworkEditingSystem system;
        private readonly RoadNetworkEditorAssets assets;

        private readonly Dictionary<RoadNetworkEditMode, GenerateParameterFunc> ParamterLayoutSet =
            new Dictionary<RoadNetworkEditMode, GenerateParameterFunc> {
            { RoadNetworkEditMode.EditLaneShape, CreateEditLaneShapeLayout },
            { RoadNetworkEditMode.EditLaneStructure, CreateEditLaneStructureLayout },
            { RoadNetworkEditMode.EditTrafficRegulation, CreateTrafficRegulationLayout },
            //{ RoadNetworkEditMode.DebugNone,
            //        (IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)=>{

            //        }
            //    } ,
            //{ RoadNetworkEditMode.DebugNode, CreateDebugNodeLayout },
            //{ RoadNetworkEditMode.DebugLink, CreateDebugLinkLayout },
            //{ RoadNetworkEditMode.DebugLane, CreateDebugLaneLayout },
            //{ RoadNetworkEditMode.DebugBlock, CreateDebugBlockLayout },
            //{ RoadNetworkEditMode.DebugTrack, CreateDebugTrackLayout },

        };

        private static void CreateDebugNodeLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            var element = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate();
             element.Q<Vector3Field>().label = "座標";
            root.Add(element);
        }

        private static void CreateDebugLinkLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

            Func<VisualElement> createDataIDField = () => {
                var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDFieldAssetName).Instantiate().Q<IntegerField>();
                dataID.label = "";
                dataID.value = -1;
                return dataID;
            };
            CreateParamterBox(assets, root, "本線レーン", createDataIDField);
            CreateParamterBox(assets, root, "右折レーン", createDataIDField);
            CreateParamterBox(assets, root, "左折レーン", createDataIDField);

            var floatFieldAsset = assets.GetAsset(RoadNetworkEditorAssets.FloatFieldAssetName);
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
            Func<VisualElement> createVec3Field = () => {
                var vecField = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate().Q<Vector3Field>();
                vecField.label = "";
                return vecField;
                };
            CreateParamterBox(assets, root, "道形状の設定(左側Way)", createVec3Field);
            CreateParamterBox(assets, root, "道形状の設定(右側Way)", createVec3Field);

            Func<VisualElement> createDataIDField = () => {
                var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDFieldAssetName).Instantiate().Q<IntegerField>();
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

            addBtn.clicked += () => {
                elementFolder.Add(createElemetFunc());
            };
            removeBtn.clicked += () => {
                if (elementFolder.childCount <= 0)
                    return;
                elementFolder.RemoveAt(elementFolder.childCount - 1);
            };

            root.Add(parameterBox);
        }

        private static void CreateEditLaneShapeLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {
            
        }

        private static void CreateEditLaneStructureLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

        }

        private static void CreateTrafficRegulationLayout(IRoadNetworkEditingSystem system, RoadNetworkEditorAssets assets, VisualElement root)
        {

            var panel = assets.GetAsset(RoadNetworkEditorAssets.TrafficRegulationAssetName);

            var panelInst = panel.Instantiate();
            var deploObjectSelecter = panelInst.Q<DropdownField>("DeploObjectSelecter");
            deploObjectSelecter.RegisterCallback<ChangeEvent<string>>((e) => { 
                
            });

            root.Add(panelInst);
            return;
            //var haveSingnalControllers = system.RoadNetwork?.SignalControllers?.Count() > 0;
            //var signalController = system.SelectedRoadNetworkElement as SignalController;
            //if (signalController == null)
            //    return;


            //Func<VisualElement> createDataIDField = () => {
            //    var signalController = new SignalController();
            //    system.RoadNetwork.SignalControllers.Add(signalController);

            //    var dataID = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate().Q<Vector3Field>();
            //    dataID.label = "";
            //    dataID.value = Vector3.zero;
            //    return dataID;
            //};
            //CreateParamterBox(assets, root, "信号制御器", createDataIDField);

            //Func<VisualElement> createDataIDField2 = () => {
            //    var signalLight = new SignalLight();
            //    system.RoadNetwork.SignalLihgts.Add(signalLight);

            //    var dataID = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate().Q<Vector3Field>();
            //    dataID.label = "";
            //    dataID.value = Vector3.zero;
            //    return dataID;
            //};
            //CreateParamterBox(assets, root, "信号機", createDataIDField2);

            //Func<VisualElement> createDataIDField3 = () => {
            //    var dataID = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate().Q<Vector3Field>();
            //    dataID.label = "";
            //    dataID.value = Vector3.zero;
            //    return dataID;
            //};
            //CreateParamterBox(assets, root, "停止線", createDataIDField3);

            //Func<VisualElement> createDataIDField4 = () => {
            //    var dataID = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate().Q<Vector3Field>();
            //    dataID.label = "";
            //    dataID.value = Vector3.zero;
            //    return dataID;
            //};
            //CreateParamterBox(assets, root, "一時停止標識", createDataIDField4);

            //Func<VisualElement> createDataIDField5 = () => {
            //    var dataID = assets.GetAsset(RoadNetworkEditorAssets.DataIDFieldAssetName).Instantiate().Q<IntegerField>();
            //    dataID.label = "";
            //    dataID.value = 0;
            //    return dataID;
            //};
            //CreateParamterBox(assets, root, "優先道路", createDataIDField5);
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
            ParamterLayoutSet[mode](system, assets, parameterView);
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
                        var trafficController = new TrafficSignalLightController("SignalController" + node.MyId, node, node.GetCenterPoint());
                        node.SignalController = (trafficController);
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
