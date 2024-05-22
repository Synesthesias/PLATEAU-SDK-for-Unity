using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// 属性情報キーを1つ、選択肢からユーザーに選択させるウィンドウです。
    /// </summary>
    internal class AttrKeySelectWindow : PlateauWindowBase
    {
        private IAttrKeySelectResultReceiver resultReceiver;
        private UniqueParentTransformList targets;
        private EditorWindow mainWindow;

        public static void Open(IAttrKeySelectResultReceiver resultReceiver, UniqueParentTransformList targets, EditorWindow parentWindow)
        {
            var window = GetWindow<AttrKeySelectWindow>("属性情報キー選択");
            window.resultReceiver = resultReceiver;
            window.targets = targets;
            window.mainWindow = parentWindow;
            window.Show();
        }

        protected override IEditorDrawable InitGui()
        {
            return new AttrKeySelectGui(targets, resultReceiver, mainWindow, this);
        }
    }

    internal interface IAttrKeySelectResultReceiver
    {
        void ReceiveAttrKeySelectResult(string selectedAttrKey);
    }
}