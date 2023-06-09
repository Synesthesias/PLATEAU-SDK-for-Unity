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
        public int minLOD;
        public int maxLOD;
        public MeshGranularity meshGranularity;
        public bool doSetMeshCollider;
        public Material fallbackMaterial;
        /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
        [NonSerialized] public bool GuiFoldOutState = true;

        public PackageLoadSetting(bool loadPackage, bool includeTexture, int minLOD, int maxLOD, MeshGranularity meshGranularity, bool doSetMeshCollider)
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
            var assetPath = MaterialPathUtil.GetFallbackMaterialPath(pack);
            if (!assetPath.Equals(""))
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (mat != null)
                {
                    fallbackMaterial = mat;
                }
                else fallbackMaterial = new Material(RenderUtil.DefaultMaterial);
            }
            else fallbackMaterial = new Material(RenderUtil.DefaultMaterial);

            return fallbackMaterial;
        }
    }
}
