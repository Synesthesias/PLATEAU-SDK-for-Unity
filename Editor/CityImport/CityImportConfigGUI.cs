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
                packageConfigGUIList,
                new PositionConfGUI(cityImportConf)
            };
        }

        /// <summary>
        /// <see cref="CityImportConfig"/> を設定するGUIを描画します。
        /// </summary>
        public void Draw()
        {
            // 動的タイルの設定GUIを最初に描画
            guiComponents[0].Draw();
            
            PlateauEditorStyle.Heading("地物別設定", "num3.png");

            foreach (var guiComponent in guiComponents)
            {
                if (guiComponent is DynamicTileConfigGUI)
                {
                    // すでに描画済みなのでスキップ
                    continue;
                }
                guiComponent.Draw();
            }
        }

        
    }
}
