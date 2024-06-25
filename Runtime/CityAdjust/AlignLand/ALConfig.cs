using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityAdjust.AlignLand
{
    /// <summary>
    /// 高さ合わせ機能の設定です。
    /// </summary>
    public class ALConfig
    {
        public PLATEAUInstancedCityModel TargetModel { get; private set; }
        public Transform[] Lands { get; private set; }
        public HashSet<PredefinedCityModelPackage> TargetPackages { get; set; }
        public bool DoDestroySrcObj { get; private set; }

        public ALConfig(PLATEAUInstancedCityModel targetModel, Transform[] lands,
            HashSet<PredefinedCityModelPackage> targetPackages, bool doDestroySrcObj)
        {
            TargetModel = targetModel;
            Lands = lands;
            TargetPackages = targetPackages;
            DoDestroySrcObj = doDestroySrcObj;
        }
        
    }
}