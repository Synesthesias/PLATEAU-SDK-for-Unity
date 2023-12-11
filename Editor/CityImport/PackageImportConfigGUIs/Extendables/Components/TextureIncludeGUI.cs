using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components
{
    /// <summary>
    /// テクスチャに関する設定GUIです。
    /// </summary>
    internal class TextureIncludeGUI : ExtendableConfigGUIBase
    {
        /// <summary>
        /// 仕様上、テクスチャが存在する可能性があるか（パッケージ種によってはない）
        /// </summary>
        public bool MayTextureExist { get; set; } = true;
        public TextureIncludeGUI(PackageImportConfigExtendable conf) : base(conf)
        {
        }
        
        public override void Draw()
        {
            if (!MayTextureExist) return;
            Conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", Conf.IncludeTexture);

            if (!Conf.IncludeTexture) return;
            
            Conf.EnableTexturePacking = EditorGUILayout.Toggle("テクスチャを結合する", Conf.EnableTexturePacking);
            if (!Conf.EnableTexturePacking) return;

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.HelpBox("現在、テクスチャ結合後に、別途PLATEAU-SDK-Toolkits-for-Unityの自動テクスチャ機能を使った場合、意図しない結果になる場合があります。", MessageType.Info);
            }
            
            Conf.TexturePackingResolution = (TexturePackingResolution)EditorGUILayout.Popup("テクスチャ解像度",
                (int)Conf.TexturePackingResolution, new[] { "2048x2048", "4096x4096", "8192x8192" });
        }
    }
}