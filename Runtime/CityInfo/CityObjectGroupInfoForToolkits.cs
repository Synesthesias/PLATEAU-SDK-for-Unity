using System;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// <see cref="PLATEAUCityObjectGroup"/>が保持する情報のうち、Toolkitsとの連携に必要な情報です。
    /// </summary>
    [Serializable]
    public struct CityObjectGroupInfoForToolkits
    {
        public CityObjectGroupInfoForToolkits(bool isTextureCombinedOrGranularityConverted)
        {
            this.isTextureCombinedOrGranularityConverted = isTextureCombinedOrGranularityConverted;
        }
        /// <summary>
        /// インポート時の設定で、テクスチャ結合にチェックが入っていたとき、または、
        /// 分割結合機能を利用したときにtrueになります。
        /// Toolkitsのテクスチャ生成の動作に影響します。
        /// </summary>
        private bool isTextureCombinedOrGranularityConverted;
    }
}