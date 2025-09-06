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
        // 地図表示用のデフォルトマテリアル名
        private const string MapMaterialNameBuiltInRP = "MapUnlitMaterial_BuiltInRP";
        private const string MapMaterialNameUrp = "MapUnlitMaterial_URP";
        private const string MapMaterialNameHdrp = "MapUnlitMaterial_HDRP";

        public static PipeLineType GetRenderPipelineType()
        {
            var pipelineAsset = GraphicsSettings.defaultRenderPipeline;
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

        public static string GetMapMatName()
        {
            var pipeline = GetRenderPipelineType();
            var materialFileName = pipeline switch
            {
                PipeLineType.BuildIn => MapMaterialNameBuiltInRP,
                PipeLineType.UniversalRenderPipelineAsset => MapMaterialNameUrp,
                PipeLineType.HDRenderPipelineAsset => MapMaterialNameHdrp,
                _ => throw new Exception("Unknown pipeline type.")
            };

            return materialFileName;
        }

        
    }
}
