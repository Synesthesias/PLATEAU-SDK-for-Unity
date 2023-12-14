using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityImport.Import.Convert
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
        
        /// <summary> 再帰的な子です。 </summary>
        private readonly List<ConvertedGameObjData> children = new List<ConvertedGameObjData>();
        
        private readonly AttributeDataHelper attributeDataHelper;
        private bool isActive;

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
            this.isActive = true; // RootはActive
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
            this.isActive = plateauNode.IsActive;
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
        public async Task<GranularityConvertResult> PlaceToScene(Transform parent, PlaceToSceneConfig conf, bool skipRoot)
        {
            var result = new GranularityConvertResult();
            try
            {
                await PlaceToSceneRecursive(result, parent, conf, skipRoot, 0);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to placing to scene.\n{e.Message}\n{e.StackTrace}");
                result = GranularityConvertResult.Fail();
            }

            return result;
        }

        private async Task PlaceToSceneRecursive(GranularityConvertResult result, Transform parent,
            PlaceToSceneConfig conf, bool skipRoot, int recursiveDepth)
        {
            conf.CancellationToken?.ThrowIfCancellationRequested();

            var nextParent = parent;
            if (!skipRoot)
            {
                if (this.meshData == null || this.meshData.VerticesCount <= 0)
                {
                    // メッシュがなければ、中身のないゲームオブジェクトを作成します。
                    var obj = new GameObject
                    {
                        transform =
                        {
                            parent = parent
                        },
                        name = this.name,
                        isStatic = true,
                    };
                    obj.SetActive(isActive);
                    nextParent = obj.transform;
                    result.Add(nextParent.gameObject, recursiveDepth == 0);
                }
                else
                {
                    // メッシュがあれば、それを配置します。（ただし頂点数が0の場合は配置しません。）
                    var placedObj = await this.meshData.PlaceToScene(parent, conf.MaterialConverter, conf.FallbackMaterial, isActive);
                    if (placedObj != null)
                    {
                        nextParent = placedObj.transform;

                        if (conf.DoSetMeshCollider)
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
                        attrInfo.Init(serialized, conf.InfoForToolkits, conf.Granularity);
                    }
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                await child.PlaceToSceneRecursive(result, nextParent, conf, false, nextRecursiveDepth);
            }
        }
    }
}