using System;
using PLATEAU.CityExport.Exporters;
using PLATEAU.Geometries;

namespace PLATEAU.CityExport
{
    public enum MeshFileFormat{OBJ, GLTF, FBX}

    public static class MeshFileFormatExtension
    {
        public static string[] ToExtensions(this MeshFileFormat format)
        {
            return format switch
            {
                MeshFileFormat.OBJ => new[] { ".obj" },
                MeshFileFormat.GLTF => new[] { ".glb", ".gltf"},
                MeshFileFormat.FBX => new[] {".fbx"},
                _ => throw new ArgumentOutOfRangeException(nameof(format), "Unknown format.")
            };
        }
    }
    
    /// <summary>
    /// SDK for Unity のエクスポートに関する設定です。
    /// </summary>
    public struct MeshExportOptions
    {
        public enum MeshTransformType{Local/*ローカル座標系*/, PlaneCartesian/*平面直角座標系*/}

        /// <summary>座標変換</summary>
        public MeshTransformType TransformType { get; }
        
        /// <summary>テクスチャをエクスポートするか</summary>
        public bool ExportTextures { get; }
        
        /// <summary>
        /// テクスチャをエクスポートする場合、SDK付属のデフォルトマテリアルのテクスチャも含めるかどうかです。
        /// falseの場合、付属のデフォルトマテリアルはマテリアル情報なしに置き換えられます。
        /// </summary>
        public bool ExportDefaultTextures { get; }
        
        /// <summary> ヒエラルキー上で非表示になっているゲームオブジェクトをエクスポートするかどうかです。</summary>
        public bool ExportHiddenObjects { get; }
        public MeshFileFormat FileFormat { get; }
        
        /// <summary>座標系</summary>
        public CoordinateSystem MeshAxis { get; }
        
        /// <summary>エクスポートを処理するインスタンスです。</summary>
        public ICityExporter Exporter { get; }

        public MeshExportOptions(MeshTransformType transformType, bool exportTextures, bool exportDefaultTextures, bool exportHiddenObjects, MeshFileFormat fileFormat, CoordinateSystem meshAxis, ICityExporter exporter)
        {
            TransformType = transformType;
            ExportTextures = exportTextures;
            ExportDefaultTextures = exportDefaultTextures;
            ExportHiddenObjects = exportHiddenObjects;
            FileFormat = fileFormat;
            MeshAxis = meshAxis;
            Exporter = exporter;
        }
    }
}
