using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PLATEAU.Dataset;
using UnityEditor;
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
        //ベースパス
        private static readonly string baseMaterialDir = PathUtil.SdkPathToAssetPath("Materials");
        // 地図表示用のデフォルトマテリアルパス
        private const string MapMaterialNameBuiltInRP = "MapUnlitMaterial_BuiltInRP.mat";
        private const string MapMaterialNameUrp = "MapUnlitMaterial_URP.mat";

        private const string MapMaterialNameHdrp = "MapUnlitMaterial_HDRP.mat";
        // フォールバックマテリアルのパス
        private const string FallbackFolderName = "Fallback";
        private const string BuildInFallBackFolder = "BuildInMaterials";
        private const string URPFallbackFolder = "URPMaterials";
        private const string HDRPFallbackFolder = "HDRPMaterials";

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

            return Path.Combine(baseMaterialDir, materialFileName);
        }

        /// <summary>
        /// 引数のパッケージ種に対応するデフォルトフォールバックマテリアルをロードして返します。
        /// 引数に対応するものがない、またはロードに失敗した場合はUnityのデフォルトマテリアルを返します。
        /// </summary>
        public static Material LoadDefaultFallbackMaterial(PredefinedCityModelPackage pack)
        {
            var pipeline = GetRenderPipelineType();

            var folderNamePipeline = pipeline switch
            {
                PipeLineType.BuildIn => BuildInFallBackFolder,
                PipeLineType.UniversalRenderPipelineAsset => URPFallbackFolder,
                PipeLineType.HDRenderPipelineAsset => HDRPFallbackFolder,
                _ => throw new Exception("Unknown pipeline type.")
            };

            string matFileName = pack switch
            {
                PredefinedCityModelPackage.Building => "DefaultBuildingMat.mat",
                PredefinedCityModelPackage.CityFurniture => "DefaultCityFurnitureMat.mat",
                PredefinedCityModelPackage.DisasterRisk => "DefaultDisasterRiskMat.mat",
                PredefinedCityModelPackage.LandUse => "DefaultLandUseMat.mat",
                PredefinedCityModelPackage.Relief => "DefaultReliefMat.mat",
                PredefinedCityModelPackage.Road => "DefaultRoadMat.mat",
                PredefinedCityModelPackage.Unknown => "DefaultUnknownMat.mat",
                PredefinedCityModelPackage.UrbanPlanningDecision => "DefaultUrbanPlanningDecisionMat.mat",
                PredefinedCityModelPackage.Vegetation => "DefaultVegetationMat.mat",
                _ => ""
            };
            if (matFileName == "")
            {
                return new Material(RenderUtil.DefaultMaterial);
            }
            string matPath = Path.Combine(baseMaterialDir, FallbackFolderName, folderNamePipeline, matFileName);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            return mat ? mat : new Material(RenderUtil.DefaultMaterial);
        }
    }
}
