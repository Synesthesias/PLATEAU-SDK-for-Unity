using System;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Texture
{
    public class HeightmapGenerator
    {
        public void GenerateFromMesh(PolygonMesh.Mesh inMesh, int textureWidth, int textureHeight, 
            PlateauVector2d Margin, bool fillEdges, out PlateauVector3d Min, out PlateauVector3d Max, 
            out PlateauVector2f MinUV, out PlateauVector2f MaxUV, out UInt16[] HeightData )
        {
            var apiResult =
                NativeMethods.heightmap_generator_generate_from_mesh(inMesh.Handle, textureWidth, textureHeight, new PlateauVector3d(Margin.X, Margin.Y, 0), CoordinateSystem.EUN, fillEdges,
                out Min, out Max , out MinUV, out MaxUV, out IntPtr HeightmapDataPtr, out int DataSize);
            DLLUtil.CheckDllError(apiResult);

            byte[] outData = DLLUtil.PtrToBytes(HeightmapDataPtr, sizeof(UInt16) * DataSize);
            NativeMethods.release_heightmap_data(HeightmapDataPtr);

            HeightData = new UInt16[DataSize];
            int byteIndex = 0;
            for (int i = 0; i < HeightData.Length; i++)
            {
                HeightData[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += 2;
            }
        }

        static public float[,] ConvertTo2DFloatArray(UInt16[] heightData, int textureWidth, int textureHeight)
        {
            float[,] textureData = new float[textureWidth, textureHeight];
            int index = 0;
            for (int y = textureHeight - 1; y >=0; y--)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    textureData[y, x] = (float)(heightData[index] / 65535f);
                    index++;
                }
            }
            return textureData; ;
        }

        static public void SavePngFile(string fileName, int width, int height, UInt16[] data)
        {
            IntPtr heightmapDataPtr = Marshal.AllocHGlobal(sizeof(UInt16) * data.Length);

            byte[] outData = new byte[sizeof(UInt16) * data.Length];
            int byteIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte[] datasets = BitConverter.GetBytes(data[i]);
                for(int j = 0; j < datasets.Length; j++)
                {
                    outData[byteIndex] = datasets[j];
                    byteIndex++;
                }
            }
  
            Marshal.Copy(outData, 0, heightmapDataPtr, sizeof(UInt16) * data.Length);
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(fileName);
            var apiResult =
                NativeMethods.heightmap_save_png_file(fileNameUtf8, width, height, heightmapDataPtr);         

            Marshal.FreeHGlobal(heightmapDataPtr);
            DLLUtil.CheckDllError(apiResult);
        }

        static public void ReadPngFile(string fileName, int width, int height, out UInt16[] data)
        {
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(fileName);
            var apiResult =
                NativeMethods.heightmap_read_png_file(fileNameUtf8, width, height, out IntPtr heightmapDataPtr, out int dataSize);
            DLLUtil.CheckDllError(apiResult);

            byte[] outData = DLLUtil.PtrToBytes(heightmapDataPtr, sizeof(UInt16) * dataSize);
            NativeMethods.release_heightmap_data(heightmapDataPtr);

            data = new UInt16[dataSize];
            
            int byteIndex = 0;
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += 2;
            }

        }

        static public void SaveRawFile(string fileName, int width, int height, UInt16[] data)
        {
            IntPtr heightmapDataPtr = Marshal.AllocHGlobal(sizeof(UInt16) * data.Length);

            byte[] outData = new byte[sizeof(UInt16) * data.Length];
            int byteIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte[] datasets = BitConverter.GetBytes(data[i]);
                for (int j = 0; j < datasets.Length; j++)
                {
                    outData[byteIndex] = datasets[j];
                    byteIndex++;
                }
            }

            Marshal.Copy(outData, 0, heightmapDataPtr, sizeof(UInt16) * data.Length);
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(fileName);
            var apiResult =
                NativeMethods.heightmap_save_raw_file(fileNameUtf8, width, height, heightmapDataPtr);

            Marshal.FreeHGlobal(heightmapDataPtr);
            DLLUtil.CheckDllError(apiResult);
        }

        static public void ReadRawFile(string fileName, int width, int height, out UInt16[] Data)
        {
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(fileName);
            var apiResult =
                NativeMethods.heightmap_read_raw_file(fileNameUtf8, width, height, out IntPtr heightmapDataPtr, out int DataSize);
            DLLUtil.CheckDllError(apiResult);

            byte[] outData = DLLUtil.PtrToBytes(heightmapDataPtr, sizeof(UInt16) * DataSize);
            NativeMethods.release_heightmap_data(heightmapDataPtr);

            Data = new UInt16[DataSize];
            int byteIndex = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += 2;
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_generator_generate_from_mesh(
                [In] IntPtr srcMeshPtr,
                [In] int textureWidth,
                [In] int textureHeight,
                [In] PlateauVector3d margin,
                [In] CoordinateSystem coordinate,
                [In] bool fillEdges,
                out PlateauVector3d min,
                out PlateauVector3d max,
                out PlateauVector2f minUV,
                out PlateauVector2f maxUV,
                out IntPtr heightmapDataPtr,
                out int dataSize
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_save_png_file(
             [In] byte[] fileName,
             [In] int width,
             [In] int height,
             [In] IntPtr data
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_read_png_file(
             [In] byte[] fileName,
             [In] int width,
             [In] int height,
             out IntPtr outData,
             out int dataSize
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_save_raw_file(
             [In] byte[] fileName,
             [In] int width,
             [In] int height,
             [In] IntPtr data
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_read_raw_file(
             [In] byte[] fileName,
             [In] int width,
             [In] int height,
             out IntPtr outData,
             out int dataSize
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult release_heightmap_data(
             [In] IntPtr data
            );
        }
    }


}
