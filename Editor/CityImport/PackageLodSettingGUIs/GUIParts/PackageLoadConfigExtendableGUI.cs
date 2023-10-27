using System.Collections.Generic;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageLodSettingGUIs;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.GUIParts
{
    /// <summary>
    /// インポートのパッケージごとの設定GUIのうち、
    /// 一括設定可能な部分です。
    /// </summary>
    internal class PackageLoadConfigExtendableGUI : PackageLoadConfigGUIComponent
    {
        private readonly List<PackageLoadConfigGUIComponent> guis;
        public PackageLoadConfigExtendableGUI(PackageLoadConfig conf) : base(conf)
        {
            // ここに一括設定の設定項目を列挙します。
            guis = new List<PackageLoadConfigGUIComponent>
            {
                new TextureIncludeGUI(conf),
                new MeshColliderSetGUI(conf),
                new MeshGranularityGUI(conf),
                new SetAttrInfoGUI(conf)
            };
        }
        
        public override void Draw()
        {
            foreach (var gui in guis)
            {
                gui.Draw();
            }
        }
    }
    
    /// <summary>
    /// テクスチャに関する設定GUIです。
    /// </summary>
    internal class TextureIncludeGUI : PackageLoadConfigGUIComponent
    {

        public TextureIncludeGUI(PackageLoadConfig conf) : base(conf)
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

    /// <summary>
    /// MeshColliderをセットするかどうかの設定GUIです。
    /// </summary>
    internal class MeshColliderSetGUI : PackageLoadConfigGUIComponent
    {
        public MeshColliderSetGUI(PackageLoadConfig conf) : base(conf)
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
    internal class MeshGranularityGUI : PackageLoadConfigGUIComponent
    {

        public MeshGranularityGUI(PackageLoadConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                (int)Conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
        }
    }

    /// <summary>
    /// 属性情報を含めるかどうかの設定GUIです。
    /// </summary>
    internal class SetAttrInfoGUI : PackageLoadConfigGUIComponent
    {
        public SetAttrInfoGUI(PackageLoadConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.DoSetAttrInfo =
                EditorGUILayout.Toggle("属性情報を含める", Conf.DoSetAttrInfo);
        }
    }
}