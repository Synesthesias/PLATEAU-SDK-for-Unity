using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    public interface RnID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {
        // PropertyDrawerでアクセスするため
        public const string IdFieldName = nameof(ID);

        //[Obsolete("使用しないでください。ストレージ内でのみ参照出来るように修正予定。")]
        public int ID { get; }

        // 有効なIdかどうか
        public bool IsValid { get; }

    }

    /// <summary>
    /// 不正値、無効な値
    /// </summary>
    /// <typeparam name="TPrimDataType"></typeparam>
    public struct InvalidRnID<TPrimDataType> : RnID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {
        // Listのindexアクセスがintなのでuintじゃなくてintにしておく
        // structなので初期値は基本0. その時に不正値扱いにするために0は不正値とする
        [SerializeField]
        private const int id = 0;

        // 有効なIdかどうか
        public bool IsValid => false;

        public int ID => id - 1;

    }

}