using PLATEAU.CityImport.Config.PackageImportConfigs;
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
                EditorGUILayout.Toggle("属性情報を含める", Conf.DoSetAttrInfo);
        }
    }
}