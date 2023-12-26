using System.Collections.Generic;
using System.Linq;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.Config.PackageImportConfigs
{
    /// <summary>
    /// 都市インポートの設定のうち、パッケージごとのインポート設定をまとめたクラスです。
    /// 参照:
    /// 具体的な設定については <see cref="PackageImportConfig"/>を参照してください。
    /// GUIについては PackageLoadConfigGUIList を参照してください。
    /// </summary>
    public class PackageImportConfigDict
    {
        private readonly Dictionary<PredefinedCityModelPackage, PackageImportConfig> data;

        /// <summary>
        /// パッケージ種とLODの組から設定値を作ります。
        /// </summary>
        public PackageImportConfigDict(PackageToLodDict dict)
        {
            data = new Dictionary<PredefinedCityModelPackage, PackageImportConfig>();
            foreach (var (package, availableMaxLOD) in dict)
            {
                data.Add(package, PackageImportConfig.CreateConfigFor(package, availableMaxLOD));
            }
        }
        
        /// <summary>
        /// 値ペア (パッケージ種, そのパッケージに関する設定) を、パッケージごとの IEnumerable にして返します。
        /// </summary>
        public IEnumerable<KeyValuePair<PredefinedCityModelPackage, PackageImportConfig>> ForEachPackagePair =>
            this.data;

        public PackageImportConfig GetConfigForPackage(PredefinedCityModelPackage package)
        {
            return this.data[package];
        }

        /// <summary>
        /// パッケージ種ごとの設定で、「ロードする」にチェックが入っているパッケージの配列を返します。
        /// </summary>
        public PredefinedCityModelPackage[] PackagesToLoad()
        {
            return ForEachPackagePair
                .Where(pair => pair.Value.ImportPackage)
                .Select(pair => pair.Key)
                .ToArray();
        }
    }
}