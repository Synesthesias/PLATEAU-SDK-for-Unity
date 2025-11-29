using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.AlignLand
{
    /// <summary>
    /// 高さ合わせ機能の設定ベースです。
    /// </summary>
    public abstract class ALConfigBase
    {
        public bool DoDestroySrcObj { get; private set; }
        /// <summary> ハイトマップを生成するときのみ利用します。マップの横幅=縦幅です。 </summary>
        public int HeightmapWidth { get; private set; }
        public bool FillEdges { get; }
        public bool ApplyConvolutionFilterToHeightMap { get; }
        public bool AlignLandNormal { get; }
        public bool AlignInvert { get; }

        /// <summary>
        /// 土地オブジェクトのTransformを取得します。
        /// </summary>
        public abstract IEnumerable<Transform> LandTransforms { get; }

        /// <summary>
        /// 高さ合わせ対象のCityObjectGroupを取得します。
        /// </summary>
        public abstract IEnumerable<PLATEAUCityObjectGroup> AlignCityObjectGroups { get; }

        /// <summary>
        /// 対象のCityObjectGroupのパッケージを取得します。
        /// </summary>
        /// <param name="cityObjectGroup"></param>
        /// <returns></returns>
        public abstract PredefinedCityModelPackage GetPackage(PLATEAUCityObjectGroup cityObjectGroup);

        /// <summary>
        /// 対象のパッケージが高さ合わせ可能かどうかを判定します。
        /// </summary>
        /// <param name="cityObjectGroup"></param>
        /// <returns></returns>
        public abstract bool CanAlign(PredefinedCityModelPackage package);

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

        public override IEnumerable<Transform> LandTransforms => Lands;
        public override IEnumerable<PLATEAUCityObjectGroup> AlignCityObjectGroups => TargetModel.GetComponentsInChildren<PLATEAUCityObjectGroup>();

        public override PredefinedCityModelPackage GetPackage(PLATEAUCityObjectGroup cityObjectGroup)
        {
            return TargetModel.GetPackage(cityObjectGroup);
        }

        public override bool CanAlign(PredefinedCityModelPackage package)
        {
            return TargetPackages.Contains(package);
        }

        private Transform[] Lands;
        private HashSet<PredefinedCityModelPackage> TargetPackages;

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

        public override IEnumerable<Transform> LandTransforms => LandTransformList;
        public override IEnumerable<PLATEAUCityObjectGroup> AlignCityObjectGroups => AlignTransformList.SelectMany(t => t.GetComponentsInChildren<PLATEAUCityObjectGroup>());

        public override PredefinedCityModelPackage GetPackage(PLATEAUCityObjectGroup cityObjectGroup)
        {
            return TileManager.CityModel.GetPackage(cityObjectGroup);
        }

        public override bool CanAlign(PredefinedCityModelPackage package)
        {
            return package.CanAlignWithLand();
        }

        private List<Transform> AlignTransformList;
        private List<Transform> LandTransformList;

        public ALTileConfig(PLATEAUTileManager tileManager, List<Transform> alignTransforms, List<Transform> landTransforms, bool doDestroySrcObj, int heightmapWidth, bool fillEdges, bool applyConvolutionFilterToHeightMap, bool alignLandNormal, bool alignInvert) 
            : base(doDestroySrcObj, heightmapWidth, fillEdges, applyConvolutionFilterToHeightMap, alignLandNormal, alignInvert)
        {
            TileManager = tileManager;
            AlignTransformList = alignTransforms;
            LandTransformList = landTransforms;
        }
    }
}