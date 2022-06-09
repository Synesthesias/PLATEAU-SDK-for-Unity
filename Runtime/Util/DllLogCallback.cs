using System;
using System.Runtime.InteropServices;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Util
{
    public static class DllLogCallback
    {
        public static readonly LogCallbackFuncType LogError = (messagePtr) => Debug.LogError(PtrToStr(messagePtr));
        public static readonly LogCallbackFuncType LogWarn = (messagePtr) => Debug.LogWarning(PtrToStr(messagePtr));
        public static readonly LogCallbackFuncType LogInfo = (messagePtr) => Debug.Log(PtrToStr(messagePtr));
        public static LogCallbacks UnityLogCallbacks = new LogCallbacks(LogError, LogWarn, LogInfo);

        private static string PtrToStr(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}