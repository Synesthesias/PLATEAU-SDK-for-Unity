using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// 分割結合の設定値です。
    /// DLL向けの設定とUnity向けの設定をまとめたものです。
    /// </summary>
    internal class GranularityConvertOptionUnity
    {
        /// <summary> DLL向けの設定 </summary>
        public GranularityConvertOption NativeOption { get; }
        public GameObject[] SrcGameObjs { get; }
        public bool DoDestroySrcObjs { get; }

        public GranularityConvertOptionUnity(
            GranularityConvertOption nativeOption,
            GameObject[] srcGameObjs,
            bool doDestroySrcObjs)
        {
            NativeOption = nativeOption;
            SrcGameObjs = srcGameObjs;
            DoDestroySrcObjs = doDestroySrcObjs;
        }

    }
}