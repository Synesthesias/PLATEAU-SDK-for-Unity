using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.Util;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System;

namespace PLATEAU.TerrainConvert
{
    public class CityTerrainConverter
    {
        public async Task<TerrainConvertResult> ConvertAsync(TerrainConvertOption option)
        {
            try
            {
                using var progressBar = new ProgressBar();
                progressBar.Display("地形モデルを変換中...", 0.1f);
                var unityMeshToDllSubMeshConverter = new UnityMeshToDllSubMeshWithTexture(true);
                var srcTransforms = new UniqueParentTransformList(option.SrcGameObjs.Select(obj=>obj.transform)) ;

                if (srcTransforms.Count == 0)
                {
                    Debug.Log("対象がないためテレイン化をスキップします。");
                    return TerrainConvertResult.Fail();
                }
                // ゲームオブジェクトを共通ライブラリのModelに変換します。
                using var srcModel = UnityMeshToDllModelConverter.Convert(
                    srcTransforms,
                    unityMeshToDllSubMeshConverter,
                    false, 
                    VertexConverterFactory.NoopConverter());

                progressBar.Display("モデルを配置中...", 0.7f);
                var result = await PlateauToUnityTerrainConverter.PlateauTerrainToScene(
                    option.SrcGameObjs,
                    new DummyProgressDisplay(), "", option, srcModel,true);

                if (!result.IsSucceed)
                {
                    throw new Exception("Failed to convert plateau model to scene game objects.");
                }
                
                if (result.GeneratedObjs.Count <= 0)
                {
                    Dialogue.Display("出力結果がありません。", "OK");
                    return TerrainConvertResult.Fail();
                }

                if (option.DoDestroySrcObjs)
                {
                    foreach (var srcObj in option.SrcGameObjs)
                    {
                        GameObject.DestroyImmediate(srcObj);
                    }
                }

#if UNITY_EDITOR
                // 変換後のゲームオブジェクトを選択状態にします。
                Selection.objects = result.GeneratedObjs.Select(go => (GameObject)go).ToArray();
#endif
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                return TerrainConvertResult.Fail();
            }
        }
    }
}
