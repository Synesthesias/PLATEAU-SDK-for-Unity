using PLATEAU.CityImport.Config.PackageLoadConfigs;
using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs
{
    /// <summary>
    /// パッケージごとの設定GUIについて、各設定項目ごとのGUIをコンポーネントと呼ぶことにします。
    /// 例えば建物のテクスチャに関する設定コンポーネント、道路のメッシュ結合粒度に関する設定コンポーネント、といったものです。
    /// これら各コンポーネントは、 PackageLoadSetting を保持し、 Draw() で設定GUIを描画し、
    /// その中でユーザーのGUI操作に応じて設定値を変更します。
    /// そのコンポーネントという概念を抽象化したものがこのクラスです。
    /// </summary>
    internal abstract class PackageLoadConfigGUIComponent : IEditorDrawable
    {
        protected readonly PackageLoadConfig Conf;

        protected PackageLoadConfigGUIComponent(PackageLoadConfig conf)
        {
            Conf = conf;
        }
        public abstract void Draw();
        public void Dispose() { }
    }
}