using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityImport.Config.PackageLoadConfigs;
using PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables.Components;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables
{
    /// <summary>
    /// インポートのパッケージごとの設定GUIのうち、一括設定の部分です。
    /// 加えて、各パッケージで一括設定をオーバーライドするGUI <see cref="PackageLoadConfigOverrideGUI"/>でも利用します。
    /// </summary>
    internal class PackageLoadConfigExtendableGUI
    {
        private readonly List<ExtendableConfigGUIBase> guis;
        public PackageLoadConfigExtendable Conf { get; }

        public PackageLoadConfigExtendableGUI(PackageLoadConfigExtendable conf)
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