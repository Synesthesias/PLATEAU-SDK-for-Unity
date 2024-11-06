using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Heightmap;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Texture;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Mesh = PLATEAU.PolygonMesh.Mesh;

namespace PLATEAU.TerrainConvert
{
    internal class ConvertedTerrainData
    {
        [Serializable]
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
            public string MapFilePath { get; set; }= "";

            //debug 
            public UInt16[] HeightData;

            public static HeightmapData CreateFromTerrain(Terrain terrain)
            {
                var trans = terrain.transform;
                var pos = trans.position;
                var data = terrain.terrainData;
                var resolution = data.heightmapResolution;
                var heights = data.GetHeights(0, 0, resolution, resolution);
                
                var ret = new HeightmapData()
                {
                    name = terrain.name,
                    min = pos.ToPlateauVector(), // TODO グローバル座標でいいのか？
                    max = (pos + data.size).ToPlateauVector(),
                    heightMapTexture = heights,
                    textureHeight = resolution,
                    textureWidth = resolution,
                    HeightData = HeightmapGenerator.ConvertToUInt16Array(heights, resolution, resolution)
                };
                return ret;
            }
        }

        private readonly HeightmapData heightmapData;
        private readonly string name;

        /// <summary> 再帰的な子です。 </summary>
        private readonly List<ConvertedTerrainData> children = new List<ConvertedTerrainData>();

        private bool isActive;

        public ConvertedTerrainData(Model plateauModel, TerrainConvertOption option)
        {
            heightmapData = null;
            name = "CityRoot";
            isActive = true; // RootはActive
            
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                children.Add(new ConvertedTerrainData(rootNode, option));
            }
        }

        /// <summary>
        /// 自身と子から、nullでない<see cref="HeightmapData"/>を集めます。
        /// </summary>
        public List<HeightmapData> GetHeightmapDataRecursive()
        {
            List<HeightmapData> ret = new List<HeightmapData>();
            GetHeightmapDataRecursive(ret);
            return ret;
        }
        
        private void GetHeightmapDataRecursive(List<HeightmapData> ret)
        {
            if(heightmapData != null) ret.Add(heightmapData);
            foreach (var child in children)
            {
                child.GetHeightmapDataRecursive(ret);
            }
        }

        private ConvertedTerrainData(Node plateauNode, TerrainConvertOption option)
        {
            heightmapData = ConvertFromMesh(plateauNode.Mesh, plateauNode.Name, option.TextureWidth, option.TextureHeight, option.FillEdges, option.ApplyConvolutionFilterToHeightMap);
            name = plateauNode.Name;
            isActive = plateauNode.IsActive;
            
            if (heightmapData != null)
            {
                //Debug Image Output
                heightmapData.MapFilePath =
                    $"{Application.dataPath}/HM_{heightmapData.name}_{heightmapData.textureWidth}_{heightmapData.textureHeight}";
                if (option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG || option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG_RAW)
                {
                    heightmapData.MapFilePath += ".png";
                    HeightmapGenerator.SavePngFile(heightmapData.MapFilePath, heightmapData.textureWidth, heightmapData.textureHeight, heightmapData.HeightData);
                    Debug.Log($"height map wrote to {heightmapData.MapFilePath}");
                }
                    
                if (option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.RAW || option.HeightmapImageOutput == TerrainConvertOption.ImageOutput.PNG_RAW)
                {
                    heightmapData.MapFilePath += ".raw";
                    HeightmapGenerator.SaveRawFile(heightmapData.MapFilePath, heightmapData.textureWidth, heightmapData.textureHeight, heightmapData.HeightData);                    
                }
            }

            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                children.Add(new ConvertedTerrainData(child, option));
            }
        }

        private HeightmapData ConvertFromMesh(Mesh mesh, string nodeName, int textureWidth, int textureHeight, bool fillEdges, bool applyConvolutionFilterToHeightMap)
        {
            if (mesh == null) 
                return null;

            PlateauVector2d margin = new PlateauVector2d(0,0);
            HeightmapGenerator gen = new();
            gen.GenerateFromMesh(mesh, textureWidth, textureHeight, margin, fillEdges, applyConvolutionFilterToHeightMap, out PlateauVector3d min, out PlateauVector3d max, out PlateauVector2f minUV, out PlateauVector2f maxUV, out UInt16[] heightData);
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

        private string GetTexturePath(Mesh mesh)
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

        public async Task<TerrainConvertResult> PlaceMeshToScene(GameObject[] srcGameObjs, TerrainConvertOption option, bool skipRoot)
        {
            var result = new TerrainConvertResult();
            PlaceMeshToSceneRecursive(result, srcGameObjs, option, skipRoot, 0);
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
                if (heightmapData != null)
                {
                    //Terrain Data
                    TerrainData terraindata = new TerrainData();
                    terraindata.heightmapResolution = heightmapData.textureHeight;
                    var terrainWidth = (float)Math.Abs(heightmapData.max.X - heightmapData.min.X);
                    var terrainLength = (float)Math.Abs(heightmapData.max.Z - heightmapData.min.Z);
                    var terrainHeight = (float)Math.Abs(heightmapData.max.Y - heightmapData.min.Y);

                    terraindata.SetHeights(0, 0, heightmapData.heightMapTexture);
                    terraindata.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

                    //Terrain Material
                    TerrainLayer materialLayer = new TerrainLayer();
                    materialLayer.diffuseTexture = LoadTexture2dWithoutAlpha(heightmapData.diffuseTexturePath);

                    var uvSize = new Vector2(
                        terrainWidth / (heightmapData.maxUV.X - heightmapData.minUV.X),
                        terrainLength / (heightmapData.maxUV.Y - heightmapData.minUV.Y));

                    materialLayer.tileSize = new Vector2(uvSize.x, uvSize.y);
                    materialLayer.tileOffset = new Vector2(
                        uvSize.x * heightmapData.minUV.X,
                        uvSize.y * heightmapData.minUV.Y);

                    materialLayer.smoothness = 0f;
                    terraindata.terrainLayers = new TerrainLayer[] { materialLayer };

                    //Terrain GameObject
                    GameObject terrain = Terrain.CreateTerrainGameObject(terraindata);
                    terrain.name = $"TERRAIN_{heightmapData.name}";
                    terrain.transform.position = new Vector3((float)heightmapData.min.X, (float)heightmapData.min.Y, (float)heightmapData.min.Z);
                    terrain.transform.SetParent(GetTransformByName(srcGameObjs, heightmapData.name).parent);

                    result.Add(terrain);
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in children)
            {
                child.PlaceToSceneRecursive(result, srcGameObjs, option, false, nextRecursiveDepth);
            }
        }

        public static UnityEngine.Mesh ConvertToMesh(HeightmapData heightmapData, string name)
        {
            var terrainHeight = (float)Math.Abs(heightmapData.max.Y - heightmapData.min.Y);
            var generator = new HeightmapMeshGenerator();
            var nativeMesh = Mesh.Create("");
            generator.Generate(
                ref nativeMesh,
                heightmapData.textureWidth,
                heightmapData.textureHeight,
                terrainHeight,
                heightmapData.HeightData,
                CoordinateSystem.EUN,
                heightmapData.min,
                heightmapData.max,
                heightmapData.minUV,
                heightmapData.maxUV,
                true);
            var mesh = MeshConverter.Convert(nativeMesh, name);
            nativeMesh.Dispose();
            return mesh.GenerateUnityMesh();
        }

        private async void PlaceMeshToSceneRecursive(TerrainConvertResult result, GameObject[] srcGameObjs,
            TerrainConvertOption option, bool skipRoot, int recursiveDepth)
        {
            if (!skipRoot)
            {
                if (heightmapData != null)
                {
                    var terrainHeight = (float)Math.Abs(heightmapData.max.Y - heightmapData.min.Y);
                    var generator = new HeightmapMeshGenerator();
                    var nativeMesh = Mesh.Create("");
                    generator.Generate(
                        ref nativeMesh,
                        heightmapData.textureWidth,
                        heightmapData.textureHeight,
                        terrainHeight,
                        heightmapData.HeightData,
                        CoordinateSystem.EUN,
                        heightmapData.min,
                        heightmapData.max,
                        heightmapData.minUV,
                        heightmapData.maxUV,
                        true);
                    var mesh = MeshConverter.Convert(nativeMesh, name);

                    var srcTrans = GetTransformByName(srcGameObjs, heightmapData.name);
                    string prevTextureName = TextureName(srcTrans);
                    
                    var gameObject = await mesh.PlaceToScene(srcTrans.parent, new DllSubMeshToUnityMaterialByTextureMaterial(), null, true);

                    var smoothedDem = gameObject.AddComponent<PLATEAUSmoothedDem>();
                    smoothedDem.HeightMapData = heightmapData;

                    var cityObjectGroup = gameObject.AddComponent<PLATEAUCityObjectGroup>();
                    foreach (var srcObj in srcGameObjs)
                    {
                        if (srcObj.name != heightmapData.name)
                            continue;

                        cityObjectGroup.CopyFrom(srcObj.GetComponent<PLATEAUCityObjectGroup>());
                        break;
                    }
                    
                    var material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                    material.mainTexture = LoadTexture2dWithoutAlpha(heightmapData.diffuseTexturePath);
                    material.mainTexture.name = prevTextureName;
                    material.SetColor("_BaseColor", Color.white);
                    material.SetFloat("_Smoothness", 0f);
                    result.Add(gameObject);

                    nativeMesh.Dispose();
                }
            }

            int nextRecursiveDepth = skipRoot ? 0 : recursiveDepth + 1;
            // 子を再帰的に配置します。
            foreach (var child in children)
            {
                child.PlaceMeshToSceneRecursive(result, srcGameObjs, option, false, nextRecursiveDepth);
            }
        }

        private string TextureName(Transform trans)
        {
            var renderer = trans.GetComponent<MeshRenderer>();
            if (renderer == null) return "";
            var material = renderer.sharedMaterial;
            if (material == null) return "";
            var texture = material.mainTexture;
            if (texture == null) return "";
            return texture.name;

        }
    }
}
