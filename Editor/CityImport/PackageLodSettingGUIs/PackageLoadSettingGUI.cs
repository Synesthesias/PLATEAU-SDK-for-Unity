using System;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using UnityEditor;
using UnityEngine;

/// <summary>
/// インポート設定のうち、パッケージ種1つの設定GUIです。
/// 具体的には、<see cref="PackageLoadSetting"/>を1つ設定するGUIです。
/// </summary>
internal class PackageLoadSettingGUI
{
    private readonly PackageLoadSetting config;

    /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
    private bool guiFoldOutState = true;

    public PackageLoadSettingGUI(PackageLoadSetting conf)
    {
        this.config = conf;
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
                    var predefined = CityModelPackageInfo.GetPredefined(package);
                    TextureIncludeGUI(conf, predefined.hasAppearance);
                    conf.DoSetMeshCollider =
                        EditorGUILayout.Toggle("Mesh Collider をセットする", conf.DoSetMeshCollider);

                    int minLOD = conf.LODRange.MinLOD;
                    int maxLOD = conf.LODRange.MaxLOD;
                    int availableMaxLOD = conf.LODRange.AvailableMaxLOD;
                    PlateauEditorStyle.LODSlider("LOD描画設定", ref minLOD, ref maxLOD,
                        Math.Min(predefined.minLOD, availableMaxLOD), availableMaxLOD);
                    conf.LODRange = new LODRange(minLOD, maxLOD, availableMaxLOD);

                    conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                        (int)conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });

                    conf.DoSetAttrInfo =
                        EditorGUILayout.Toggle("属性情報を含める", conf.DoSetAttrInfo);

                    conf.FallbackMaterial = (Material)EditorGUILayout.ObjectField("デフォルトマテリアル",
                        conf.FallbackMaterial, typeof(Material), false);
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

    private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
    {
        if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
        conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.IncludeTexture);

        if (!conf.IncludeTexture) return;
        conf.EnableTexturePacking = EditorGUILayout.Toggle("テクスチャを結合する", conf.EnableTexturePacking);
        if (!conf.EnableTexturePacking) return;
        conf.TexturePackingResolution = (TexturePackingResolution)EditorGUILayout.Popup("テクスチャ解像度",
            (int)conf.TexturePackingResolution, new[] { "2048x2048", "4096x4096", "8192x8192" });
    }
}