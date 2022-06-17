using System;

namespace PlateauUnitySDK.Runtime.CityMeta
{

    /// <summary>
    /// <see cref="GmlSelectorGUI"/> の設定部分です。
    /// </summary>
    [Serializable]
    public class GmlSelectorConfig
    {
        /// <summary> 見つかったエリアIDの一覧です。 </summary>
        public string[] areaIds = { };

        /// <summary> <see cref="areaIds"/> の i番目を変換対象とするかどうかです。 </summary>
        public bool[] isAreaIdTarget = { };

        public GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

        public void SetAllAreaId(bool isTarget)
        {
            for (int i = 0; i < this.isAreaIdTarget.Length; i++)
            {
                this.isAreaIdTarget[i] = isTarget;
            }
        }
    }
}