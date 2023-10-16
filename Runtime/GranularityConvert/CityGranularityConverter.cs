using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using CityObjectList = PLATEAU.PolygonMesh.CityObjectList;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.GranularityConvert
{
    public class CityGranularityConverter
    {
        public async Task ConvertAsync(IReadOnlyList<GameObject> srcGameObjs, GranularityConvertOption option)
        {
            try
            {
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("PLATEAU", "属性情報を取得中...", 0.1f);
#endif

                // 属性情報を覚えておきます。
                var attributes = GmlIdToSerializedCityObj.ComposeFrom(srcGameObjs);

#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("PLATEAU", "3Dモデルを出力中...", 0.25f);
#endif

                // ゲームオブジェクトを共通ライブラリのModelに変換します。
                using var srcModel = UnityMeshToDllModelConverter.Convert(srcGameObjs, true, false, ConvertVertex);

#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("PLATEAU", "3Dモデルの変換中...", 0.5f);
#endif

                // 共通ライブラリの機能でモデルを分割・結合します。
                var converter = new GranularityConverter();
                using var dstModel = converter.Convert(srcModel, option);

#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("PLATEAU", "変換後の3Dモデルを配置中...", 0.8f);
#endif

                // Modelをゲームオブジェクトに変換して配置します。
                var generatedObjs = new List<GameObject>();
                bool result = await PlateauToUnityModelConverter.PlateauModelToScene(generatedObjs,
                    null, new DummyProgressDisplay(), "", true,
                    null, null, dstModel,
                    new AttributeDataHelper(new SerializedCityObjectGetterFromDict(attributes), option.Granularity,
                        true), true);
                if (!result)
                {
                    throw new Exception("Failed to convert plateau model to scene game objects.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
            
            #if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
            #endif
            
        }
        
        private static PlateauVector3d ConvertVertex(Vector3 src)
        {
            return new PlateauVector3d(src.x, src.y, src.z);
        } 
    }


    public class SerializedCityObjectGetterFromDict : ISerializedCityObjectGetter
    {
        private GmlIdToSerializedCityObj data;

        public SerializedCityObjectGetterFromDict(GmlIdToSerializedCityObj dict)
        {
            data = dict;
        }

        public CityInfo.CityObjectList.CityObject GetByID(string gmlID, CityObjectIndex? _)
        {
            if (data.TryGet(gmlID, out var serializedCityObj))
            {
                return serializedCityObj;
            }
            else
            {
                Debug.LogWarning($"gmlID not found : {gmlID}");
                return null;
            }
        }

        public void Dispose()
        {
            // NOP
        }
    }
}