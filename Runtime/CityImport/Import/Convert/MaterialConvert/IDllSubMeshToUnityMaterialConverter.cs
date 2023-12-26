using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityImport.Import.Convert.MaterialConvert
{
    /// <summary>
    /// 共通ライブラリのSubMeshをUnityのマテリアルに変換する機能を抽象化したものです。
    /// なぜ抽象化するかというと、マテリアルの変換の方法が次のとおり複数あるためです。
    /// 1つ目は、インポート時に、CityGMLのテクスチャとマテリアル情報からマテリアルを生成する方法。
    /// 2つ目は、分割結合時に、ゲームエンジンのマテリアル番号からマテリアルを復元する方法です。
    /// それぞれ子クラスで実装し、クラスによって挙動を切り替えます。
    /// </summary>
    internal interface IDllSubMeshToUnityMaterialConverter
    {
        Task<Material> ConvertAsync(ConvertedMeshData meshData, int subMeshIndex, Material fallbackMaterial);
    }
}