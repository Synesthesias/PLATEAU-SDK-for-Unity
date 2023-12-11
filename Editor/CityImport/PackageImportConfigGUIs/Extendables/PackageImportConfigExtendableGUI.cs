using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables
{
    /// <summary>
    /// インポートのパッケージごとの設定GUIのうち、一括設定の部分です。
    /// 加えて、各パッケージで一括設定をオーバーライドするGUI <see cref="PackageImportConfigOverrideGUI"/>でも利用します。
    /// </summary>
    internal class PackageImportConfigExtendableGUI
    {
        private readonly List<ExtendableConfigGUIBase> guis;
        public PackageImportConfigExtendable Conf { get; }

        public PackageImportConfigExtendableGUI(PackageImportConfigExtendable conf)
        {
            Conf = conf;
            guis = new List<ExtendableConfigGUIBase>
            {
                // ここに一括設定の設定項目を列挙します。
                new TextureIncludeGUI(conf),
                new MeshColliderSetGUI(conf),
                new MeshGranularityGUI(conf),
                new SetAttrInfoGUI(conf)
            };
        }

        public void Draw()
        {
            foreach (var gui in guis)
            {
                gui.Draw();
            }
        }
        
        public T GetGUIByType<T>() where T : ExtendableConfigGUIBase
        {
            return guis.OfType<T>().First();
        }
    }
}