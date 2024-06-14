using System;
using UnityEngine;
using static PLATEAU.RoadNetwork.Data.PrimitiveDataStorage;

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
        // PropertyDrawerでアクセスするため
        public const string IdFieldName = nameof(id);

        public readonly static RnID<TPrimDataType> Undefind = new RnID<TPrimDataType> { id = -1 };

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

    }
}