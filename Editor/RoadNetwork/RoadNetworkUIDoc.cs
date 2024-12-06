using PLATEAU.Editor.RoadNetwork.UIDocBind;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    public enum RoadNetworkEditingResultType
    {
        _Undefind = 0xfffffff,
        Success = 0x0000000,
        InvalidNewValue = 1 << 0,      // 適切な変更値ではない
        InvalidTarget = 1 << 1,      // 適切な変更対象ではない(主に第1引数のLink,Laneなどについて)         
        CantApplyEditing = 1 << 2,      // 適用できない編集。リンクの幅員を小さくしすぎて一部のレーンの幅員が0になるなど
        InvalidArgs = 1 << 3,      // 適切な引数ではない。(主に第2引数以降　Wayのポイントを削除する際に渡したポイントが存在しないなど)
        _UndefindError = 1 << 256,
        //Faild // 失敗原因を明確にするために未使用
    }

    public struct RoadNetworkEditingResult
    {
        public readonly RoadNetworkEditingResultType Result;
        public readonly string Msg;

        public bool IsSuccess { get => Result == RoadNetworkEditingResultType.Success; }
        public RoadNetworkEditingResult(RoadNetworkEditingResultType result, string msg = "")
        {
            this.Result = result;
            this.Msg = msg;
        }
    }


}
