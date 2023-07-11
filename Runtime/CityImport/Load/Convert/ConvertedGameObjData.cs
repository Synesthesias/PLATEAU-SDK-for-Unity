using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using Codice.Client.BaseCommands;
using Codice.Client.Common.FsNodeReaders;
using JetBrains.Annotations;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using PLATEAU.CityGML;
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

        private readonly PLATEAU.CityGML.CityModel cityModel;
        private readonly MeshGranularity meshGranularity;
        private readonly string parant;
        private readonly List<CityObjectListID> cityObjectListId = new List<CityObjectListID>();
        class CityObjectListID
        {
            public CityObjectIndex Index;
            public string AtomicID;
            public string PrimaryID;
        }

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Model"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        /// <param name="plateauModel"></param>
        public ConvertedGameObjData(Model plateauModel, PLATEAU.CityGML.CityModel cityModel, MeshGranularity granularity)
        {
            this.meshData = null;
            this.name = "CityRoot";
            this.cityModel = cityModel;
            this.meshGranularity = granularity;
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                this.children.Add(new ConvertedGameObjData(rootNode, cityModel, granularity));
            }
            Debug.Log("converted plateau model.");
        }

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Node"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        public ConvertedGameObjData(Node plateauNode, PLATEAU.CityGML.CityModel cityModel, MeshGranularity granularity)
        {
            this.meshData = MeshConverter.Convert(plateauNode.Mesh, plateauNode.Name);
            this.name = plateauNode.Name;
            this.cityModel = cityModel;
            this.meshGranularity = granularity;
            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedGameObjData(child, cityModel, granularity));
            }

            if(meshData != null)
            {
                var cityObjectList = plateauNode.Mesh.CityObjectList;
                foreach (var key in cityObjectList.GetAllKeys())
                {
                    var atomicGmlID = cityObjectList.GetAtomicID(key);
                    var primaryGmlID = cityObjectList.GetPrimaryID(key.PrimaryIndex);

                    if(granularity == MeshGranularity.PerCityModelArea || 
                        (granularity == MeshGranularity.PerPrimaryFeatureObject && primaryGmlID == this.name))
                        cityObjectListId.Add(new CityObjectListID { Index = key,  AtomicID = atomicGmlID, PrimaryID = primaryGmlID });

                    if (granularity == MeshGranularity.PerAtomicFeatureObject && atomicGmlID == this.name)
                        this.parant = primaryGmlID;
                }
            }
        }

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// 再帰によって子も配置します。
        /// </summary>
        public async Task PlaceToScene(Transform parent, Dictionary<string, UnityEngine.Texture> cachedTexture, bool skipRoot, bool doSetMeshCollider, CancellationToken token)
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

                        //　属性情報表示コンポーネントを追加します。
                        var attrInfo = placedObj.AddComponent<PLATEAUCityObjectGroup>();
                        attrInfo.SetSerializableCityObject(GetSerializableCityObject());

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

        /// <summary>
        /// 各CityObjectの属性情報を取得してシリアライズ可能なデータに変換します
        /// </summary>
        public CityInfo.CityObject GetSerializableCityObject()
        {
            if (this.meshGranularity == MeshGranularity.PerCityModelArea)
                return GetSerializableCityObjectForArea();

            CityInfo.CityObject cityObjSer = new CityInfo.CityObject();

            if (!string.IsNullOrEmpty(parant))
                cityObjSer.parent = parant;

            var cityObj = GetCityObjectById(this.name);
            if (cityObj != null)
            {
                var ser = CityObjectSerializableConvert.FromCityGMLCityObject<CityInfo.CityObject.CityObjectParam>(cityObj);
                foreach (var id in cityObjectListId)
                {
                    if (id.PrimaryID == id.AtomicID) continue;
                    var childCityObj = GetCityObjectById(id.AtomicID);
                    if (childCityObj == null) continue;
                    ser.children.Add(CityObjectSerializableConvert.FromCityGMLCityObject<CityInfo.CityObject.CityObjectChildParam> (childCityObj, id.Index));
                }
                cityObjSer.cityObjects.Add(ser);
            }
            return cityObjSer;
        }

        //地域単位結合モデルの場合のシリアライズ可能なデータへの変換です
        public CityInfo.CityObject GetSerializableCityObjectForArea()
        {
            CityInfo.CityObject cityObjSer = new CityInfo.CityObject();

            List<string> cityObjList = new List<string>(); 
            Dictionary<string, List<CityObjectListID>> chidrenMap = new Dictionary<string, List<CityObjectListID>>();

            foreach (var id in cityObjectListId)
            {
                Debug.Log($"<color=magenta>[{id.Index.PrimaryIndex},{id.Index.AtomicIndex}] , Pri:{id.PrimaryID}, Atm:{id.AtomicID} </color>");

                if (string.IsNullOrEmpty(id.PrimaryID))
                    cityObjList.Add(id.AtomicID);
                else
                {
                    if (chidrenMap.ContainsKey(id.PrimaryID))
                        chidrenMap[id.PrimaryID].Add(id);
                    else
                        chidrenMap.Add(id.PrimaryID, new List<CityObjectListID>() {id});
                }
            }

            foreach (var id in cityObjectListId)
            {
                var cityObj = GetCityObjectById(id.AtomicID);
                if (cityObj == null) continue;
                var ser = CityObjectSerializableConvert.FromCityGMLCityObject<CityInfo.CityObject.CityObjectParam>(cityObj, id.Index);
                if (!chidrenMap.ContainsKey(id.AtomicID)) continue;
                var childrenId = chidrenMap[id.AtomicID];
                foreach(var c in childrenId)
                {
                    if (c.PrimaryID == c.AtomicID) continue;
                    var childCityObj = GetCityObjectById(c.AtomicID);
                    if (childCityObj == null) continue;
                    ser.children.Add(CityObjectSerializableConvert.FromCityGMLCityObject<CityInfo.CityObject.CityObjectChildParam>(childCityObj, c.Index));
                }
                cityObjSer.cityObjects.Add(ser);
            }
            return cityObjSer;
        }

        public PLATEAU.CityGML.CityObject GetCityObjectById(string id)
        {
            try
            {
                return cityModel.GetCityObjectById(id);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
            return null;
        }
    }
}