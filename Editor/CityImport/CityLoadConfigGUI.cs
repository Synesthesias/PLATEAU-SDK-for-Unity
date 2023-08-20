using System;
using System.Collections.Generic;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityLoadConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityLoadConfigGUI
    {
        private CityLoadConfig cityLoadConf;
        private readonly List<PackageLoadSettingGUI> packageGUIList;
        private readonly PackageToLodDict availablePackageLODDict;

        public CityLoadConfigGUI(CityLoadConfig cityLoadConf, PackageToLodDict availablePackageLodsArg)
        {
            this.cityLoadConf = cityLoadConf ?? throw new ArgumentNullException(nameof(cityLoadConf));
            availablePackageLODDict = availablePackageLodsArg;
            packageGUIList = new List<PackageLoadSettingGUI>();
            foreach (var (package, maxLOD) in availablePackageLODDict)
            {
                if (maxLOD < 0)
                {
                    cityLoadConf.GetConfigForPackage(package).LoadPackage = false;
                    continue;
                }
                packageGUIList.Add(new PackageLoadSettingGUI(cityLoadConf.GetConfigForPackage(package)));
            }
        }
        
        /// <summary>
        /// <see cref="CityLoadConfig"/> を設定するGUIを描画します。
        /// </summary>
        public void Draw()
        {
            // パッケージごとの設定
            foreach (var packageGUI in packageGUIList)
            {
                packageGUI.Draw(availablePackageLODDict.GetLod(packageGUI.Package));
            }

            // 位置指定
            PositionConfGui(cityLoadConf);
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
    }
}
