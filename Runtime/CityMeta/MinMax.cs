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
            this.min = min;
            this.max = max;
            SwapIfReversed();
        }

        public T Min
        {
            get => this.min;
            set
            {
                this.min = value;
                SwapIfReversed();
            }
        }

        public T Max
        {
            get => this.max;
            set
            {
                this.max = value;
                SwapIfReversed();
            }
        }
        
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