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

        // 専用パネル
        public static readonly string TrafficRegulationAssetName = "RoadNetworkTrafficRegulationPanel.uxml";


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
                { TrafficRegulationAssetName, LoadAsset(TrafficRegulationAssetName) },
            };
        }


        private VisualTreeAsset LoadAsset(string fileName)
        {
            string assetPath = UIAssetDirPath + fileName;
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
        }
    }

}
