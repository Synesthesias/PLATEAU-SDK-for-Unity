using PLATEAU.CityImport.Config.PackageImportConfigs;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables
{
    /// <summary>
    /// 一括設定の中にある各項目を抽象化したものです。
    /// <see cref="Draw"/>で設定GUIを描画します。
    /// </summary>
    internal abstract class ExtendableConfigGUIBase
    {
        protected readonly PackageImportConfigExtendable Conf;

        protected ExtendableConfigGUIBase(PackageImportConfigExtendable conf)
        {
            Conf = conf;
        }
        
        public abstract void Draw();
    }
}