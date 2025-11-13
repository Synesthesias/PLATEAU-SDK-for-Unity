using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Util
{
    /// <summary>
    /// デバッグ用に処理時間を計測し、ログに出力するためのクラスです。
    /// </summary>
    internal class DebugStopwatch
    {
        private readonly bool enabled;
        private readonly Stopwatch stopwatch;
        private readonly string title;
        private string message;
        
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public DebugStopwatch(string title, bool enabled = true)
        {
            this.enabled = enabled;
            this.title = title;
            if (!enabled) return;
            stopwatch = new Stopwatch();
        }

        /// <summary>
        /// 計測を開始します。
        /// </summary>
        public void Start(string messageArg)
        {
            message = messageArg;
            if (!enabled) return;
            stopwatch.Start();
        }

        /// <summary>
        /// 計測を終了し、ログを出力します。
        /// </summary>
        public void Stop()
        {
            if (!enabled) return;
            stopwatch.Stop();
            Debug.Log($"StopWatch:【{title}】{message}: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Reset();
        }
    }
}
