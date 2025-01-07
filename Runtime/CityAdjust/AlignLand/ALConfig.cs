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
        public PLATEAUInstancedCityModel targetModel { get; private set; }
        public Transform[] Lands { get; private set; }
        public HashSet<PredefinedCityModelPackage> TargetPackages { get; set; }
        public bool DoDestroySrcObj { get; private set; }
        /// <summary> ハイトマップを生成するときのみ利用します。マップの横幅=縦幅です。 </summary>
        public int HeightmapWidth { get; private set; } 
        public bool FillEdges { get; }
        public bool ApplyConvolutionFilterToHeightMap { get; }
        public bool AlignLandNormal { get; }
        public bool AlignInvert { get; }

        public ALConfig(PLATEAUInstancedCityModel targetModel, Transform[] lands,
            HashSet<PredefinedCityModelPackage> targetPackages, bool doDestroySrcObj,
            int heightmapWidth, bool fillEdges, bool applyConvolutionFilterToHeightMap, bool alignLandNormal, bool alignInvert)
        {
            this.targetModel = targetModel;
            Lands = lands;
            TargetPackages = targetPackages;
            DoDestroySrcObj = doDestroySrcObj;
            HeightmapWidth = heightmapWidth;
            FillEdges = fillEdges;
            ApplyConvolutionFilterToHeightMap = applyConvolutionFilterToHeightMap;
            AlignLandNormal = alignLandNormal;
            AlignInvert = alignInvert;
        }
        
    }
    
    
}