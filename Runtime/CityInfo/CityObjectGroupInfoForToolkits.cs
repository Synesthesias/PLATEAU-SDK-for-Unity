using System;
using UnityEngine;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// <see cref="PLATEAUCityObjectGroup"/>が保持する情報のうち、Toolkitsとの連携に必要な情報です。
    /// </summary>
    [Serializable]
    public struct CityObjectGroupInfoForToolkits
    {
        public CityObjectGroupInfoForToolkits(bool isTextureCombined, bool isGranularityConverted)
        {
            this.isTextureCombined = isTextureCombined;
            this.isGranularityConverted = isGranularityConverted;
        }

        /// <summary>
        /// インポート時の設定で、テクスチャ結合にチェックが入っていたときにtrueになります。
        /// Toolkitsのテクスチャ生成の動作に影響します。
        /// </summary>
        [SerializeField] private bool isTextureCombined;
        
        /// <summary>
        /// 分割結合機能を利用したときにtrueになります。
        /// </summary>
        [SerializeField] private bool isGranularityConverted;
        public bool IsTextureCombined => isTextureCombined;
        public bool IsGranularityConverted => isGranularityConverted;
    }
}