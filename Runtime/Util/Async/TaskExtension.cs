using System;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.Util.Async
{
    public static class TaskExtension
    {
        /// <summary>
        ///  投げっぱなしのタスクの例外をエラーログで確認できるようにする拡張メソッドです。
        /// </summary>
        public static Task ContinueWithErrorCatch(this Task task)
        {
            return task.ContinueWith(t =>
            {
                if (t.Exception is { } age)
                {
                    LogInnerExceptions(age);
                }
            });
        }

        private static void LogInnerExceptions(AggregateException age)
        {
            var innerExceptions = age.InnerExceptions;
            foreach (var inner in innerExceptions)
            {
                if (inner is AggregateException innerAge)
                {
                    LogInnerExceptions(innerAge);
                }
                else
                {
                    Debug.LogError($"{inner.Message}\n{inner.StackTrace}");
                }
                
            }
        }
    }
}
