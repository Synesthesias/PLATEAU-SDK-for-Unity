using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityMaterialAdjustGUI : IEditorDrawable
    {
        public CityMaterialAdjustGUI()
        {

        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");
        }
    }
}
