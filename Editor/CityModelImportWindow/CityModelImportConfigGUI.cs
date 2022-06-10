using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// <see cref="CityModelImportWindow"/> の一部分で、変換の設定をするGUIを提供します。
    /// </summary>
    public class CityModelImportConfigGUI
    {
        private CityModelImportConfig config = new CityModelImportConfig();

        public CityModelImportConfig Draw()
        {
            HeaderDrawer.Draw("変換設定");
            this.config.OptimizeFlg = EditorGUILayout.Toggle("最適化", this.config.OptimizeFlg);
            this.config.MeshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", this.config.MeshGranularity);
            this.config.LogLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", this.config.LogLevel);
            return this.config;
        }
    }
}