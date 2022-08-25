using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.GeometryModel;
using UnityEngine;

namespace PLATEAU.CityGrid
{
    internal class ConvertedGameObjData
    {
        private readonly ConvertedMeshData meshData;
        private readonly string name;
        private readonly List<ConvertedGameObjData> children = new List<ConvertedGameObjData>();

        /// <summary>
        /// C++側の <see cref="GeometryModel.Model"/> から変換して
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
                this.children.Add(new ConvertedGameObjData(rootNode));
            }
            Debug.Log("converted plateau model.");
        }

        /// <summary>
        /// C++側の <see cref="GeometryModel.Node"/> から変換して
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
        public async Task PlaceToScene(Transform parent, string gmlAbsolutePath)
        {
            GameObject nextParent;
            if (this.meshData == null)
            {
                nextParent = new GameObject
                {
                    transform =
                    {
                        parent = parent
                    },
                    name = this.name
                };
            }
            else
            {
                nextParent = await this.meshData.PlaceToScene(parent, gmlAbsolutePath);
            }

            foreach (var child in this.children)
            {
                await child.PlaceToScene(nextParent.transform, gmlAbsolutePath);
            }
        }
    }
}