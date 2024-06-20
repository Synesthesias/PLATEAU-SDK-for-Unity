using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Texture;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.TerrainConvert
{
    internal class ConvertedTerrainData
    {
        internal class HeightmapData
        {
            public string name;
            public PlateauVector3d min;
            public PlateauVector3d max;
            public int textureHeight;
            public int textureWidth;
            public float[,] heightMapTexture;

            public string diffuseTexturePath;
            public PlateauVector2f minUV;
            public PlateauVector2f maxUV;

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
            this.heightmapData = ConvertFromMesh(plateauNode.Mesh, plateauNode.Name, convertOption.TextureWidth, convertOption.TextureHeight, convertOption.FillEdges);
            this.name = plateauNode.Name;
            this.isActive = plateauNode.IsActive;

            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedTerrainData(child, convertOption));
            }
        }

        private HeightmapData ConvertFromMesh(PolygonMesh.Mesh mesh, string nodeName, int textureWidth, int textureHeight, bool fillEdges)
        {
            if (mesh == null) 
                return null;

            PlateauVector2d margin = new PlateauVector2d(0,0);
            HeightmapGenerator gen = new();
            gen.GenerateFromMesh(mesh, textureWidth, textureHeight, margin, fillEdges, out PlateauVector3d min, out PlateauVector3d max, out PlateauVector2f minUV, out PlateauVector2f maxUV, out UInt16[] heightData);
            float[,] HeightMapTexture = HeightmapGenerator.ConvertTo2DFloatArray(heightData, textureWidth, textureHeight);

            HeightmapData data = new();
            data.name = nodeName;
            data.min = min;
            data.max = max;
            data.textureHeight = textureHeight;
            data.textureWidth = textureWidth;
            data.heightMapTexture = HeightMapTexture;

            //Diffuse Texture
            data.diffuseTexturePath = GetTexturePath(mesh);
            if(!string.IsNullOrEmpty(data.diffuseTexturePath))
            {
                data.minUV = minUV;
                data.maxUV = maxUV;
            }

            //For debug image output
            data.HeightData = heightData;
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

        //同名のGameObjectのTransformを取得
        private Transform GetTransformByName(GameObject[] gameObjects, string name)
        {
            var found = gameObjects.First(x => x.name == name);
            if (found != null)
                return found.transform;
            return gameObjects.First().transform.root;
        }

        public async Task<TerrainConvertResult> PlaceToScene(GameObject[] srcGameObjs, TerrainConvertOption option, bool skipRoot)
        {
            var result = new TerrainConvertResult();
            PlaceToSceneRecursive(result, srcGameObjs, option, skipRoot, 0);
            return await Task.FromResult<TerrainConvertResult>(result);
        }

        private Texture2D LoadTexture2dWithoutAlpha(string texturePath)
        {
            var texture = new Texture2D(1, 1);
            if (string.IsNullOrEmpty(texturePath))
                texturePath = PathUtil.SdkPathToAssetPath("Materials/Textures/White.PNG");
            var rawData = System.IO.File.ReadAllBytes(texturePath);
            texture.LoadImage(rawData);
            //terrainマテリアルはalphaをsmoothnessとして扱うため、alphaなしに変換
            var outTexture = new Texture2D(texture.width,　texture.height, TextureFormat.RGB24, false);
            outTexture.SetPixels(texture.GetPixels());
            outTexture.Apply();
            return outTexture;
        }

        private void PlaceToSceneRecursive(TerrainConvertResult result, GameObject[] srcGameObjs,
            TerrainConvertOption option, bool skipRoot, int recursiveDepth)
        {
            if (!skipRoot)
            {
                if (this.heightmapData != null)
                {
                    //Terrain Data
                    TerrainData terraindata = new TerrainData();
                    terraindata.heightmapResolution = this.heightmapData.textureHeight;
                    var terrainWidth = (float)Math.Abs(this.heightmapData.max.X - this.heightmapData.min.X);
                    var terrainLength = (float)Math.Abs(this.heightmapData.max.Z - this.heightmapData.min.Z);
                    var terrainHeight = (float)Math.Abs(this.heightmapData.max.Y - this.heightmapData.min.Y);

                    terraindata.SetHeights(0, 0, this.heightmapData.heightMapTexture);
                    terraindata.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

                    //Terrain Material
                    TerrainLayer materialLayer = new TerrainLayer();
                    materialLayer.diffuseTexture = LoadTexture2dWithoutAlpha(this.heightmapData.diffuseTexturePath);

                    var uvSize = new Vector2(
                        terrainWidth / (this.heightmapData.maxUV.X - this.heightmapData.minUV.X),
                        terrainLength / (this.heightmapData.maxUV.Y - this.heightmapData.minUV.Y));

                    materialLayer.tileSize = new Vector2(uvSize.x, uvSize.y);
                    materialLayer.tileOffset = new Vector2(
                        uvSize.x * this.heightmapData.minUV.X,
                        uvSize.y * this.heightmapData.minUV.Y);

                    materialLayer.smoothness = 0f;
                    terraindata.terrainLayers = new TerrainLayer[] { materialLayer };

                    //Terrain GameObject
                    GameObject terrain = Terrain.CreateTerrainGameObject(terraindata);
                    terrain.name = $"TERRAIN_{this.heightmapData.name}";
                    terrain.transform.position = new Vector3((float)this.heightmapData.min.X, (float)this.heightmapData.min.Y, (float)this.heightmapData.min.Z);
                    terrain.transform.SetParent(GetTransformByName(srcGameObjs, this.heightmapData.name).parent);

                    result.Add(terrain);

                    //Debug Image Output
                    if (option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG || option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG_RAW)
                        HeightmapGenerator.SavePngFile($"{Application.dataPath}/HM_{this.heightmapData.name}_{this.heightmapData.textureWidth}_{this.heightmapData.textureHeight}.png", this.heightmapData.textureWidth, this.heightmapData.textureHeight, this.heightmapData.HeightData);
                    if (option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.RAW || option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG_RAW)
                        HeightmapGenerator.SaveRawFile($"{Application.dataPath}/HM_{this.heightmapData.name}_{this.heightmapData.textureWidth}_{this.heightmapData.textureHeight}.raw", this.heightmapData.textureWidth, this.heightmapData.textureHeight, this.heightmapData.HeightData);
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in this.children)
            {
                child.PlaceToSceneRecursive(result, srcGameObjs, option, false, nextRecursiveDepth);
            }
        }
    }
}
