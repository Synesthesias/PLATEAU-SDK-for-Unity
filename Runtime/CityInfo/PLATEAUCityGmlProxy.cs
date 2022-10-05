using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// GMLファイルから都市情報を読み込みます。
    /// </summary>
    public class PLATEAUCityGmlProxy
    {
        /// <summary>
        /// GMLパス と <see cref="CityModel"/> を紐付けるキャッシュです。
        /// </summary>
        private static ConcurrentDictionary<string, CityModel> cache = new ConcurrentDictionary<string, CityModel>();

        public async Task<CityModel> LoadAsync(GameObject go)
        {
            string gmlName = go.name;
            string gmlPath;
            try
            {
                using var gmlInfo = GmlFileInfo.Create(gmlName);
                string gmlFeatureDir = gmlInfo.FeatureType;
                string rootDirName = go.transform.parent.name;
                gmlPath = Path.Combine(PathUtil.plateauSrcFetchDir, rootDirName, PathUtil.UdxFolderName, gmlFeatureDir, gmlName);
            }
            catch (Exception)
            {
                Debug.LogError("Could not get gmlInfo from gmlName.");
                return null;
            }
            return await LoadAsync(gmlPath);
        }

        
        /// <summary>
        /// 非同期で GMLパスから <see cref="CityModel"/> を取得します。
        /// </summary>
        /// <param name="gmlRelativePath"> <see cref="PathUtil.plateauSrcFetchDir"/> からの相対パスでGMLファイルを指定します。 </param>
        /// <returns>
        /// <see cref="CityModel"/> を返します。
        /// キャッシュにあればそれを返し、なければGMLファイルをパースして結果を返します。
        /// パースに失敗した場合 null を返します。
        /// パースの高速化のため、返す CityModel には属性情報が含まれますが、3Dモデル情報は含まれません。
        /// </returns>
        public async Task<CityModel> LoadAsync(string gmlRelativePath)
        {
            // キャッシュを確認します。
            if (cache.TryGetValue(gmlRelativePath, out var cachedCityModel))
            {
                return cachedCityModel;
            }
            // GMLファイルをパースして返します。
            var cityModel = await LoadInnerAsync(gmlRelativePath);
            cache.TryAdd(gmlRelativePath, cityModel);
            return cityModel;
        }

        private static async Task<CityModel> LoadInnerAsync(string gmlRelativePath)
        {
            string gmlFullPath = Path.Combine(PathUtil.plateauSrcFetchDir, gmlRelativePath);
            
            if (!File.Exists(gmlFullPath))
            {
                Debug.LogError($"GMLファイルが存在しません。 : {gmlFullPath}");
                return null;
            }
            
            // 3Dモデルはすでに配置済みという前提で、GMLパース時に3Dモデル部分を省略することで高速化します。
            var parserParams = new CitygmlParserParams(true, false, ignoreGeometries: true);
            
            CityModel cityModel = null;
            try
            {
                // 時間がかかるので別スレッドで実行します。
                cityModel = await Task.Run(() => CityGml.Load(gmlFullPath, parserParams, DllLogCallback.UnityLogCallbacks));
            }
            catch (Exception e)
            {
                Debug.LogError($"GMLファイルのロードに失敗しました。 : {gmlFullPath}.\n{e.Message}");
            }

            return cityModel;
        }
    }
}
