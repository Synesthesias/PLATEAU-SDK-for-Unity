using System.Collections.Generic;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.GUIParts;
using PLATEAU.Editor.EditorWindow.Common;
#if UNITY_EDITOR
#endif


namespace PLATEAU.Editor.CityImport.PackageLodSettingGUIs
{
    /// <summary>
    /// インポート設定GUIのうち、パッケージ種ごとの設定GUI <see cref="PackageLoadConfigGUI"/>を、利用可能パッケージ分だけ集めたものです。
    /// また一括設定のGUIも描画します。
    /// 参照:
    /// 具体的な設定GUIについては<see cref="PackageLoadConfigGUI"/>を参照してください。
    /// 設定値のクラスについては <see cref="PackageLoadConfigDict"/> を参照してください。
    /// </summary>
    internal class PackageLoadConfigGUIList : IEditorDrawable
    {
        /// <summary> パッケージ種ごとのGUIをパッケージごとに集めたものです。 </summary>
        private readonly List<PackageLoadConfigGUI> packageGUIList;
        
        /// <summary> 一括設定のGUIです。 </summary>
        private PackageLoadConfigExtendableGUI masterConfGUI;

        private readonly PackageLoadConfigExtendable masterConf = new PackageLoadConfigExtendable();

        public PackageLoadConfigGUIList(PackageToLodDict availablePackageLODDict, CityLoadConfig cityLoadConf)
        {
            // 初期化では、利用可能なパッケージ種1つにつき、それに対応するGUIインスタンスを1つ生成します。
            this.packageGUIList = new();
            foreach (var (package, maxLOD) in availablePackageLODDict)
            {
                if (maxLOD < 0)
                {
                    continue;
                }

                var packageConf = cityLoadConf.GetConfigForPackage(package);

                // パッケージ種による場合分けです。
                // これと似たロジックが PackageLoadSetting.CreateSettingFor にあるので、変更時はそちらも合わせて変更をお願いします。
                var gui = package switch
                {
                    PredefinedCityModelPackage.Relief => new ReliefLoadConfigGUI((ReliefLoadConfig)packageConf, masterConf, MeshCode.Parse(cityLoadConf.AreaMeshCodes[0])),
                    _ => new PackageLoadConfigGUI(packageConf, masterConf)
                };
                this.packageGUIList.Add(gui);
            }

            masterConfGUI = new PackageLoadConfigExtendableGUI(masterConf);
        }

        public void Draw()
        {
            // todo 折りたたみ可能にする
            PlateauEditorStyle.FoldOut(true, "一括設定", () =>
            {
                masterConfGUI.Draw();
            });
            
            foreach (var gui in this.packageGUIList)
            {
                gui.Draw();
            }
        }

        public void Dispose() { }
    }
}