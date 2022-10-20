using PLATEAU.MeshWriter;

namespace PLATEAU.Editor.CityExport
{
    internal struct MeshExportOptions
    {
        public enum MeshTransformType{Local/*ローカル座標系*/, PlaneCartesian/*平面直角座標系*/}
        public enum MeshFileFormat{Obj, Fbx, Gltf}

        public MeshTransformType TransformType;
        public bool ExportHiddenObjects;
        public MeshFileFormat FileFormat;
        public GltfWriteOptions GltfWriteOptions; // Gltf形式のときのみ利用します。
    }
}
