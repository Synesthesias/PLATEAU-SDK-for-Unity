using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 国土地理院の地図画像を取得します。
    /// </summary>
    [Obsolete] // TODO あとで消す
    internal static class GSIPhotoLoader
    {
        private const string gsiMapURL = "https://cyberjapandata.gsi.go.jp/xyz";
        private const int timeOutSec = 10;
        private const string mapMaterialPath = "Packages/com.synesthesias.plateau-unity-sdk/Materials/MapUnlitMaterial.mat";
        
        /// <summary>
        /// 国土地理院の地図画像を取得し、引数の <paramref name="renderer"/> のマテリアルのメインテクスチャにその地図画像を割り当てます。
        /// 引数の id, z, x, y はAPIにURLとして渡す値です。
        /// 引数の値の意味は <see href="https://maps.gsi.go.jp/development/siyou.html#siyou-url"> 国土地理院のWebページ </see> を参照してください。
        /// どのような id の地図が存在するかは、 <see href="https://maps.gsi.go.jp/development/ichiran.html"> 地理院地図の一覧 </see> を参照してください。
        /// また、生成されたマテリアルは 引数の <paramref name="mapMaterials"/> に追加されます。
        /// Editモードで生成されたマテリアルは利用終了後に廃棄しないとリークするので、
        /// メソッド利用者の責任で <paramref name="mapMaterials"/> の各マテリアルを廃棄する必要があります。
        /// </summary>
        public static async Task Load(string id, int z, int x, int y, MeshRenderer renderer, List<Material> mapMaterials)
        {
            #if UNITY_EDITOR
            string url = $"{gsiMapURL}/{id}/{z}/{x}/{y}.jpg";
            Texture texture = await TextureLoader.LoadAsync(url, timeOutSec);
            if (texture == null) return;
            var loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(mapMaterialPath);
            if (loadedMaterial == null)
            {
                Debug.LogError("Could not find material.");
                return;
            }
            renderer.sharedMaterial = loadedMaterial;
            var mat = new Material(renderer.sharedMaterial)
            {
                mainTexture = texture
            };
            // ReSharper disable once Unity.InefficientPropertyAccess
            renderer.sharedMaterial = mat;
            mapMaterials.Add(mat);
            #endif
        }
    }
}
