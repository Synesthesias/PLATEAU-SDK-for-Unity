using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityExportGUI : IEditorDrawable
    {
        private int dummy = 0; // TODO あとでちゃんとしたのに置き換える
        public void Draw()
        {
            HeaderDrawer.Draw("都市モデルのエクスポート");
            EditorGUILayout.LabelField("モデルデータのエクスポートを行います。");
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("選択オブジェクト");
            // TODO 仮
            dummy = EditorGUILayout.Popup("出力形式", this.dummy, new []{"OBJ", "FBX", "glTF"});
            HeaderDrawer.Draw("Option");
            PlateauEditorStyle.Separator(0);
            if (PlateauEditorStyle.MainButton("エクスポート"))
            {
                Debug.Log("export button pushed (仮)");
            }
        }
    }
}
