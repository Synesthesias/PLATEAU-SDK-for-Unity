using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityImport.Load.Convert
{
    /// <summary>
    /// PLATEAU から Unity の GameObject を生成するためのデータです。
    /// メッシュデータ、名前、子を持ちます。
    /// PLATEAU 側の <see cref="PolygonMesh.Model"/> 以下にある <see cref="Node"/> の木構造が
    /// ゲームエンジン側のゲームオブジェクト階層に対応するよう設計されているので、それを Unity用のデータに直したものです。
    /// 子は木構造を形成し、それはゲームエンジン側のヒエラルキーに対応します。
    /// </summary>
    internal class ConvertedGameObjData
    {
        private readonly ConvertedMeshData meshData;
        private readonly string name;
        private readonly List<ConvertedGameObjData> children = new List<ConvertedGameObjData>();

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Model"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        /// <param name="plateauModel"></param>
        public ConvertedGameObjData(Model plateauModel)
        {
            this.meshData = null;
            this.name = "CityRoot";
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                this.children.Add(new ConvertedGameObjData(rootNode));
            }
            Debug.Log("converted plateau model.");
        }

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Node"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        public ConvertedGameObjData(Node plateauNode)
        {
            this.meshData = MeshConverter.Convert(plateauNode.Mesh, plateauNode.Name);
            this.name = plateauNode.Name;
            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedGameObjData(child));
            }
        }

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// 再帰によって子も配置します。
        /// </summary>
        public async Task PlaceToScene(Transform parent, Dictionary<string, Texture> cachedTexture, bool skipRoot, bool doSetMeshCollider, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var nextParent = parent;
            if (!skipRoot)
            {
                if (this.meshData == null)
                {
                    // メッシュがなければ、中身のないゲームオブジェクトを作成します。
                    nextParent = new GameObject
                    {
                        transform =
                        {
                            parent = parent
                        },
                        name = this.name
                    }.transform;
                }
                else
                {
                    // メッシュがあれば、それを配置します。（ただし頂点数が0の場合は配置しません。）
                    var placedObj = await this.meshData.PlaceToScene(parent, cachedTexture);
                    if (placedObj != null)
                    {
                        nextParent = placedObj.transform;
                        if (doSetMeshCollider)
                        {
                            placedObj.AddComponent<MeshCollider>();
                        }
                    }
                }
            }
            
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                await child.PlaceToScene(nextParent.transform, cachedTexture, false, doSetMeshCollider, token);
            }
        }
    }
}