using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PLATEAU.Util.Async
{
    /// <summary>
    /// URLから画像をロードしてテクスチャとして返します。
    /// </summary>
    public static class TextureLoader
    {
        /// <summary>
        /// 非同期で url から画像をダウンロードしてテクスチャとして返します。
        /// コルーチンを使うので、メインスレッドで行われる必要があります。
        /// 失敗した場合は null を返します。
        /// </summary>
        public static async Task<Texture2D> LoadAsync(string url, int timeOutSec)
        {
            var request = UnityWebRequestTexture.GetTexture(url);
            request.timeout = timeOutSec;
            // 注意 :
            // 下の SendWebRequest は、見た目に反してメインスレッドで行われ、Unityのコルーチンによって await します。
            // UnityWebRequestExtension クラスの拡張メソッドにより、コルーチンを await できるようにする機能を使っており、
            // コルーチンはメインスレッド限定です。
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"failed to load texture : {url} result = {(int)request.result}");
                return null;
            }

            var texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            return texture;
        }
    }
}
