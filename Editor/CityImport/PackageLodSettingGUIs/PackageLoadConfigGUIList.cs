using System.Collections.Generic;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
#if UNITY_EDITOR
#endif


namespace PLATEAU.Editor.CityImport.PackageLodSettingGUIs
{
    /// <summary>
    /// インポート設定GUIのうち、パッケージ種ごとの設定GUI <see cref="PackageLoadConfigGUI"/>を、利用可能パッケージ分だけ集めたものです。
    /// 参照:
    /// 具体的な設定GUIについては<see cref="PackageLoadConfigGUI"/>を参照してください。
    /// 設定値のクラスについては <see cref="PackageLoadConfigDict"/> を参照してください。
    /// </summary>
    internal class PackageLoadConfigGUIList : IEditorDrawable
    {
        private readonly List<PackageLoadConfigGUI> packageGUIList;

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
                    PredefinedCityModelPackage.Relief => new ReliefLoadConfigGUI((ReliefLoadConfig)packageConf, MeshCode.Parse(cityLoadConf.AreaMeshCodes[0])),
                    _ => new PackageLoadConfigGUI(packageConf)
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

        public void Dispose() { }
    }
}