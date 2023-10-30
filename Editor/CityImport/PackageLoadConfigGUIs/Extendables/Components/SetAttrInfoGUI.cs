using PLATEAU.CityImport.Config.PackageLoadConfigs;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables.Components
{
    /// <summary>
    /// 属性情報を含めるかどうかの設定GUIです。
    /// </summary>
    internal class SetAttrInfoGUI : ExtendableConfigGUIBase
    {
        
        public SetAttrInfoGUI(PackageLoadConfigExtendable conf) : base(conf){}

        public override void Draw()
        {
            Conf.DoSetAttrInfo =
                EditorGUILayout.Toggle("属性情報を含める", Conf.DoSetAttrInfo);
        }
    }
}