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
        public FbxWriteOptions FbxWriteOptions; // fbx形式のときのみ利用します。

        public MeshExportOptions(MeshTransformType transformType, bool exportTextures, bool exportHiddenObjects, MeshFileFormat fileFormat, CoordinateSystem meshAxis, GltfWriteOptions gltfWriteOptions, FbxWriteOptions fbxWriteOptions)
        {
            this.TransformType = transformType;
            this.ExportTextures = exportTextures;
            this.ExportHiddenObjects = exportHiddenObjects;
            this.FileFormat = fileFormat;
            this.MeshAxis = meshAxis;
            
            // TODO GltfWriteOptions と FbxWriteOptions は出力形式が gltf, fbx でなければ不要なのにいちいち渡している。スマートに書けないか？
            this.GltfWriteOptions = gltfWriteOptions;
            this.FbxWriteOptions = fbxWriteOptions;
        }
    }
}
