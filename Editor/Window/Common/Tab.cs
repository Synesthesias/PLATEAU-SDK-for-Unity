using System;
using System.Linq;

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
        
        
        protected int TabIndex
        {
            get => tabIndex;
            set
            {
                if (value != tabIndex)
                {
                    CurrentContent.OnTabUnselect();
                    tabIndex = value;
                }
            }
        }
        
        public TabBase(params TabElement[] tabElements)
        {
            this.tabElements = tabElements;
        }

        /// <summary>
        /// タブを描画します。
        /// </summary>
        public abstract void DrawTab();

        /// <summary>
        /// 選択中のタブに対応するコンテンツを描画します。
        /// </summary>
        public virtual void DrawContent()
        {
            CurrentContent.Draw();
        }

        protected ITabContent CurrentContent => tabElements[TabIndex].Content;

        public void Dispose()
        {
            foreach (var elem in tabElements)
            {
                elem.Dispose();
            }
        }

        /// <summary>
        /// タブとコンテンツを両方表示します。
        /// </summary>
        public void Draw()
        {
            DrawTab();
            DrawContent();
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
        
        public override void DrawTab()
        {
            TabIndex = PlateauEditorStyle.TabsForFrame(TabIndex, tabElements.Select(te => te.Name).ToArray());
                      
        }
        
        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalLineFrame())
            {
                base.DrawContent();
            }    
        }
    }
    
    /// <summary>
    /// タブが画像になっている版です。
    /// </summary>
    internal class TabWithImage : TabBase
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
        
        public override void DrawTab()
        {
            TabIndex = PlateauEditorStyle.TabWithImages(
                TabIndex,
                tabElements.Select(ti => ((TabElementWithImage)ti).Image).ToArray(),
                buttonWidth
            );
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

        public void Draw()
        {
            Content.Draw();
        }
    }

    internal interface ITabContent : IEditorDrawable
    {
        /// <summary> タブの選択が解除されたときに呼びます </summary>
        public void OnTabUnselect();
    }
}