using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Behaviour;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Editor.Diagnostics;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.CityImport
{
    internal static class CityMeshPlacerModel
    {
        /// <summary>
        /// 都市モデルをシーンに配置します。
        /// 
        /// 引数の <paramref name="metadata"/> に記録されたパスのGMLファイルをロードし、それに対応する変換済みの3Dモデルを探し、それを配置対象（PlaceConfigで配置する設定なら）とします。
        /// 3Dモデルはすでに生成済みであることが前提です。
        /// 引数 <paramref name="placeConfig"/> の設定に従って配置します。
        /// GMLファイルをロードする代わりに、すでにロード済みの <see cref="CityModel"/> がある場合は、
        /// 引数 <paramref name="gmlToCityModelCache"/> に渡すことでGMLロード時間を節約できます。
        /// </summary>
        /// <param name="placeConfig">配置の設定です。</param>
        /// <param name="metadata">このメタデータに記録されたパスのGMLファイルに対応する変換済みの3Dモデルを配置対象とします。</param>
        ///
        /// <param name="gmlToCityModelCache">
        /// 高速化のためのキャッシュです。
        /// 実装意図は、インポート時に3Dモデル変換でGMLがパースされ、続くメッシュ配置でもGMLがパースされて二度手間だったのを一度で済ませて処理時間を削減するために設けました。
        /// ロード済みの <see cref="CityModel"/> のキャッシュです。
        /// なければ null で構いませんが、あれば GMLのパースにかかる時間を節約できます。
        /// 辞書であり、キーはGMLファイル名(拡張子抜き)で、値はCityModelです。
        /// </param>
        public static void Place(CityMeshPlacerConfig placeConfig, CityMetadata metadata, GmlToCityModelDict gmlToCityModelCache)
        {
            string rootDirName = metadata.cityImportConfig.rootDirName;
            
            var rootGameObj = PlaceCityBehaviourGameObject(rootDirName, metadata);
            
            string[] gmlRelativePaths = metadata.gmlRelativePaths;

            var timer = new TimeDiagnosticsTable(); // 時間計測
            
            // ループ : gmlファイルごと
            int numGml = gmlRelativePaths.Length;
            for (int i = 0; i < numGml; i++)
            {
                var gmlRelativePath = gmlRelativePaths[i];
                EditorUtility.DisplayProgressBar("配置中", $"[{i}/{numGml}] {GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath)}.gml", (float)i/numGml);
                PlaceGmlModel(metadata, gmlRelativePath, placeConfig, rootGameObj.transform, timer, gmlToCityModelCache);
            }
            EditorUtility.ClearProgressBar();
            Debug.Log($"[メッシュ配置 処理時間ログ]\n処理別 : \n{timer.SummaryByProcess()}\nデータ別 :\n{timer.SummaryByData()}");
        }

        private static GameObject PlaceCityBehaviourGameObject(string rootDirName, CityMetadata metadata)
        {
            // すでに配置されている場合は削除します。
            var oldObj = GameObject.Find(rootDirName);
            if(oldObj != null) GameObjectUtil.DestroyChildOf(oldObj);
            
            // Plateau元データのルートフォルダと同名の ルートGame Objectを作ります。 
            var rootGameObj = GameObjectUtil.AssureGameObject(rootDirName);
            
            // ルートGameObjectに CityBehaviour をアタッチしてメタデータをリンクします。
            var cityBehaviour = GameObjectUtil.AssureComponent<CityBehaviour>(rootGameObj);
            cityBehaviour.CityMetadata = metadata;
            return rootGameObj;
        }

        /// <summary>
        /// シーン配置の gmlファイルごとの処理です。
        /// </summary>　
        private static void PlaceGmlModel(CityMetadata metadata, string gmlRelativePath,
            CityMeshPlacerConfig placeConfig, Transform parentTrans, TimeDiagnosticsTable timer, GmlToCityModelDict gmlToCityModelCache)
        {
            var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
            var placeMethod = placeConfig.GetPerTypeConfig(gmlType).placeMethod;

            if (placeMethod == CityMeshPlacerConfig.PlaceMethod.DoNotPlace) return;
            
            string gmlFileName = $"{GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath)}.gml";
            timer.Start("ParseGml", gmlFileName);
            var cityModel = LoadGml(metadata, gmlRelativePath, gmlToCityModelCache);
            if (cityModel == null)
            {
                Debug.LogError($"Could not read gml file: {gmlRelativePath}");
                return;
            }

            // gmlファイル名と同名のGameObjectをルート直下に作ります。
            var gmlGameObj =
                GameObjectUtil.AssureGameObject(gmlFileName);
            gmlGameObj.transform.parent = parentTrans;


            // 3Dモデルファイルへの変換でのLOD範囲
            var lodRange = metadata.cityImportConfig.objConvertTypesConfig.GetLodRangeForType(gmlType);
            int selectedLod = placeConfig.GetPerTypeConfig(gmlType).selectedLod;
            lodRange = placeMethod.LodRangeToPlace(lodRange, selectedLod);

            var primaryCityObjs = cityModel.GetCityObjectsByType(PrimaryCityObjectTypes.PrimaryTypeMask);
            
            timer.Start("Place3dModels", gmlFileName);

            // ループ : LODごと
            for (int currentLod = lodRange.Max; currentLod >= lodRange.Min; currentLod--)
            {
                bool anyModelExist = PlaceGmlModelOfLod(currentLod, gmlRelativePath, metadata, primaryCityObjs, gmlGameObj.transform);
                
                if (anyModelExist && !placeMethod.DoesAllowMultipleLodPlaced())
                {
                    // 今のLODですでにモデルが配置されており、かつ複数LODを同時に配置しない設定ならば、
                    // これ以下のLODの配置はスキップします。
                    break;
                }
            }
        }

        /// <summary>
        /// シーン配置について、gmlファイルごとの処理の内部の、LODごとの処理です。
        /// </summary>
        private static bool PlaceGmlModelOfLod(int lod, string gmlRelativePath, CityMetadata metadata, IReadOnlyCollection<CityObject> primaryCityObjs, Transform parentTrans)
        {
            // 対応する3Dモデルファイルを探します。
            var foundObj = FindObjFile(metadata, lod, gmlRelativePath);
            if (foundObj == null) return false;

            bool anyModelExist = false;
            
            // 例外的に、メッシュ分けの粒度が PerCityModelArea のとき、
            // 主要地物は存在せず、3Dモデルファイル内にメッシュが1つしか存在しないので
            // それを配置して return します。
            if (metadata.cityImportConfig.meshGranularity == MeshGranularity.PerCityModelArea)
            {
                // この設定下では、3Dモデルファイルの中の唯一のメッシュの名前は ファイル名と同一になります。
                string meshName = ModelFileNameParser.FileNameWithoutExtension(lod, gmlRelativePath);
                var placed = PlaceToScene(foundObj, meshName, parentTrans);
                if (placed == null)
                {
                    Debug.Log($"not found.");
                }
                return placed != null;
            }
            
            var type = GmlFileNameParser.GetGmlTypeEnum(gmlRelativePath);
            var typeConf = metadata.cityImportConfig.cityMeshPlacerConfig.GetPerTypeConfig(type);

            // ループ : 主要地物ごと
            foreach (var primaryCityObj in primaryCityObjs)
            {
                // この LOD で 主要地物モデルが存在するなら、それを配置します。
                string primaryGameObjName = GameObjNameParser.ComposeName(lod, primaryCityObj.ID);
                
                var primaryGameObj = PlaceToSceneIfConditionMatch(foundObj, primaryGameObjName, parentTrans, primaryCityObj, typeConf);


                // このLODで主要地物が存在しないなら、空のGameObjectのみ用意します。
                bool isPrimaryGameObjEmpty = false;
                if (primaryGameObj == null)
                {
                    string primaryObjName = GameObjNameParser.ComposeName(lod, primaryCityObj.ID);
                    // 古いGameObjectがあれば削除します。
                    var oldPrimaryTrans = GameObjectUtil.FindRecursive(parentTrans, primaryObjName);
                    if (oldPrimaryTrans != null) Object.DestroyImmediate(oldPrimaryTrans.gameObject);
                    // 空のGameObjectを作ります。
                    primaryGameObj = new GameObject(primaryObjName)
                    {
                        transform =
                        {
                            parent = parentTrans
                        }
                    };
                    isPrimaryGameObjEmpty = true;
                }
                else
                {
                    anyModelExist = true;
                }

                // LOD <= 1 の場合 : 主要地物を配置すれば完了となります。主要でない地物の配置をスキップします。
                // （メッシュの結合単位に関わらず、 LOD <= 1 では主要地物より細かいものは出てきません。）
                if (lod <= 1)
                {
                    if (isPrimaryGameObjEmpty)
                    {
                        // 子のない空のオブジェクトは不要なので削除します。
                        Object.DestroyImmediate(primaryGameObj);
                    }
                    continue;
                }

                // LOD >= 2 の場合 : 子の CityObject をそれぞれ配置します。
                // （メッシュの結合単位が最小地物の場合に出てくる細かいモデルです。）
                var childCityObjs = primaryCityObj.CityObjectDescendantsDFS;

                // 主要地物の子をすべて配置します。
                bool placedAnyChild = PlaceCityObjsIfConditionMatch(foundObj, childCityObjs.ToArray(), primaryGameObj.transform, typeConf);
                anyModelExist |= placedAnyChild;
                
                // 子の数がゼロで、親も空（メッシュがない）なら、親は不要なので削除します。
                if (!placedAnyChild && isPrimaryGameObjEmpty)
                {
                    Object.DestroyImmediate(primaryGameObj);
                }

            } // ループ ここまで (主要地物ごと) 

            return anyModelExist;
        }

        

        /// <summary>
        /// GMLファイルをロードします。
        /// 引数のキャッシュにあればそれを返し、なければGMLをパースして <see cref="CityModel"/> を返します。
        /// </summary>
        private static CityModel LoadGml(CityMetadata metadata, string gmlRelativePath, GmlToCityModelDict gmlToCityModeCache)
        {
            string gmlFileName = GmlFileNameParser.FileNameWithoutExtension(gmlRelativePath);
            if (gmlToCityModeCache != null && gmlToCityModeCache.TryGetValue(gmlFileName, out var cachedCityModel))
            {
                return cachedCityModel;
            }
            string gmlFullPath = metadata.cityImportConfig.sourcePath.UdxRelativeToFullPath(gmlRelativePath);
            // tessellate を false にすることで、3Dモデルができない代わりにパースが高速になります。
            var gmlParserParams = new CitygmlParserParams(true, false);
            var cityModel = CityGml.Load(gmlFullPath, gmlParserParams, DllLogCallback.UnityLogCallbacks);
            if (cityModel == null)
            {
                Debug.LogError($"failed to load city model.\ngmlFullPath = {gmlFullPath}");
            }
            return cityModel;
        }

        /// <summary>
        /// LOD, gml に対応する3Dモデルファイルを探します。
        /// </summary>
        private static ObjInfo FindObjFile(CityMetadata metadata, int lod, string gmlRelativePath)
        {
            var objInfos = metadata.cityImportConfig.generatedObjFiles;
            string targetObjName = ModelFileNameParser.FileName(lod, gmlRelativePath);
            return objInfos.FirstOrDefault(info => Path.GetFileName(info.assetsPath) == targetObjName);
        }
        

        /// <summary>
        /// <see cref="PlaceToSceneIfConditionMatch"/> の複数版です。
        /// </summary>
        /// <returns>1つでも3Dモデルを配置したら true、そうでなければ false を返します。</returns>
        private static bool PlaceCityObjsIfConditionMatch(ObjInfo objInfo, ICollection<CityObject> cityObjs, Transform parent, ScenePlacementConfigPerType typeConfig)
        {
            bool anyModelPlaced = false;
            foreach (var cityObj in cityObjs)
            {
                string gameObjName = GameObjNameParser.ComposeName(objInfo.lod, cityObj.ID);
                var placed = PlaceToSceneIfConditionMatch(objInfo, gameObjName, parent, cityObj, typeConfig);
                if (placed != null)
                {
                    anyModelPlaced = true;
                }
            }

            return anyModelPlaced;
        }

        private static GameObject PlaceToSceneIfConditionMatch(ObjInfo objInfo, string objName,
            Transform parentTransform, CityObject cityObj, ScenePlacementConfigPerType configPerType)
        {
            if (!IsCityObjectMatchesTypeFlagsConfig(cityObj, configPerType))
            {
                return null;
            }
            return PlaceToScene(objInfo, objName, parentTransform);
        }

        /// <summary>
        /// <paramref name="objInfo"/> の3Dモデルメッシュのうち、名前が <paramref name="objName"/> であるものを探します。
        /// あればシーンに配置して、それを返します。配置のとき、すでに同名のものがある場合は削除してから配置します。
        /// なければ null を返します。
        /// </summary>
        private static GameObject PlaceToScene(ObjInfo objInfo, string objName, Transform parentTransform)
        {
            // LOD >=2 のときに親と子でまったく同じオブジェクトが配置されてしまう場合があり、それを回避します。
            if (parentTransform.gameObject.name == objName) return null;

            var gameObj = objInfo.GetGameObjByName(objName);
            bool isValidMeshObj = gameObj != null && gameObj.GetComponent<MeshFilter>() != null;
            if (!isValidMeshObj)
            {
                return null;
            }
            
            // すでに同名のものがある場合は削除します。
            GameObjectUtil.DestroyChildOf(parentTransform, objName);
                    
            // メッシュをシーンに配置します。
            var newGameObj = Object.Instantiate(gameObj, parentTransform, true);
            newGameObj.name = newGameObj.name.Replace("(Clone)", "");
            return newGameObj;
        }

        /// <summary>
        /// 配置する <see cref="CityObject"/> が、
        /// 配置設定における 配置対象 <see cref="CityObjectType"/> に含まれているかどうかを bool で返します。
        /// </summary>
        private static bool IsCityObjectMatchesTypeFlagsConfig(CityObject cityObj, ScenePlacementConfigPerType placeConfPerType)
        {
            var coType = cityObj.Type;
            bool matches = ((ulong)coType & placeConfPerType.cityObjectTypeFlags) != 0;
            return matches;
        }
        

    }
}