using System.IO;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// 「範囲選択」ボタンを表示し、押されたときに範囲選択を開始します。
    /// </summary>
    internal static class AreaSelectButton
    {
        /// <summary>
        /// 「範囲選択」ボタンを表示し、押された時に範囲選択を開始します。
        /// 範囲が選択されているかどうかをboolで返します。
        /// </summary>
        public static bool Draw(MeshCodeList areaMeshCodes, ConfigBeforeAreaSelect confBeforeAreaSelect, IAreaSelectResultReceiver resultReceiver)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                // ボタンを表示します。
                if (PlateauEditorStyle.MainButton("範囲選択"))
                { 
                    // ボタンを実行します。
                    StartAreaSelect(confBeforeAreaSelect, resultReceiver);
                    GUIUtility.ExitGUI();
                }
            
                // 範囲選択が済かどうかを表示します。
                bool isAreaSelectComplete = areaMeshCodes != null && areaMeshCodes.Count > 0;
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    string str = isAreaSelectComplete ? "範囲選択 : セット済" : "範囲選択 : 未";
                    PlateauEditorStyle.LabelSizeFit(new GUIContent(str), EditorStyles.label);
                });
                return isAreaSelectComplete;
            }
        }

        private static void StartAreaSelect(ConfigBeforeAreaSelect confBeforeAreaSelect, IAreaSelectResultReceiver resultReceiver)
        {
            if ((confBeforeAreaSelect.DatasetSourceConfig is DatasetSourceConfigLocal localConf) && (!Directory.Exists(localConf.LocalSourcePath)))
            {
                Dialogue.Display($"入力フォルダが存在しません。\nフォルダを指定してください。", "OK");
                return;
            }
            AreaSelectorStarter.Start(confBeforeAreaSelect, resultReceiver);
        }
    }
}
