using System;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、パッケージごとの設定です。
    /// <see cref="CityLoadConfig"/> によって保持されます。
    /// </summary>
    [Serializable]
    internal class PackageLoadSetting
    {
        public bool loadPackage;
        public bool includeTexture;
        public uint minLOD;
        public uint maxLOD;
        public MeshGranularity meshGranularity;
        public bool doSetMeshCollider;
        public Material fallbackMaterial;
        /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
        [NonSerialized] public bool GuiFoldOutState = true;

        public PackageLoadSetting(bool loadPackage, bool includeTexture, uint minLOD, uint maxLOD, MeshGranularity meshGranularity, bool doSetMeshCollider)
        {
            this.loadPackage = loadPackage;
            this.includeTexture = includeTexture;
            this.minLOD = minLOD;
            this.maxLOD = maxLOD;
            this.meshGranularity = meshGranularity;
            this.doSetMeshCollider = doSetMeshCollider;
        }

        public Material GetFallbackMaterial(PredefinedCityModelPackage pack)
        {
            const string defaultMatPath = "Materials/DefaultModule/#mdl";
            if (fallbackMaterial == null)
            {
                string assetPath = "";
                switch (pack)
                {
                    case PredefinedCityModelPackage.Building:
                       assetPath = defaultMatPath.Replace("#mdl", "DefaultBuildingMat.mat");
                        break;
                    case PredefinedCityModelPackage.CityFurniture:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultCityFurnitureMat.mat");
                        break;
                    case PredefinedCityModelPackage.DisasterRisk:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultDisasterRiskMat.mat");
                        break;
                    case PredefinedCityModelPackage.LandUse:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultLandUseMat.mat");
                        break;
                    case PredefinedCityModelPackage.Relief:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultReliefMat.mat");
                        break;
                    case PredefinedCityModelPackage.Road:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultRoadMat.mat");
                        break;
                    case PredefinedCityModelPackage.Unknown:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultUnknownMat.mat");
                        break;
                    case PredefinedCityModelPackage.UrbanPlanningDecision:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultUrbanPlanningDecisionMat.mat");
                        break;
                    case PredefinedCityModelPackage.Vegetation:
                        assetPath = defaultMatPath.Replace("#mdl", "DefaultVegetationMat.mat");
                        break;
                }

                if (!assetPath.Equals(""))
                {
                    assetPath = PathUtil.SdkPathToAssetPath(assetPath);
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    if (mat != null)
                    {
                        fallbackMaterial = mat;
                    }
                    else fallbackMaterial = new Material(RenderUtil.DefaultMaterial);
                }
                else fallbackMaterial = new Material(RenderUtil.DefaultMaterial);
            }

            return fallbackMaterial;
        }
    }
}
