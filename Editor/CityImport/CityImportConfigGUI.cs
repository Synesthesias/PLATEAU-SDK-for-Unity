using System;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components;
using PLATEAU.Editor.Window.Common;
using System.Collections.Generic;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityImportConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityImportConfigGUI
    {
        private readonly CityImportConfig cityImportConf;
        private readonly IEditorDrawable[] guiComponents;
        private readonly PackageImportConfigGUIList packageConfigGUIList;
        
        // DynamicTile <-> Scene 切替時に元の粒度設定を復元するためのバックアップです。
        private readonly Dictionary<PredefinedCityModelPackage, PolygonMesh.MeshGranularity> savedGranularityPerPackage = new ();
        private PolygonMesh.MeshGranularity? savedMasterGranularity;
        
        public CityImportConfigGUI(CityImportConfig cityImportConf, PackageToLodDict availablePackageLodsArg)
        {
            if (cityImportConf == null) throw new ArgumentNullException(nameof(cityImportConf));
            this.cityImportConf = cityImportConf;

            // パッケージ設定GUIListを保持
            packageConfigGUIList = new PackageImportConfigGUIList(availablePackageLodsArg, cityImportConf);

            // パッケージ種ごとの設定GUI、その下に基準座標設定GUIが表示されるようにGUIコンポーネントを置きます。
            guiComponents = new IEditorDrawable[]
            {
                new HeaderElementGroup("", "地物別設定", HeaderType.HeaderNum3),
                packageConfigGUIList,
                new PositionConfGUI(cityImportConf)
            };
        }

        /// <summary>
        /// <see cref="CityImportConfig"/> を設定するGUIを描画します。
        /// </summary>
        public void Draw()
        {
            // 動的タイル選択時は、個別パッケージ設定の粒度を地域単位へ強制
            EnforceGranularityIfDynamicTile();
            foreach (var guiComponent in guiComponents)
            {
                guiComponent.Draw();
            }
        }

        /// <summary>
        /// 動的タイルモードのとき、粒度を地域単位に強制します。
        /// また設定UIを「動的タイル」から「シーンに配置」に戻したとき、粒度の設定値を戻せるように記録と復元を行います。
        /// </summary>
        private void EnforceGranularityIfDynamicTile()
        {
            if (cityImportConf == null || cityImportConf.PackageImportConfigDict == null)
            {
                return;
            }
            bool forceToCityModelArea = cityImportConf.DynamicTileImportConfig.ImportType == ImportType.DynamicTile;

            foreach (var packagePair in cityImportConf.PackageImportConfigDict.ForEachPackagePair)
            {
                var package = packagePair.Key;
                var conf = packagePair.Value;
                if (forceToCityModelArea)
                {
                    // 初回のみ現在値をバックアップ
                    if (!savedGranularityPerPackage.ContainsKey(package))
                    {
                        savedGranularityPerPackage[package] = conf.MeshGranularity;
                    }
                    conf.ForcePerCityModelArea = true;
                    conf.MeshGranularity = PLATEAU.PolygonMesh.MeshGranularity.PerCityModelArea;
                }
                else
                {
                    conf.ForcePerCityModelArea = false;
                    // 以前の値があれば復元
                    if (savedGranularityPerPackage.TryGetValue(package, out var prev))
                    {
                        conf.MeshGranularity = prev;
                    }
                }
            }

            if (packageConfigGUIList != null)
            {
                if (forceToCityModelArea)
                {
                    if (!savedMasterGranularity.HasValue)
                    {
                        savedMasterGranularity = packageConfigGUIList.MasterConf.MeshGranularity;
                    }
                    packageConfigGUIList.MasterConf.ForcePerCityModelArea = true;
                    packageConfigGUIList.MasterConf.MeshGranularity = PolygonMesh.MeshGranularity.PerCityModelArea;
                }
                else
                {
                    packageConfigGUIList.MasterConf.ForcePerCityModelArea = false;
                    if (savedMasterGranularity.HasValue)
                    {
                        packageConfigGUIList.MasterConf.MeshGranularity = savedMasterGranularity.Value;
                    }
                }
            }
            // Scene側へ戻した時はバックアップをクリア（以降の変更を新たにバックアップできるように）
            if (!forceToCityModelArea)
            {
                savedGranularityPerPackage.Clear();
                savedMasterGranularity = null;
            }
        }


    }
}
