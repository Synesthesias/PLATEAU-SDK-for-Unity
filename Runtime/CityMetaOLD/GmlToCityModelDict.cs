using PLATEAU.CityGML;
using System;
using System.Collections.Generic;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// Gmlファイル名と <see cref="CityModel"/> を対応付ける辞書です。
    /// Dispose時に辞書内の各 CityModel を Dispose します。
    /// </summary>
    internal class GmlToCityModelDict : IDisposable
    {
        private readonly Dictionary<string, CityModel> dict = new Dictionary<string, CityModel>();
        private bool isDisposed;

        public void Add(string gmlFileName, CityModel cityModel)
        {
            this.dict.Add(gmlFileName, cityModel);
        }

        public bool TryGetValue(string gmlFileName, out CityModel cityModel)
        {
            return this.dict.TryGetValue(gmlFileName, out cityModel);
        }

        /// <summary>
        /// <see cref="CityModel"/> は利用後は Dispose する必要があります。
        /// そうでないと、Unity終了まで GMLファイル利用プロセスが解放されなくなります。
        /// </summary>
        ~GmlToCityModelDict()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.isDisposed) return;
            foreach (var pair in this.dict)
            {
                pair.Value.Dispose();
            }

            this.isDisposed = true;
        }
    }
}