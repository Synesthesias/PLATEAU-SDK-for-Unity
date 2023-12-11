using System.Collections.Generic;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs
{
    /// <summary>
    /// インポート設定のうち、パッケージ種1つの設定GUIです。
    /// 具体的には、<see cref="PackageImportConfig"/>を1つ設定するGUIです。
    /// 他クラスとの関係:
    /// パッケージ設定GUIを、利用可能な全種類について集めたクラスが <see cref="PackageImportConfigGUIList"/> です。
    /// </summary>
    internal class PackageImportConfigGUI
    {
        private readonly PackageImportConfig config;
        
        /// <summary> 設定GUIのリスト </summary>
        private readonly List<PackageImportConfigGUIComponent> guisForInclude;

        /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
        private bool guiFoldOutState = true;

        public PackageImportConfigGUI(PackageImportConfig packageConf, PackageImportConfigExtendable masterConf)
        {
            config = packageConf;
            guisForInclude = new List<PackageImportConfigGUIComponent>
            {
                // ここに設定項目を列挙します
                new PackageImportConfigOverrideGUI(packageConf, masterConf),
                new LodGUI(packageConf),
                new FallbackMaterialGUI(packageConf)
            };
        }

        public void Draw()
        {
            var conf = this.config;
            var package = conf.Package;
            this.guiFoldOutState = PlateauEditorStyle.FoldOut(this.guiFoldOutState, package.ToJapaneseName(), () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    conf.ImportPackage = EditorGUILayout.Toggle("インポートする", conf.ImportPackage);
                    if (!conf.ImportPackage) return;
                    using (PlateauEditorStyle.VerticalScopeLevel1(1))
                    {
                        AdditionalSettingGUI();
                        foreach(var gui in guisForInclude)
                        {
                            gui.Draw();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// パッケージによって追加のGUIがある場合、サブクラスでこのメソッドをオーバーライドして実装します。
        /// </summary>
        protected virtual void AdditionalSettingGUI()
        {
            // サブクラスで実装しない限り何もしません
        }
    
    }
}