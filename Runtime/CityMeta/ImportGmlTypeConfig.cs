﻿using System;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// インポート時の設定について、地物タイプごとの設定項目です。
    /// </summary>
    
    // 補足:
    // 他クラスとの関係は CityImporterConfig -> 保持 -> GmlSearcherConfig -> 保持 -> GmlTypeTarget -> 保持 -> ImportGmlTypeConfig
    // という関係なので、 CityImporterConfig の注意事項に基づいてこのクラスには Serializable属性が付いている必要があります。
    
    [Serializable]
    internal class ImportGmlTypeConfig
    {
        public bool isTarget;
        public int minLod;
        public int maxLod;
        [NonSerialized] public float SliderMinLod = 3f;
        [NonSerialized] public float SliderMaxLod = 4f;

        public ImportGmlTypeConfig()
        {
            this.isTarget = true;
        }
    }
}