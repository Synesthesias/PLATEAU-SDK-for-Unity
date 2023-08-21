using System.IO;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.Common.PathSelector
{
    /// <summary>
    /// <see cref="PathSelectorFolder"/> に PLATEAU入力パスとして固有の機能を追加したものです。
    /// PLATEAUフォルダが選択されていなければ警告を表示します。
    /// </summary>
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
        private bool IsPathPlateauRoot()
        {
            if (string.IsNullOrEmpty(SelectedPath)) return false;
            string udxPath = Path.Combine(SelectedPath, "udx");
            return Directory.Exists(udxPath);
        }
    }
}
