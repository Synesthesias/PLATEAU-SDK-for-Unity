using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable]
    public struct RnID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {
        // PropertyDrawerでアクセスするため
        public const string IdFieldName = nameof(id);
        // 不正値
        public static RnID<TPrimDataType> Undefined => new RnID<TPrimDataType>(-1);

        // Listのindexアクセスがintなのでuintじゃなくてintにしておく
        // structなので初期値は基本0. その時に不正値扱いにするために0は不正値とする
        [SerializeField]
        private int id;

        //[Obsolete("使用しないでください。ストレージ内でのみ参照出来るように修正予定。")]
        public int _Val => id - 1;

        // 有効なIdかどうか
        public bool IsValid => id > 0;

        public RnID(int id)
        {
            this.id = id + 1;
        }
    }
}