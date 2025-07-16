using System;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components;
using PLATEAU.Editor.Window.Common;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityImportConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityImportConfigGUI
    {
        private readonly IEditorDrawable[] guiComponents;
        private readonly PackageImportConfigGUIList packageConfigGUIList;

        public CityImportConfigGUI(CityImportConfig cityImportConf, PackageToLodDict availablePackageLodsArg)
        {
            if(cityImportConf == null) throw new ArgumentNullException(nameof(cityImportConf));
            
            // パッケージ設定GUIListを保持
            packageConfigGUIList = new PackageImportConfigGUIList(availablePackageLodsArg, cityImportConf);
            
            // パッケージ種ごとの設定GUI、その下に基準座標設定GUIが表示されるようにGUIコンポーネントを置きます。
            guiComponents = new IEditorDrawable[]
            {
                new DynamicTileConfigGUI(cityImportConf, packageConfigGUIList),
                new HeaderElementGroup("", "地物別設定", HeaderType.HeaderNum3),
                packageConfigGUIList,
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
