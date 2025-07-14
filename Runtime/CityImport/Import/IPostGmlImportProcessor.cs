using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityImport.Import
{
    /// <summary>
    /// GMLインポート後の処理を行うインターフェース
    /// </summary>
    public interface IPostGmlImportProcessor
    {
        Task ProcessAsync(GmlImportResult result, CancellationToken token);
    }

    /// <summary>
    /// GMLインポート結果をまとめたクラス
    /// </summary>
    public class GmlImportResult
    {
        public List<GameObject> GeneratedObjects { get; }
        public int TotalGmlCount { get; }
        public string GridCode { get; }
        public object GmlFile { get; } // 型は必要に応じてGmlFile等に変更

        public GmlImportResult(List<GameObject> generatedObjects, int totalGmlCount, string gridCode, object gmlFile)
        {
            GeneratedObjects = generatedObjects;
            TotalGmlCount = totalGmlCount;
            GridCode = gridCode;
            GmlFile = gmlFile;
        }
    }
} 