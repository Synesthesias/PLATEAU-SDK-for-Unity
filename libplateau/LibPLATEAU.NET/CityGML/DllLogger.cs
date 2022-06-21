using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LibPLATEAU.NET.Util;

namespace LibPLATEAU.NET.CityGML
{
    /// <summary>
    /// DLL側のログをC#側でコールバックで受け取ることができますが、
    /// その Error, Warn, Info の3つのコールバックをまとめたクラスです。
    /// 詳しくは <see cref="DllLogger"/> をご覧ください。
    /// </summary>
    public class LogCallbacks
    {
        public LogCallbackFuncType LogError { get; set; }
        public LogCallbackFuncType LogWarn { get; set; }
        public LogCallbackFuncType LogInfo { get; set; }
        public IntPtr LogErrorFuncPtr => DelegateToPtr(LogError);
        public IntPtr LogWarnFuncPtr => DelegateToPtr(LogWarn);
        public IntPtr LogInfoFuncPtr => DelegateToPtr(LogInfo);
        
        public static readonly LogCallbackFuncType StdErrFunc = messagePtr =>
            Console.Error.WriteLine(PtrToStr(messagePtr));

        public static readonly LogCallbackFuncType StdOutFunc = messagePtr => Console.WriteLine(PtrToStr(messagePtr));

        public LogCallbacks(LogCallbackFuncType logError, LogCallbackFuncType logWarn, LogCallbackFuncType logInfo)
        {
            LogError = logError;
            LogWarn = logWarn;
            LogInfo = logInfo;
        }

        /// <summary>
        /// DLLのログをC#の標準出力に転送するよう設定されたコールバックメソッドです。
        /// </summary>
        public static readonly LogCallbacks StdOut = new LogCallbacks(
            StdErrFunc, StdOutFunc, StdOutFunc
        );
        
        private static string PtrToStr(IntPtr strPtr)
        {
            return Marshal.PtrToStringAnsi(strPtr);
        }

        private static IntPtr DelegateToPtr(LogCallbackFuncType func)
        {
            return Marshal.GetFunctionPointerForDelegate(func);
        } 
    }
    
    
    /// <summary>
    /// DLL側のログをC#側にコールバックで転送するクラスです。
    /// C#側でログを表示するためのコールバックメソッドとログ詳細度を指定できます。
    /// </summary>
    public class DllLogger
    {
        private IntPtr handle;

        
        
        public DllLogger(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// DLLから受け取ったログメッセージを C#での標準出力に転送するよう設定します。
        /// </summary>
        public void SetCallbacksToStdOut()
        {
            SetLogCallbacks(LogCallbacks.StdOut);
        }

        /// <summary>
        /// DLL内でのログをコールバックによって受け取ることができるようにします。
        /// </summary>
        public void SetLogCallbacks(
            LogCallbacks callbacks)
        {
            APIResult result = NativeMethods.plateau_dll_logger_set_callbacks(
                this.handle,
                Marshal.GetFunctionPointerForDelegate(callbacks.LogError),
                Marshal.GetFunctionPointerForDelegate(callbacks.LogWarn),
                Marshal.GetFunctionPointerForDelegate(callbacks.LogInfo)
            );
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// 指定したログレベル未満のログは無視するようにします。
        /// </summary>
        public void SetLogLevel(DllLogLevel logLevel)
        {
            var result = NativeMethods.plateau_dll_logger_set_log_level(
                this.handle, logLevel
            );
            DLLUtil.CheckDllError(result);
        }
    }
}