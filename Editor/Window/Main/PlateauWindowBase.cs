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

        private void CreateGUI()
        {
            var scrollView = new ScrollView
            {
                viewDataKey = "plateau-window-scroll-view",
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto,  // 垂直スクロールを表示
                style =
                {
                    flexGrow = 1 // 利用可能な空間いっぱいに広がる
                }
            };

            scrollView.Add(new IMGUIContainer(DrawImgui));
            gui = CreateGui();
            scrollView.Add(gui.VisualElement);
            rootVisualElement.Add(scrollView);
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

    /// <summary>
    /// スクロール機能を持たないPLATEAU SDKのEditorWindowが共通で備えているべき機能をまとめたものです。
    /// </summary>
    internal abstract class PlateauWindowBaseNoScroll : EditorWindow
    {
        private VisualElementDisposable gui;

        /// <summary> GUIの中身の生成はサブクラスに任せます。 </summary>
        protected abstract VisualElementDisposable CreateGui();

        private void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(DrawImgui));
            gui = CreateGui();
            rootVisualElement.Add(gui.VisualElement);
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