using System;
using System.Runtime.InteropServices;
using PLATEAU.Native;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// ネイティブDLL内のログをコールバックで受け取るためのメソッドです。
    /// ログを Unity の Debug.Log に転送します。
    /// </summary>
    internal static class DllLogCallback
    {
        // エラー、警告、情報　それぞれのコールバックです。
        private static readonly LogCallbacks.LogCallbackFuncType logError = (messagePtr) => Debug.LogError(PtrToStr(messagePtr));
        private static readonly LogCallbacks.LogCallbackFuncType logWarn = (messagePtr) => Debug.LogWarning(PtrToStr(messagePtr));
        private static readonly LogCallbacks.LogCallbackFuncType logInfo = (messagePtr) => Debug.Log(PtrToStr(messagePtr));

        // 上の3つをまとめたものです。
        public static readonly LogCallbacks UnityLogCallbacks = new LogCallbacks(logError, logWarn, logInfo);

        private static string PtrToStr(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }

    internal static class DllUnityLogger
    {
        public static DllLogger Create()
        {
            var logger = new DllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            return logger;
        }
    }
}