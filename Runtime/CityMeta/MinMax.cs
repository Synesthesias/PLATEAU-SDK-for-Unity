using System;
using UnityEngine;

namespace PLATEAU.CityMeta
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

        public void SetMinMax(T minArg, T maxArg)
        {
            this.min = minArg;
            this.max = maxArg;
            SwapIfReversed();
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
    }
}