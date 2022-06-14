using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// <see cref="CityModelImportWindow"/> の一部分で、変換の設定をするGUIを提供します。
    /// </summary>
    public class CityModelImportConfigGUI
    {
        public CityModelImportConfig Config { get; set; } = new CityModelImportConfig();

        public CityModelImportConfig Draw()
        {
            this.Config.optimizeFlag = EditorGUILayout.Toggle("最適化", Config.optimizeFlag);
            this.Config.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", Config.meshGranularity);
            this.Config.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", this.Config.logLevel);
            return this.Config;
        }
    }
}