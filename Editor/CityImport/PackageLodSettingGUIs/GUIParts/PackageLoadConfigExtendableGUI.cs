using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageLodSettingGUIs;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.GUIParts
{
    /// <summary>
    /// インポートのパッケージごとの設定GUIのうち、一括設定の部分です。
    /// 加えて、各パッケージで一括設定をオーバーライドするGUI <see cref="PackageLoadConfigOverrideGUI"/>でも利用します。
    /// </summary>
    internal class PackageLoadConfigExtendableGUI
    {
        private readonly List<ExtendableConfigGUIBase> guis;

        private readonly PackageLoadConfigExtendable conf;

        public PackageLoadConfigExtendableGUI(PackageLoadConfigExtendable conf)
        {
            this.conf = conf;
            guis = new()
            {
                // ここに一括設定の設定項目を列挙します。
                new TextureIncludeGUI(conf),
                new MeshColliderSetGUI(conf),
                new MeshGranularityGUI(conf),
                new SetAttrInfoGUI(conf)
            };
        }


        public PackageLoadConfigExtendable GetConf => conf;
        
        public void Draw()
        {
            foreach (var gui in guis)
            {
                gui.Draw();
            }
        }
        
        public T GetGUIByType<T>() where T : ExtendableConfigGUIBase
        {
            return guis.OfType<T>().First();
        }
    }

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
                gui.GetConf.CopyFrom(ParentConf);
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

    /// <summary>
    /// 一括設定の中にある各項目を抽象化したものです。
    /// <see cref="Draw"/>で設定GUIを描画します。
    /// それが個別設定（パッケージごと）である場合は、<see cref="ApplyTo"/>で1つのパッケージ設定に対して適用します。
    /// それが一括設定（共通）である場合は、
    /// </summary>
    internal abstract class ExtendableConfigGUIBase
    {
        protected PackageLoadConfigExtendable Conf;

        public ExtendableConfigGUIBase(PackageLoadConfigExtendable conf)
        {
            Conf = conf;
        }
        
        public abstract void Draw();
    }
    
    /// <summary>
    /// テクスチャに関する設定GUIです。
    /// </summary>
    internal class TextureIncludeGUI : ExtendableConfigGUIBase
    {
        /// <summary>
        /// 仕様上、テクスチャが存在する可能性があるか（パッケージ種によってはない）
        /// </summary>
        public bool MayTextureExist { get; set; } = true;
        public TextureIncludeGUI(PackageLoadConfigExtendable conf) : base(conf)
        {
        }
        
        public override void Draw()
        {
            if (!MayTextureExist) return;
            Conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", Conf.IncludeTexture);

            if (!Conf.IncludeTexture) return;
            Conf.EnableTexturePacking = EditorGUILayout.Toggle("テクスチャを結合する", Conf.EnableTexturePacking);
            if (!Conf.EnableTexturePacking) return;
            Conf.TexturePackingResolution = (TexturePackingResolution)EditorGUILayout.Popup("テクスチャ解像度",
                (int)Conf.TexturePackingResolution, new[] { "2048x2048", "4096x4096", "8192x8192" });
        }
    }

    /// <summary>
    /// MeshColliderをセットするかどうかの設定GUIです。
    /// </summary>
    internal class MeshColliderSetGUI : ExtendableConfigGUIBase
    {

        public MeshColliderSetGUI(PackageLoadConfigExtendable conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.DoSetMeshCollider =
                EditorGUILayout.Toggle("Mesh Collider をセットする", Conf.DoSetMeshCollider);
        }

    }

    /// <summary>
    /// メッシュ粒度の設定GUIです。
    /// </summary>
    internal class MeshGranularityGUI : ExtendableConfigGUIBase
    {

        public MeshGranularityGUI(PackageLoadConfigExtendable conf) : base(conf){}

    public override void Draw()
        {
            Conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                (int)Conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
        }
    }

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