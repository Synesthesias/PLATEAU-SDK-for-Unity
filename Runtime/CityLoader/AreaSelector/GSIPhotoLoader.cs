using System.Threading.Tasks;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.Networking;

namespace PLATEAU.CityLoader.AreaSelector
{
    /// <summary>
    /// 国土地理院の地図画像を取得します。
    /// </summary>
    public static class GSIPhotoLoader
    {
        private const string GSIMapURL = "https://cyberjapandata.gsi.go.jp/xyz";
        private const int timeOutSec = 10;
        
        public static async Task Load(string id, int z, int x, int y, MeshRenderer renderer)
        {
            string url = $"{GSIMapURL}/{id}/{z}/{x}/{y}.jpg";
            Texture texture = await TextureLoader.LoadAsync(url, timeOutSec);
            if (texture == null) return;
            renderer.material.mainTexture = texture;
        }
    }
}
