using System;
using System.Runtime.InteropServices;
using System.Threading;
using PLATEAU.Interop;

namespace PLATEAU.Native
{
    
    public enum DllLogLevel
    {
        Error = 4,
        Warning = 3,
        Info = 2,
        Debug = 1,
        Trace = 0
    }
    
    /// <summary>
    /// DLL側のログをC#側でコールバックで受け取ることができますが、
    /// その Error, Warn, Info の3つのコールバックをまとめたクラスです。
    /// 詳しくは <see cref="DllLogger"/> をご覧ください。
    /// </summary>
    public class LogCallbacks
    {
        public delegate void LogCallbackFuncType(IntPtr textPtr);
        
        public LogCallbackFuncType LogError { get; set; }
        public LogCallbackFuncType LogWarn { get; set; }
        public LogCallbackFuncType LogInfo { get; set; }
        public IntPtr LogErrorFuncPtr => DelegateToPtr(LogError);
        public IntPtr LogWarnFuncPtr => DelegateToPtr(LogWarn);
        public IntPtr LogInfoFuncPtr => DelegateToPtr(LogInfo);

        private static readonly LogCallbackFuncType StdErrFunc = messagePtr =>
            Console.Error.WriteLine(PtrToStr(messagePtr));

        private static readonly LogCallbackFuncType StdOutFunc = messagePtr => Console.WriteLine(PtrToStr(messagePtr));

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
    public class DllLogger : IDisposable
    {
        private readonly IntPtr handle;
        private int disposed;
        private readonly bool hasOwnership;

        internal IntPtr Handle => this.handle;

        public DllLogger()
        {
            APIResult result = NativeMethods.plateau_create_dll_logger(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outPtr;
            this.hasOwnership = true;
        }

        public DllLogger(IntPtr handle)
        {
            this.handle = handle;
            this.hasOwnership = false;
        }

        ~DllLogger()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!this.hasOwnership)
                return;

            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                NativeMethods.plateau_delete_dll_logger(this.handle);
            }

            GC.SuppressFinalize(this);
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

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_dll_logger(
                out IntPtr outHandle
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern void plateau_delete_dll_logger([In] IntPtr dllLogger);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dll_logger_set_callbacks(
                [In] IntPtr handle,
                [In] IntPtr errorCallbackFuncPtr,
                [In] IntPtr warnCallbackPtrFuncPtr,
                [In] IntPtr infoCallbackFuncPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dll_logger_set_log_level(
                [In] IntPtr handle,
                DllLogLevel dllLogLevel);
        }
    }
}
