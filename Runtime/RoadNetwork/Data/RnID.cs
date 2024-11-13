using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// RnIDを生成可能なクラスをマークするための空インターフェイス
    /// ただしRnIDのデフォルトコンストラクタの呼び出しは制限していない（技術的に諦めました　C#10.0仕様的に出来るらしい）
    /// </summary>
    public interface IRnIDGeneratable { }

    [Serializable]
    public struct RnID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {

        public override int GetHashCode()
        {
            return id;
        }

        // PropertyDrawerでアクセスするため
        public const string IdFieldName = nameof(id);

        public static readonly RnID<TPrimDataType> Undefined = new RnID<TPrimDataType> { id = -1 };

        // Listのindexアクセスがintなのでuintじゃなくてintにしておく
        // structなので初期値は基本0. その時に不正値扱いにするために0は不正値とする
        [SerializeField]
        private int id;

        // 有効なIdかどうか
        public bool IsValid => id > 0;

        public int ID => id - 1;

        public RnID(int id, in IRnIDGeneratable _)
        {
            this.id = id + 1;
        }

        public static bool operator ==(RnID<TPrimDataType> a, RnID<TPrimDataType> b)
        {
            return a.id == b.id;
        }
        public static bool operator !=(RnID<TPrimDataType> a, RnID<TPrimDataType> b)
        {
            return !(a == b);
        }
        public bool Equals(RnID<TPrimDataType> other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is RnID<TPrimDataType> other && Equals(other);
        }
    }
}