using PLATEAU.CityMeta;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Interop;
using PLATEAU.IO;
using UnityEditor;

namespace PLATEAU.Editor.CityImportOLD
{
    /// <summary>
    /// 基本メッシュ設定のGUIを提供します。
    /// 
    /// 設定内容:
    /// テクスチャを含めるか / メッシュ粒度 / ログレベル
    /// </summary>
    internal static class BasicMeshConfigGUI
    {
        public static void Draw(CityImportConfig importConfig)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                importConfig.exportAppearance = EditorGUILayout.Toggle("テクスチャを含める", importConfig.exportAppearance);
                importConfig.meshGranularity = (MeshGranularity)EditorGUILayout.Popup("メッシュ結合単位", (int)importConfig.meshGranularity,
                    new[] { "最小地物単位", "主要地物単位", "都市モデル地域単位" });
                importConfig.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", importConfig.logLevel);
            }
        }
    }
}