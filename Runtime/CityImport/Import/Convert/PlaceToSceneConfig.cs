using System.Threading;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Import.Convert
{
    /// <summary>
    /// PLATEAUモデルをシーンに配置するときの設定のうち、1つのモデルの処理中の間は値が不変であるものを集めたクラスです。
    /// </summary>
    internal class PlaceToSceneConfig
    {
        /// <summary> マテリアルを変換する方法です。 </summary>
        public IDllSubMeshToUnityMaterialConverter MaterialConverter { get; }
        
        /// <summary> メッシュコライダーをセットするかしないかです。 </summary>
        public bool DoSetMeshCollider { get; }
        
        /// <summary> キャンセル用トークンです。 </summary>
        public CancellationToken? CancellationToken { get; }
        
        /// <summary> モデル中にテクスチャやマテリアル指定がない場合に利用するデフォルトマテリアルです。 </summary>
        public UnityEngine.Material FallbackMaterial { get; }
        
        /// <summary> PLATEAU SDK Toolkits for Unityとの連携のために必要となるデータです。 </summary>
        public CityObjectGroupInfoForToolkits InfoForToolkits { get; }

        public MeshGranularity Granularity { get; }
        

        public PlaceToSceneConfig(IDllSubMeshToUnityMaterialConverter materialConverter, bool doSetMeshCollider, CancellationToken? cancellationToken, UnityEngine.Material fallbackMaterial, CityObjectGroupInfoForToolkits infoForToolkits, MeshGranularity granularity)
        {
            MaterialConverter = materialConverter;
            DoSetMeshCollider = doSetMeshCollider;
            CancellationToken = cancellationToken;
            FallbackMaterial = fallbackMaterial;
            InfoForToolkits = infoForToolkits;
            Granularity = granularity;
        }
    }
}