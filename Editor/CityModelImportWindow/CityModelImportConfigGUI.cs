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
            this.config.LogLevel = (DllLogLevel)EditorGUILayout.EnumPopup("ログの詳細度", this.config.LogLevel);
            return this.config;
        }
    }
}