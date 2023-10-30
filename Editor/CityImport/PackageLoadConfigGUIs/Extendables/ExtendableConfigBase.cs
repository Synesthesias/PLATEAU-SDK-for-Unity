using PLATEAU.CityImport.Config.PackageLoadConfigs;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables
{
    /// <summary>
    /// 一括設定の中にある各項目を抽象化したものです。
    /// <see cref="Draw"/>で設定GUIを描画します。
    /// </summary>
    internal abstract class ExtendableConfigGUIBase
    {
        protected readonly PackageLoadConfigExtendable Conf;

        protected ExtendableConfigGUIBase(PackageLoadConfigExtendable conf)
        {
            Conf = conf;
        }
        
        public abstract void Draw();
    }
}