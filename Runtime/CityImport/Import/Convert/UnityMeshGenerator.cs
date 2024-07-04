using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PLATEAU.CityImport.Import.Convert
{
    /// <summary>
    /// メッシュを作成します。
    /// メッシュを軽量にする手法があることをご指摘くださったSagar Patelさんに感謝します。
    /// ご指摘の手法をこのクラスで利用しています。
    /// その手法とは、Unity Advanced Mesh APIを利用して、頂点のデータ構造を厳密に定義することで、
    /// 精度が不要な箇所でビット数を削減します。
    /// </summary>
    /// 
    /// 補足:
    /// メッシュのビット数が削減されたことを確認するには、インスペクタのMeshFilterコンポーネントのメッシュ選択部分をダブルクリックします。
    /// そこに頂点のデータ構造とデータ量が表示されます。
    /// 
    /// Unityのデフォルトでは、頂点データは次の通りです:
    /// ・ Position: Float32*3 = 12bytes
    /// ・ Normal  : Float32*4 = 16bytes
    /// ・ Tangent : Flaot32*4 = 16bytes
    /// ・ UV0     : Float32*2 = 8bytes
    /// ・ UV3     : Float32*2 = 8bytes
    ///   計 60bytes
    ///
    /// このクラスでメッシュを生成すると、頂点データは次の通りです。:
    /// ・ Position: Float32*3 = 12bytes
    /// ・ Normal  : Float16*4 = 8bytes
    /// ・ Tangent : Float16*4 = 8bytes
    /// ・ UV0     : Float32*2 = 8bytes
    /// ・ UV3     : Float32*2 = 8bytes
    ///   計 44bytes
    internal class UnityMeshGenerator
    {
        /// <summary>
        /// 頂点のデータ構造を厳密に定義します。
        /// 変更時は、下のGenerateメソッドと辻褄が合うように厳密に順番や型のビット数を合わせてください。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexStruct
        {
            public Vector3 position;
            public HalfVector4 normal; // normalとtangentで、デフォルトのVector3よりも軽量なデータ型を使うのが最適化の要です。
            public HalfVector4 tangent;
            public Vector2 uv1; // UV1は削減したくないです。4Kテクスチャを扱うには16bit浮動小数点ではピクセルがずれる恐れがあります。
            public Vector2 uv4; // UV4も削減したくないです。16bit浮動小数点では、UV4が4桁の数に達した場合、数値が1ずれて属性情報がズレる恐れがあります。
        }
        
        
        [StructLayout(LayoutKind.Sequential)]
        private struct HalfVector4
        {
            public ushort x, y, z, w;

            public HalfVector4(Vector4 vec)
            {
                x = Mathf.FloatToHalf(vec.x);
                y = Mathf.FloatToHalf(vec.y);
                z = Mathf.FloatToHalf(vec.z);
                w = Mathf.FloatToHalf(vec.w);
            }
        }
        
        /// <summary>
        /// Unityのメッシュを生成します。
        /// </summary>
        public Mesh Generate(ConvertedMeshData src)
        {
            int vertexCount = src.VerticesCount;
            int indicesCount = src.IndicesCount;
            new NormalCalculator().Calc(src.Vertices, src.Indices, out var normals, out var tangents);
            var mesh = new Mesh();
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];
            
            // 頂点のデータ構造を定義します。上記のVertexStructと厳密に順番とビット数を合わせます。
            var attributes = new NativeArray<VertexAttributeDescriptor>(5, Allocator.Temp);
            attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
            attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4, 0);
            attributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4, 0);
            attributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 2, 0); // UV1
            attributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0); // UV4
            meshData.SetVertexBufferParams(vertexCount, attributes);
            
            // インデックス
            meshData.SetIndexBufferParams(indicesCount, IndexFormat.UInt32);

            // 頂点設定
            var srcVertices = src.Vertices;
            var srcUV1 = src.UV1;
            var srcUV4 = src.UV4;
            var dstVertices = meshData.GetVertexData<VertexStruct>();
            for (int i = 0; i < vertexCount; i++)
            {
                dstVertices[i] = new VertexStruct
                {
                    position = srcVertices[i],
                    normal = new HalfVector4(normals[i]),
                    tangent = new HalfVector4(tangents[i]),
                    uv1 = srcUV1[i],
                    uv4 = srcUV4[i]
                }; 
            }
            
            // インデックス設定
            var dstIndices = meshData.GetIndexData<int>();
            dstIndices.CopyFrom(src.Indices);
            
            // サブメッシュ設定
            int subMeshCount = src.SubMeshCount;
            meshData.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                meshData.SetSubMesh(i, new SubMeshDescriptor(src.SubMeshStarts[i], src.SubMeshLengths[i]));
            }
            
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            mesh.RecalculateBounds();
            
            attributes.Dispose();
            
            mesh.name = src.Name;
            return mesh;
        }
    }

    /// <summary>
    /// 法線とTangentsを計算します。
    /// </summary>
    internal class NormalCalculator
    {
        /// <summary>
        /// このメソッドでのみ利用する仮のメッシュを作り、メッシュに対してUnityの法線計算機能を呼ぶことでnormalとtangentsを計算します。
        /// </summary>
        public void Calc(Vector3[] vertices, int[] indices, out Vector3[] outNormals,
            out Vector4[] outTangents)
        {
            int vertexCount = vertices.Length;
            int indicesCount = indices.Length;
            var mesh = new Mesh();
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];
            
            // 頂点
            var attributes = new NativeArray<VertexAttributeDescriptor>(1, Allocator.Temp);
            attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
            meshData.SetVertexBufferParams(vertexCount, attributes);
            
            // インデックス
            meshData.SetIndexBufferParams(indicesCount, IndexFormat.UInt32);

            // 頂点設定
            var dstVertices = meshData.GetVertexData<Vector3>();
            dstVertices.CopyFrom(vertices);
            
            // インデックス設定
            var dstIndices = meshData.GetIndexData<int>();
            dstIndices.CopyFrom(indices);
            
            // サブメッシュの設定
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indicesCount));
            
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            
            // 法線とタンジェント計算
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            outNormals = mesh.normals;
            outTangents = mesh.tangents;
            
            Object.DestroyImmediate(mesh);
            attributes.Dispose();
        }
    }
}