using PLATEAU.Dataset;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityImport.Import
{
    /// <summary>
    /// GMLを1つインポートした後の処理を行うインターフェースです。
    /// </summary>
    public interface IPostGmlImportProcessor
    {
        void OnGmlImported(GmlImportResult result);
    }

    public interface IPostTileImportProcessor : IPostGmlImportProcessor
    {
        void OnTileImported(TileImportResult result);
    }

    /// <summary>
    /// GMLインポート結果をまとめたクラス
    /// </summary>
    public class GmlImportResult
    {
        public List<GameObject> GeneratedObjects { get; }
        public int TotalGmlCount { get; }
        public string GridCode { get; }
        public GmlFile GmlFile { get; }

        public GmlImportResult(List<GameObject> generatedObjects, int totalGmlCount, string gridCode, GmlFile gmlFile)
        {
            GeneratedObjects = generatedObjects;
            TotalGmlCount = totalGmlCount;
            GridCode = gridCode;
            GmlFile = gmlFile;
        }
    }

    /// <summary>
    /// Tileインポート結果をまとめたクラス
    /// </summary>
    public class TileImportResult: GmlImportResult
    {
        public int ZoomLevel { get; }

        public GameObject RootObject { get; }

        public TileImportResult(List<GameObject> generatedObjects, int totalGmlCount, string gridCode, GmlFile gmlFile, GameObject rootObject, int zoomLevel): 
            base(generatedObjects, totalGmlCount, gridCode, gmlFile)
        {
            ZoomLevel = zoomLevel;
            RootObject = rootObject;
        }
    }
} 