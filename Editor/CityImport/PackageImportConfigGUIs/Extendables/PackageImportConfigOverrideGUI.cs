using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables
{
    /// <summary>
    /// インポートのパッケージごとの設定GUIのうち、
    /// 一括設定を各パッケージでオーバーライドする部分です。
    /// </summary>
    internal class PackageImportConfigOverrideGUI : PackageImportConfigGUIComponent
    {
        private readonly PackageImportConfigExtendableGUI gui;
        private bool DoUseMasterConfig { get; set; } = true;
        /// <summary> 「一括設定と同じ」場合に使用する一括設定への参照です。 </summary>
        private PackageImportConfigExtendable MasterConf { get; }

        public PackageImportConfigOverrideGUI(
            PackageImportConfig packageConf, PackageImportConfigExtendable masterConf)
            : base(packageConf)
        {
            MasterConf = masterConf;
            gui = new PackageImportConfigExtendableGUI(packageConf.ConfExtendable);
            
            bool mayTextureExist = CityModelPackageInfo.GetPredefined(Conf.Package).hasAppearance;
            gui.GetGUIByType<TextureIncludeGUI>().MayTextureExist = mayTextureExist;

        }


        public override void Draw()
        {
            DoUseMasterConfig = EditorGUILayout.Toggle("一括設定と同じ", DoUseMasterConfig);
            if (DoUseMasterConfig)
            {
                // 「一括設定と同じ」にチェックが入っているなら、一括設定を適用します。
                gui.Conf.CopyFrom(MasterConf);
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