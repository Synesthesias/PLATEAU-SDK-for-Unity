using PLATEAU.Editor.Window.Common;
using System;

namespace PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts
{
    /// <summary>
    /// 元のオブジェクトを削除するか残すか選択するGUIを描画します。
    /// このクラスの利用者が値を受け取るには、コンストラクタでコールバックを登録します。
    /// </summary>
    internal class DestroyOrPreserveSrcGui : Element
    {
        
        private static readonly string[] DestroySrcOptions = { "新規追加（元オブジェクトは残す）", "置き換える（元オブジェクトは削除）" };
        private PreserveOrDestroy current;

        private PreserveOrDestroy Current // set時にコールバックを呼びます
        {
            get
            {
                return current;
            }
            set
            {
                bool isChanged = current != value;
                current = value;
                if (isChanged)
                {
                    Callback();
                }
            }
        }

        private Action<PreserveOrDestroy> onValueChanged;

        public DestroyOrPreserveSrcGui(Action<PreserveOrDestroy> onValueChanged)
        {
            this.onValueChanged = onValueChanged;
            Current = PreserveOrDestroy.Destroy;
            Callback(); // 初期値を伝える
        }

        private void Callback()
        {
            onValueChanged?.Invoke(Current);
        }

        public override void DrawContent()
        {
            Current = (PreserveOrDestroy)
                PlateauEditorStyle.PopupWithLabelWidth(
                    "オブジェクト配置", (int)Current, DestroySrcOptions, 90);
        }


        public override void Dispose(){}
    }
    
    public enum PreserveOrDestroy
    {
        Preserve, Destroy
    }
}