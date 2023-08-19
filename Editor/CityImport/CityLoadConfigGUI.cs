using System;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityLoadConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityLoadConfigGUI
    {

        private PackageToLodDict availablePackageLods;

        public CityLoadConfigGUI(PackageToLodDict availablePackageLods)
        {
            this.availablePackageLods = availablePackageLods;
        }
        
        /// <summary>
        /// <see cref="CityLoadConfig"/> を設定するGUIを描画します。
        /// </summary>
        public void Draw(CityLoadConfig cityLoadConf)
        {
            // パッケージごとの設定
            foreach (var (package, conf) in cityLoadConf.ForEachPackagePair)
            {
                if (!this.availablePackageLods.Contains(package)) continue;
                int availableMaxLod = this.availablePackageLods.GetLod(package);
                if (availableMaxLod >= 0)
                {
                    PerPackageLoadConfGui(package, conf, availableMaxLod);
                }
                else
                {
                    conf.LoadPackage = false;
                }
            }

            // 位置指定
            PositionConfGui(cityLoadConf);
        }

        private static void PerPackageLoadConfGui(PredefinedCityModelPackage package, PackageLoadSetting conf, int availableMaxLod)
        {
            conf.GuiFoldOutState = PlateauEditorStyle.FoldOut(conf.GuiFoldOutState, package.ToJapaneseName(), () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    conf.LoadPackage = EditorGUILayout.Toggle("インポートする", conf.LoadPackage);
                    if (conf.LoadPackage)
                    {
                        using (PlateauEditorStyle.VerticalScopeLevel1(1))
                        {
                            var predefined = CityModelPackageInfo.GetPredefined(package);
                            TextureIncludeGUI(conf, predefined.hasAppearance);
                            conf.DoSetMeshCollider =
                                EditorGUILayout.Toggle("Mesh Collider をセットする", conf.DoSetMeshCollider);

                            conf.DoSetAttrInfo =
                                EditorGUILayout.Toggle("属性情報を含める", conf.DoSetAttrInfo);

                            PlateauEditorStyle.LODSlider("LOD描画設定", ref conf.MinLOD, ref conf.MaxLOD,
                                Math.Min(predefined.minLOD, availableMaxLod), availableMaxLod);

                            conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                                (int)conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
                        }
                    }
                }
            });
        }

        private static void PositionConfGui(CityLoadConfig conf)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.Heading("基準座標系からのオフセット値(メートル)", null);

                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    var refPoint = conf.ReferencePoint;
                    PlateauEditorStyle.CenterAlignHorizontal(() =>
                    {
                        if (PlateauEditorStyle.MiniButton("範囲の中心点を入力", 140))
                        {
                            GUI.FocusControl("");
                            refPoint = conf.SetReferencePointToExtentCenter();
                        }
                    });

                    refPoint.X = EditorGUILayout.DoubleField("X (東が正方向)", refPoint.X);
                    refPoint.Y = EditorGUILayout.DoubleField("Y (高さ)", refPoint.Y);
                    refPoint.Z = EditorGUILayout.DoubleField("Z (北が正方向)", refPoint.Z);
                    conf.ReferencePoint = refPoint;
                }
            }
        }

        private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
        {
            if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
            conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.IncludeTexture);
        }
    }
}
