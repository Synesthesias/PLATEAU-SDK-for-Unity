using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class ConvertToAssetGUI : IEditorDrawable
    {
        private ConvertToAssetConfig conf = new ConvertToAssetConfig(null, "Assets/");
        public void Draw()
        {   PlateauEditorStyle.Heading("Assetsに保存", null);
            PlateauEditorStyle.SubTitle("シーン内に直接保存されている都市モデルを、属性情報等を保ったままFBXに書き出します");
            conf.SrcGameObj = (GameObject)EditorGUILayout.ObjectField("元オブジェクト", conf.SrcGameObj, typeof(GameObject), true);
            conf.AssetPath = EditorGUILayout.TextField("Assetsパス", conf.AssetPath);
            if (PlateauEditorStyle.MainButton("Assetsに抽出"))
            {
                new ConvertToAsset().Convert(conf); 
            }
        }

        public void Dispose()
        {
        }
    }
}