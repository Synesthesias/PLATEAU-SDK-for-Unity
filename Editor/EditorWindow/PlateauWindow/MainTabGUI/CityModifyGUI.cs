using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「モデル調整」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityModifyGUI : IEditorDrawable
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray;
        private string[] tabNames = { "ゲームオブジェクト\nON/OFF" , "マテリアル分け", "結合/分離" };
        public CityModifyGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.tabGUIArray = new IEditorDrawable[]
            {
                new CityAdjustGUI(),
                new CityMaterialAdjustGUI(parentEditorWindow),
                new CityCombineSeparateGUI(parentEditorWindow)
            };
        }

        public void Draw()
        {
            this.tabIndex = PlateauEditorStyle.TabsForFrame(this.tabIndex, tabNames);
            using (PlateauEditorStyle.VerticalLineFrame())
            {
                this.tabGUIArray[this.tabIndex].Draw();
            }  
        }
    }
}
