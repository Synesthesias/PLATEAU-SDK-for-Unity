using System.Collections.Generic;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
#if UNITY_EDITOR
#endif


namespace PLATEAU.Editor.CityImport.PackageLodSettingGUIs
{
    /// <summary>
    /// インポート設定GUIのうち、パッケージ種ごとの設定GUI <see cref="PackageLoadSettingGUI"/>を、利用可能パッケージ分だけ集めたものです。
    /// </summary>
    internal class PackageLoadSettingGUIList : IEditorDrawable
    {
        private readonly List<PackageLoadSettingGUI> packageGUIList;

        public PackageLoadSettingGUIList(PackageToLodDict availablePackageLODDict, CityLoadConfig cityLoadConf)
        {
            // 初期化では、利用可能なパッケージ種1つにつき、それに対応するGUIインスタンスを1つ生成します。
            this.packageGUIList = new();
            foreach (var (package, maxLOD) in availablePackageLODDict)
            {
                if (maxLOD < 0)
                {
                    // cityLoadConf.GetConfigForPackage(package).LoadPackage = false;
                    continue;
                }

                var packageConf = cityLoadConf.GetConfigForPackage(package);

                // パッケージ種による場合分けです。
                // これと似たロジックが PackageLoadSetting.CreateSettingFor にあるので、変更時はそちらも合わせて変更をお願いします。
                var gui = package switch
                {
                    PredefinedCityModelPackage.Relief => new ReliefLoadSettingGUI((ReliefLoadSetting)packageConf, MeshCode.Parse(cityLoadConf.AreaMeshCodes[0])),
                    _ => new PackageLoadSettingGUI(packageConf)
                };
                this.packageGUIList.Add(gui);
            }
        }

        public void Draw()
        {
            foreach (var gui in this.packageGUIList)
            {
                gui.Draw();
            }
        }

        
    }
}