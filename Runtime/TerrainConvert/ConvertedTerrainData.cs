using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Texture;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.TerrainConvert
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

            public string DiffuseTexturePath;
            public PlateauVector2f MinUV;
            public PlateauVector2f MaxUV;

            //debug 
            public UInt16[] HeightData;
        }

        private readonly HeightmapData heightmapData;
        private readonly string name;

        /// <summary> 再帰的な子です。 </summary>
        private readonly List<ConvertedTerrainData> children = new List<ConvertedTerrainData>();

        private bool isActive;

        public ConvertedTerrainData(Model plateauModel, TerrainConvertOption convertOption)
        {
            this.heightmapData = null;
            this.name = "CityRoot";
            this.isActive = true; // RootはActive
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                this.children.Add(new ConvertedTerrainData(rootNode, convertOption));
            }
        }

        private ConvertedTerrainData(Node plateauNode, TerrainConvertOption convertOption)
        {
            this.heightmapData = ConvertFromMesh(plateauNode.Mesh, plateauNode.Name, convertOption.TextureWidth, convertOption.TextureHeight);
            this.name = plateauNode.Name;
            this.isActive = plateauNode.IsActive;

            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedTerrainData(child, convertOption));
            }
        }

        private HeightmapData ConvertFromMesh(PolygonMesh.Mesh mesh, string NodeName, int TextureWidth, int TextureHeight)
        {
            if (mesh == null) 
                return null;

            PlateauVector2d Margin = new PlateauVector2d(0,0);

            HeightmapGenerator gen = new();

            Debug.Log($"GenerateFromMesh ======================================");

            gen.GenerateFromMesh(mesh, TextureWidth, TextureHeight, Margin, out PlateauVector3d Min, out PlateauVector3d Max, out PlateauVector2f MinUV, out PlateauVector2f MaxUV, out UInt16[] HeightData);

            Debug.Log($"<color=yellow>HeightData Length:{HeightData.Length}</color>");

            float[,] HeightMapTexture = HeightmapGenerator.ConvertTo2DFloatArray(HeightData, TextureWidth, TextureHeight);

            Debug.Log($"<color=green>HeightMapTexture Length:{HeightMapTexture.Length}</color>");

            HeightmapData data = new();
            data.Name = NodeName;
            data.Min = Min;
            data.Max = Max;
            data.TextureHeight = TextureHeight;
            data.TextureWidth = TextureWidth;
            data.HeightMapTexture = HeightMapTexture;

            //Diffuse Texture
            data.DiffuseTexturePath = GetTexturePath(mesh);
            if(!string.IsNullOrEmpty(data.DiffuseTexturePath))
            {
                data.MinUV = MinUV;
                data.MaxUV = MaxUV;
            }

            //For debug image output
            data.HeightData = HeightData;
            return data;
        }

        private string GetTexturePath(PolygonMesh.Mesh mesh)
        {
            if(mesh.SubMeshCount > 0)
            {
                SubMesh _subMesh = mesh.GetSubMeshAt(0);
                return _subMesh.TexturePath;
            }
            return null;
        }

        public async Task<TerrainConvertResult> PlaceToScene(Transform parent, TerrainConvertOption option, bool skipRoot)
        {
            var result = new TerrainConvertResult();
            PlaceToSceneRecursive(result, parent, option, skipRoot, 0);
            return await Task.FromResult<TerrainConvertResult>(result);
        }

        private void PlaceToSceneRecursive(TerrainConvertResult result, Transform parent,
            TerrainConvertOption option, bool skipRoot, int recursiveDepth)
        {
            var nextParent = parent;
            if (!skipRoot)
            {
                if (this.heightmapData != null)
                {
                    TerrainData _terraindata = new TerrainData();
                    _terraindata.heightmapResolution = this.heightmapData.TextureHeight;
                    //TODO:
                    //_terraindata.baseMapResolution = 1024;
                    //_terraindata.SetDetailResolution(1024, 32);

                    var terrainWidth = (float)Math.Abs(this.heightmapData.Max.X - this.heightmapData.Min.X);
                    var terrainLength = (float)Math.Abs(this.heightmapData.Max.Z - this.heightmapData.Min.Z);
                    var terrainHeight = (float)Math.Abs(this.heightmapData.Max.Y - this.heightmapData.Min.Y);

                    _terraindata.SetHeights(0, 0, this.heightmapData.HeightMapTexture);
                    _terraindata.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

                    //Material
                    if(!string.IsNullOrEmpty(this.heightmapData.DiffuseTexturePath))
                    {
                        var DiffuseTexture = new Texture2D(1, 1);
                        var rawData = System.IO.File.ReadAllBytes(this.heightmapData.DiffuseTexturePath);
                        DiffuseTexture.LoadImage(rawData);

                        TerrainLayer _materialLayer = new TerrainLayer();
                        _materialLayer.diffuseTexture = DiffuseTexture;

                        var UVSize = new Vector2(
                            terrainWidth / (this.heightmapData.MaxUV.X - this.heightmapData.MinUV.X),
                            terrainLength / (this.heightmapData.MaxUV.Y - this.heightmapData.MinUV.Y));

                        _materialLayer.tileSize = new Vector2(UVSize.x, UVSize.y);
                        _materialLayer.tileOffset = new Vector2(
                            UVSize.x * this.heightmapData.MinUV.X,
                            UVSize.y * this.heightmapData.MinUV.Y);

                        _terraindata.terrainLayers = new TerrainLayer[] { _materialLayer };
                    }

                    GameObject terrain = Terrain.CreateTerrainGameObject(_terraindata);
                    terrain.name = this.heightmapData.Name;
                    terrain.transform.position = new Vector3((float)this.heightmapData.Min.X, (float)this.heightmapData.Min.Y, (float)this.heightmapData.Min.Z);

                    //Debug Image Output
                    if(option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG || option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG_RAW)
                        HeightmapGenerator.SavePngFile($"{Application.dataPath}/HM_{this.heightmapData.Name}_{this.heightmapData.TextureWidth}_{this.heightmapData.TextureHeight}.png", this.heightmapData.TextureWidth, this.heightmapData.TextureHeight, this.heightmapData.HeightData);
                    if (option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.RAW || option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG_RAW)
                        HeightmapGenerator.SaveRawFile($"{Application.dataPath}/HM_{this.heightmapData.Name}_{this.heightmapData.TextureWidth}_{this.heightmapData.TextureHeight}.raw", this.heightmapData.TextureWidth, this.heightmapData.TextureHeight, this.heightmapData.HeightData);

                    if (terrain != null)
                    {
                        result.Add(terrain);
                        nextParent = terrain.transform;
                    } 
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                child.PlaceToSceneRecursive(result, nextParent, option, false, nextRecursiveDepth);
            }
        }
    }
}
