using Codice.Client.BaseCommands;
using Codice.Client.Common.FsNodeReaders;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

using GenerateParameterFunc = System.Action<PLATEAU.Editor.RoadNetwork.RoadNetworkEditorAssets, UnityEngine.UIElements.VisualElement>;


namespace PLATEAU.Editor.RoadNetwork
{
    public class RoadNetworkEditorWindow : UnityEditor.EditorWindow
    {
        RoadNetworkEditor editor;
        RoadNetworkEditorAssets assets;
        private static readonly string WindowName = "PLATEAU RoadNetwork Editor";



        [MenuItem("PLATEAU/PLATEAU RoadNetwork Editor")]
        public static void ShowWindow()
        {
            GetWindow<RoadNetworkEditorWindow>(WindowName);
        }


        private void OnEnable()
        {         
            var window = GetWindow<RoadNetworkEditorWindow>(WindowName);
            if (window.editor == null)
            {
                assets = new RoadNetworkEditorAssets();

                var visualTree = assets.GetAsset(RoadNetworkEditorAssets.EditorAssetName);
                var root = rootVisualElement;
                visualTree.CloneTree(root);
                

                window.editor = new RoadNetworkEditor(root, assets);
                window.editor.Initialize();


            }
        }
    }

    public class RoadNetworkEditorAssets
    {
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
        /// アセットの取得‘
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VisualTreeAsset GetAsset(string name)
        {
            return visualTreeAssets[name];
        }

        private bool IsLoaded()
        {
            return visualTreeAssets?.Count > 0;
        }

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
    }

    /// <summary>
    /// 道路ネットワークエディタのUIDocumentを扱うクラス
    /// インスペクター、エディタ、ランタイムでも利用できるように汎用化する
    /// (エディタ専用のUIを使用してるため現状はランタイム使用不可)
    /// </summary>
    public class RoadNetworkEditor
    {
        private EnumField modeSelector;
        private ScrollView parameterView;

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

        public RoadNetworkEditor(VisualElement editorRoot, in RoadNetworkEditorAssets assets) 
        {
            // 正当性チェック 最低限の初期化
            Assert.IsNotNull(editorRoot); 
            Assert.IsNotNull(assets);
            Debug.Log(editorRoot.name);
            
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
            modeSelector.RegisterCallback<ChangeEvent<Enum>>((evt) =>
            {
                var mode = (RoadNetworkEditMode)evt.newValue;
                ChangeMode(mode);
            });

            ChangeMode((RoadNetworkEditMode)modeSelector.value);

        }

        /// <summary>
        /// 終了
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// モードの変更
        /// </summary>
        /// <param name="mode"></param>
        public void ChangeMode(RoadNetworkEditMode mode)
        {
            parameterView.Clear();
            ParamterLayoutSet[mode](assets, parameterView);
        }

    }
}
