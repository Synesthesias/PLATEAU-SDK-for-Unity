using System.IO;
using UnityEditor;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.Util.PathSelector
{
    internal class PathSelectorFolderPlateauInput : PathSelectorFolder
    {
        public override string Draw(string labelText)
        {
            string path = base.Draw(labelText);
            if (!IsPathPlateauRoot())
            {
                EditorGUILayout.HelpBox("PLATEAUフォルダが選択されていません。\n直下にudxフォルダを持つフォルダを選択してください。", MessageType.Error);
                return path;
            }
            return path;
        }
        
        /// <summary>
        /// 与えられたパスが Plateau元データのRootフォルダ であるかどうか判別します。
        /// Root直下に udx という名前のフォルダがあればOKとみなします。
        /// </summary>
        public bool IsPathPlateauRoot()
        {
            if (string.IsNullOrEmpty(SelectedPath)) return false;
            string udxPath = Path.Combine(SelectedPath, "udx");
            return Directory.Exists(udxPath);
        }
    }
}
