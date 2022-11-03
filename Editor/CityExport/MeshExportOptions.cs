using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.MeshWriter;

namespace PLATEAU.Editor.CityExport
{
    internal struct MeshExportOptions
    {
        public enum MeshTransformType{Local/*ローカル座標系*/, PlaneCartesian/*平面直角座標系*/}

        public MeshTransformType TransformType;
        public bool ExportTextures;
        public bool ExportHiddenObjects;
        public MeshFileFormat FileFormat;
        public CoordinateSystem MeshAxis;
        public GltfWriteOptions GltfWriteOptions; // Gltf形式のときのみ利用します。

        public MeshExportOptions(MeshTransformType transformType, bool exportTextures, bool exportHiddenObjects, MeshFileFormat fileFormat, CoordinateSystem meshAxis, GltfWriteOptions gltfWriteOptions)
        {
            this.TransformType = transformType;
            this.ExportTextures = exportTextures;
            this.ExportHiddenObjects = exportHiddenObjects;
            this.FileFormat = fileFormat;
            this.MeshAxis = meshAxis;
            this.GltfWriteOptions = gltfWriteOptions;
        }
    }
}
