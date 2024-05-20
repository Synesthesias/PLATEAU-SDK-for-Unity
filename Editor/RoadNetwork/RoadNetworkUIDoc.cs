using NUnit.Framework;
using PLATEAU.RoadNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

using GenerateParameterFunc = System.Action<PLATEAU.Editor.RoadNetwork.RoadNetworkEditorAssets, UnityEngine.UIElements.VisualElement>;


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
    /// 道路ネットワーク手動編集機能を構成するアセットを管理するクラス
    /// </summary>
    public class RoadNetworkEditorAssets
    {
        /// <summary>
        /// 
        /// </summary>
        public RoadNetworkEditorAssets()
        {
            LoadAsset();
        }

        private static readonly string UIAssetDirPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/PlateauUIDocument/RoadNetwork/";

        // エディタ
        public static readonly string EditorAssetName = "RoadNetworkEditor.uxml";

        // 各パラメータ
        public static readonly string DataIDFieldAssetName = "RoadNetworkDataIDFiled.uxml";
        public static readonly string FilePathFieldAssetName = "RoadNetworkFilePathField.uxml";
        public static readonly string IntFieldAssetName = "RoadNetworkIntField.uxml";
        public static readonly string NameFieldAssetName = "RoadNetworkNameField.uxml";
        public static readonly string StatusMultiSelectorAssetName = "RoadNetworkStatusMultiSelector.uxml";
        public static readonly string StatusSelectorAssetName = "RoadNetworkStatusSelector.uxml";
        public static readonly string ToggleBoxAssetName = "RoadNetworkToggleBox.uxml";
        public static readonly string Vec3FieldAssetName = "RoadNetworkVec3Field.uxml";
        public static readonly string FloatFieldAssetName = "RoadNetworkFloatField.uxml";

        // 特殊パラメータ
        public static readonly string ParameterBoxAssetName = "RoadNetworkParameterBox.uxml";


        // ロードしたアセット
        private Dictionary<string, VisualTreeAsset> visualTreeAssets;

        /// <summary>
        /// アセットの取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VisualTreeAsset GetAsset(string name)
        {
            return visualTreeAssets[name];
        }

        /// <summary>
        /// アセットが読み込まれているか
        /// </summary>
        /// <returns></returns>
        private bool IsLoaded()
        {
            return visualTreeAssets?.Count > 0;
        }

        /// <summary>
        /// アセットを読み込む
        /// </summary>
        private void LoadAsset()
        {
            if (IsLoaded())
                return;

            visualTreeAssets = new Dictionary<string, VisualTreeAsset>
            {
                { EditorAssetName, LoadAsset(EditorAssetName) },
                { DataIDFieldAssetName, LoadAsset(DataIDFieldAssetName) },
                { FilePathFieldAssetName, LoadAsset(FilePathFieldAssetName) },
                { IntFieldAssetName, LoadAsset(IntFieldAssetName) },
                { NameFieldAssetName, LoadAsset(NameFieldAssetName) },
                { StatusMultiSelectorAssetName, LoadAsset(StatusMultiSelectorAssetName) },
                { StatusSelectorAssetName, LoadAsset(StatusSelectorAssetName) },
                { ToggleBoxAssetName, LoadAsset(ToggleBoxAssetName) },
                { Vec3FieldAssetName, LoadAsset(Vec3FieldAssetName) },
                { FloatFieldAssetName, LoadAsset(FloatFieldAssetName) },
                { ParameterBoxAssetName, LoadAsset(ParameterBoxAssetName) },
            };
        }


        private VisualTreeAsset LoadAsset(string fileName)
        {
            string assetPath = UIAssetDirPath + fileName;
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
        }
    }

    /// <summary>
    /// 道路ネットワークの編集モード
    /// </summary>
    public enum RoadNetworkEditMode
    {
        DebugNone,
        DebugNode,
        DebugLink,
        DebugLane,
        DebugBlock,
        DebugTrack,
        EditLaneShape,
        EditLaneStructure,
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

        private EnumField modeSelector;
        private ScrollView parameterView;

        private readonly IRoadNetworkEditingSystem system;
        private readonly RoadNetworkEditorAssets assets;

        private readonly Dictionary<RoadNetworkEditMode, GenerateParameterFunc> ParamterLayoutSet =
            new Dictionary<RoadNetworkEditMode, GenerateParameterFunc> {
            { RoadNetworkEditMode.DebugNone,
                    (RoadNetworkEditorAssets assets, VisualElement root)=>{

                    }
                } ,
            { RoadNetworkEditMode.DebugNode, CreateDebugNodeLayout },
            { RoadNetworkEditMode.DebugLink, CreateDebugLinkLayout },
            { RoadNetworkEditMode.DebugLane, CreateDebugLaneLayout },
            { RoadNetworkEditMode.DebugBlock, CreateDebugBlockLayout },
            { RoadNetworkEditMode.DebugTrack, CreateDebugTrackLayout },
            { RoadNetworkEditMode.EditLaneShape, CreateEditLaneShapeLayout },
            { RoadNetworkEditMode.EditLaneStructure, CreateEditLaneStructure },

        };

        private static void CreateDebugNodeLayout(RoadNetworkEditorAssets assets, VisualElement root)
        {
            var element = assets.GetAsset(RoadNetworkEditorAssets.Vec3FieldAssetName).Instantiate();
             element.Q<Vector3Field>().label = "座標";
            root.Add(element);
        }

        private static void CreateDebugLinkLayout(RoadNetworkEditorAssets assets, VisualElement root)
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

        private static void CreateDebugLaneLayout(RoadNetworkEditorAssets assets, VisualElement root)
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

        private static void CreateDebugBlockLayout(RoadNetworkEditorAssets assets, VisualElement root)
        {
        }

        private static void CreateDebugTrackLayout(RoadNetworkEditorAssets assets, VisualElement root)
        {
        }

        private static void CreateEditLaneShapeLayout(RoadNetworkEditorAssets assets, VisualElement root)
        {
            
        }

        private static void CreateEditLaneStructure(RoadNetworkEditorAssets assets, VisualElement root)
        {

        }

        public RoadNetworkUIDoc(IRoadNetworkEditingSystem system, VisualElement editorRoot, in RoadNetworkEditorAssets assets) 
        {
            // 正当性チェック 最低限の初期化
            Assert.IsNotNull(system);
            Assert.IsNotNull(editorRoot); 
            Assert.IsNotNull(assets);

            this.system = system;
            this.assets = assets;

            modeSelector = editorRoot.Q<EnumField>("ModeSelector");
            Assert.IsNotNull(modeSelector);

            parameterView = editorRoot.Q<ScrollView>("ParameterView");
            Assert.IsNotNull(parameterView);

        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            //　memo 自分でモードを変えた時と外部から変更された時のルートを用意する

            system.OnChangeEditMode += UpdateMode;
            modeSelector.RegisterCallback<ChangeEvent<Enum>>((evt) =>
            {
                var mode = (RoadNetworkEditMode)evt.newValue;
                CurrentMode = (RoadNetworkEditMode)modeSelector.value;
            });
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
            ParamterLayoutSet[mode](assets, parameterView);
        }
    }

}
