using System;
using System.Collections.Generic;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageLodSettingGUIs
{
    /// <summary>
    /// インポート設定のうち、パッケージ種1つの設定GUIです。
    /// 具体的には、<see cref="PackageLoadSetting"/>を1つ設定するGUIです。
    /// 他クラスとの関係:
    /// パッケージ設定GUIを、利用可能な全種類について集めたクラスが <see cref="PackageLoadSettingGUIList"/> です。
    /// </summary>
    internal class PackageLoadSettingGUI
    {
        private readonly PackageLoadSetting config;
        
        /// <summary> 設定GUIのリスト </summary>
        private readonly List<PackageLoadSettingGUIComponent> guisForInclude;

        /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
        private bool guiFoldOutState = true;

        public PackageLoadSettingGUI(PackageLoadSetting conf)
        {
            config = conf;
            guisForInclude = new List<PackageLoadSettingGUIComponent>
            {
                // ここに設定項目を列挙します
                new TextureIncludeGUI(conf),
                new MeshColliderSetGUI(conf),
                new LodGUI(conf),
                new MeshGranularityGUI(conf),
                new SetAttrInfoGUI(conf),
                new FallbackMaterialGUI(conf)
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
    internal abstract class PackageLoadSettingGUIComponent : IEditorDrawable
    {
        protected readonly PackageLoadSetting Conf;

        protected PackageLoadSettingGUIComponent(PackageLoadSetting conf)
        {
            Conf = conf;
        }
        public abstract void Draw();
    }


    internal class TextureIncludeGUI : PackageLoadSettingGUIComponent
    {

        public TextureIncludeGUI(PackageLoadSetting conf) : base(conf)
        {
        
        }

        public override void Draw()
        {
            bool mayTextureExist = CityModelPackageInfo.GetPredefined(Conf.Package).hasAppearance;
            if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
            Conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", Conf.IncludeTexture);

            if (!Conf.IncludeTexture) return;
            Conf.EnableTexturePacking = EditorGUILayout.Toggle("テクスチャを結合する", Conf.EnableTexturePacking);
            if (!Conf.EnableTexturePacking) return;
            Conf.TexturePackingResolution = (TexturePackingResolution)EditorGUILayout.Popup("テクスチャ解像度",
                (int)Conf.TexturePackingResolution, new[] { "2048x2048", "4096x4096", "8192x8192" });
        }
    }

    internal class MeshColliderSetGUI : PackageLoadSettingGUIComponent
    {
        public MeshColliderSetGUI(PackageLoadSetting conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.DoSetMeshCollider =
                EditorGUILayout.Toggle("Mesh Collider をセットする", Conf.DoSetMeshCollider);
        }
    }

    internal class LodGUI : PackageLoadSettingGUIComponent
    {
        public LodGUI(PackageLoadSetting conf) : base(conf)
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

    internal class MeshGranularityGUI : PackageLoadSettingGUIComponent
    {

        public MeshGranularityGUI(PackageLoadSetting conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                (int)Conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
        }
    }

    internal class SetAttrInfoGUI : PackageLoadSettingGUIComponent
    {
        public SetAttrInfoGUI(PackageLoadSetting conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.DoSetAttrInfo =
                EditorGUILayout.Toggle("属性情報を含める", Conf.DoSetAttrInfo);
        }
    }

    internal class FallbackMaterialGUI : PackageLoadSettingGUIComponent
    {
        public FallbackMaterialGUI(PackageLoadSetting conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.FallbackMaterial = (Material)EditorGUILayout.ObjectField("デフォルトマテリアル",
                Conf.FallbackMaterial, typeof(Material), false);
        }
    }
}