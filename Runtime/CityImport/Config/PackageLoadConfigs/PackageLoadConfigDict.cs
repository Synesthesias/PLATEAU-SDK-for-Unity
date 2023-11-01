using System.Collections.Generic;
using System.Linq;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.Config.PackageLoadConfigs
{
    /// <summary>
    /// 都市インポートの設定のうち、パッケージごとのインポート設定をまとめたクラスです。
    /// 参照:
    /// 具体的な設定については <see cref="PackageLoadConfig"/>を参照してください。
    /// GUIについては PackageLoadConfigGUIList を参照してください。
    /// </summary>
    internal class PackageLoadConfigDict
    {
        private readonly Dictionary<PredefinedCityModelPackage, PackageLoadConfig> data;

        /// <summary>
        /// パッケージ種とLODの組から設定値を作ります。
        /// </summary>
        public PackageLoadConfigDict(PackageToLodDict dict)
        {
            data = new Dictionary<PredefinedCityModelPackage, PackageLoadConfig>();
            foreach (var (package, availableMaxLOD) in dict)
            {
                data.Add(package, PackageLoadConfig.CreateConfigFor(package, availableMaxLOD));
            }
        }
        
        /// <summary>
        /// 値ペア (パッケージ種, そのパッケージに関する設定) を、パッケージごとの IEnumerable にして返します。
        /// </summary>
        public IEnumerable<KeyValuePair<PredefinedCityModelPackage, PackageLoadConfig>> ForEachPackagePair =>
            this.data;

        public PackageLoadConfig GetConfigForPackage(PredefinedCityModelPackage package)
        {
            return this.data[package];
        }

        /// <summary>
        /// パッケージ種ごとの設定で、「ロードする」にチェックが入っているパッケージの配列を返します。
        /// </summary>
        public PredefinedCityModelPackage[] PackagesToLoad()
        {
            return ForEachPackagePair
                .Where(pair => pair.Value.LoadPackage)
                .Select(pair => pair.Key)
                .ToArray();
        }
    }
}