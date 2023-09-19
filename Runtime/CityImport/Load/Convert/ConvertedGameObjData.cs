using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
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
        private readonly AttributeDataHelper attributeDataHelper;

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Model"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        /// <param name="plateauModel"></param>
        public ConvertedGameObjData(Model plateauModel, AttributeDataHelper attributeDataHelper)
        {
            this.meshData = null;
            this.name = "CityRoot";
            this.attributeDataHelper = attributeDataHelper;
            this.attributeDataHelper.SetId(this.name);
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                this.children.Add(new ConvertedGameObjData(rootNode, new AttributeDataHelper(attributeDataHelper)));
            }
            Debug.Log("converted plateau model.");
        }

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Node"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        private ConvertedGameObjData(Node plateauNode, AttributeDataHelper attributeDataHelper)
        {
            this.meshData = MeshConverter.Convert(plateauNode.Mesh, plateauNode.Name);
            this.name = plateauNode.Name;
            this.attributeDataHelper = attributeDataHelper;
            this.attributeDataHelper.SetId(this.name);
            if (meshData != null)
                this.attributeDataHelper.SetCityObjectList(plateauNode.Mesh.CityObjectList);

            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedGameObjData(child, new AttributeDataHelper(attributeDataHelper)));
                this.attributeDataHelper.AddOutsideChildren(child?.Name);
            }
        }

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// 再帰によって子も配置します。
        /// </summary>
        public async Task PlaceToScene(Transform parent, Dictionary<MaterialSet, Material> cachedMaterials, bool skipRoot, bool doSetMeshCollider, CancellationToken token, Material fallbackMaterial)
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
                        name = this.name,
                        isStatic = true
                    }.transform;
                }
                else
                {
                    // メッシュがあれば、それを配置します。（ただし頂点数が0の場合は配置しません。）
                    var placedObj = await this.meshData.PlaceToScene(parent, cachedMaterials, fallbackMaterial);
                    if (placedObj != null)
                    {
                        nextParent = placedObj.transform;

                        if (doSetMeshCollider)
                        {
                            placedObj.AddComponent<MeshCollider>();
                        }
                    }
                }
 
                if(nextParent.gameObject.GetComponent<PLATEAUCityObjectGroup>() == null && nextParent.gameObject.name == this.name)
                {
                    //　属性情報表示コンポーネントを追加します。
                    var serialized = this.attributeDataHelper.GetSerializableCityObject();
                    if (serialized != null)
                    {
                        var attrInfo = nextParent.gameObject.AddComponent<PLATEAUCityObjectGroup>();
                        attrInfo.SetSerializableCityObject(serialized);
                    }
                }
                this.attributeDataHelper.Dispose();
            }
            
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                await child.PlaceToScene(nextParent.transform, cachedMaterials, false, doSetMeshCollider, token, fallbackMaterial);
            }
        }
    }
}