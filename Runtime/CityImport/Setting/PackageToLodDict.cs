using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// パッケージとそこに含まれる最大LODの組です。
    /// </summary>
    public class PackageToLodDict
    {
        private readonly ConcurrentDictionary<PredefinedCityModelPackage, int> data = new ConcurrentDictionary<PredefinedCityModelPackage, int>();
        
        public void AddOrUpdate(PredefinedCityModelPackage package, int maxLod)
        {
            this.data.AddOrUpdate(package,
                _ => maxLod, 
                (_, __) => maxLod);
        }

        /// <summary>
        /// 引数の <paramref name="package"/> で利用可能な最大LODを返します。
        /// ない場合は -1 を返します。
        /// </summary>
        public int GetLod(PredefinedCityModelPackage package)
        {
            if (!this.data.TryGetValue(package, out int lod))
            {
                return -1;
            }

            return lod;
        }

        public void MargeDict(PackageToLodDict other)
        {
            foreach (var pair in other)
            {
                var package = pair.Key;
                int otherLod = pair.Value;
                MergePackage(package, otherLod);
            }
        }

        public void MergePackage(PredefinedCityModelPackage package, int otherLod)
        {
            this.data.AddOrUpdate(package,
                _ => otherLod,
                (modelPackage, lod) => Math.Max(otherLod, lod));
        }

        public bool Contains(PredefinedCityModelPackage package)
        {
            return this.data.ContainsKey(package);
        }
        
        public IEnumerator<KeyValuePair<PredefinedCityModelPackage, int>> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
    }
}
