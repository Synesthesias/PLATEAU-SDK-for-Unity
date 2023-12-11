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
    
    public struct MeshExportOptions
    {
        public enum MeshTransformType{Local/*ローカル座標系*/, PlaneCartesian/*平面直角座標系*/}

        public MeshTransformType TransformType { get; }
        public bool ExportTextures { get; }
        public bool ExportHiddenObjects { get; }
        public MeshFileFormat FileFormat { get; }
        public CoordinateSystem MeshAxis { get; }

        public ICityExporter Exporter { get; }

        public MeshExportOptions(MeshTransformType transformType, bool exportTextures, bool exportHiddenObjects, MeshFileFormat fileFormat, CoordinateSystem meshAxis, ICityExporter exporter)
        {
            TransformType = transformType;
            ExportTextures = exportTextures;
            ExportHiddenObjects = exportHiddenObjects;
            FileFormat = fileFormat;
            MeshAxis = meshAxis;
            Exporter = exporter;
        }
    }
}
