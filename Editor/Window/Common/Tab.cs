using PLATEAU.Editor.Window.Main;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Common
{
    
    /// <summary>
    /// タブから1つ選び、そのタブのGUIを描画するクラスです。
    /// 入れ子を可能にするため、自身も<see cref="ITabContent"/>となっています。
    /// </summary>
    internal abstract class TabBase : ITabContent
    {
        private int tabIndex;
        protected readonly TabElement[] tabElements;
        private VisualElement[] tabContents;
        
        
        protected int TabIndex
        {
            get => tabIndex;
            set
            {
                if (value != tabIndex)
                {
                    CurrentContent.OnTabUnselect();
                    tabIndex = value;
                    SwitchContent();
                }
            }
        }
        
        public TabBase(params TabElement[] tabElements)
        {
            this.tabElements = tabElements;
        }
        
        public VisualElement CreateGui()
        {
            tabContents = new VisualElement[tabElements.Length];
            var tabContainer = new IMGUIContainer(DrawTab);
            var contents = new VisualElement();
            for(int i=0; i<tabElements.Length; i++)
            {
                var e = tabElements[i];
                var contentVe = e.Content.CreateGui();
                contentVe = AdditionalContent(contentVe);
                contents.Add(contentVe);
                tabContents[i] = contentVe;
            }

            var ve = new VisualElement();
            ve.Add(tabContainer);
            ve.Add(contents);
            SwitchContent();
            return ve;
        }

        /// <summary>
        /// タブを描画します。
        /// </summary>
        protected abstract void DrawTab();

        /// <summary>
        /// サブクラスでタブコンテンツに変更を加えたい場合に利用します。
        /// 変更したタブコンテンツを返します。変更がなければ引数をそのまま返します。
        /// </summary>
        protected abstract VisualElement AdditionalContent(VisualElement tabContent);

        /// <summary>
        /// 選択中のタブを切り替えます。
        /// </summary>
        protected void SwitchContent()
        {
            foreach (var t in tabContents)
            {
                t.style.display = DisplayStyle.None;
            }
            tabContents[TabIndex].style.display = DisplayStyle.Flex;
        }

        protected ITabContent CurrentContent => tabElements[TabIndex].Content;

        public void Dispose()
        {
            foreach (var elem in tabElements)
            {
                elem.Dispose();
            }
        }

        public void OnTabUnselect()
        {
        }
    }

    /// <summary>
    /// テキストによるタブを描画し、その下に枠線付きのコンテンツ（選択中のタブに対応）を描画します。
    /// </summary>
    internal class TabWithFrame : TabBase
    {
        public TabWithFrame(params TabElement[] tabElements) : base(tabElements)
        {
        }

        protected override void DrawTab()
        {
            TabIndex = PlateauEditorStyle.TabsForFrame(TabIndex, tabElements.Select(te => te.Name).ToArray());
        }
        
        protected override VisualElement AdditionalContent(VisualElement tabContent)
        {
            var verticalLayout = new VisualElement();
            var style = verticalLayout.style;
            var color = Color.gray;
            const int BorderRadius = 8;
            const int BorderWidth = 2;
            style.paddingBottom = 8;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.marginLeft = 8;
            style.marginRight = 8;
            style.marginTop = 0;
            style.borderBottomWidth = BorderWidth;
            style.borderLeftWidth = BorderWidth;
            style.borderRightWidth = BorderWidth;
            style.borderTopWidth = BorderWidth;
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
            style.borderTopLeftRadius = BorderRadius;
            style.borderTopRightRadius = BorderRadius;
            style.borderBottomLeftRadius = BorderRadius;
            style.borderBottomRightRadius = BorderRadius;
            verticalLayout.Add(tabContent);
            return verticalLayout;
        }
        
    }
    
    /// <summary>
    /// タブが画像になっている版です。
    /// </summary>
    internal class TabWithImage : TabBase, IGuiCreatable
    {
        private readonly int buttonWidth;
        public TabWithImage(int buttonWidth, params TabElement[] tabElements) : base(tabElements)
        {
            this.buttonWidth = buttonWidth;
            
            // 型チェック
            if (tabElements.Any(te => te is not TabElementWithImage))
            {
                throw new Exception($"Only {nameof(TabElementWithImage)} are allowed.");
            }
        }
        
        // public VisualElement CreateGui()
        // {
        //     return new IMGUIContainer(Draw);
        // }

        protected override void DrawTab()
        {
            TabIndex = PlateauEditorStyle.TabWithImages(
                TabIndex,
                tabElements.Select(ti => ((TabElementWithImage)ti).Image).ToArray(),
                buttonWidth
            );
        }

        protected override VisualElement AdditionalContent(VisualElement tabContent)
        {
            return tabContent;
        }
    }

    internal class TabElement
    {
        public string Name { get; }
        public ITabContent Content { get; }

        public TabElement(string name, ITabContent content)
        {
            Name = name;
            Content = content;
        }
        
        public void Dispose()
        {
            Content.Dispose();
        }
    }

    /// <summary>
    /// タブ用の画像パス1つと、そのタブ選択時の描画GUIを保持するクラスです。
    /// タブを並べて選択する機能である<see cref="TabWithImage"/>の一部を構成します。
    /// </summary>
    internal class TabElementWithImage : TabElement
    {
        public string Image { get; }

        public TabElementWithImage(string image, ITabContent content) : base("", content)
        {
            Image = image;
        }
        
    }

    internal interface ITabContent : IGuiCreatable
    {
        /// <summary> タブの選択が解除されたときに呼びます </summary>
        public void OnTabUnselect();
    }

}