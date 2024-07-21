using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.MaterialAdjust;
using PLATEAU.Util;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2
{
    /// <summary>
    /// タイプによるマテリアル分けを実行します。
    /// </summary>
    internal class MAExecutorV2ByType : IMAExecutorV2
    {
        public async Task<UniqueParentTransformList> ExecAsync(MAExecutorConf conf)
        {
            using var adjuster = MaterialAdjusterByType.Create();
            var common = new MAExecutorV2Common();
            common.Prepare(conf.TargetTransforms, out var nonLibData, out var materialRegistry, out var sceneResult);
            SendTypesToCpp(conf, adjuster);
            SendConfToCpp(conf, materialRegistry, adjuster);
            foreach (var target in conf.TargetTransforms.Get)
            {
                var model = common.ConvertToCppModel(target, materialRegistry);
                adjuster.Exec(model);
                await common.PlaceModelToSceneAsync(model, target, materialRegistry, nonLibData, sceneResult);
            }
            common.Finishing(sceneResult, nonLibData, conf);
            return sceneResult.GeneratedRootTransforms;
        }

        private void SendTypesToCpp(MAExecutorConf conf , MaterialAdjusterByType adjuster)
        {
            var matConfigs = (MAMaterialConfig<CityObjectTypeHierarchy.Node>)conf.MaterialAdjustConf;
            conf.TargetTransforms.BfsExec(trans =>
            {
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog == null) return NextSearchFlow.Continue;
                foreach (var co in cog.GetAllCityObjects())
                {
                    var type = co.type;
                    var matConfig = matConfigs.GetConfFor(type.ToTypeNode());
                    if (matConfig != null)
                    {
                        adjuster.RegisterType(co.GmlID, type);
                    }
                }
                return NextSearchFlow.Continue;
            });
        }

        private void SendConfToCpp(MAExecutorConf conf, GameMaterialIDRegistry materialRegistry, MaterialAdjusterByType adjuster)
        {
            var materialsConf = (MAMaterialConfig<CityObjectTypeHierarchy.Node>)conf.MaterialAdjustConf;
            for (int i = 0; i < materialsConf.Length; i++)
            {
                var typeNode = materialsConf.GetKeyAt(i);
                var matConf = materialsConf.GetMaterialChangeConfAt(i);
                if (!matConf.ChangeMaterial) continue;
                materialRegistry.TryAddMaterial(matConf.Material, out var matID);
                var types = typeNode.Types;
                if (types == null) continue;
                foreach (var t in types)
                {
                    bool registered = adjuster.RegisterMaterialPattern(t, matID);
                    if (!registered)
                    {
                        Debug.LogWarning($"duplicate type: {t}");
                    }
                }
            }
        }
    }
}