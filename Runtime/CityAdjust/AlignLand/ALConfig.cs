using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityAdjust.AlignLand
{
    /// <summary>
    /// 高さ合わせ機能の設定ベースです。
    /// </summary>
    public class ALConfigBase
    {
        public bool DoDestroySrcObj { get; private set; }
        /// <summary> ハイトマップを生成するときのみ利用します。マップの横幅=縦幅です。 </summary>
        public int HeightmapWidth { get; private set; }
        public bool FillEdges { get; }
        public bool ApplyConvolutionFilterToHeightMap { get; }
        public bool AlignLandNormal { get; }
        public bool AlignInvert { get; }

        public ALConfigBase( bool doDestroySrcObj, int heightmapWidth, bool fillEdges, bool applyConvolutionFilterToHeightMap, bool alignLandNormal, bool alignInvert)
        {
            DoDestroySrcObj = doDestroySrcObj;
            HeightmapWidth = heightmapWidth;
            FillEdges = fillEdges;
            ApplyConvolutionFilterToHeightMap = applyConvolutionFilterToHeightMap;
            AlignLandNormal = alignLandNormal;
            AlignInvert = alignInvert;
        }

    }

    /// <summary>
    /// CityModel用の高さ合わせ機能の設定です。
    /// </summary>
    public class ALConfig : ALConfigBase
    {
        public PLATEAUInstancedCityModel TargetModel { get; private set; }
        public Transform[] Lands { get; private set; }
        public HashSet<PredefinedCityModelPackage> TargetPackages { get; set; }
        public ALConfig(PLATEAUInstancedCityModel targetModel, Transform[] lands,
            HashSet<PredefinedCityModelPackage> targetPackages, bool doDestroySrcObj,
            int heightmapWidth, bool fillEdges, bool applyConvolutionFilterToHeightMap, bool alignLandNormal, bool alignInvert)
            : base(doDestroySrcObj, heightmapWidth, fillEdges, applyConvolutionFilterToHeightMap, alignLandNormal, alignInvert)
        {
            TargetModel = targetModel;
            Lands = lands;
            TargetPackages = targetPackages;
        }

    }

    /// <summary>
    /// Tile用の高さ合わせ機能の設定です。
    /// </summary>
    public class ALTileConfig : ALConfigBase
    {
        public PLATEAUTileManager TileManager { get; private set; }

        public List<Transform> AlignTransformList { get; private set; }

        public List<Transform> LandTransformList { get; private set; }

        public ALTileConfig(PLATEAUTileManager tileManager, List<Transform> alignTransforms, List<Transform> landTransforms, bool doDestroySrcObj, int heightmapWidth, bool fillEdges, bool applyConvolutionFilterToHeightMap, bool alignLandNormal, bool alignInvert) 
            : base(doDestroySrcObj, heightmapWidth, fillEdges, applyConvolutionFilterToHeightMap, alignLandNormal, alignInvert)
        {
            TileManager = tileManager;
            AlignTransformList = alignTransforms;
            LandTransformList = landTransforms;
        }
    }
}