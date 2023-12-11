using PLATEAU.CityImport.Config;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// インポートの設定GUIにおいて、範囲選択前の設定GUIでローカルとサーバーの差異を吸収するインターフェイスです。
    /// </summary>
    public interface IConfigGUIBeforeAreaSelect
    {
        ConfigBeforeAreaSelect Draw();
    }
}