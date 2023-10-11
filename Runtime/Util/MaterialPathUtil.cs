using System;
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
            string matFileName = pack switch
            {
                PredefinedCityModelPackage.Building => "PlateauDefaultBuilding.mat",
                PredefinedCityModelPackage.Road => "PlateauDefaultRoad.mat",
                PredefinedCityModelPackage.UrbanPlanningDecision => "PlateauDefaultUrbanPlanningDecision.mat",
                PredefinedCityModelPackage.LandUse => "PlateauDefaultLandUse.mat",
                PredefinedCityModelPackage.CityFurniture => "PlateauDefaultCityFurniture.mat",
                PredefinedCityModelPackage.Vegetation => "PlateauDefaultVegetation.mat",
                PredefinedCityModelPackage.Relief => "PlateauDefaultRelief.mat",
                PredefinedCityModelPackage.DisasterRisk => "PlateauDefaultDisasterRisk.mat",
                PredefinedCityModelPackage.Railway => "PlateauDefaultRailway.mat",
                PredefinedCityModelPackage.Waterway => "PlateauDefaultWaterway.mat",
                PredefinedCityModelPackage.WaterBody => "PlateauDefaultWaterBody.mat",
                PredefinedCityModelPackage.Bridge => "PlateauDefaultBridge.mat",
                PredefinedCityModelPackage.Track => "PlateauDefaultTrack.mat",
                PredefinedCityModelPackage.Square => "PlateauDefaultSquare.mat",
                PredefinedCityModelPackage.Tunnel => "PlateauDefaultTunnel.mat",
                PredefinedCityModelPackage.UndergroundFacility => "PlateauDefaultUndergroundFacility.mat",
                PredefinedCityModelPackage.UndergroundBuilding => "PlateauDefaultUndergroundBuilding.mat",
                PredefinedCityModelPackage.Area => "PlateauDefaultLandUse.mat", // 土地利用を流用
                PredefinedCityModelPackage.Unknown => "PlateauDefaultUnknown.mat",
                PredefinedCityModelPackage.OtherConstruction => "PlateauDefaultUnknown.mat",
                _ => ""
            };
            if (matFileName == "")
            {
                return new Material(RenderUtil.DefaultMaterial);
            }
            string matPath = Path.Combine(baseMaterialDir, FallbackFolderName, matFileName);
            #if UNITY_EDITOR
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            return mat ? mat : new Material(RenderUtil.DefaultMaterial);
            #else
            throw new NotImplementedException("This function is only supported in editor."); 
            #endif
        }
    }
}
