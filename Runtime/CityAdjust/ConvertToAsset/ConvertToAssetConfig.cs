using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.ConvertToAsset
{
    public class ConvertToAssetConfig
    {
        public GameObject SrcGameObj { get; set; }
        public string AssetPath { get; set; }

        public ConvertToAssetConfig(GameObject srcGameObj, string assetPath)
        {
            SrcGameObj = srcGameObj;
            assetPath = AssetPath;
        }
    }
}