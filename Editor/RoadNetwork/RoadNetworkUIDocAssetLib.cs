using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{

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
        public static readonly string DataIDField = "RoadNetworkDataIDFiled.uxml";
        public static readonly string FilePathField = "RoadNetworkFilePathField.uxml";
        public static readonly string IntField = "RoadNetworkIntField.uxml";
        public static readonly string NameField = "RoadNetworkNameField.uxml";
        public static readonly string StatusMultiSelector = "RoadNetworkStatusMultiSelector.uxml";
        public static readonly string StatusSelector = "RoadNetworkStatusSelector.uxml";
        public static readonly string ToggleBox = "RoadNetworkToggleBox.uxml";
        public static readonly string Vec3Field = "RoadNetworkVec3Field.uxml";
        public static readonly string FloatFieldAsset = "RoadNetworkFloatField.uxml";
        public static readonly string RadioButton = "RoadNetworkRadioButton.uxml";

        // 特殊パラメータ
        public static readonly string ParameterBoxAssetName = "RoadNetworkParameterBox.uxml";

        // 専用パネル
        public static readonly string TrafficRegulationPanel = "RoadNetworkTrafficRegulationPanel.uxml";
        public static readonly string RoadNetworkPatternPanel = "RoadNetworkPatternPanel.uxml";
        public static readonly string RoadNetworkTrafficLightPatternPhasePanel = "RoadNetworkTrafficLightPatternPhasePanel.uxml";


        // ロードしたアセット
        private Dictionary<string, VisualTreeAsset> visualTreeAssets;

        /// <summary>
        /// アセットの取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VisualTreeAsset GetAsset(string name)
        {
            if (visualTreeAssets.ContainsKey(name) == false)
            {
                var asset = LoadAsset(name);
                if (asset != null)
                {
                    visualTreeAssets.Add(name, asset);
                }
            }

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
        /// 初期化時に読み込むアセット
        /// </summary>
        private void LoadAsset()
        {
            if (IsLoaded())
                return;

            visualTreeAssets = new Dictionary<string, VisualTreeAsset>
            {
                { EditorAssetName, LoadAsset(EditorAssetName) },
                { DataIDField, LoadAsset(DataIDField) },
                { FilePathField, LoadAsset(FilePathField) },
                { IntField, LoadAsset(IntField) },
                { NameField, LoadAsset(NameField) },
                { StatusMultiSelector, LoadAsset(StatusMultiSelector) },
                { StatusSelector, LoadAsset(StatusSelector) },
                { ToggleBox, LoadAsset(ToggleBox) },
                { Vec3Field, LoadAsset(Vec3Field) },
                { FloatFieldAsset, LoadAsset(FloatFieldAsset) },
                { ParameterBoxAssetName, LoadAsset(ParameterBoxAssetName) },
                { TrafficRegulationPanel, LoadAsset(TrafficRegulationPanel) },
            };
        }


        private VisualTreeAsset LoadAsset(string fileName)
        {
            string assetPath = UIAssetDirPath + fileName;
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
        }
    }

}
