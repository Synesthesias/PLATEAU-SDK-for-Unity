using System;

namespace PlateauUnitySDK.Runtime.CityMeta
{

    /// <summary>
    /// GmlSearcher の設定です。
    /// これは <see cref="CityImporterConfig"/> によって保持されるので、そちらの注意事項もご覧ください。
    /// </summary>
    [Serializable]
    public class GmlSearcherConfig
    {
        /// <summary> 見つかったエリアIDの一覧です。 </summary>
        public int[] areaIds = { };

        /// <summary> <see cref="areaIds"/> の i番目を変換対象とするかどうかです。 </summary>
        public bool[] isAreaIdTarget = { };

        public GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

        
        /// <summary>
        /// 引数が true のとき、すべてのエリアを対象とします。
        /// 　　　 false のとき、すべてのエリアを除外します。
        /// </summary>
        public void SetAllAreaId(bool isTarget)
        {
            for (int i = 0; i < this.isAreaIdTarget.Length; i++)
            {
                this.isAreaIdTarget[i] = isTarget;
            }
        }
    }
}