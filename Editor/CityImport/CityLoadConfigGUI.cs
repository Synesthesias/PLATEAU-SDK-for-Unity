using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.PolygonMesh;
using UnityEditor;
using UnityEngine;

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
            // パッケージごとの設定
            foreach (var (package, conf) in cityLoadConf.ForEachPackagePair)
            {
                PerPackageLoadConfGui(package, conf);
            }
            // 位置指定
            PositionConfGui(cityLoadConf);
        }

        private static void PerPackageLoadConfGui(PredefinedCityModelPackage package, PackageLoadSetting conf)
        {
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

        private static void PositionConfGui(CityLoadConfig conf)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.Heading("位置設定", null);
                conf.ReferencePointSetMethod = 
                    (CityLoadConfig.ReferencePointSetMethodEnum)
                    EditorGUILayout.EnumPopup("3Dモデルの原点", conf.ReferencePointSetMethod);
                if (conf.ReferencePointSetMethod == CityLoadConfig.ReferencePointSetMethodEnum.Custom)
                {
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        EditorGUILayout.LabelField("3Dモデルの原点を手動で設定するモードです。");
                        EditorGUILayout.LabelField($"あなたが選択した平面直角座標系は");
                        EditorGUILayout.LabelField($"{GeoReference.ZoneIdExplanation[conf.CoordinateZoneID - 1]} です。");
                        EditorGUILayout.LabelField($"その座標系の原点は次のWebサイトで示されます:");
                        if (EditorGUILayout.LinkButton("国土地理院のWebサイトを開く"))
                        {
                            Application.OpenURL("https://www.gsi.go.jp/sokuchikijun/jpc.html");
                        }
                        EditorGUILayout.LabelField("その座標系の原点から東西南北に何メートルの箇所を3Dモデルの原点とするか指定してください。");
                    }

                    using(PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        var refPoint = conf.CustomReferencePoint;
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                        {
                            if (PlateauEditorStyle.MiniButton("範囲の中心点を入力", 140))
                            {
                                refPoint = CityImporter.CalcCenterPoint(conf.SearchMatchingGMLList(), conf.CoordinateZoneID);
                            }
                        });
                        
                        refPoint.X = EditorGUILayout.DoubleField("X (正が東,負が西,メートル)", refPoint.X);
                        refPoint.Y = EditorGUILayout.DoubleField("Y (高さ,メートル)", refPoint.Y);
                        refPoint.Z = EditorGUILayout.DoubleField("Z (正が北,負が南,メートル)", refPoint.Z);
                        conf.CustomReferencePoint = refPoint;
                    }
                    
                }
            }
           
        }

        private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
        {
            if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
            conf.includeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.includeTexture);
        }
    }
}
