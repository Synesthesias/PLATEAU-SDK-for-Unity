using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAU SDKのEditorWindowが共通で備えているべき機能をまとめたものです。
    /// これを継承すれば、上下スクロール可能であることと廃棄可能であることが保証されます。
    /// 中身であるGUIの生成はサブクラスに任せます。
    /// </summary>
    internal abstract class PlateauWindowBase : EditorWindow
    {
        private readonly ScrollView scrollView = new();
        private IEditorDrawable gui;
        private IEditorDrawable footerGui;
        
        /// <summary> GUIの中身の生成はサブクラスに任せます。 </summary>
        protected abstract IEditorDrawable InitGui();
        protected virtual IEditorDrawable InitFooterGui() => null;

        private void OnGUI()
        {
            gui ??= InitGui();
            PlateauEditorStyle.SetCurrentWindow(this);
            scrollView.Draw(
                gui.Draw
            );
            footerGui ??= InitFooterGui();
            footerGui?.Draw();
        }

        private void OnDestroy()
        {
            gui.Dispose();
        }
    }
}