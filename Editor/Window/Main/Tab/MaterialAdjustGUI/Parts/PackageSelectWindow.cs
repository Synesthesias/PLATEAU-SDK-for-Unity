using PLATEAU.Editor.Window.Common;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// 都市モデルに含まれるパッケージ種を選択するウィンドウです。
    /// </summary>
    internal class PackageSelectWindow : PlateauWindowBase
    {
        private IPackageSelectResultReceiver resultReceiver;
        public static void Open(IPackageSelectResultReceiver resultReceiver)
        {
            var window = GetWindow<PackageSelectWindow>("パッケージ種選択");
            window.resultReceiver = resultReceiver;
            window.Show();
        }

        protected override IEditorDrawable InitGui()
        {
            return new PackageSelectGui(resultReceiver, this);
        }

    }
}