using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// Unityのランタイムでゲームオブジェクト名から <see cref="CityGML.CityObject"/> を取得します。
    /// </summary>
    public class CityModelLoader
    {
        private readonly GmlToCityModelDict fileToCityModelCache = new GmlToCityModelDict();

        /// <summary>
        /// <paramref name="gameObj"/> に対応する <see cref="CityObject"/> を
        /// <paramref name="cityMetadata"/> を使ってロードします。
        /// </summary>
        /// <returns><see cref="CityObject"/>を返します。例外時はnullを返します。</returns>
        public CityObject Load(GameObject gameObj, CityMetadata cityMetadata)
        {
            if (cityMetadata == null)
            {
                Debug.LogError("argument cityMetadata is null.");
                return null;
            }
            
            string gmlFileName = FindGmlNameByMetadata(gameObj ,cityMetadata);
            if (gmlFileName == null) return null;


            // gameObjName から cityObjId を取得します。
            bool succeed = GameObjNameParser.TryGetId(gameObj.name, out string cityObjId);
            if (!succeed)
            {
                Debug.LogError($"{nameof(gameObj.name)} is invalid formatted.");
                return null;
            }
            
            // 名前が gmlFileName である gmlファイルを検索します。
            // gmlファイルは StreamingAssets フォルダ内にあることを前提とします（そうでないと実行時に読めないので）。
            
            // キャッシュにあればそれを返します。
            if (this.fileToCityModelCache.TryGetValue(gmlFileName, out CityModel cityModel))
            {
                return GetCityObjectById(cityModel, cityObjId);
            }
            
            string udxFullPath = cityMetadata.cityImportConfig.sourcePath.UdxFullPath();
            // udxフォルダは StreamingAssets フォルダにあることを前提とします。
            if (!PathUtil.IsSubDirectory(udxFullPath, PlateauUnityPath.StreamingFolder))
            {
                Debug.Log($"udxFullPath = {udxFullPath}\nstreamingFolder = {PlateauUnityPath.StreamingGmlFolder}");
                throw new IOException(
                    $"Could not find gml file, because udx path is not in StreamingAssets folder.\nudxFullPath = {udxFullPath}");
            }
            string gmlPath = SearchGmlPath(gmlFileName);
            // GMLパース設定は、高速であることを重視し、ジオメトリはパースしない設定とします。
            var loadedModel = CityGml.Load(gmlPath, new CitygmlParserParams(true, false, true), DllLogCallback.UnityLogCallbacks);
            if (loadedModel == null)
            {
                return null;
            }

            this.fileToCityModelCache.Add(gmlFileName, loadedModel);
            
            return GetCityObjectById(loadedModel, cityObjId);
        }

        private static string SearchGmlPath(string gmlFileNameWithoutExtension)
        {
            string foundGmlPath = Directory.EnumerateFiles(Application.streamingAssetsPath, gmlFileNameWithoutExtension + ".gml",
                SearchOption.AllDirectories).First();
            foundGmlPath = Path.GetFullPath(foundGmlPath); // Windowsでパスの区切り記号が '/' と '\' で混在するのを統一します。
            return foundGmlPath;
        }

        private static CityObject GetCityObjectById(CityModel model, string id)
        {
            return model.GetCityObjectById(id);
        }

        /// <summary>
        /// <paramref name="gameObj"/> に対応する gmlファイル名を <see cref="CityMetadata"/> から探します。
        /// </summary>
        private static string FindGmlNameByMetadata(GameObject gameObj, CityMetadata cityMetadata)
        {
            // テーブルから cityObjectId に対応する gmlFileName を検索します。
            if( !cityMetadata.TryGetValueFromGmlTable(gameObj.name, out var gmlFileName))
            {
                return null;
            }
            return gmlFileName;
        }

        /// <summary>
        /// <paramref name="gameObj"/> に対応する Gmlファイル名を、ヒエラルキー構造を利用して探して返します。
        /// <see cref="CityBehaviour"/> の子の GameObject の名称に Gmlファイル名が含まれることを利用し、
        /// <paramref name="gameObj"/> の親をさかのぼることで gmlファイル名を探します。
        /// <paramref name="gameObj"/> の親(再帰的)に <see cref="CityBehaviour"/> がなければ処理は失敗し、nullを返します。
        /// </summary>
        private static string FindGmlNameByHierarchy(GameObject gameObj)
        {
            var parentTrans = gameObj.transform.parent;
            // 親がなければ失敗します。
            if (parentTrans == null) return null;
            // 親が CityBehaviour なら、gameObjの名称が Gmlファイル名に対応します。
            if (parentTrans.GetComponent<CityBehaviour>() != null)
            {
                return gameObj.name;
            }
            // 再帰的に親を探します。
            return FindGmlNameByHierarchy(parentTrans.gameObject);
        }
    }
}