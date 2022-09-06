using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PLATEAU.Util.Async
{
    public static class TextureLoader
    {
        public static async Task<Texture> LoadAsync(string url, int timeOutSec)
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

            Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            return texture;
        }
    }
}
