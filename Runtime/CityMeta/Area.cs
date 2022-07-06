using System;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{

    // 地域ID は 6桁または8桁からなります。
    // 最初の6桁が 第2次地域区画 (1辺10km) です。
    // 残りの2桁がある場合、それが 第3次地域区画 (1辺1km) です。
    
    [Serializable]
    internal class Area : IComparable<Area>
    {
        [SerializeField] private int id;
        [SerializeField] private bool isTarget;

        private const int numDigitsOfSecondSection = 6;

        public Area(int id)
        {
            this.id = id;
        }

        /// <summary> 6桁または8桁の地域IDです。 </summary>
        public int Id
        {
            get => this.id;
            set => this.id = value;
        }

        /// <summary> この地域をインポート対象とするかどうかの設定です。 </summary>
        public bool IsTarget
        {
            get => this.isTarget;
            set => this.isTarget = value;
        }

        /// <summary> 第2地域区画 (1辺10km)の番号、すなわちIDの最初の6桁です。 </summary>
        public int SecondSectionId() => SecondSectionId(Id); 

        public static int SecondSectionId(int id)
        {
            return DigitsUtil.PickFirstDigits(id, numDigitsOfSecondSection);
        }

        public int CompareTo(Area other)
        {
            return this.id.CompareTo(other.id);
        }

        
        
        
    }
}