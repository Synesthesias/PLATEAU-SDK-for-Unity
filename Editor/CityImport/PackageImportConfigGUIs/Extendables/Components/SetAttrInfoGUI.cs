using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components
{
    /// <summary>
    /// 属性情報を含めるかどうかの設定GUIです。
    /// </summary>
    internal class SetAttrInfoGUI : ExtendableConfigGUIBase
    {
        
        public SetAttrInfoGUI(PackageImportConfigExtendable conf) : base(conf){}

        public override void Draw()
        {
            Conf.DoSetAttrInfo =
                PlateauEditorStyle.Toggle("属性情報を含める", Conf.DoSetAttrInfo);
        }
    }
}