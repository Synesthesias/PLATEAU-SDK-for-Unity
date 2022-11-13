using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PLATEAU.Udx;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class AreaLodView
    {
        private PackageLods[] packageLods = null;
        private Vector3 position;
        // private static readonly Color textColor = Color.black;
        private const string iconDirPath = "Packages/com.synesthesias.plateau-unity-sdk/Images/AreaSelect";
        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> iconDict;

        public AreaLodView(IEnumerable<PackageLods> packageLods, Vector3 position)
        {
            this.packageLods = packageLods.ToArray();
            this.position = position;
        }

        public static void Init()
        {
            iconDict = ComposeIconDict();
        }

        public void DrawHandles()
        {
            if (this.packageLods == null) return;
            if (iconDict == null)
            {
                Debug.LogError("Failed to load icons.");
                return;
            }
            // var sb = new StringBuilder();
            // foreach (var packageLod in this.packageLods)
            // {
            //     if (packageLod.Lods.Count == 0) continue;
            //     sb.Append($"{packageLod.Package} => ");
            //     foreach(var lod in packageLod.Lods)
            //     {
            //         sb.Append($"{lod}, ");
            //     }
            //
            //     sb.Append("\n");
            // }

            var pos = this.position;
            foreach (var packageLod in this.packageLods)
            {
                if (packageLod.Lods.Count == 0) continue;
                uint maxLod = packageLod.Lods.Max();
                var package = packageLod.Package;
                if (iconDict.TryGetValue((package, maxLod), out var iconTex))
                {
                    var style = new GUIStyle(EditorStyles.label);
                    style.fixedHeight = 50;
                    style.fixedWidth = 50;
                    var content = new GUIContent(iconTex);
                    Handles.Label(pos, content, style);
                    pos += new Vector3(100, 0, 0);
                }
            }
        }

        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> ComposeIconDict()
        {
            return new ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture>(
                new Dictionary<(PredefinedCityModelPackage package, uint lod), Texture>
                {
                    {(PredefinedCityModelPackage.Building, 0), LoadIcon("icon_building_lod1.png")},
                    {(PredefinedCityModelPackage.Building, 1), LoadIcon("icon_building_lod1.png")},
                    {(PredefinedCityModelPackage.Building, 2), LoadIcon("icon_building_lod2.png")},
                    {(PredefinedCityModelPackage.Building, 3), LoadIcon("icon_building_lod3.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 0), LoadIcon("icon_cityfurniture_lod1.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 1), LoadIcon("icon_cityfurniture_lod1.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 2), LoadIcon("icon_cityfurniture_lod2.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 3), LoadIcon("icon_cityfurniture_lod3.png")},
                    {(PredefinedCityModelPackage.Road, 0), LoadIcon("icon_road_lod1.png")},
                    {(PredefinedCityModelPackage.Road, 1), LoadIcon("icon_road_lod1.png")},
                    {(PredefinedCityModelPackage.Road, 2), LoadIcon("icon_road_lod2.png")},
                    {(PredefinedCityModelPackage.Road, 3), LoadIcon("icon_road_lod3.png")},
                    {(PredefinedCityModelPackage.Vegetation, 0), LoadIcon("icon_vegetation_lod1.png")},
                    {(PredefinedCityModelPackage.Vegetation, 1), LoadIcon("icon_vegetation_lod1.png")},
                    {(PredefinedCityModelPackage.Vegetation, 2), LoadIcon("icon_vegetation_lod2.png")},
                    {(PredefinedCityModelPackage.Vegetation, 3), LoadIcon("icon_vegetation_lod3.png")},
                });
        }

        public static bool HasIconOfPackage(PredefinedCityModelPackage package)
        {
            if (iconDict == null) iconDict = ComposeIconDict();
            return iconDict.TryGetValue((package, 1), out _);
        }

        private static Texture LoadIcon(string relativePath)
        {
            string path = Path.Combine(iconDirPath, relativePath).Replace('\\', '/');
            var texture =  AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (texture == null)
            {
                Debug.LogError($"Icon image file is not found : {path}");
            }
            return texture;
        }
    }
}
