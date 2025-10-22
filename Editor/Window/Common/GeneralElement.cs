using System;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// 描画内容を任意に設定できる<see cref="Element"/>です。
    /// </summary>
    public class GeneralElement : Element
    {
        private readonly Action drawFunc;
        
        public GeneralElement(string elementName, Action drawFunc) : base(elementName)
        {
            this.drawFunc = drawFunc;
        }
        
        public override void DrawContent()
        {
            drawFunc();
        }
        public override void Reset()
        {
        }

        public override void Dispose()
        {
        }
    }
}