using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// メッシュコード内で利用可能なLODを検索します。
    /// <see cref="AreaLodController"/> によって保持されます。
    /// </summary>
    public class AreaLodSearcher
    {
        // MeshCode <- (1対多) <- [ Package種, (多)LODs ]
        private readonly ConcurrentDictionary<string, PackageToLodDict> meshCodeToPackageLodDict;
        private readonly DatasetSource datasetSource;

        public AreaLodSearcher(DatasetSourceConfig datasetSourceConfig)
        {
            this.meshCodeToPackageLodDict = new ConcurrentDictionary<string, PackageToLodDict>();
            this.datasetSource = DatasetSource.Create(datasetSourceConfig);
        }
        
        
        /// <summary>
        /// 与えられたメッシュコードと、その上位に含まれるパッケージとLODを返します。
        /// </summary>
        public PackageToLodDict LoadLodsInMeshCode(string meshCode)
        {

            SearchLodsInMeshCode(meshCode);
            
            if (this.meshCodeToPackageLodDict.TryGetValue(meshCode, out var packageToLodDict))
            {
                // 上位のメッシュコードがあれば、そのパッケージとLODも戻り値に加えます。
                if (MeshCode.Parse(meshCode).Level == 3)
                {
                    if (this.meshCodeToPackageLodDict.TryGetValue(MeshCode.Parse(meshCode).Level2(), out var packageToLodDictLevel2))
                    {
                        packageToLodDict.MargeDict(packageToLodDictLevel2);
                    }
                }
            }

            return packageToLodDict;
        }

        /// <summary>
        /// メッシュコードと、その上位のメッシュコードに含まれるパッケージとLODを検索します。
        /// <see cref="meshCodeToPackageLodDict"/> に格納されます。
        /// </summary>
        private void SearchLodsInMeshCode(string meshCodeStr)
        {
            
            var meshCodes = new List<string> { meshCodeStr };
            // 上位のメッシュコードも対象とします。
            var parsedMeshCode = MeshCode.Parse(meshCodeStr);
            if(parsedMeshCode.Level == 3) meshCodes.Add(parsedMeshCode.Level2());

            foreach (string currentMeshCode in meshCodes)
            {
                // すでに検索済みデータがあればそれを利用します。
                this.meshCodeToPackageLodDict.TryGetValue(currentMeshCode, out var existing);
                if (existing != null) continue;
                
                // LODを検索します。
                using var accessor = this.datasetSource.Accessor;

                foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
                {
                    if (!AreaLodView.HasIconOfPackage(package)) continue; // 地図に表示しないパッケージはスキップします。
                    var gmls = accessor.GetGmlFiles(package);

                    int maxLod = -1;
                    foreach (var gml in gmls)
                    {
                        var gmlMeshCode = gml.MeshCode;
                        if (!gmlMeshCode.IsValid) continue;
                        if (gmlMeshCode.ToString() != currentMeshCode) continue;
                        
                        // ローカルの場合、ファイルの中身を検索するので時間がかかります。
                        // サーバーの場合、APIサーバーに問い合わせます。
                        maxLod = gml.GetMaxLod();
                    }
                    // 検索結果を追加します。
                    this.meshCodeToPackageLodDict.AddOrUpdate(currentMeshCode,
                        _ =>
                        {
                            var d = new PackageToLodDict();
                            d.AddOrUpdate(package, maxLod);
                            return d;
                        },
                        (_, d) =>
                        {
                            d.AddOrUpdate(package, maxLod);
                            return d;
                        });
                }
            }
        } 
    }

    
    
    
}
