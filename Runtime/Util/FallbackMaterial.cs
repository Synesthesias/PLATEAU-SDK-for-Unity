using System.Collections.Generic;
using System.IO;
using PLATEAU.Dataset;
using UnityEngine;
#if !UNITY_EDITOR
using System;
#endif

namespace PLATEAU.Util
{
    public class FallbackMaterial
    {
        // フォールバックマテリアルが入っているフォルダ名
        private const string FallbackFolderName = "Fallback";

        private static readonly IReadOnlyDictionary<PredefinedCityModelPackage, string> PackageToMaterialFileName =
            new Dictionary<PredefinedCityModelPackage, string>
            {
                {PredefinedCityModelPackage.Building, "PlateauDefaultBuilding"},
                {PredefinedCityModelPackage.Road, "PlateauDefaultRoad"},
                {PredefinedCityModelPackage.UrbanPlanningDecision, "PlateauDefaultUrbanPlanningDecision"},
                {PredefinedCityModelPackage.LandUse, "PlateauDefaultLandUse"},
                {PredefinedCityModelPackage.CityFurniture, "PlateauDefaultCityFurniture"},
                {PredefinedCityModelPackage.Vegetation, "PlateauDefaultVegetation"},
                {PredefinedCityModelPackage.Relief, "PlateauDefaultRelief"},
                {PredefinedCityModelPackage.DisasterRisk, "PlateauDefaultDisasterRisk"},
                {PredefinedCityModelPackage.Railway, "PlateauDefaultRailway"},
                {PredefinedCityModelPackage.Waterway, "PlateauDefaultWaterway"},
                {PredefinedCityModelPackage.WaterBody, "PlateauDefaultWaterBody"},
                {PredefinedCityModelPackage.Bridge, "PlateauDefaultBridge"},
                {PredefinedCityModelPackage.Track, "PlateauDefaultTrack"},
                {PredefinedCityModelPackage.Square, "PlateauDefaultSquare"},
                {PredefinedCityModelPackage.Tunnel, "PlateauDefaultTunnel"},
                {PredefinedCityModelPackage.UndergroundFacility, "PlateauDefaultUndergroundFacility"},
                {PredefinedCityModelPackage.UndergroundBuilding, "PlateauDefaultUndergroundBuilding"},
                {PredefinedCityModelPackage.Area, "PlateauDefaultLandUse"}, // 土地利用を流用
                {PredefinedCityModelPackage.Unknown, "PlateauDefaultUnknown"},
                {PredefinedCityModelPackage.OtherConstruction, "PlateauDefaultUnknown"}
            };

        private static Dictionary<string, Material> mainTexNameToMaterial = null;
        
        /// <summary>
        /// 引数のパッケージ種に対応するデフォルトフォールバックマテリアルをロードして返します。
        /// 引数に対応するものがない、またはロードに失敗した場合はUnityのデフォルトマテリアルを返します。
        /// </summary>
        public static Material LoadByPackage(PredefinedCityModelPackage pack)
        {
            if (!PackageToMaterialFileName.TryGetValue(pack, out var matFileName))
            {
                return new Material(RenderUtil.DefaultMaterial);
            }
            
            // string matPath = Path.Combine(MaterialPathUtil.BaseMaterialDir, FallbackFolderName, matFileName);
            // #if UNITY_EDITOR
            // var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            // return mat ? mat : new Material(RenderUtil.DefaultMaterial);
            // #else
            // throw new NotImplementedException("This function is only supported in editor."); 
            // #endif
            var mat = Resources.Load<Material>("PlateauSdkDefaultMaterials/" + matFileName);
            return mat ? mat : new Material(RenderUtil.DefaultMaterial);
        }

        /// <summary>
        /// メインテクスチャ名からデフォルトマテリアルを探して返します。
        /// なければnullを返します。
        /// </summary>
        public static Material ByMainTextureName(string mainTexNameArg)
        {
            string mainTexName = Path.GetFileNameWithoutExtension(mainTexNameArg);
            if (mainTexNameToMaterial == null)
            {
                // テクスチャ名をキー、マテリアルをバリューとする辞書を作ります。
                mainTexNameToMaterial = new();
                foreach (var pair in PackageToMaterialFileName)
                {
                    var package = pair.Key;
                    var mat = LoadByPackage(package);
                    mainTexNameToMaterial.TryAdd(mat.mainTexture.name, mat);
                }
            }
            
            if(mainTexNameToMaterial.TryGetValue(mainTexName, out var material))
            {
                return new Material(material);
            }

            return null;
        }
    }
}