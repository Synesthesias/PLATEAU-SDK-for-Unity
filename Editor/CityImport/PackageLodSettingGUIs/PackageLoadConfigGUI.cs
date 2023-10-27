using System;
using System.Collections.Generic;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.GUIParts;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageLodSettingGUIs
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

        public PackageLoadConfigGUI(PackageLoadConfig packageConf, PackageLoadConfigExtendable parentConf)
        {
            config = packageConf;
            guisForInclude = new List<PackageLoadConfigGUIComponent>
            {
                // ここに設定項目を列挙します
                new PackageLoadConfigOverrideGUI(packageConf, parentConf),
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

    /// <summary>
    /// パッケージごとの設定GUIについて、各設定項目ごとのGUIをコンポーネントと呼ぶことにします。
    /// 例えば建物のテクスチャに関する設定コンポーネント、道路のメッシュ結合粒度に関する設定コンポーネント、といったものです。
    /// これら各コンポーネントは、 PackageLoadSetting を保持し、 Draw() で設定GUIを描画し、
    /// その中でユーザーのGUI操作に応じて設定値を変更します。
    /// そのコンポーネントという概念を抽象化したものがこのクラスです。
    /// </summary>
    internal abstract class PackageLoadConfigGUIComponent : IEditorDrawable
    {
        protected readonly PackageLoadConfig Conf;

        protected PackageLoadConfigGUIComponent(PackageLoadConfig conf)
        {
            Conf = conf;
        }
        public abstract void Draw();
        public void Dispose() { }
    }

    
    /// <summary>
    /// LOD範囲の設定GUIです。
    /// </summary>
    internal class LodGUI : PackageLoadConfigGUIComponent
    {
        public LodGUI(PackageLoadConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            var predefined = CityModelPackageInfo.GetPredefined(Conf.Package);
            int minLOD = Conf.LODRange.MinLOD;
            int maxLOD = Conf.LODRange.MaxLOD;
            int availableMaxLOD = Conf.LODRange.AvailableMaxLOD;
            PlateauEditorStyle.LODSlider("LOD描画設定", ref minLOD, ref maxLOD,
                Math.Min(predefined.minLOD, availableMaxLOD), availableMaxLOD);
            Conf.LODRange = new LODRange(minLOD, maxLOD, availableMaxLOD);
        }
    }

    /// <summary>
    /// デフォルトマテリアルの設定GUIです。
    /// </summary>
    internal class FallbackMaterialGUI : PackageLoadConfigGUIComponent
    {
        public FallbackMaterialGUI(PackageLoadConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.FallbackMaterial = (Material)EditorGUILayout.ObjectField("デフォルトマテリアル",
                Conf.FallbackMaterial, typeof(Material), false);
        }
    }
}