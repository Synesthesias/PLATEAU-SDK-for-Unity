using PLATEAU.CityImport.Config;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs;
using PLATEAU.Editor.Window.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components
{
    /// <summary>
    /// 動的タイルの設定GUIクラス
    /// </summary>
    internal class DynamicTileConfigGUI : IEditorDrawable
    {
        private readonly CityImportConfig conf;
        private readonly PackageImportConfigGUIList packageConfigGUIList;
        
        public DynamicTileConfigGUI(CityImportConfig conf) : this(conf, null)
        {
        }
        
        public DynamicTileConfigGUI(CityImportConfig conf, PackageImportConfigGUIList packageConfigGUIList)
        {
            this.conf = conf;
            this.packageConfigGUIList = packageConfigGUIList;
            // 初期化時に現在の設定に基づいてForcePerCityModelAreaを設定
            UpdateForcePerCityModelAreaForAllPackages(conf.DynamicTileImportConfig.ImportTypeIndex == 1);
        }
            
        public void Draw()
        {
            var config = conf.DynamicTileImportConfig;

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                // インポート形式選択
                string[] importTypes = { "シーンに配置", "動的タイル（Addressable出力）" };
                int prevIndex = config.ImportTypeIndex;
                config.ImportTypeIndex = EditorGUILayout.Popup("インポート形式選択", config.ImportTypeIndex, importTypes);

                // インポート形式が変更された場合の処理
                if (config.ImportTypeIndex != prevIndex)
                {
                    UpdateForcePerCityModelAreaForAllPackages(config.ImportTypeIndex == 1);
                    
                    // 「シーンに配置」に切り替えた瞬間に値をリセット
                    if (config.ImportTypeIndex == 0)
                    {
                        config.OutputPath = string.Empty;
                        config.Lod1Texture = false;
                    }
                }

                // 「動的タイル（Addressable出力）」選択時のみ、続きのUIを表示
                if (config.ImportTypeIndex == 1)
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        // 出力先選択
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("出力先", GUILayout.Width(50));
                        config.OutputPath = EditorGUILayout.TextField(config.OutputPath ?? "");
                        if (GUILayout.Button("参照...", GUILayout.Width(60)))
                        {
                            string selected = EditorUtility.OpenFolderPanel("出力先フォルダを選択", "", "");
                            if (!string.IsNullOrEmpty(selected))
                            {
                                config.OutputPath = selected;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(5);

                        // infoメッセージボックス
                        EditorGUILayout.HelpBox(
                            "[info] Assetsフォルダ内ならビルドに含めます。Assetsフォルダ外ならビルドに含めません",
                            MessageType.Info);

                        GUILayout.Space(5);

                        // LOD1の建物にテクスチャを貼る
                        config.Lod1Texture = EditorGUILayout.Toggle("LOD1の建物にテクスチャを貼る", config.Lod1Texture);

                        GUILayout.Space(5);

                        // 警告表示
                        if (string.IsNullOrEmpty(config.OutputPath))
                        {
                            EditorGUILayout.HelpBox("出力先を選択してください。", MessageType.Warning);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 全てのパッケージ設定のForcePerCityModelAreaプロパティを更新します。
        /// 動的タイル（Addressable出力）が選択された場合、MeshGranularityを「地域単位」に固定します。
        /// </summary>
        /// <param name="forceToCityModelArea">地域単位に固定するかどうか</param>
        private void UpdateForcePerCityModelAreaForAllPackages(bool forceToCityModelArea)
        {
            // 個別のパッケージ設定を更新
            foreach (var packagePair in conf.PackageImportConfigDict.ForEachPackagePair)
            {
                packagePair.Value.ForcePerCityModelArea = forceToCityModelArea;
            }
            
            // 一括設定（マスター設定）も更新
            if (packageConfigGUIList != null)
            {
                packageConfigGUIList.MasterConf.ForcePerCityModelArea = forceToCityModelArea;
            }
        }

        public void Dispose() { }
    }
}