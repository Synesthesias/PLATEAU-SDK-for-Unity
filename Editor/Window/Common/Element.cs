using UnityEditor;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// GUIの1要素を抽象化したクラスです。
    /// 描画のON/OFFを切り替え機能や、
    /// <see cref="ElementGroup"/>から検索されるための名前を有します。
    /// このクラスを継承してボタンやオブジェクトフィールドなどGUI要素を構築できます。
    /// または、<see cref="GeneralElement"/>を利用して任意の描画メソッドを実行できます。
    /// </summary>
    public abstract class Element : IEditorDrawable
    {
        /// <summary> falseのとき、<see cref="Draw"/>が呼ばれても描画しません </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary> falseのとき、Disabled(灰色で操作不可の状態)にします </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary> 検索用の名前です。 </summary>
        public string Name { get; }

        public Element() : this("")
        {
            
        }
        
        /// <summary>
        /// 引数<paramref name="name"/>には検索用の名前を指定しますが、名前検索を使わなければ空文字列でも構いません。
        /// </summary>
        public Element(string name)
        {
            Name = name;
        }

        public void Draw()
        {
            if (!IsVisible) return;
            using (new EditorGUI.DisabledScope(!IsEnabled))
            {
                DrawContent();
            }
        }

        public abstract void DrawContent();
        public abstract void Dispose();
    }
}