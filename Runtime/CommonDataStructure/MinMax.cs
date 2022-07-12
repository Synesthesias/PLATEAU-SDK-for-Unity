using System;
using Codice.CM.Common;
using UnityEngine;

namespace PLATEAU.CommonDataStructure
{
    [Serializable]
    internal class MinMax<T> where T : IComparable
    {
        [SerializeField] private T min;
        [SerializeField] private T max;

        public MinMax()
        {
            
        }

        public MinMax(T min, T max)
        {
            SetMinMax(min, max);
        }

        public MinMax(MinMax<T> other)
        {
            SetMinMax(other.Min, other.Max);
        }

        /// <summary>
        /// min と max を設定します。
        /// 必ず両方同時に設定する仕様にしています。
        /// なぜなら片方ずつだと <see cref="SwapIfReversed"/> が発動して意図しない挙動になるためです。
        /// </summary>
        public void SetMinMax(T minArg, T maxArg)
        {
            this.min = minArg;
            this.max = maxArg;
            SwapIfReversed();
        }

        public void SetMinMax(MinMax<T> minmax)
        {
            SetMinMax(minmax.Min, minmax.Max);
        }

        public T Min => this.min;

        public T Max => this.max;

        /// <summary>
        /// min と max が逆転している場合はスワップします。
        /// フェイルセーフとして実装しています。
        /// 万が一逆転すると、minからmaxまでのループが無限ループになってしまうため念のためです。
        /// </summary>
        private void SwapIfReversed()
        {
            if (this.min.CompareTo(this.max) > 0)
            {
                (this.min, this.max) = (this.max, this.min);
            }
        }

        public override string ToString()
        {
            return $"(min: {Min}, max: {Max})";
        }
    }

    
    /// <summary>
    /// <see cref="MinMax{T}"/> の値を readonly にしたバージョンです。 
    /// </summary>
    internal class ReadOnlyMinMax<T> where T : IComparable
    {
        private readonly MinMax<T> minMax;

        public ReadOnlyMinMax(T min, T max)
        {
            this.minMax = new MinMax<T>(min, max);
        }

        public T Min => this.minMax.Min;
        public T Max => this.minMax.Max;

        public MinMax<T> ToWritable => new MinMax<T>(Min, Max);
    }
}