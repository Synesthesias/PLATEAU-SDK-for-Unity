using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Udx;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// メッシュコード内で利用可能なLODを検索します。
    /// <see cref="AreaLodController"/> によって保持されます。
    /// </summary>
    public class AreaLodSearcher
    {
        // MeshCode <- (1対多) <- [ Package種, (多)LODs ]
        private readonly ConcurrentDictionary<string, PackageToLodDict> data;
        private readonly string rootPath;

        public AreaLodSearcher(string rootPath)
        {
            this.data = new ConcurrentDictionary<string, PackageToLodDict>();
            this.rootPath = rootPath;
        }
        
        
        /// <summary>
        /// 与えられたメッシュコードと、その上位に含まれるパッケージとLODを返します。
        /// </summary>
        public PackageToLodDict LoadLodsInMeshCode(string meshCode)
        {

            SearchLodsInMeshCode(meshCode, this.rootPath, this.data);
            if (this.data.TryGetValue(meshCode, out var packageToLodDict))
            {
                if (MeshCode.Parse(meshCode).Level == 3)
                {
                    // 上位のメッシュコードがあれば、そのパッケージとLODも戻り値に加えます。
                    if (this.data.TryGetValue(MeshCode.Parse(meshCode).Level2(), out var packageToLodDictLevel2))
                    {
                        packageToLodDict.Marge(packageToLodDictLevel2);
                    }
                }
            }

            return packageToLodDict;
        }

        /// <summary>
        /// メッシュコードと、その上位のメッシュコードに含まれるパッケージとLODを検索します。
        /// 結果は引数の <paramref name="data" /> に格納されます。
        /// </summary>
        private static void SearchLodsInMeshCode(string meshCode, string rootPath, ConcurrentDictionary<string, PackageToLodDict> data)
        {
            
            var meshCodes = new List<string> { meshCode };
            // 上位のメッシュコードも対象とします。
            var parsedMeshCode = MeshCode.Parse(meshCode);
            if(parsedMeshCode.Level == 3) meshCodes.Add(parsedMeshCode.Level2());
            
            foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
            {
                if (!AreaLodView.HasIconOfPackage(package)) continue; // 地図に表示しないパッケージはスキップします。
                foreach (var mCode in meshCodes)
                {
                    // すでに検索済みデータがあればそれを利用します。
                    if (data.TryGetValue(mCode, out var existing))
                    {
                        if (existing.ExistLod(package))
                        {
                            continue;
                        }
                    }
                    
                    // LODを検索します。
                    using var collection = UdxFileCollection.Find(rootPath).FilterByMeshCodes(new []{MeshCode.Parse(mCode)});

                    var gmlPaths = collection.GetGmlFiles(package);
                    var lodSet = new SortedSet<uint>();
                    foreach (var gmlPath in gmlPaths)
                    {
                        string fullPath = Path.GetFullPath(gmlPath);
                        Debug.Log($"Searching LOD for {Path.GetFileName(gmlPath)}, {package}");
                    
                        // ファイルの中身を検索するので時間がかかります。
                        var lods = LodSearcher.SearchLodsInFile(fullPath);
                    
                        foreach (var lod in lods)
                        {
                            lodSet.Add(lod);
                        }
                    }
                    // 検索結果を追加します。
                    data.AddOrUpdate(mCode,
                        _ =>
                        {
                            var d = new PackageToLodDict();
                            d.AddOrUpdate(package, lodSet);
                            return d;
                        },
                        (_, d) =>
                        {
                            d.AddOrUpdate(package, lodSet);
                            return d;
                        });
                }
            }
        } 
    }

    /// <summary>
    /// パッケージとそこに含まれるLODの組です。
    /// </summary>
    public class PackageToLodDict
    {
        private ConcurrentDictionary<PredefinedCityModelPackage, ConcurrentBag<uint>> data = new ConcurrentDictionary<PredefinedCityModelPackage, ConcurrentBag<uint>>();
        
        public void AddOrUpdate(PredefinedCityModelPackage package, IEnumerable<uint> lods)
        {
            this.data.AddOrUpdate(package,
                _ => new ConcurrentBag<uint>(lods), 
                (_, __) => new ConcurrentBag<uint>(lods));
        }
        
        public bool ExistLod(PredefinedCityModelPackage package)
        {
            if (!this.data.TryGetValue(package, out var lods))
            {
                return false;
            }

            return lods.Any();
        }

        public void Marge(PackageToLodDict other)
        {
            foreach (var pair in other)
            {
                var package = pair.Key;
                var otherLods = pair.Value;
                this.data.AddOrUpdate(package,
                    _ => otherLods,
                    (modelPackage, bag) =>
                    {
                        foreach (uint l in otherLods) bag.Add(l);
                        return bag;
                    }
                );
            }
        }
        
        public IEnumerator<KeyValuePair<PredefinedCityModelPackage, ConcurrentBag<uint>>> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
    }
    
    
}
