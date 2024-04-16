using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Texture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Mesh;

namespace PLATEAU.CityImport.Import.Convert
{
    internal class ConvertedTerrainData
    {
        internal class HeightmapData
        {
            public string Name;
            public PlateauVector3d Min;
            public PlateauVector3d Max;
            public int TextureHeight;
            public int TextureWidth;
            public float[,] HeightMapTexture;

            //debug 
            public UInt16[] HeightData;
        }


        private readonly HeightmapData heightmapData;
        private readonly string name;

        /// <summary> 再帰的な子です。 </summary>
        private readonly List<ConvertedTerrainData> children = new List<ConvertedTerrainData>();

        private bool isActive;

        public ConvertedTerrainData(Model plateauModel)
        {
            this.heightmapData = null;
            this.name = "CityRoot";
            this.isActive = true; // RootはActive
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                this.children.Add(new ConvertedTerrainData(rootNode));
            }
        }

        private ConvertedTerrainData(Node plateauNode)
        {
            this.heightmapData = ConvertFromMesh(plateauNode.Mesh, plateauNode.Name);
            this.name = plateauNode.Name;
            this.isActive = plateauNode.IsActive;

            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedTerrainData(child));
            }
        }

        private HeightmapData ConvertFromMesh(PolygonMesh.Mesh mesh, string NodeName)
        {
            if (mesh == null) 
                return null;

            int TextureHeight = 513;
            int TextureWidth = 513;

            PlateauVector2d Margin = new PlateauVector2d(0,0);

            HeightmapGenerator gen = new();
            gen.GenerateFromMesh(mesh, TextureWidth, TextureHeight, Margin, out PlateauVector3d Min, out PlateauVector3d Max, out UInt16[] HeightData);

            float[,] HeightMapTexture = HeightmapGenerator.ConvertTo2DFloatArray(HeightData, TextureWidth, TextureHeight);

            
            HeightmapData data = new();
            data.Name = NodeName;
            data.Min = Min;
            data.Max = Max;
            data.TextureHeight = TextureHeight;
            data.TextureWidth = TextureWidth;
            data.HeightMapTexture = HeightMapTexture;

            //Debug
            data.HeightData = HeightData;

            Debug.Log($"Convert Terrain {NodeName} ({Min.X},{Min.Y},{Min.Z}) ({Max.X},{Max.Y},{Max.Z})");

            return data;
        }


        float[,] GetGradation(int TextureHeight, int TextureWidth)
        {
            float[,] data = new float[TextureHeight, TextureWidth];
            //int index = 0;
            for (int y = 0; y < TextureHeight; y++)
            {
                for (int x = 0; x < TextureWidth; x++)
                {

                    float xpercent = (float)x / (float)TextureWidth;
                    float ypercent = (float)y / (float)TextureHeight;


                    data[y, x] = ypercent;

                    //index++;

                }
            }
            return data;
        }

        public void PlaceToScene(Transform parent, PlaceToSceneConfig conf, bool skipRoot)
        {
            PlaceToSceneRecursive(parent, conf, skipRoot, 0);
        }

        private void PlaceToSceneRecursive(Transform parent,
            PlaceToSceneConfig conf, bool skipRoot, int recursiveDepth)
        {
            var nextParent = parent;
            if (!skipRoot)
            {
                if (this.heightmapData == null)
                {

                }
                else
                {
                    // メッシュがあれば、それを配置します。（ただし頂点数が0の場合は配置しません。）
                    //var placedObj = await this.meshData.PlaceToScene(parent, conf.MaterialConverter, conf.FallbackMaterial, isActive);

                    TerrainData _terraindata = new TerrainData();

                    Debug.Log("Convert Terrain");

                    Debug.Log($"Size:x{Math.Abs(this.heightmapData.Max.X - this.heightmapData.Min.X)} z{Math.Abs(this.heightmapData.Max.Z - this.heightmapData.Min.Z)}");

                    
                    _terraindata.heightmapResolution = this.heightmapData.TextureHeight;
                    _terraindata.baseMapResolution = 1024;
                    _terraindata.SetDetailResolution(1024, 32);

                    _terraindata.SetHeights(0, 0, this.heightmapData.HeightMapTexture);
                    _terraindata.size = new Vector3(
                        (float)Math.Abs(this.heightmapData.Max.X - this.heightmapData.Min.X),
                        (float)Math.Abs(this.heightmapData.Max.Y - this.heightmapData.Min.Y),
                        (float)Math.Abs(this.heightmapData.Max.Z - this.heightmapData.Min.Z));


                    GameObject terrain = Terrain.CreateTerrainGameObject(_terraindata);
                    terrain.name = this.heightmapData.Name;
                    terrain.transform.position = new Vector3((float)this.heightmapData.Min.X, (float)this.heightmapData.Min.Y, (float)this.heightmapData.Min.Z);
                    //terrain.transform.position = new Vector3((float)this.heightmapData.Min.X, (float)this.heightmapData.Min.Y, -(float)this.heightmapData.Max.Z);

                    //Debug Png Export
                    HeightmapGenerator.SavePngFile(Application.dataPath + "/Generated_HeightMap.png", this.heightmapData.TextureWidth, this.heightmapData.TextureHeight, this.heightmapData.HeightData);
                    

                    if (terrain != null)
                    {
                        nextParent = terrain.transform;
                    }
                    
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                child.PlaceToSceneRecursive(nextParent, conf, false, nextRecursiveDepth);
            }
        }
    }
}
