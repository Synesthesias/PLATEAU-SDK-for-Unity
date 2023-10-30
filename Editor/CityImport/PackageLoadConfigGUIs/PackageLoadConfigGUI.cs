using System.Collections.Generic;
using PLATEAU.CityImport.Config.PackageLoadConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Components;
using PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.Extendables;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs
{
    /// <summary>
    /// インポート設定のうち、パッケージ種1つの設定GUIです。
    /// 具体的には、<see cref="PackageLoadConfig"/>を1つ設定するGUIです。
    /// 他クラスとの関係:
    /// パッケージ設定GUIを、利用可能な全種類について集めたクラスが <see cref="PackageLoadConfigGUIList"/> です。
    /// </summary>
    internal class PackageLoadConfigGUI
    {
        private readonly PackageLoadConfig config;
        
        /// <summary> 設定GUIのリスト </summary>
        private readonly List<PackageLoadConfigGUIComponent> guisForInclude;

        /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
        private bool guiFoldOutState = true;

        public PackageLoadConfigGUI(PackageLoadConfig packageConf, PackageLoadConfigExtendable masterConf)
        {
            config = packageConf;
            guisForInclude = new List<PackageLoadConfigGUIComponent>
            {
                // ここに設定項目を列挙します
                new PackageLoadConfigOverrideGUI(packageConf, masterConf),
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
                    conf.LoadPackage = EditorGUILayout.Toggle("インポートする", conf.LoadPackage);
                    if (!conf.LoadPackage) return;
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