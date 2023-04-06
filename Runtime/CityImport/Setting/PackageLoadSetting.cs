using System;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
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
            if (fallbackMaterial == null)
            {
                Material newMat = new Material(RenderUtil.DefaultMaterial);
                switch (pack)
                {
                    case PredefinedCityModelPackage.Building:
                        newMat.name = "Building-looking material";
                        break;
                    case PredefinedCityModelPackage.CityFurniture:
                        newMat.name = "CityFurniture-looking material";
                        break;
                    case PredefinedCityModelPackage.DisasterRisk:
                        newMat.name = "DisasterRisk-looking material";
                        break;
                    case PredefinedCityModelPackage.LandUse:
                        newMat.name = "LandUse-looking material";
                        break;
                    case PredefinedCityModelPackage.Relief:
                        newMat.name = "Relief-looking material";
                        break;
                    case PredefinedCityModelPackage.Road:
                        newMat.name = "Road-looking material";
                        break;
                    case PredefinedCityModelPackage.Unknown:
                        newMat.name = "Unknown-looking material";
                        break;
                    case PredefinedCityModelPackage.UrbanPlanningDecision:
                        newMat.name = "UrbanPlanningDecision-looking material";
                        break;
                    case PredefinedCityModelPackage.Vegetation:
                        newMat.name = "Vegetation-looking material";
                        break;
                }
                fallbackMaterial = newMat;
            }

            return fallbackMaterial;
        }
    }
}
