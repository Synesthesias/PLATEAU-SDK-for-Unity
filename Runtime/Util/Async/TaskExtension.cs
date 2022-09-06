using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.Util.Async
{
    public static class TaskExtension
    {
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
