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
        private bool DoUseMasterConfig { get; set; } = true;
        /// <summary> 「一括設定と同じ」場合に使用する一括設定への参照です。 </summary>
        private PackageLoadConfigExtendable MasterConf { get; }

        public PackageLoadConfigOverrideGUI(
            PackageLoadConfig packageConf, PackageLoadConfigExtendable masterConf)
            : base(packageConf)
        {
            MasterConf = masterConf;
            gui = new PackageLoadConfigExtendableGUI(packageConf.ConfExtendable);
            
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