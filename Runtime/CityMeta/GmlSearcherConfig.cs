using System;

namespace PLATEAU.CityMeta
{

    /// <summary>
    /// GmlSearcher の設定です。
    /// 対象gmlファイル選択において、地域IDと地物タイプの絞り込み設定を保持します。
    /// </summary>
    
    // 注意事項:
    // このクラスは CityImporterConfig によって保持されるので、そちらの注意事項もご覧ください。
    
    [Serializable]
    public class GmlSearcherConfig
    {
        /// <summary> 見つかったエリアIDの一覧です。 </summary>
        public int[] areaIds = { };

        /// <summary> <see cref="areaIds"/> の i番目を変換対象とするかどうかです。 </summary>
        public bool[] isAreaIdTarget = { };

        /// <summary> 地物タイプの絞り込み情報です。 </summary>
        public GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

        
        /// <summary>
        /// 引数が true のとき、すべてのエリアを対象とします。
        /// 　　　 false のとき、すべてのエリアを除外します。
        /// </summary>
        internal void SetAllAreaId(bool isTarget)
        {
            for (int i = 0; i < this.isAreaIdTarget.Length; i++)
            {
                this.isAreaIdTarget[i] = isTarget;
            }
        }
    }
}