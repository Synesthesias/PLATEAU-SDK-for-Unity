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
        private static readonly ConcurrentDictionary<string, CityModel> cache = new ConcurrentDictionary<string, CityModel>();

        /// <summary>
        /// GMLファイルをパースし、<see cref="CityModel"/> を返します。
        /// パースには時間がかかりますが、結果はキャッシュに入るので、2回目以降はパースはスキップされて速いです。
        /// </summary>
        /// <param name="go">GMLファイルの名称が、与えられたゲームオブジェクトの名称であるものをパースします。</param>
        /// <param name="rootDirName">
        /// 都市データが入っているディレクトリのルートの名称を指定します。
        /// 省略した場合、 <paramref name="go"/>.transform.parent.name になります。
        /// </param>
        /// <param name="parentPathOfRootDir">
        /// <paramref name="rootDirName"/>の親ディレクトリのパスを指定します。
        /// 省略した場合、インポート時に自動でコピーされる場所になります。
        /// すなわち、 Assets/StreamingAssets/.PLATEAU になります。
        /// </param>
        /// <returns>パースした結果を返します。パース失敗時は null を返します。</returns>
        public static async Task<CityModel> LoadAsync(GameObject go, string rootDirName = null, string parentPathOfRootDir = null)
        {
            // デフォルト値は PLATEAUウィンドウで操作したときのインポート先です。
            parentPathOfRootDir ??= PathUtil.plateauSrcFetchDir;
            rootDirName ??= go.transform.parent.name;
            
            string gmlName = go.name;
            string gmlPath;
            try
            {
                using var gmlInfo = GmlFileInfo.Create(gmlName);
                string gmlFeatureDir = gmlInfo.FeatureType;
                gmlPath = Path.Combine(parentPathOfRootDir, rootDirName, PathUtil.UdxFolderName, gmlFeatureDir, gmlName);
            }
            catch (Exception)
            {
                Debug.LogError("Could not get gmlInfo from gmlName.");
                return null;
            }
            string gmlFullPath = Path.GetFullPath(Path.Combine(PathUtil.plateauSrcFetchDir, gmlPath));
            return await LoadAsync(gmlFullPath);
        }

        
        /// <summary>
        /// 非同期で GMLパスから <see cref="CityModel"/> を取得します。
        /// </summary>
        /// <returns>
        /// <see cref="CityModel"/> を返します。
        /// キャッシュにあればそれを返し、なければGMLファイルをパースして結果を返します。
        /// パースに失敗した場合 null を返します。
        /// パースの高速化のため、返す CityModel には属性情報が含まれますが、3Dモデル情報は含まれません。
        /// </returns>
        private static async Task<CityModel> LoadAsync(string gmlFullPath)
        {
            // キャッシュを確認します。
            if (cache.TryGetValue(gmlFullPath, out var cachedCityModel))
            {
                return cachedCityModel;
            }
            // GMLファイルをパースして返します。
            var cityModel = await LoadInnerAsync(gmlFullPath);
            cache.TryAdd(gmlFullPath, cityModel);
            return cityModel;
        }

        private static async Task<CityModel> LoadInnerAsync(string gmlFullPath)
        {

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
