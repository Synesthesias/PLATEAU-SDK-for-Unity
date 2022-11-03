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
                    var inner = age.InnerException;
                    if (inner == null) return;
                    Debug.LogError($"{inner.Message}\n{inner.StackTrace}");
                }
            });
        } 
    }
}
