using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// 分割結合の設定値です。
    /// DLL向けの設定とUnity向けの設定をまとめたものです。
    /// </summary>
    public class GranularityConvertOptionUnity
    {
        /// <summary> DLL向けの設定です。 </summary>
        public GranularityConvertOption NativeOption { get; }
        public GameObject[] SrcGameObjs { get; }
        public bool DoDestroySrcObjs { get; }

        public GranularityConvertOptionUnity(
            GranularityConvertOption nativeOption, // 注意: GranularityConvertOption.GridCountの設定は未実装であり、何の値に設定しても動作に影響しません。
            GameObject[] srcGameObjs,
            bool doDestroySrcObjs)
        {
            NativeOption = nativeOption;
            SrcGameObjs = srcGameObjs;
            DoDestroySrcObjs = doDestroySrcObjs;
        }

    }
}