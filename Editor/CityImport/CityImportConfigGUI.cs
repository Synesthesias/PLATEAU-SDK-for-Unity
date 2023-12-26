using System;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components;
using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityImportConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityImportConfigGUI
    {
        private readonly IEditorDrawable[] guiComponents;

        public CityImportConfigGUI(CityImportConfig cityImportConf, PackageToLodDict availablePackageLodsArg)
        {
            if(cityImportConf == null) throw new ArgumentNullException(nameof(cityImportConf));
            // パッケージ種ごとの設定GUI、その下に基準座標設定GUIが表示されるようにGUIコンポーネントを置きます。
            guiComponents = new IEditorDrawable[]
            {
                new PackageImportConfigGUIList(availablePackageLodsArg, cityImportConf),
                new PositionConfGUI(cityImportConf)
            };
        }

        /// <summary>
        /// <see cref="CityImportConfig"/> を設定するGUIを描画します。
        /// </summary>
        public void Draw()
        {
            foreach (var guiComponent in guiComponents)
            {
                guiComponent.Draw();
            }
        }

        
    }
}
