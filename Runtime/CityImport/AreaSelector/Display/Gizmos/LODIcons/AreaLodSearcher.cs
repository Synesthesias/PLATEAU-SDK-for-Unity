using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos.LODIcons
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

        public AreaLodSearcher(IDatasetSourceConfig datasetSourceConfig)
        {
            this.meshCodeToPackageLodDict = new ConcurrentDictionary<string, PackageToLodDict>();
            this.datasetSource = DatasetSource.Create(datasetSourceConfig);
        }
        
        
        /// <summary>
        /// 与えられたメッシュコードと、その上位に含まれるパッケージとLODを返します。
        /// </summary>
        public PackageToLodDict LoadLodsInMeshCode(string gridCodeStr)
        {

            SearchLodsInMeshCode(gridCodeStr);
            
            if (this.meshCodeToPackageLodDict.TryGetValue(gridCodeStr, out var packageToLodDict))
            {
                // 上位のメッシュコードがあれば、そのパッケージとLODも戻り値に加えます。
                var gridCode = GridCode.Create(gridCodeStr);
                if (gridCode.IsNormalGmlLevel)
                {
                    using var upperGridCode = gridCode.Upper();
                    if (this.meshCodeToPackageLodDict.TryGetValue(upperGridCode.StringCode, out var packageToLodDictLevel2))
                    {
                        packageToLodDict.MargeDict(packageToLodDictLevel2);
                    }
                }
            }

            return packageToLodDict;
        }

        /// <summary>
        /// グリッドコードと、その上位のグリッドコードに含まれるパッケージとLODを検索します。
        /// <see cref="meshCodeToPackageLodDict"/> に格納されます。
        /// </summary>
        private void SearchLodsInMeshCode(string gridCodeStr)
        {
            
            var gridCodes = new List<string> { gridCodeStr };
            // 上位のグリッドコードも対象とします。
            var gridCode = GridCode.Create(gridCodeStr);
            if (gridCode.IsNormalGmlLevel)
            {
                using var upper = gridCode.Upper();
                gridCodes.Add(upper.StringCode);
            }

            foreach (string currentMeshCode in gridCodes)
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
                        var gmlGridCode = gml.GridCode;
                        if (!gmlGridCode.IsValid) continue;
                        if (gmlGridCode.ToString() != currentMeshCode) continue;
                        
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
