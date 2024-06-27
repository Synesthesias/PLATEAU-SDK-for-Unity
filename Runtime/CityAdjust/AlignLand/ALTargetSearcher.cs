using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.AlignLand
{
    /// <summary>
    /// 高さ合わせが可能な対象を探します。
    /// ALはAlignLandの略です。
    /// </summary>
    public class ALTargetSearcher
    {
        private PLATEAUInstancedCityModel targetModel;
        private List<PLATEAUCityObjectGroup> targetCityObjGroups;
        public HashSet<PredefinedCityModelPackage> TargetPackages { get; private set; }
        public List<Transform> Lands { get; private set; }
        
        public ALTargetSearcher(PLATEAUInstancedCityModel targetModel)
        {
            this.targetModel = targetModel;
            Search();
        }

        public bool IsValid()
        {
            if (Lands.Count == 0)
            {
                Debug.LogError("地形が見つかりませんでした。");
                return false;
            }
            if (TargetPackages.Count == 0)
            {
                Debug.LogError("高さ合わせ可能なパッケージ種が見つかりませんでした。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 検索結果から高さ合わせ設定を生成します。
        /// 検索から明らかでない設定内容を引数で渡します。
        /// </summary>
        public ALConfig ToConfig(bool doDestroySrcObj, int heightmapWidth)
        {
            return new ALConfig(targetModel, Lands.ToArray(), TargetPackages, doDestroySrcObj, heightmapWidth);
        }
        

        private void Search()
        {
            // 土地メッシュと高さ合わせ可能なパッケージを探します。
            var cogs = targetModel.GetComponentsInChildren<PLATEAUCityObjectGroup>();
            targetCityObjGroups = new List<PLATEAUCityObjectGroup>();
            TargetPackages = new HashSet<PredefinedCityModelPackage>();
            var lands = new List<Transform>();
            foreach (var cog in cogs)
            {
                var package = cog.Package;
                if(package == PredefinedCityModelPackage.Relief)
                {
                    lands.Add(cog.transform);
                }
                if (package.CanAlignWithLand())
                {
                    TargetPackages.Add(package);
                    targetCityObjGroups.Add(cog);
                }
            }
            
            // テレインを土地とみなします。
            lands.AddRange(
                targetModel.GetComponentsInChildren<Terrain>()
                    .Select(ter => ter.transform));
            Lands = lands;

        }
        
    }

    public static class PackageHeightMatchExtension
    {
        /// <summary>
        /// 「高さを地形と合わせる」機能を利用して地形に沿わせることが適切なパッケージならtrue、そうでなければfalseを返します。
        /// </summary>
        public static bool CanAlignWithLand(this PredefinedCityModelPackage package)
        {
            bool ret = package switch
            {
                PredefinedCityModelPackage.Area => true,
                PredefinedCityModelPackage.Road => true,
                PredefinedCityModelPackage.Square => true,
                PredefinedCityModelPackage.Track => true,
                PredefinedCityModelPackage.Waterway => true,
                PredefinedCityModelPackage.DisasterRisk => true,
                PredefinedCityModelPackage.LandUse => true,
                PredefinedCityModelPackage.WaterBody => true,
                PredefinedCityModelPackage.UrbanPlanningDecision => true,
                _ => false
            };
            return ret;
        }
    }
    
}