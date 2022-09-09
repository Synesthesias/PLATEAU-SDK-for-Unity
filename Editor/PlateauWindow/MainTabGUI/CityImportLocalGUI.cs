﻿using PLATEAU.CityLoader;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Runtime.CityLoader.AreaSelector.Util.PathSelector;
using UnityEditor;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.MainTabGUI
{
    internal class CityImportLocalGUI : IEditorDrawable
    {
        private readonly PathSelectorFolderPlateauInput folderSelector = new PathSelectorFolderPlateauInput();
        private string folderPath;

        
        public void Draw()
        {
            this.folderPath = this.folderSelector.Draw("入力フォルダ");
            if (this.folderSelector.IsPathPlateauRoot())
            {
                if (PlateauEditorStyle.MainButton("都市の追加"))
                {
                    var obj = PLATEAUCityModelLoader.Create(this.folderPath);
                    Selection.activeObject = obj;
                }
            }
            
        }
    }
}
