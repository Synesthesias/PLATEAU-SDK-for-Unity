using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityImport.Load.Convert.MaterialConvert;
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
                this.children.Add(new ConvertedGameObjData(rootNode, attributeDataHelper.Copy()));
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
                this.children.Add(new ConvertedGameObjData(child, attributeDataHelper.Copy()));
                this.attributeDataHelper.AddOutsideChildren(child?.Name);
            }
        }

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// 再帰によって子も配置します。
        /// 配置したゲームオブジェクトのリストを返します。
        /// </summary>
        public async Task<PlateauToUnityModelConverter.ConvertResult> PlaceToScene(
            Transform parent, IDllSubMeshToUnityMaterialConverter materialConverter, bool skipRoot, bool doSetMeshCollider,
            CancellationToken? token, Material fallbackMaterial, CityObjectGroupInfoForToolkits infoForToolkits)
        {
            var result = new PlateauToUnityModelConverter.ConvertResult();
            try
            {
                await PlaceToSceneRecursive(result, parent, materialConverter, skipRoot, doSetMeshCollider, token,
                    fallbackMaterial, 0, infoForToolkits);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to placing to scene.\n{e.Message}\n{e.StackTrace}");
                result = PlateauToUnityModelConverter.ConvertResult.Fail();
            }

            return result;
        }

        private async Task PlaceToSceneRecursive(PlateauToUnityModelConverter.ConvertResult result, Transform parent,
            IDllSubMeshToUnityMaterialConverter materialConverter, bool skipRoot, bool doSetMeshCollider,
            CancellationToken? token, Material fallbackMaterial, int recursiveDepth,
            CityObjectGroupInfoForToolkits infoForToolkits)
        {
            token?.ThrowIfCancellationRequested();

            var nextParent = parent;
            if (!skipRoot)
            {
                if (this.meshData == null || this.meshData.VerticesCount <= 0)
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
                    result.Add(nextParent.gameObject, recursiveDepth == 0);
                }
                else
                {
                    // メッシュがあれば、それを配置します。（ただし頂点数が0の場合は配置しません。）
                    var placedObj = await this.meshData.PlaceToScene(parent, materialConverter, fallbackMaterial);
                    if (placedObj != null)
                    {
                        nextParent = placedObj.transform;

                        if (doSetMeshCollider)
                        {
                            placedObj.AddComponent<MeshCollider>();
                        }
                        result.Add(nextParent.gameObject, recursiveDepth == 0);
                    }
                }
 
                if(nextParent != null && nextParent.gameObject.GetComponent<PLATEAUCityObjectGroup>() == null && nextParent.gameObject.name == this.name)
                {
                    //　属性情報表示コンポーネントを追加します。
                    var serialized = this.attributeDataHelper.GetSerializableCityObject();
                    if (serialized != null)
                    {
                        var attrInfo = nextParent.gameObject.AddComponent<PLATEAUCityObjectGroup>();
                        attrInfo.Init(serialized, infoForToolkits);
                    }
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                await child.PlaceToSceneRecursive(result, nextParent, materialConverter, false, doSetMeshCollider, token, fallbackMaterial, nextRecursiveDepth, infoForToolkits);
            }
        }
    }
}