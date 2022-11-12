using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Udx;

namespace PLATEAU.Editor.CityImport.AreaSelector
{
    public class AreaLodSearcher
    {
        // MeshCode <- (1対多) <- [ Package種, (多)LODs ]
        private ConcurrentDictionary<MeshCode, ConcurrentBag<PackageLod>> data;
        private string rootPath;

        public AreaLodSearcher(string rootPath)
        {
            this.data = new ConcurrentDictionary<MeshCode, ConcurrentBag<PackageLod>>();
            this.rootPath = rootPath;
        }
        
        public class PackageLod
        {
            public PredefinedCityModelPackage Package { get; private set; }
            public List<uint> Lods { get; private set; }

            public PackageLod(PredefinedCityModelPackage package, IEnumerable<uint> lods)
            {
                Package = package;
                Lods = lods.ToList();
            }
        }

        public void AddLodsInMeshCode(MeshCode meshCode)
        {
            var packageLods = SearchLodsInMeshCodeInner(meshCode, rootPath);
            foreach (var packageLod in packageLods)
            {
                this.data.AddOrUpdate(meshCode,
                    _ => new ConcurrentBag<PackageLod> { packageLod },
                    (_, bag) =>
                    {
                        bag.Add(packageLod);
                        return bag;
                    });
            }
        }

        public IEnumerable<PackageLod> GetLodsInMeshCode(MeshCode meshCode)
        {
            if (this.data.TryGetValue(meshCode, out var packageLods))
            {
                return packageLods.ToArray();
            }

            return null;
        }

        public static IEnumerable<PackageLod> SearchLodsInMeshCodeInner(MeshCode meshCode, string rootPath)
        {
            using var collection = UdxFileCollection
                .Find(rootPath)
                .FilterByMeshCodes(new[] { meshCode });

            var packageLods = new List<PackageLod>();
            foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
            {
                var gmlPaths = collection.GetGmlFiles(package);
                var lodSet = new SortedSet<uint>();
                foreach (var gmlPath in gmlPaths)
                {
                    string fullPath = Path.GetFullPath(gmlPath);
                    var lods = LodSearcher.SearchLodsInFile(fullPath);
                    foreach (var lod in lods)
                    {
                        lodSet.Add(lod);
                    }
                }
                packageLods.Add(new PackageLod(package, lodSet));
            }

            return packageLods;
        } 
    }
}
