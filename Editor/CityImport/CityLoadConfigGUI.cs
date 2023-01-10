using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityLoadConfig"/> を設定するGUIです。
    /// </summary>
    internal static class CityLoadConfigGUI
    {
        /// <summary>
        /// <see cref="CityLoadConfig"/> を設定するGUIを描画します。
        /// </summary>
        public static void Draw(CityLoadConfig cityLoadConf)
        {
            foreach (var pair in cityLoadConf.ForEachPackagePair)
            {
                var package = pair.Key;
                var conf = pair.Value;
                conf.GuiFoldOutState = PlateauEditorStyle.FoldOut(conf.GuiFoldOutState, package.ToJapaneseName(), () =>
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        conf.loadPackage = EditorGUILayout.Toggle("インポートする", conf.loadPackage);
                        if (conf.loadPackage)
                        {
                            using (PlateauEditorStyle.VerticalScopeLevel1(1))
                            {
                                var predefined = CityModelPackageInfo.GetPredefined(package);
                                TextureIncludeGUI(conf, predefined.hasAppearance);
                                conf.doSetMeshCollider = EditorGUILayout.Toggle("Mesh Collider をセットする", conf.doSetMeshCollider);
                                
                                PlateauEditorStyle.LODSlider("LOD描画設定", ref conf.minLOD, ref conf.maxLOD, (uint)predefined.minLOD, (uint)predefined.maxLOD);
                                
                                conf.meshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                                    (int)conf.meshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
                            }
                        }
                    }
                });
            }
        }

        private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
        {
            if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
            conf.includeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.includeTexture);
        }
    }
}
