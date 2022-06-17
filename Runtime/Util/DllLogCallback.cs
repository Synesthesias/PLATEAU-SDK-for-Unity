using System;
using System.Runtime.InteropServices;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Util
{
    /// <summary>
    /// ネイティブDLL内のログをコールバックで受け取るためのメソッドです。
    /// ログを Unity の Debug.Log に転送します。
    /// </summary>
    public static class DllLogCallback
    {
        // エラー、警告、情報　それぞれのコールバックです。
        private static readonly LogCallbackFuncType logError = (messagePtr) => Debug.LogError(PtrToStr(messagePtr));
        private static readonly LogCallbackFuncType logWarn = (messagePtr) => Debug.LogWarning(PtrToStr(messagePtr));
        private static readonly LogCallbackFuncType logInfo = (messagePtr) => Debug.Log(PtrToStr(messagePtr));
        
        // 上の3つをまとめたものです。
        public static readonly LogCallbacks UnityLogCallbacks = new LogCallbacks(logError, logWarn, logInfo);

        private static string PtrToStr(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}