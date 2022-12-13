using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.Load.FileCopy
{
    internal static class TargetGmlFinder
    {

        public static List<GmlFile> FindTargetGmls(DatasetAccessor datasetAccessor, CityLoadConfig config)
        {
            var fetchTargetGmls = new List<GmlFile>();
            var gmlFilesDict = config.SearchMatchingGMLList(datasetAccessor);
            foreach (var gmlFile in gmlFilesDict.SelectMany(pair => pair.Value))
            {
                fetchTargetGmls.Add(gmlFile);
            }

            return fetchTargetGmls;
        }
    }
}
