using System.Collections.Generic;
using System.IO;
using PLATEAU.Dataset;
using UnityEditor;
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
                {PredefinedCityModelPackage.Building, "PlateauDefaultBuilding.mat"},
                {PredefinedCityModelPackage.Road, "PlateauDefaultRoad.mat"},
                {PredefinedCityModelPackage.UrbanPlanningDecision, "PlateauDefaultUrbanPlanningDecision.mat"},
                {PredefinedCityModelPackage.LandUse, "PlateauDefaultLandUse.mat"},
                {PredefinedCityModelPackage.CityFurniture, "PlateauDefaultCityFurniture.mat"},
                {PredefinedCityModelPackage.Vegetation, "PlateauDefaultVegetation.mat"},
                {PredefinedCityModelPackage.Relief, "PlateauDefaultRelief.mat"},
                {PredefinedCityModelPackage.DisasterRisk, "PlateauDefaultDisasterRisk.mat"},
                {PredefinedCityModelPackage.Railway, "PlateauDefaultRailway.mat"},
                {PredefinedCityModelPackage.Waterway, "PlateauDefaultWaterway.mat"},
                {PredefinedCityModelPackage.WaterBody, "PlateauDefaultWaterBody.mat"},
                {PredefinedCityModelPackage.Bridge, "PlateauDefaultBridge.mat"},
                {PredefinedCityModelPackage.Track, "PlateauDefaultTrack.mat"},
                {PredefinedCityModelPackage.Square, "PlateauDefaultSquare.mat"},
                {PredefinedCityModelPackage.Tunnel, "PlateauDefaultTunnel.mat"},
                {PredefinedCityModelPackage.UndergroundFacility, "PlateauDefaultUndergroundFacility.mat"},
                {PredefinedCityModelPackage.UndergroundBuilding, "PlateauDefaultUndergroundBuilding.mat"},
                {PredefinedCityModelPackage.Area, "PlateauDefaultLandUse.mat"}, // 土地利用を流用
                {PredefinedCityModelPackage.Unknown, "PlateauDefaultUnknown.mat"},
                {PredefinedCityModelPackage.OtherConstruction, "PlateauDefaultUnknown.mat"}
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
            
            string matPath = Path.Combine(MaterialPathUtil.BaseMaterialDir, FallbackFolderName, matFileName);
            #if UNITY_EDITOR
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            return mat ? mat : new Material(RenderUtil.DefaultMaterial);
            #else
            throw new NotImplementedException("This function is only supported in editor."); 
            #endif
        }

        /// <summary>
        /// メインテクスチャ名からデフォルトマテリアルを探して返します。
        /// なければnullを返します。
        /// </summary>
        public static Material ByMainTextureName(string mainTexNameArg)
        {
            string mainTexName = Path.GetFileName(mainTexNameArg);
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