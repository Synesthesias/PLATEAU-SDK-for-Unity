using System;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageLoadConfigs;
using PLATEAU.Editor.CityImport.PackageLoadConfigGUIs;
using PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.GUIParts;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityLoadConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityLoadConfigGUI
    {
        private readonly IEditorDrawable[] guiComponents;

        public CityLoadConfigGUI(CityLoadConfig cityLoadConf, PackageToLodDict availablePackageLodsArg)
        {
            if(cityLoadConf == null) throw new ArgumentNullException(nameof(cityLoadConf));
            // パッケージ種ごとの設定GUI、その下に基準座標設定GUIが表示されるようにGUIコンポーネントを置きます。
            guiComponents = new IEditorDrawable[]
            {
                new PackageLoadConfigGUIList(availablePackageLodsArg, cityLoadConf),
                new PositionConfGUI(cityLoadConf)
            };
        }

        /// <summary>
        /// <see cref="CityLoadConfig"/> を設定するGUIを描画します。
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
