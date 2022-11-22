using System.Collections;
using System.Threading.Tasks;

namespace PLATEAU.Tests.TestUtils
{
    public static class TaskExtension
    {
        /// <summary>
        /// 非同期Taskをコルーチンに変換することで、Unityテストで実行できるようにします。
        /// 参考 : https://light11.hatenadiary.com/entry/2020/05/23/194951
        /// </summary>
        public static IEnumerator AsIEnumerator(this Task task)
        {
            
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
    }
}
