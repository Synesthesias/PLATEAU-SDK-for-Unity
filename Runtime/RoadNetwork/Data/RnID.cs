using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    public interface RnID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {
        // PropertyDrawerでアクセスするため
        public string IdFieldName { get; }

        //[Obsolete("使用しないでください。ストレージ内でのみ参照出来るように修正予定。")]
        public int _Val { get; }

        public static RnID<TPrimDataType> Undefind { get; }

        // 有効なIdかどうか
        public bool IsValid { get; }

    }


}