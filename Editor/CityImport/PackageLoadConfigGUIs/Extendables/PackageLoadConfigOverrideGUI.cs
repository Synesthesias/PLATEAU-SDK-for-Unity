using PLATEAU.CityImport.Config.PackageLoadConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables.Components;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables
{
    /// <summary>
    /// インポートのパッケージごとの設定GUIのうち、
    /// 一括設定を各パッケージでオーバーライドする部分です。
    /// </summary>
    internal class PackageLoadConfigOverrideGUI : PackageLoadConfigGUIComponent
    {
        private readonly PackageLoadConfigExtendableGUI gui;
        private PackageLoadConfigExtendable ParentConf { get; set; }

        public PackageLoadConfigOverrideGUI(
            PackageLoadConfig packageConf, PackageLoadConfigExtendable parentConf)
            : base(packageConf)
        {
            ParentConf = parentConf;
            gui = new PackageLoadConfigExtendableGUI(packageConf.ConfExtendable);
            
            bool mayTextureExist = CityModelPackageInfo.GetPredefined(Conf.Package).hasAppearance;
            gui.GetGUIByType<TextureIncludeGUI>().MayTextureExist = mayTextureExist;

        }
        
        private bool DoUseParentConfig { get; set; } = true;


        public override void Draw()
        {
            DoUseParentConfig = EditorGUILayout.Toggle("一括設定と同じ", DoUseParentConfig);
            if (DoUseParentConfig)
            {
                // 「一括設定と同じ」にチェックが入っているなら、一括設定を適用します。
                gui.Conf.CopyFrom(ParentConf);
            }
            else
            {
                // チェックが入っていないなら、設定をオーバーライドするためのGUIを表示します。
                using (PlateauEditorStyle.VerticalScopeLevel2())
                {
                    gui.Draw();
                }
                
            }
        }
    }
}