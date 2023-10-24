using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
#endif

namespace PLATEAU.GranularityConvert
{
    public class CityGranularityConverter
    {
        public async Task ConvertAsync(IReadOnlyList<GameObject> srcGameObjs, GranularityConvertOption option)
        {
            try
            {
                using var progressBar = new ProgressBar();
                progressBar.Display("属性情報を取得中...", 0.1f);

                // 属性情報を覚えておきます。
                var attributes = GmlIdToSerializedCityObj.ComposeFrom(srcGameObjs);

                progressBar.Display("ゲームオブジェクトを共通モデルに変換中...", 0.2f);

                // ゲームオブジェクトを共通ライブラリのModelに変換します。
                using var srcModel = UnityMeshToDllModelConverter.ConvertIncludingParents(srcGameObjs, true, false, ConvertVertex);

                progressBar.Display("共通モデルの変換中...", 0.5f);

                // 共通ライブラリの機能でモデルを分割・結合します。
                var converter = new GranularityConverter();
                using var dstModel = converter.Convert(srcModel, option);

                Transform parentTransform = srcGameObjs[0].transform;
                while (parentTransform != null && parentTransform.GetComponent<PLATEAUInstancedCityModel>() == null)
                {
                    parentTransform = parentTransform.parent;
                }

                progressBar.Display("変換前の3Dモデルを削除中...", 0.6f);
                foreach (var obj in srcGameObjs)
                {
                    Object.DestroyImmediate(obj);
                }

                progressBar.Display("変換後の3Dモデルを配置中...", 0.8f);

                // Modelをゲームオブジェクトに変換して配置します。
                var generatedObjs = new List<GameObject>();
                bool result = await PlateauToUnityModelConverter.PlateauModelToScene(generatedObjs,
                    parentTransform, new DummyProgressDisplay(), "", true,
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