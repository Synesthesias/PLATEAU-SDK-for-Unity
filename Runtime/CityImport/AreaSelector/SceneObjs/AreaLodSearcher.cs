using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.Ast;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Udx;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class AreaLodSearcher
    {
        // MeshCode <- (1対多) <- [ Package種, (多)LODs ]
        private readonly ConcurrentDictionary<string, ConcurrentBag<PackageLods>> data;
        private readonly string rootPath;

        public AreaLodSearcher(string rootPath)
        {
            this.data = new ConcurrentDictionary<string, ConcurrentBag<PackageLods>>();
            this.rootPath = rootPath;
        }
        
        

        public IEnumerable<PackageLods> LoadLodsInMeshCode(string meshCode)
        {

            SearchLodsInMeshCodeInner(meshCode, this.rootPath, this.data);
            if (this.data.TryGetValue(meshCode, out var packageLods))
            {
                if (MeshCode.Parse(meshCode).Level == 3)
                {
                    if (this.data.TryGetValue(MeshCode.Parse(meshCode).Level2(), out var packageLodsLevel2))
                    {
                        foreach(var l2 in packageLodsLevel2) packageLods.Add(l2);
                    }
                }
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

        public static void SearchLodsInMeshCodeInner(string meshCode, string rootPath, ConcurrentDictionary<string, ConcurrentBag<PackageLods>> data)
        {
            // すでに読み込み済（data にあれば）何もしません
            // if (data.TryGetValue(meshCode, out var packageLodBag))
            // {
            //     return;
            // }

            // meshCode自体は未読込であっても、その上位(level2)の　meshCode が読込済みであれば、
            // それを検索済みデータとして利用します。
            // PackageLods[] existing = null;
            // if (this.data.TryGetValue(MeshCode.Parse(meshCode.Level2()), out var level2PackageLods))
            // {
            //     existing = level2PackageLods.ToArray();
            // }
            
            
            var meshCodes = new List<string> { meshCode };
            // 上位のメッシュコードも対象とします。
            var parsedMeshCode = MeshCode.Parse(meshCode);
            if(parsedMeshCode.Level == 3) meshCodes.Add(parsedMeshCode.Level2());
            
            // using var collection = UdxFileCollection
            //     .Find(rootPath)
            //     .FilterByMeshCodes(meshCodes.ToArray());

            // var result = new Dictionary<MeshCode, List<PackageLods>>();
            foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
            {
                if (!AreaLodView.HasIconOfPackage(package)) continue; // 地図に表示しないパッケージはスキップします。
                foreach (var mCode in meshCodes)
                {
                    // すでに検索済みデータがあればそれを利用します。
                    if (data.TryGetValue(mCode, out var existing))
                    {
                        if (existing.Any(pl => pl.Package == package))
                        {
                            continue;
                        }
                        // result.Add(meshCode, new PackageLods(package, existing.SelectMany(e => e.Lods)));
                        // if(resul)
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
                    // packageLods.Add(new PackageLods(package, lodSet));
                    // 検索結果を追加します。
                    data.AddOrUpdate(mCode,
                        _ => new ConcurrentBag<PackageLods> { new PackageLods(package, lodSet) },
                        (_, bag) =>
                        {
                            bag.Add(new PackageLods(package, lodSet));
                            return bag;
                        });
                }
            }
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
