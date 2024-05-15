﻿using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「モデル調整」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityModificationFrameGUI : IEditorDrawable
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray;
        private string[] tabNames = { "Assetsに保存","ゲームオブジェクト\nON/OFF" , "マテリアル分け", "結合/分離", "地形変換" };

        public CityModificationFrameGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.tabGUIArray = new IEditorDrawable[]
            {
                new ConvertToAssetGUI(),
                new CityChangeActiveGUI(),
                new CityMaterialAdjustGUI(parentEditorWindow),
                new CityGranularityConvertGUI(parentEditorWindow),
                new CityTerrainConvertGUI(parentEditorWindow)
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

        public void Dispose() 
        {
            foreach (var gui in tabGUIArray)
                gui.Dispose();
        }
    }
}
