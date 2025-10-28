using PLATEAU.Editor.Window.Common;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using ScrollView = UnityEngine.UIElements.ScrollView;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAU SDKのEditorWindowが共通で備えているべき機能をまとめたものです。
    /// これを継承すれば、上下スクロール可能であることと廃棄可能であることが保証されます。
    /// 中身であるGUIの生成はサブクラスに任せます。
    /// </summary>
    internal abstract class PlateauWindowBase : EditorWindow
    {
        private VisualElementDisposable gui;
        
        /// <summary> GUIの中身の生成はサブクラスに任せます。 </summary>
        protected abstract VisualElementDisposable CreateGui();

        /// <summary> スクロールビューを使うかどうか。サブクラスでオーバーライド可能。 </summary>
        protected virtual bool UseScrollView => true;

        private void CreateGUI()
        {
            var container = UseScrollView ? new ScrollView
            {
                viewDataKey = "plateau-window-scroll-view",
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
                style = { flexGrow = 1 }
            }
            : new VisualElement { style = { flexGrow = 1 } };

            container.Add(new IMGUIContainer(DrawImgui));
            gui = CreateGui();
            container.Add(gui.VisualElement);
            rootVisualElement.Add(container);
            rootVisualElement.Add(new PlateauWindowFooterGui().CreateGui());
        }

        private void DrawImgui()
        {
            PlateauEditorStyle.SetCurrentWindow(this);
        }

        private void OnDestroy()
        {
            gui?.Dispose();
        }
        
    }

    internal class VisualElementDisposable
    {
        public VisualElement VisualElement { get; private set; }
        private readonly Action onDispose;
        
        public VisualElementDisposable(VisualElement visualElement, Action onDispose)
        {
            VisualElement = visualElement;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose();
        }
    }
}