using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// 都市モデルに含まれるパッケージ種を選択するウィンドウです。
    /// </summary>
    internal class PackageSelectWindow : PlateauWindowBase
    {
        private IPackageSelectResultReceiver resultReceiver;
        private PackageSelectGui gui;
        private bool isInitialized;
        
        public static PackageSelectWindow Open()
        {
            var window = GetWindow<PackageSelectWindow>("パッケージ種選択");
            window.Show();
            return window;
        }

        public void Init(IPackageSelectResultReceiver resultReceiverArg, bool ignoreTileDataset)
        {
            this.resultReceiver = resultReceiverArg;
            gui = new PackageSelectGui(resultReceiver, this, ignoreTileDataset);
            this.isInitialized = true;
        }

        public void Init(PLATEAUInstancedCityModel data, IPackageSelectResultReceiver resultReceiverArg)
        {
            Init(resultReceiverArg, false);
            gui.SetData(data);
        }

        protected override VisualElementDisposable CreateGui()
        {
            var container = new IMGUIContainer(() =>
            {
                if (!isInitialized) return;
                PlateauEditorStyle.SetCurrentWindow(this);
                gui.Draw();
            });
            return new VisualElementDisposable(container, Dispose);
        }

        private void Dispose()
        {
            gui?.Dispose();
        }

    }
}