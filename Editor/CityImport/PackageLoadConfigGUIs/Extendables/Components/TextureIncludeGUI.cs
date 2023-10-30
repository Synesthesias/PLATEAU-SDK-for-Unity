using PLATEAU.CityImport.Config.PackageLoadConfigs;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables.Components
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
        public TextureIncludeGUI(PackageLoadConfigExtendable conf) : base(conf)
        {
        }
        
        public override void Draw()
        {
            if (!MayTextureExist) return;
            Conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", Conf.IncludeTexture);

            if (!Conf.IncludeTexture) return;
            Conf.EnableTexturePacking = EditorGUILayout.Toggle("テクスチャを結合する", Conf.EnableTexturePacking);
            if (!Conf.EnableTexturePacking) return;
            Conf.TexturePackingResolution = (TexturePackingResolution)EditorGUILayout.Popup("テクスチャ解像度",
                (int)Conf.TexturePackingResolution, new[] { "2048x2048", "4096x4096", "8192x8192" });
        }
    }
}