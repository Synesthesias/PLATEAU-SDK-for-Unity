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
        
        public DynamicTileConfigGUI(CityImportConfig conf)
        {
            this.conf = conf;
        }
            
        public void Draw()
        {
            var config = conf.DynamicTileImportConfig;
            bool folderButtonClicked = false;

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                // インポート形式選択
                string[] importTypes = { "シーンに配置", "動的タイル（Addressable出力）" };
                config.ImportType = (ImportType)EditorGUILayout.Popup("インポート形式", (int)config.ImportType, importTypes);

                // 「動的タイル（Addressable出力）」選択時のみ、続きのUIを表示
                if (config.ImportType == ImportType.DynamicTile)
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        // 出力先選択
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("出力先", GUILayout.Width(50));
                        config.OutputPath = EditorGUILayout.TextField(config.OutputPath ?? "");
                        if (GUILayout.Button("参照...", GUILayout.Width(60)))
                        {
                            folderButtonClicked = true;
                        }
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(5);
                        
                        // infoメッセージボックス
                        EditorGUILayout.HelpBox(
                            "Assetsフォルダ内またはUnityプロジェクト外の任意のフォルダを選択してください。",
                            MessageType.Info);
                        
                        // 警告表示
                        if (string.IsNullOrEmpty(config.OutputPath))
                        {
                            EditorGUILayout.HelpBox("出力先を選択してください。", MessageType.Warning);
                        }

                        GUILayout.Space(5);

                        // LOD1の建物にテクスチャを貼るかのチェックです。
                        // ここは2025年アルファ版では利用しないのでいったんコメントアウトします。その次のバージョンで利用します。
                        // config.Lod1Texture = EditorGUILayout.Toggle("LOD1の建物にテクスチャを貼る", config.Lod1Texture);

                        GUILayout.Space(5);

                        
                    }
                    
                }
            }
            
            // タイル出力先のフォルダ選択パネルを表示します。これはVerticalScopeの外で行わないとエラーログが出ます。
            if (folderButtonClicked)
            {
                string selected = EditorUtility.OpenFolderPanel("出力先フォルダを選択", "Assets", "");
                GUI.FocusControl(null);
                if (!string.IsNullOrEmpty(selected))
                {
                    config.OutputPath = selected.Replace('\\', '/');
                }
            }
        }

        public void Dispose() { }
    }
}