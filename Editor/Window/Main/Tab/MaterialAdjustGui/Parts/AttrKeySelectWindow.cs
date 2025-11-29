using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// 属性情報キーを1つ、選択肢からユーザーに選択させるウィンドウです。
    /// </summary>
    internal class AttrKeySelectWindow : PlateauWindowBase
    {
        private IAttrKeySelectResultReceiver resultReceiver;
        private UniqueParentTransformList targets;
        private EditorWindow mainWindow;
        private AttrKeySelectGui gui;
        private bool isInitialized;

        public static AttrKeySelectWindow Open()
        {
            var window = GetWindow<AttrKeySelectWindow>("属性情報キー選択");
            
            window.Show();
            return window;
        }

        public void Init(IAttrKeySelectResultReceiver resultReceiverArg, UniqueParentTransformList targetsArg,
            EditorWindow parentWindowArg)
        {
            resultReceiver = resultReceiverArg;
            targets = targetsArg;
            mainWindow = parentWindowArg;
            gui = new AttrKeySelectGui(targets, resultReceiver, mainWindow, this);
            isInitialized = true;
        }

        protected override VisualElementDisposable CreateGui()
        {
            var container = new IMGUIContainer(() =>
            {
                if(!isInitialized) return;
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

    internal interface IAttrKeySelectResultReceiver
    {
        void ReceiveAttrKeySelectResult(string selectedAttrKey);
    }
}