using UnityEngine;

namespace PLATEAU.Editor.CityExport.ModelConvert
{
    /// <summary>
    /// ゲームエンジンとDLL側でモデルデータをやりとりするには、C++の Model という名前の中間データ構造を使います。
    /// このクラスはUnityのシーンに配置されたモデルを C++の Model に変換します。
    /// </summary>
    internal class UnityMeshToDllModelConverter
    {
        public void Convert(GameObject gameObject)
        {
            
        }

        private void ConvertGameObj(GameObject go)
        {
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter == null) return;
            var mesh = meshFilter.mesh;
            if (mesh == null) return;
            
        }

        private void ConvertMesh(Mesh unityMesh)
        {
            var dllMesh = PolygonMesh.Mesh.Create(unityMesh.name);
            
        }
    }
}
