using System;
using System.IO;
#if UNITY_EDITOR
#endif
using UnityEngine.Rendering;

namespace PLATEAU.Util
{
    public enum PipeLineType
    {
        BuildIn,
        UniversalRenderPipelineAsset,
        HDRenderPipelineAsset
    }

    public static class MaterialPathUtil
    {
        //ベースパス
        public static readonly string BaseMaterialDir = PathUtil.SdkPathToAssetPath("Materials");
        // 地図表示用のデフォルトマテリアルパス
        private const string MapMaterialNameBuiltInRP = "MapUnlitMaterial_BuiltInRP.mat";
        private const string MapMaterialNameUrp = "MapUnlitMaterial_URP.mat";

        private const string MapMaterialNameHdrp = "MapUnlitMaterial_HDRP.mat";

        public static PipeLineType GetRenderPipelineType()
        {
            var pipelineAsset = GraphicsSettings.renderPipelineAsset;
            if (pipelineAsset == null)
            {   // Built-in Render Pipeline
                return PipeLineType.BuildIn;
            }
            else
            {   // URP or HDRP
                var pipelineName = pipelineAsset.GetType().Name;
                return pipelineName switch
                {
                    "UniversalRenderPipelineAsset" => PipeLineType.UniversalRenderPipelineAsset,
                    "HDRenderPipelineAsset" => PipeLineType.HDRenderPipelineAsset,
                    _ => throw new InvalidDataException("Unknown material for pipeline.")
                };
            }
        }

        public static string GetMapMatPath()
        {
            var pipeline = GetRenderPipelineType();
            var materialFileName = pipeline switch
            {
                PipeLineType.BuildIn => MapMaterialNameBuiltInRP,
                PipeLineType.UniversalRenderPipelineAsset => MapMaterialNameUrp,
                PipeLineType.HDRenderPipelineAsset => MapMaterialNameHdrp,
                _ => throw new Exception("Unknown pipeline type.")
            };

            return Path.Combine(BaseMaterialDir, materialFileName);
        }

        
    }
}
