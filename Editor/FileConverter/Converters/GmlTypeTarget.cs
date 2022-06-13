using System;
using System.Collections.Generic;
using System.Linq;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// gmlファイル群は地物オブジェクトのタイプ別にフォルダ分けで格納されていますが、
    /// そのうちのどのタイプ（フォルダ）を変換ターゲットとするかを決めます。
    /// 例: udx/bldg下のgmlは対象にするが、udx/veg下のgmlは対象にしない、など。
    /// </summary>
    public class GmlTypeTarget
    {
        /// <summary> GmlTypeごとに、変換対象とするかどうかの辞書です。 </summary>
        public Dictionary<GmlType, bool> TargetDict { get; set; }

        public GmlTypeTarget()
        {
            // 各タイプ true で初期化します。
            this.TargetDict =
                Enum.GetValues(typeof(GmlType))
                    .OfType<GmlType>()
                    .ToDictionary(t => t, _ => true);
        }

        public bool IsTypeTarget(GmlType t)
        {
            return TargetDict[t];
        }

        /// <summary> すべて true または すべて false にします。 </summary>
        public void SetAll(bool val)
        {
            foreach (var key in TargetDict.Keys.ToArray())
            {
                TargetDict[key] = val;
            }
        }
    }
}