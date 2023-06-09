using System.Collections;
using System.Collections.Generic;
using System.IO;
using PLATEAU.Dataset;
using UnityEngine;
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
        // base paths
        static readonly string mapMaterialDir = PathUtil.SdkPathToAssetPath("Materials");
        // default path
        const string mapMaterialNameBuiltInRP = "MapUnlitMaterial_BuiltInRP.mat";
        const string mapMaterialNameURP = "MapUnlitMaterial_URP.mat";
        const string mapMaterialNameHDRP = "MapUnlitMaterial_HDRP.mat";
        // fallback path
        const string fallbackFolderName = "Fallback";
        const string buildInFallBackFolder = "BuildInMaterials";
        const string uRPFallbackFolder = "URPMaterials";
        const string hDRPFallbackFolder = "HDRPMaterials";

        public static PipeLineType GetRenderPipelineType()
        {
            var pipelineAsset = GraphicsSettings.renderPipelineAsset;
            if (pipelineAsset == null)
            {   // Built-in Render Pipeline ???
                return PipeLineType.BuildIn;
            }
            else
            {   // URP ??? HDRP ???
                var pipelineName = pipelineAsset.GetType().Name;
                return pipelineName switch
                {
                    "UniversalRenderPipelineAsset" => PipeLineType.UniversalRenderPipelineAsset,
                    "HDRenderPipelineAsset" => PipeLineType.HDRenderPipelineAsset,
                    _ => throw new InvalidDataException("Unknown material for pipeline.")
                };
            }
        }

        public static string GetDefaultMatPath()
        {
            var plt = GetRenderPipelineType();
            var rtMat = "";
            if(plt == PipeLineType.BuildIn)
            {
                rtMat = mapMaterialNameBuiltInRP;
            }
            else if (plt == PipeLineType.UniversalRenderPipelineAsset)
            {
                rtMat = mapMaterialNameURP;
            }
            else if(plt == PipeLineType.HDRenderPipelineAsset)
            {
                rtMat = mapMaterialNameHDRP;
            }

            return Path.Combine(mapMaterialDir, rtMat);
        }

        public static string GetFallbackMaterialPath(PredefinedCityModelPackage pack)
        {
            string matPath = Path.Combine(mapMaterialDir, fallbackFolderName);
            var plt = GetRenderPipelineType();
            var fbp = "";

            if (plt == PipeLineType.BuildIn)
            {
                fbp = buildInFallBackFolder;
            }
            else if (plt == PipeLineType.UniversalRenderPipelineAsset)
            {
                fbp = uRPFallbackFolder;
            }
            else if (plt == PipeLineType.HDRenderPipelineAsset)
            {
                fbp = hDRPFallbackFolder;
            }

            matPath = Path.Combine(matPath, fbp);

            switch (pack)
            {
                case PredefinedCityModelPackage.Building:
                    matPath = Path.Combine(matPath, "DefaultBuildingMat.mat");
                    break;
                case PredefinedCityModelPackage.CityFurniture:
                    matPath = Path.Combine(matPath, "DefaultCityFurnitureMat.mat");
                    break;
                case PredefinedCityModelPackage.DisasterRisk:
                    matPath = Path.Combine(matPath, "DefaultDisasterRiskMat.mat");
                    break;
                case PredefinedCityModelPackage.LandUse:
                    matPath = Path.Combine(matPath, "DefaultLandUseMat.mat");
                    break;
                case PredefinedCityModelPackage.Relief:
                    matPath = Path.Combine(matPath, "DefaultReliefMat.mat");
                    break;
                case PredefinedCityModelPackage.Road:
                    matPath = Path.Combine(matPath, "DefaultRoadMat.mat");
                    break;
                case PredefinedCityModelPackage.Unknown:
                    matPath = Path.Combine(matPath, "DefaultUnknownMat.mat");
                    break;
                case PredefinedCityModelPackage.UrbanPlanningDecision:
                    matPath = Path.Combine(matPath, "DefaultUrbanPlanningDecisionMat.mat");
                    break;
                case PredefinedCityModelPackage.Vegetation:
                    matPath = Path.Combine(matPath, "DefaultVegetationMat.mat");
                    break;
            }

            return matPath;
        }
    }
}
