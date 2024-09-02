using System;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// ボタンを<see cref="Element"/>でラップしたクラスです。
    /// </summary>
    internal class ButtonElement : Element
    {
        private readonly Action onClick;
        private Vector4 padding = Vector4.zero; // left, right, top, bottom

        public string ButtonText { get; set; }
        private string initialText;

        public ButtonElement(string name, string buttonText, Action onClick) : base(name)
        {
            this.ButtonText = buttonText;
            initialText = buttonText;
            this.onClick = onClick;
        }

        public ButtonElement(string name, string buttonText, Vector4 padding, Action onClick) : this(name, buttonText, onClick)
        {
            this.padding = padding;
        }

        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding((int)padding.x, (int)padding.y, (int)padding.z,
                       (int)padding.w))
            {
                if (PlateauEditorStyle.MainButton(ButtonText))
                {
                    onClick();
                }
            }
        }

        /// <summary>
        /// ボタンのテキストを例えば「処理中...」のように変えて、押せないようにします。
        /// <see cref="RecoverFromProcessing"/>で元に戻します。
        /// ボタンのテキストを引数にとります。
        /// </summary>
        public void SetProcessing(string processingText)
        {
            ButtonText = processingText;
            IsEnabled = false;
        }

        /// <summary>
        /// <see cref="SetProcessing"/>による変更を元に戻します。
        /// </summary>
        public void RecoverFromProcessing()
        {
            ButtonText = initialText;
            IsEnabled = true;
        }

        public override void Dispose()
        {
        }
    }
}