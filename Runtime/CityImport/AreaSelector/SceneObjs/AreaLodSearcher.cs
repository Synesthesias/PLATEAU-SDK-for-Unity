using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Udx;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class AreaLodSearcher
    {
        // MeshCode <- (1対多) <- [ Package種, (多)LODs ]
        private readonly ConcurrentDictionary<MeshCode, ConcurrentBag<PackageLods>> data;
        private readonly string rootPath;

        public AreaLodSearcher(string rootPath)
        {
            this.data = new ConcurrentDictionary<MeshCode, ConcurrentBag<PackageLods>>();
            this.rootPath = rootPath;
        }
        
        

        public IEnumerable<PackageLods> LoadLodsInMeshCode(MeshCode meshCode)
        {
            // すでに読み込み済（data にあれば）それを返します。
            if (this.data.TryGetValue(meshCode, out var packageLodBag))
            {
                return packageLodBag.ToArray();
            }
            
            // data になければ新たに読み込んで返します。
            var packageLods = SearchLodsInMeshCodeInner(meshCode, this.rootPath).ToArray();
            foreach (var packageLod in packageLods)
            {
                this.data.AddOrUpdate(meshCode,
                    _ => new ConcurrentBag<PackageLods> { packageLod },
                    (_, bag) =>
                    {
                        bag.Add(packageLod);
                        return bag;
                    });
            }

            return packageLods;
        }

        // public IEnumerable<PackageLod> GetLodsInMeshCode(MeshCode meshCode)
        // {
        //     if (this.data.TryGetValue(meshCode, out var packageLods))
        //     {
        //         return packageLods.ToArray();
        //     }
        //
        //     return null;
        // }

        public static IEnumerable<PackageLods> SearchLodsInMeshCodeInner(MeshCode meshCode, string rootPath)
        {
            var meshCodes = new List<MeshCode> { meshCode };
            // 上位のメッシュコードも対象とします。
            if(meshCode.Level == 3) meshCodes.Add(MeshCode.Parse(meshCode.Level2()));
            
            using var collection = UdxFileCollection
                .Find(rootPath)
                .FilterByMeshCodes(meshCodes.ToArray());

            var packageLods = new List<PackageLods>();
            foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
            {
                if (((uint)package & (uint)collection.Packages) == 0) continue;
                if (!AreaLodView.HasIconOfPackage(package)) continue;
                var gmlPaths = collection.GetGmlFiles(package);
                var lodSet = new SortedSet<uint>();
                foreach (var gmlPath in gmlPaths)
                {
                    string fullPath = Path.GetFullPath(gmlPath);
                    
                    // ファイルの中身を検索するので時間がかかります。
                    var lods = LodSearcher.SearchLodsInFile(fullPath);
                    
                    foreach (var lod in lods)
                    {
                        lodSet.Add(lod);
                    }
                }
                packageLods.Add(new PackageLods(package, lodSet));
            }

            return packageLods;
        } 
    }
    
    /// <summary>
    /// <see cref="PredefinedCityModelPackage"/> と LODリストの組です。
    /// </summary>
    public class PackageLods
    {
        public PredefinedCityModelPackage Package { get; private set; }
        public List<uint> Lods { get; private set; }

        public PackageLods(PredefinedCityModelPackage package, IEnumerable<uint> lods)
        {
            Package = package;
            Lods = lods.ToList();
        }
    }
}
